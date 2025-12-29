using System.ComponentModel;

namespace ImageAutomate.Core;

/// <summary>
/// Represents a named socket with a unique identifier.
/// </summary>
/// <param name="Id">The unique identifier for the socket. Cannot be null.</param>
/// <param name="Name">The display name of the socket. Cannot be null.</param>
public record Socket(string Id, string Name);

/// <summary>
/// Represents a configurable block in the workflow graph.
/// </summary>
/// <remarks>The IBlock interface provides properties for naming, titling, and content management, as well as
/// layout dimensions and socket-based input/output connections. Implementations should notify property changes via the
/// INotifyPropertyChanged interface and release resources appropriately by implementing IDisposable. Thread safety and
/// property change semantics depend on the specific implementation.
/// </remarks>
public interface IBlock : INotifyPropertyChanged, IDisposable
{
    #region Basic Properties
    /// <summary>
    /// The block name.
    /// </summary>
    public string Name { get; }
    /// <summary>
    /// Gets or sets the display header of this block.
    /// </summary>
    public string Title { get; set; }
    /// <summary>
    /// Gets the display content of this block.
    /// </summary>
    public string Content { get; }
    #endregion

    #region Layout Properties
    /// <summary>
    /// Gets or sets the X position of the block in the graph.
    /// </summary>
    public double X { get; set; }

    /// <summary>
    /// Gets or sets the Y position of the block in the graph.
    /// </summary>
    public double Y { get; set; }

    /// <summary>
    /// Gets or sets the width of the block.
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// Gets or sets the height of the block.
    /// </summary>
    public int Height { get; set; }
    #endregion

    #region Sockets and Execution Contracts
    /// <summary>
    /// Gets the collection of input sockets for this block.
    /// </summary>
    public IReadOnlyList<Socket> Inputs { get; }
    /// <summary>
    /// Gets the collection of output sockets for this block.
    /// </summary>
    public IReadOnlyList<Socket> Outputs { get; }
    /// <summary>
    /// Executes the block operation on the given inputs.
    /// </summary>
    /// <param name="inputs">Map of sockets to input work items.</param>
    /// <returns>Map of sockets to output work items.</returns>
    public IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> Execute(IDictionary<Socket, IReadOnlyList<IBasicWorkItem>> inputs);

    /// <summary>
    /// Executes the block operation on the given inputs with cancellation support.
    /// </summary>
    /// <param name="inputs">Map of sockets to input work items.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Map of sockets to output work items.</returns>
    public IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> Execute(IDictionary<Socket, IReadOnlyList<IBasicWorkItem>> inputs, CancellationToken cancellationToken);

    /// <summary>
    /// Executes the block operation on the given inputs (by socket ID).
    /// </summary>
    /// <param name="inputs">Map of socket IDs to input work items.</param>
    /// <returns>Map of socket to output work items.</returns>
    public IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> Execute(IDictionary<string, IReadOnlyList<IBasicWorkItem>> inputs);

    /// <summary>
    /// Executes the block operation on the given inputs (by socket ID) with cancellation support.
    /// </summary>
    /// <param name="inputs">Map of socket IDs to input work items.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Map of socket to output work items.</returns>
    public IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> Execute(IDictionary<string, IReadOnlyList<IBasicWorkItem>> inputs, CancellationToken cancellationToken);
    #endregion
}
