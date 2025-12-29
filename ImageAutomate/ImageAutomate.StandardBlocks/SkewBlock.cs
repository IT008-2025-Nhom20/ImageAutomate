using ImageAutomate.Core;
using SixLabors.ImageSharp.Processing;
using System.ComponentModel;

namespace ImageAutomate.StandardBlocks;

public class SkewBlock : IBlock
{
    #region Fields

    private readonly IReadOnlyList<Socket> _inputs = [new("Skew.In", "Image.In")];
    private readonly IReadOnlyList<Socket> _outputs = [new("Skew.Out", "Image.Out")];

    private bool _disposed;

    // Configuration fields
    private float _degreesX = 0.0f;
    private float _degreesY = 0.0f;

    // Layout fields
    private double _x;
    private double _y;
    private int _width;
    private int _height;
    private string _title = "Skew";

    #endregion

    public SkewBlock()
        : this(200, 100)
    {
    }

    public SkewBlock(int width, int height)
    {
        _width = width;
        _height = height;
    }

    #region IBlock basic

    [Browsable(false)]
    public string Name => "Skew";

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
    public string Content => $"X: {DegreesX:F1}, Y: {DegreesY:F1}";

    #endregion

    #region Layout Properties

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
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
    [Description("Skew angle along the X-axis in degrees.")]
    public float DegreesX
    {
        get => _degreesX;
        set
        {
            if (Math.Abs(_degreesX - value) > float.Epsilon)
            {
                _degreesX = value;
                OnPropertyChanged(nameof(DegreesX));
            }
        }
    }

    [Category("Configuration")]
    [Description("Skew angle along the Y-axis in degrees.")]
    public float DegreesY
    {
        get => _degreesY;
        set
        {
            if (Math.Abs(_degreesY - value) > float.Epsilon)
            {
                _degreesY = value;
                OnPropertyChanged(nameof(DegreesY));
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

            if (Math.Abs(DegreesX) >= 0.01f || Math.Abs(DegreesY) >= 0.01f)
            {
                sourceItem.Image.Mutate(x => x.Skew(DegreesX, DegreesY));
            }

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
