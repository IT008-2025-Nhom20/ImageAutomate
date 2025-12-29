using ImageAutomate.Core;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.ComponentModel;
using System.Numerics;

namespace ImageAutomate.StandardBlocks;

public class AdaptiveThresholdBlock : IBlock
{
    #region Fields

    private readonly IReadOnlyList<Socket> _inputs = [new("AdaptThreshold.In", "Image.In")];
    private readonly IReadOnlyList<Socket> _outputs = [new("AdaptThreshold.Out", "Image.Out")];

    private bool _disposed;

    // Configuration fields
    private float _thresholdLimit = 0.5f;
    private Color _upperColor = Color.White;
    private Color _lowerColor = Color.Black;

    // Layout fields
    private double _x;
    private double _y;
    private int _width;
    private int _height;
    private string _title = "Adaptive Threshold";

    #endregion

    public AdaptiveThresholdBlock()
        : this(220, 130)
    {
    }

    public AdaptiveThresholdBlock(int width, int height)
    {
        _width = width;
        _height = height;
    }

    #region IBlock basic

    [Browsable(false)]
    public string Name => "AdaptiveThreshold";

    [Category("Title")]
    public string Title
    {
        get => _title;
        set
        {
            if (_title != value)
            {
                _title = value;
                OnPropertyChanged(nameof(Title));
            }
        }
    }

    [Browsable(false)]
    public string Content => $"Limit: {ThresholdLimit:F2}";

    #endregion

    #region Layout Properties

    [Category("Layout")]
    public double X
    {
        get => _x;
        set
        {
            if (Math.Abs(_x - value) > double.Epsilon)
            {
                _x = value;
                OnPropertyChanged(nameof(X));
            }
        }
    }

    [Category("Layout")]
    public double Y
    {
        get => _y;
        set
        {
            if (Math.Abs(_y - value) > double.Epsilon)
            {
                _y = value;
                OnPropertyChanged(nameof(Y));
            }
        }
    }

    [Category("Layout")]
    public int Width
    {
        get => _width;
        set
        {
            if (_width != value)
            {
                _width = value;
                OnPropertyChanged(nameof(Width));
            }
        }
    }

    [Category("Layout")]
    public int Height
    {
        get => _height;
        set
        {
            if (_height != value)
            {
                _height = value;
                OnPropertyChanged(nameof(Height));
            }
        }
    }

    #endregion

    #region Sockets

    [Browsable(false)]
    public IReadOnlyList<Socket> Inputs => _inputs;
    [Browsable(false)]
    public IReadOnlyList<Socket> Outputs => _outputs;

    #endregion

    #region Configuration

    [Category("Configuration")]
    [Description("The threshold limit (0.0 to 1.0). Determines the sensitivity of the local thresholding calculation.")]
    public float ThresholdLimit
    {
        get => _thresholdLimit;
        set
        {
            var clamped = Math.Clamp(value, 0.0f, 1.0f);
            if (Math.Abs(_thresholdLimit - clamped) > float.Epsilon)
            {
                _thresholdLimit = clamped;
                OnPropertyChanged(nameof(ThresholdLimit));
            }
        }
    }

    [Category("Configuration")]
    [Description("The color assigned to pixels above the local threshold.")]
    public Color UpperColor
    {
        get => _upperColor;
        set
        {
            if (_upperColor != value)
            {
                _upperColor = value;
                OnPropertyChanged(nameof(UpperColor));
            }
        }
    }

    [Category("Configuration")]
    [Description("The color assigned to pixels below the local threshold.")]
    public Color LowerColor
    {
        get => _lowerColor;
        set
        {
            if (_lowerColor != value)
            {
                _lowerColor = value;
                OnPropertyChanged(nameof(LowerColor));
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
        return Execute(inputs, CancellationToken.None);
    }

    public IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> Execute(
        IDictionary<Socket, IReadOnlyList<IBasicWorkItem>> inputs, CancellationToken cancellationToken)
    {
        return Execute(inputs.ToDictionary(kvp => kvp.Key.Id, kvp => kvp.Value), cancellationToken);
    }

    public IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> Execute(
        IDictionary<string, IReadOnlyList<IBasicWorkItem>> inputs)
    {
        return Execute(inputs, CancellationToken.None);
    }

    public IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> Execute(
        IDictionary<string, IReadOnlyList<IBasicWorkItem>> inputs, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(inputs, nameof(inputs));
        if (!inputs.TryGetValue(_inputs[0].Id, out var inItems))
            throw new ArgumentException($"Input items not found for the expected input socket {_inputs[0].Id}.", nameof(inputs));

        var outputItems = new List<IBasicWorkItem>();

        foreach (var sourceItem in inItems.OfType<WorkItem>())
        {
            cancellationToken.ThrowIfCancellationRequested();

            sourceItem.Image.Mutate(x => x.AdaptiveThreshold(UpperColor, LowerColor, ThresholdLimit));

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