/**
 * PipelineGraph.cs
 *
 * Graph datastructure for managing Block nodes and their connections.
 */

using ImageAutomate.Core.Serialization;

namespace ImageAutomate.Core;

/// <summary>
/// Defines a connection between specific sockets of two blocks.
/// </summary>
public record Connection(
    IBlock Source,
    Socket SourceSocket,
    IBlock Target,
    Socket TargetSocket
);

/// <summary>
/// Represents a directed graph of blocks and connections.
/// </summary>
/// <remarks>
/// The class is not thread-safe; external synchronization is required if accessed concurrently.
/// </remarks>
public class PipelineGraph
{
    private static readonly System.Text.Json.JsonSerializerOptions _serializerOptions = new()
    {
        IncludeFields = true,
        WriteIndented = true,
    };

    #region Private Fields
    private readonly List<IBlock> _nodes = []; // nodes maintain layer hierarchy
    private readonly List<Connection> _edges = [];
    #endregion
    private IBlock? SelectedBlock
    {
        get => SelectedItem as IBlock;
        set => SelectedItem = value;
    }

    #region Properties
    public object? SelectedItem { get; set; }
    public IReadOnlyList<IBlock> Nodes => _nodes;
    public IReadOnlyList<Connection> Edges => _edges;
    /// <summary>
    /// Occurs when the graph structure changes (nodes/edges added or removed).
    /// </summary>
    public event Action<IBlock>? OnNodeRemoved;
    #endregion

    #region Public API

    /// <summary>
    /// Adds a block to the graph.
    /// </summary>
    public void AddBlock(IBlock block)
    {
        if (!_nodes.Contains(block))
        {
            _nodes.Add(block);
        }
    }

    /// <summary>
    /// Removes a block and all its associated connections from the graph.
    /// </summary>
    public void RemoveNode(IBlock block)
    {
        if (_nodes.Contains(block))
        {
            // Remove edges connected to this block
            _edges.RemoveAll(e => e.Source == block || e.Target == block);

            _nodes.Remove(block);

            if (SelectedItem == block) SelectedItem = null;
            // Also need to check if SelectedItem was an edge connected to this block
            if (SelectedItem is Connection edge && (edge.Source == block || edge.Target == block))
            {
                SelectedItem = null;
            }

            OnNodeRemoved?.Invoke(block);
        }
    }

    /// <summary>
    /// Connects two blocks via specific sockets.
    /// </summary>
    public void AddEdge(IBlock source, Socket sourceSocket, IBlock target, Socket targetSocket)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(sourceSocket);
        ArgumentNullException.ThrowIfNull(targetSocket);
        if (!Nodes.Contains(source))
            throw new ArgumentException($"Source block '{source.Title}' not found in graph");
        if (!Nodes.Contains(target))
            throw new ArgumentException($"Target block '{target.Title}' not found in graph");
        if (source.Outputs.All(s => s != sourceSocket))
            throw new ArgumentException($"Source socket '{sourceSocket.Id}' not found on {source.Title}");
        if (target.Inputs.All(s => s != targetSocket))
            throw new ArgumentException($"Target socket '{targetSocket.Id}' not found on {target.Title}");

