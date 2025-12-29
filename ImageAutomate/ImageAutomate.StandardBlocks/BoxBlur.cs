using ImageAutomate.Core;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Convolution;
using System.ComponentModel;

namespace ImageAutomate.StandardBlocks;

public class BoxBlurBlock : IBlock
{
    #region Fields

    private readonly IReadOnlyList<Socket> _inputs = [new("BoxBlur.In", "Image.In")];
    private readonly IReadOnlyList<Socket> _outputs = [new("BoxBlur.Out", "Image.Out")];

    private bool _disposed;

    // Configuration fields
    private int _radius = 10; // Default radius
    private BorderWrappingMode _borderWrapModeX = BorderWrappingMode.Wrap;
    private BorderWrappingMode _borderWrapModeY = BorderWrappingMode.Wrap;

    private bool _isRelative = true;
    private float _rectX = 0.0f;
    private float _rectY = 0.0f;
    private float _rectWidth = 1.0f;
    private float _rectHeight = 1.0f;

    // Layout fields
    private double _x;
    private double _y;
    private int _width;
    private int _height;
    private string _title = "Box Blur";

    #endregion

    public BoxBlurBlock()
        : this(200, 100)
    {
    }

    public BoxBlurBlock(int width, int height)
    {
        _width = width;
        _height = height;
    }

    #region IBlock basic

    [Browsable(false)]
    public string Name => "BoxBlur";

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
    public string Content => $"Radius: {Radius}\nBorderWrapX: {BorderWrapModeX}\nBorderWrapY: {BorderWrapModeY}";

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
    [Description("The radius of the blur. Higher values create a stronger, blockier blur effect.")]
    public int Radius
    {
        get => _radius;
        set
        {
            // Radius must be at least 0.
            var clamped = Math.Max(value, 0);
            if (_radius != clamped)
            {
                _radius = clamped;
                OnPropertyChanged(nameof(Radius));
            }
        }
    }
    [Category("Configuration")]
    [Description("Controls how pixels are extrapolated at the left/right image borders.")]
    public BorderWrappingMode BorderWrapModeX
    {
        get => _borderWrapModeX;
        set
        {
            if (_borderWrapModeX != value)
            {
                _borderWrapModeX = value;
                OnPropertyChanged(nameof(BorderWrapModeX));
            }
        }
    }

    [Category("Configuration")]
    [Description("Controls how pixels are extrapolated at the top/bottom image borders.")]
    public BorderWrappingMode BorderWrapModeY
    {
        get => _borderWrapModeY;
        set
        {
            if (_borderWrapModeY != value)
            {
                _borderWrapModeY = value;
                OnPropertyChanged(nameof(BorderWrapModeY));
            }
        }
    }
    [Category("Region Configuration")]
    [Description("If true, values are percentages (0.0-1.0). If false, values are pixels.")]
    public bool IsRelative
    {
        get => _isRelative;
        set
        {
            if (_isRelative != value)
            {
                _isRelative = value;
                OnPropertyChanged(nameof(IsRelative));
            }
        }
    }

    [Category("Region Configuration")]
    [Description("X coordinate of the top-left corner.")]
    public float RectX
    {
        get => _rectX;
        set
        {
            if (Math.Abs(_rectX - value) > float.Epsilon)
            {
                _rectX = value;
                OnPropertyChanged(nameof(RectX));
            }
        }
    }

    [Category("Region Configuration")]
    [Description("Y coordinate of the top-left corner.")]
    public float RectY
    {
        get => _rectY;
        set
        {
            if (Math.Abs(_rectY - value) > float.Epsilon)
            {
                _rectY = value;
                OnPropertyChanged(nameof(RectY));
            }
        }
    }

    [Category("Region Configuration")]
    [Description("Width of the region.")]
    public float RectWidth
    {
        get => _rectWidth;
        set
        {
            // Đảm bảo chiều rộng không âm
            if (value < 0) value = 0;

            if (Math.Abs(_rectWidth - value) > float.Epsilon)
            {
                _rectWidth = value;
                OnPropertyChanged(nameof(RectWidth));
            }
        }
    }

    [Category("Region Configuration")]
    [Description("Height of the region.")]
    public float RectHeight
    {
        get => _rectHeight;
        set
        {
            // Đảm bảo chiều cao không âm
            if (value < 0) value = 0;

            if (Math.Abs(_rectHeight - value) > float.Epsilon)
            {
                _rectHeight = value;
                OnPropertyChanged(nameof(RectHeight));
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
            var img = sourceItem.Image;
            int w = img.Width;
            int h = img.Height;

            Rectangle region = GetProcessRegion(w, h);
            if (Radius > 0)
            {
                sourceItem.Image.Mutate(x => x.BoxBlur(Radius, region, BorderWrapModeX, BorderWrapModeY));
            }

            outputItems.Add(sourceItem);
        }

        return new Dictionary<Socket, IReadOnlyList<IBasicWorkItem>>
            {
                { _outputs[0], outputItems }
            };
    }
    private Rectangle GetProcessRegion(int sourceWidth, int sourceHeight)
    {
        int x, y, w, h;

        if (IsRelative)
        {
            x = (int)(RectX * sourceWidth);
            y = (int)(RectY * sourceHeight);
            w = (int)(RectWidth * sourceWidth);
            h = (int)(RectHeight * sourceHeight);
        }
        else
        {
            x = (int)RectX;
            y = (int)RectY;
            w = (int)RectWidth;
            h = (int)RectHeight;
        }

        var rect = new Rectangle(x, y, w, h);
        rect.Intersect(new Rectangle(0, 0, sourceWidth, sourceHeight));
        return rect;
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