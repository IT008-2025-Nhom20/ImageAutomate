using System.ComponentModel;

namespace ImageAutomate.Core;

/// <summary>
/// Represents a named socket with a unique identifier.
/// </summary>
/// <param name="Id">The unique identifier for the socket. Cannot be null.</param>
/// <param name="Name">The display name of the socket. Cannot be null.</param>
public record Socket(string Id, string Name);

/// <summary>
/// Represents a configurable block that exposes input and output sockets, display header,
/// display content, and layout properties for use in the ImageAutomate workflow graph.
/// </summary>
/// <remarks>The IBlock interface provides properties for naming, titling, and content management, as well as
/// layout dimensions and socket-based input/output connections. Implementations should notify property changes via the
/// INotifyPropertyChanged interface and release resources appropriately by implementing IDisposable. Thread safety and
/// property change semantics depend on the specific implementation.
/// </remarks>
public interface IBlock: INotifyPropertyChanged, IDisposable
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
    /// Gets or sets the display content of this block.
    /// </summary>
    public string Content { get; set; }
    #endregion

    #region Layout Properties
    /// <summary>
    /// Gets or sets the width of this block.
    /// </summary>
    public int Width { get; set; }
    /// <summary>
    /// Gets or sets the height of this block.
    /// </summary>
    public int Height { get; set; }
    #endregion

    #region Sockets and Execution Contracts
    /// <summary>
    /// Gets the collection of input sockets for this block.
    /// </summary>
    /// <remarks>The returned list is read-only and reflects the input sockets available for processing. The
    /// order of sockets in the collection may be significant depending on the execution contract.
    /// </remarks>
    public IReadOnlyList<Socket> Inputs { get; }
    /// <summary>
    /// Gets the collection of output sockets for this block.
    /// </summary>
    /// <remarks>The returned list is read-only and reflects the output sockets available for processing. The
    /// order of sockets in the collection may be significant depending on the execution contract.
    /// </remarks>
    public IReadOnlyList<Socket> Outputs { get; }
    /// <summary>
    /// Executes a processing operation on the provided collection of sockets and associated work items.
    /// </summary>
    /// <param name="inputs">A dictionary mapping each <see cref="Socket"/> to a read-only list of <see cref="IBasicWorkItem"/>
    /// instances to be processed. Cannot be null.</param>
    /// <returns>A read-only dictionary containing the results of the operation at each output socket. Each entry maps a socket
    /// to a read-only list of processed work items.</returns>
    public IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> Execute(IDictionary<Socket, IReadOnlyList<IBasicWorkItem>> inputs);
    /// <summary>
    /// Executes a processing operation on the provided collection of sockets and associated work items.
    /// </summary>
    /// <param name="inputs">A dictionary mapping each <see cref="Socket"/> by socket Id to a
    /// read-only list of <see cref="IBasicWorkItem"/> instances to be processed by socket Id. Cannot be null.</param>
    /// <returns>A read-only dictionary containing the results of the operation at each output socket. Each entry maps a socket
    /// to a read-only list of processed work items.</returns>
    public IReadOnlyDictionary<string, IReadOnlyList<IBasicWorkItem>> Execute(IDictionary<string, IReadOnlyList<IBasicWorkItem>> inputs);
    #endregion
}