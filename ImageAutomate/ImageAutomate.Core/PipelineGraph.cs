/**
 * PipelineGraph.cs
 *
 * Graph datastructure for managing Block nodes and their connections.
 */

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
/// Represents a directed graph of blocks, providing methods to manage blocks, connections,
/// and the graph's center point.
/// </summary>
/// <remarks>
/// Use the PipelineGraph class to construct and manipulate a network of blocks, where each block can be
/// connected to others via sockets. The class maintains collections of blocks and connections, and
/// allows for adding, removing, and connecting blocks.<br/>
/// <br/>
/// The Center property designates a central block within the graph, which can be used as a logical starting
/// point or focus for graph operations.<br/>
/// <br/>
/// All modifications to the graph, such as adding or removing blocks and connections, are performed through the
/// provided methods to ensure consistency.<br/>
/// <br/>
/// The class is not thread-safe; external synchronization is required if accessed concurrently.
/// </remarks>
public class PipelineGraph
{
    private static System.Text.Json.JsonSerializerOptions _serializerOptions = new()
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
    /// Occurs when the graph is modified, such as when nodes or edges are added, removed, or updated.
    /// </summary>
    /// <remarks>Subscribers can use this event to respond to changes in the graph's structure or data. The
    /// event is raised after the modification is complete. This event is not thread-safe; ensure appropriate
    /// synchronization when subscribing or handling the event in multithreaded scenarios.</remarks>
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
    /// Removes the specified block from the collection and disconnects any connections associated with it.
    /// </summary>
    /// <remarks>
    /// If the specified block is the current center block, the center is reset to null. Any
    /// connections where the block is a source or target are also removed. If the block is successfully removed, the
    /// OnBlockRemoved event is invoked.
    /// </remarks>
    /// <param name="block">The block to remove. Must not be null.</param>
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

        // Remove existing connection to target socket to overwrite it with new connection
        _connections.RemoveAll(c => c.Target == target && c.TargetSocket == targetSocket);

        // Add the connection
        _connections.Add(new Connection(source, sourceSocket, target, targetSocket));

        GraphChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Connects two blocks via Socket IDs
    /// </summary>
    public void Connect(IBlock source, string sourceSocketId, IBlock target, string targetSocketId)
    {
        var srcSocket = source.Outputs.FirstOrDefault(s => s.Id == sourceSocketId)
            ?? throw new ArgumentException($"Socket ID '{sourceSocketId}' not found on source '{source.Title}'");
        var tgtSocket = target.Inputs.FirstOrDefault(s => s.Id == targetSocketId)
            ?? throw new ArgumentException($"Socket ID '{targetSocketId}' not found on target '{target.Title}'");

        Connect(source, srcSocket, target, tgtSocket);
        // The event is raised in the other overload
    }

    /// <summary>
    /// Removes the specified connection from the connections list.
    /// </summary>
    /// <param name="connection">The connection to be disconnected. Cannot be null.</param>
    public void Disconnect(Connection connection)
    {
        if (_connections.Remove(connection))
            GraphChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Removes all connections and blocks from the graph and resets the center point to null.
    /// </summary>
    /// <remarks>
    /// After calling this method, the collection will be empty and the center point will be unset.
    /// Use this method to reset the state before repopulating the collection.
    /// </remarks>
    public void Clear()
    {
        _connections.Clear();
        _blocks.Clear();
        Center = null;
        GraphChanged?.Invoke(this, EventArgs.Empty);
    }
    #endregion

    #region Serialization
    public string ToJson()
    {
        throw new NotImplementedException("PipelineGraph serialization is not implemented yet.");
        //return System.Text.Json.JsonSerializer.Serialize(this, _serializerOptions);
    }
    public static PipelineGraph FromJson(string json)
    {
        throw new NotImplementedException("PipelineGraph deserialization is not implemented yet.");
        //return System.Text.Json.JsonSerializer.Deserialize<PipelineGraph>(json, _serializerOptions)
        //    ?? throw new InvalidOperationException("Failed to deserialize PipelineGraph from JSON.");
    }
    #endregion
}