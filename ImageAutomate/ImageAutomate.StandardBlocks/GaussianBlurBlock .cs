using ImageAutomate.Core;
using SixLabors.ImageSharp.Processing;
using System.ComponentModel;

namespace ImageAutomate.StandardBlocks;

public class GaussianBlurBlock : IBlock
{
    #region Fields

    private readonly IReadOnlyList<Socket> _inputs = [new("GaussianBlur.In", "Image.In")];
    private readonly IReadOnlyList<Socket> _outputs = [new("GaussianBlur.Out", "Image.Out")];

    private bool _disposed;

    private int _nodeWidth = 220;
    private int _nodeHeight = 110;

    private float _sigma = 1.0f;
    private int? _radius = null;

    #endregion

    #region IBlock basic

    public string Name => "GaussianBlur";

    public string Title => "Gaussian Blur";

    public string Content => $"Sigma: {Sigma}\nRadius: {Radius}";

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
    [Description("Blur intensity (sigma). Recommended range: 0.5–25.0. 0.0 = no blur.")]
    public float Sigma
    {
        get => _sigma;
        set
        {
            var clamped = Math.Clamp(value, 0.0f, 25.0f);
            if (Math.Abs(_sigma - clamped) > float.Epsilon)
            {
                _sigma = clamped;
                OnPropertyChanged(nameof(Sigma));
            }
        }
    }

    [Category("Configuration")]
    [Description("Optional override for Gaussian kernel radius. If null, ImageSharp auto-computes based on Sigma.")]
    public int? Radius
    {
        get => _radius;
        set
        {
            if (_radius != value)
            {
                if (value.HasValue && value.Value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(Radius), "Radius must be positive when set.");

                _radius = value;
                OnPropertyChanged(nameof(Radius));
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

        foreach (var sourceItem in inItems.OfType<WorkItem>())
        {
            if (Sigma > 0.0f)
                sourceItem.Image.Mutate(x => x.GaussianBlur(Sigma));
            outputItems.Add(sourceItem);
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