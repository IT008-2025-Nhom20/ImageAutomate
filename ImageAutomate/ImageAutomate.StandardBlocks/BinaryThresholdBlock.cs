using ImageAutomate.Core;
using SixLabors.ImageSharp;

using SixLabors.ImageSharp.Processing;
using System.ComponentModel;
using SharpColor = SixLabors.ImageSharp.Color;
using WinColor = System.Drawing.Color;

namespace ImageAutomate.StandardBlocks;

public enum BinaryThresholdOption
{
    Luminance,
    MaxChroma,
    Saturation
}

public class BinaryThresholdBlock : IBlock
{
    #region Fields

    private readonly IReadOnlyList<Socket> _inputs = [new("BinThreshold.In", "Image.In")];
    private readonly IReadOnlyList<Socket> _outputs = [new("BinThreshold.Out", "Image.Out")];

    private bool _disposed;

    // Configuration fields
    private float _threshold = 0.5f;
    private WinColor _upperColor = WinColor.White;
    private WinColor _lowerColor = WinColor.Black;
    private BinaryThresholdOption _mode = BinaryThresholdOption.Luminance;

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
    private string _title = "Binary Threshold";


    #endregion

    public BinaryThresholdBlock()
        : this(200, 100)
    {
    }

    public BinaryThresholdBlock(int width, int height)
    {
        _width = width;
        _height = height;
    }

    #region IBlock basic

    [Browsable(false)]
    public string Name => "BinaryThreshold";

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
    public string Content => $"Threshold: {Threshold:F2}\nMode: {Mode}";

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
    [Description("The threshold value (0.0 to 1.0). Pixels with a calculated value higher than this get the UpperColor.")]
    public float Threshold
    {
        get => _threshold;
        set
        {
            var clamped = Math.Clamp(value, 0.0f, 1.0f);
            if (Math.Abs(_threshold - clamped) > float.Epsilon)
            {
                _threshold = clamped;
                OnPropertyChanged(nameof(Threshold));
                OnPropertyChanged(nameof(Content));
            }
        }
    }

    [Category("Configuration")]
    [Description("The metric used to compare against the threshold.")]
    public BinaryThresholdOption Mode
    {
        get => _mode;
        set
        {
            if (_mode != value)
            {
                _mode = value;
                OnPropertyChanged(nameof(Mode));
            }
        }
    }

    [Category("Configuration")]
    [Description("The color assigned when the value is >= Threshold.")]
    public WinColor UpperColor
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
    [Description("The color assigned when the value is < Threshold.")]
    public WinColor LowerColor
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

            var sharpUpperColor = SharpColor.FromRgba(_upperColor.R, _upperColor.G, _upperColor.B, _upperColor.A);
            var sharpLowerColor = SharpColor.FromRgba(_upperColor.R, _upperColor.G, _lowerColor.B, _lowerColor.A);
            sourceItem.Image.Mutate(x => x.BinaryThreshold(Threshold, sharpUpperColor, sharpLowerColor, MappingToBinaryThresholdMode(Mode), region));

            outputItems.Add(sourceItem);
        }

        return new Dictionary<Socket, IReadOnlyList<IBasicWorkItem>>
            {
                { _outputs[0], outputItems }
            };
    }

    public static BinaryThresholdMode MappingToBinaryThresholdMode(BinaryThresholdOption mode)
    {
        if (mode == BinaryThresholdOption.Luminance)
            return BinaryThresholdMode.Luminance;
        else if (mode == BinaryThresholdOption.Saturation)
            return BinaryThresholdMode.Saturation;
        return BinaryThresholdMode.MaxChroma; 
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
