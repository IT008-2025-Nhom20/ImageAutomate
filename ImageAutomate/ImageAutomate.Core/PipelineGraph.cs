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
    private IBlock? _center;
    private readonly List<IBlock> _blocks = [];
    private readonly List<Connection> _connections = [];
    #endregion

    #region Properties
    public IBlock? Center
    {
        get => _center;
        set
        {
            if (value != null && !_blocks.Contains(value))
                throw new InvalidOperationException("Center block must be part of the graph.");
            _center = value;
            GraphChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    public IReadOnlyList<IBlock> Blocks => _blocks;
    public IReadOnlyList<Connection> Connections => _connections;
    /// <summary>
    /// Occurs when the graph structure changes (nodes/edges added or removed).
    /// </summary>
    public event EventHandler? GraphChanged;
    #endregion

    #region Public API
    public void AddBlock(IBlock block)
    {
        if (!_blocks.Contains(block))
        {
            _blocks.Add(block);
            GraphChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Removes a block and all its associated connections from the graph.
    /// </summary>
    public void RemoveBlock(IBlock block)
    {
        if (_blocks.Remove(block))
        {
            // Remove all connections touching this block
            _connections.RemoveAll(c => c.Source == block || c.Target == block);

            if (Center == block)
                Center = null;
            GraphChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Connects two blocks via specific sockets.
    /// </summary>
    public void Connect(IBlock source, Socket sourceSocket, IBlock target, Socket targetSocket)
    {
        if (!Blocks.Contains(source))
            throw new ArgumentException($"Source block '{source.Title}' not found in graph");
        if (!Blocks.Contains(target))
            throw new ArgumentException($"Target block '{target.Title}' not found in graph");
        if (source.Outputs.All(s => s != sourceSocket))
            throw new ArgumentException($"Source socket '{sourceSocket.Id}' not found on {source.Title}");
        if (target.Inputs.All(s => s != targetSocket))
            throw new ArgumentException($"Target socket '{targetSocket.Id}' not found on {target.Title}");

        _connections.Add(new Connection(source, sourceSocket, target, targetSocket));

        GraphChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Connects two blocks via Socket IDs.
    /// </summary>
    public void Connect(IBlock source, string sourceSocketId, IBlock target, string targetSocketId)
    {
        var srcSocket = source.Outputs.FirstOrDefault(s => s.Id == sourceSocketId)
            ?? throw new ArgumentException($"Socket ID '{sourceSocketId}' not found on source '{source.Title}'");
        var tgtSocket = target.Inputs.FirstOrDefault(s => s.Id == targetSocketId)
            ?? throw new ArgumentException($"Socket ID '{targetSocketId}' not found on target '{target.Title}'");

        Connect(source, srcSocket, target, tgtSocket);
    }

    /// <summary>
    /// Removes the specified connection.
    /// </summary>
    public void Disconnect(Connection connection)
    {
        if (_connections.Remove(connection))
            GraphChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Clears all blocks and connections from the graph.
    /// </summary>
    public void Clear()
    {
        _connections.Clear();
        _blocks.Clear();
        Center = null;
        GraphChanged?.Invoke(this, EventArgs.Empty);
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
    /// </summary>
    internal PipelineGraphDto ToDto()
    {
        var dto = new PipelineGraphDto();

        // Serialize blocks
        foreach (var block in _blocks)
        {
            dto.Blocks.Add(BlockSerializer.Serialize(block));
        }

        // Serialize connections (using block indices)
        foreach (var connection in _connections)
        {
            var sourceIndex = _blocks.IndexOf(connection.Source);
            var targetIndex = _blocks.IndexOf(connection.Target);

            if (sourceIndex < 0 || targetIndex < 0)
                continue;

            dto.Connections.Add(new ConnectionDto
            {
                SourceBlockIndex = sourceIndex,
                SourceSocketId = connection.SourceSocket.Id,
                TargetBlockIndex = targetIndex,
                TargetSocketId = connection.TargetSocket.Id
            });
        }

        // Serialize center block
        if (_center != null)
        {
            dto.CenterBlockIndex = _blocks.IndexOf(_center);
        }

        return dto;
    }

    /// <summary>
    /// Creates a PipelineGraph from a DTO.
    /// </summary>
    internal static PipelineGraph FromDto(PipelineGraphDto dto)
    {
        var graph = new PipelineGraph();

        // Deserialize blocks
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
                graph.Connect(sourceBlock, sourceSocket, targetBlock, targetSocket);
            }
        }

        // Restore center block
        if (dto.CenterBlockIndex.HasValue &&
            dto.CenterBlockIndex.Value >= 0 &&
            dto.CenterBlockIndex.Value < blocks.Count)
        {
            graph.Center = blocks[dto.CenterBlockIndex.Value];
        }

        return graph;
    }
    #endregion
}