        _edges.Add(new Connection(source, sourceSocket, target, targetSocket));
    }

    /// <summary>
    /// Connects two blocks via socket IDs.
    /// </summary>
    /// <exception cref="ArgumentException">when socket ID not found on their respective block</exception>
    public void AddEdge(IBlock source, string sourceSocketId, IBlock target, string targetSocketId)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(target);
        var srcSocket = source.Outputs.FirstOrDefault(s => s.Id == sourceSocketId)
            ?? throw new ArgumentException($"Socket ID '{sourceSocketId}' not found on source '{source.Title}'");
        var tgtSocket = target.Inputs.FirstOrDefault(s => s.Id == targetSocketId)
            ?? throw new ArgumentException($"Socket ID '{targetSocketId}' not found on target '{target.Title}'");

        AddEdge(source, srcSocket, target, tgtSocket);
    }

    /// <summary>
    /// Removes the specified connection.
    /// </summary>
    public void RemoveEdge(Connection edge)
    {
        if (_edges.Remove(edge))
        {
            if (SelectedItem is Connection conn && conn == edge)
                SelectedItem = null;
        }
    }

    /// <summary>
    /// Moves the specified block to the top layer (end of the list).
    /// </summary>
    public void BringToTop(IBlock block)
    {
        if (_nodes.Remove(block))
            _nodes.Add(block);
    }

    /// <summary>
    /// Clears all blocks and connections from the graph.
    /// </summary>
    public void Clear()
    {
        _edges.Clear();
        _nodes.Clear();
        SelectedItem = null;
    }
    #endregion

    #region Serialization
    /// <summary>
    /// Serializes the PipelineGraph to JSON string.
    /// </summary>
    public string ToJson()
    {
        var dto = ToDto();
        return System.Text.Json.JsonSerializer.Serialize(dto, _serializerOptions);
    }

    /// <summary>
    /// Deserializes a PipelineGraph from JSON string.
    /// </summary>
    public static PipelineGraph FromJson(string json)
    {
        var dto = System.Text.Json.JsonSerializer.Deserialize<PipelineGraphDto>(json, _serializerOptions);
        if (dto == null)
            throw new InvalidOperationException("Failed to deserialize PipelineGraph from JSON.");
        return FromDto(dto);
    }

    /// <summary>
    /// Converts the PipelineGraph to a DTO for serialization.
    /// Layout is stored directly in each block's properties.
    /// </summary>
    internal PipelineGraphDto ToDto()
    {
        List<BlockDto> blocks = [];
        List<ConnectionDto> connections = [];
        int? centerBlockIndex = null;

        // Serialize blocks (layout is now part of block properties)
        foreach (var block in _nodes)
        {
            blocks.Add(BlockSerializer.Serialize(block));
        }

        // Serialize connections (using block indices)
        foreach (var connection in _edges)
        {
            var sourceIndex = _nodes.IndexOf(connection.Source);
            var targetIndex = _nodes.IndexOf(connection.Target);

            if (sourceIndex < 0 || targetIndex < 0)
                continue;

            connections.Add(new ConnectionDto
            {
                SourceBlockIndex = sourceIndex,
                SourceSocketId = connection.SourceSocket.Id,
                TargetBlockIndex = targetIndex,
                TargetSocketId = connection.TargetSocket.Id
            });
        }

        // Serialize selected block
        if (SelectedBlock != null)
        {
            centerBlockIndex = _nodes.IndexOf(SelectedBlock);
        }

        return new PipelineGraphDto(blocks, connections, centerBlockIndex);
    }

    /// <summary>
    /// Creates a PipelineGraph from a DTO.
    /// </summary>
    internal static PipelineGraph FromDto(PipelineGraphDto dto)
    {
        var graph = new PipelineGraph();

        // Deserialize blocks (layout is restored directly into block properties)
        var blocks = new List<IBlock>();
        foreach (var blockDto in dto.Blocks)
        {
            var block = BlockSerializer.Deserialize(blockDto);
            blocks.Add(block);
            graph.AddBlock(block);
        }

        // Deserialize connections
        foreach (var connDto in dto.Connections)
        {
            if (connDto.SourceBlockIndex < 0 || connDto.SourceBlockIndex >= blocks.Count)
                continue;
            if (connDto.TargetBlockIndex < 0 || connDto.TargetBlockIndex >= blocks.Count)
                continue;

            var sourceBlock = blocks[connDto.SourceBlockIndex];
            var targetBlock = blocks[connDto.TargetBlockIndex];

            var sourceSocket = sourceBlock.Outputs.FirstOrDefault(s => s.Id == connDto.SourceSocketId);
            var targetSocket = targetBlock.Inputs.FirstOrDefault(s => s.Id == connDto.TargetSocketId);

            if (sourceSocket != null && targetSocket != null)
            {
                graph.AddEdge(sourceBlock, sourceSocket, targetBlock, targetSocket);
            }
        }

        // Restore selected block
        if (dto.CenterBlockIndex.HasValue &&
            dto.CenterBlockIndex.Value >= 0 &&
            dto.CenterBlockIndex.Value < blocks.Count)
        {
            graph.SelectedItem = blocks[dto.CenterBlockIndex.Value];
        }

        return graph;
    }
    #endregion
}