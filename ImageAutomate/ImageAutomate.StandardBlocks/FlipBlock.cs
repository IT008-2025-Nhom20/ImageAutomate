using ImageAutomate.Core;
using SixLabors.ImageSharp.Processing;
using System.ComponentModel;

namespace ImageAutomate.StandardBlocks;

public enum FlipModeOption
{
    Horizontal,
    Vertical
}
public class FlipBlock : IBlock
{
    #region Fields

    private readonly IReadOnlyList<Socket> _inputs = [new("Flip.In", "Image.In")];
    private readonly IReadOnlyList<Socket> _outputs = [new("Flip.Out", "Image.Out")];

    private bool _disposed;

    private int _nodeWidth = 200;
    private int _nodeHeight = 100;

    private FlipModeOption _flipMode = FlipModeOption.Horizontal;
    #endregion

    #region IBlock basic

    public string Name => "Flip";

    public string Title => "Flip";

    public string Content => $"Flip direction: {FlipMode}";

    #endregion

    #region Layout

    [Category("Layout")]
    [Description("Width of the block node")]
    public int Width
    {
        get => _nodeWidth;
        set
        {
            if (_nodeWidth != value)
            {
                _nodeWidth = value;
                OnPropertyChanged(nameof(Width));
            }
        }
    }

    [Category("Layout")]
    [Description("Height of the block node")]
    public int Height
    {
        get => _nodeHeight;
        set
        {
            if (_nodeHeight != value)
            {
                _nodeHeight = value;
                OnPropertyChanged(nameof(Height));
            }
        }
    }

    #endregion

    #region Sockets

    public IReadOnlyList<Socket> Inputs => _inputs;
    public IReadOnlyList<Socket> Outputs => _outputs;

    #endregion

    #region Configuration

    [Category("Configuration")]
    [Description("Flip direction: Horizontal or Vertical.")]
    public FlipModeOption FlipMode
    {
        get => _flipMode;
        set
        {
            if (_flipMode != value)
            {
                _flipMode = value;
                OnPropertyChanged(nameof(FlipMode));
            }
        }
    }

    #endregion

    #region INotifyPropertyChanged

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    #endregion

    #region Execute

    public IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> Execute(
        IDictionary<Socket, IReadOnlyList<IBasicWorkItem>> inputs)
    {
        return Execute(inputs.ToDictionary(kvp => kvp.Key.Id, kvp => kvp.Value));
    }

    public IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> Execute(
        IDictionary<string, IReadOnlyList<IBasicWorkItem>> inputs)
    {
        if (!inputs.TryGetValue(_inputs[0].Id, out var inItems))
            throw new ArgumentException($"Input items not found for the expected input socket {_inputs[0].Id}.", nameof(inputs));

        var outputItems = new List<IBasicWorkItem>();

        foreach (var item in inItems)
        {
            if (item is WorkItem sourceItem)
            {
                var clonedImage = sourceItem.Image.Clone(x => x.Flip(
                    _flipMode == FlipModeOption.Horizontal
                    ? SixLabors.ImageSharp.Processing.FlipMode.Horizontal
                    : SixLabors.ImageSharp.Processing.FlipMode.Vertical));

                var newItem = new WorkItem(clonedImage);
                
                // Deep-copy metadata
                foreach (var kvp in sourceItem.Metadata)
                {
                    newItem.Metadata[kvp.Key] = kvp.Value;
                }
                
                outputItems.Add(newItem);
            }
        }

        return new Dictionary<Socket, IReadOnlyList<IBasicWorkItem>>
            {
                { _outputs[0], outputItems }
            };
    }

    #endregion

    #region IDisposable

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion
}
