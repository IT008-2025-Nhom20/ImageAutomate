using ImageAutomate.Core;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ImageAutomate.StandardBlocks;

public class VignetteBlock
{
    #region Fields

    private readonly Socket _inputSocket = new("Vignette.In", "Image.In");
    private readonly Socket _outputSocket = new("Vignette.Out", "Image.Out");
    private readonly IReadOnlyList<Socket> _inputs;
    private readonly IReadOnlyList<Socket> _outputs;

    private int _width = 200;
    private int _height = 100;

    private string _title = "Vignette";
    private string _content = "Apply vignette effect";

    // Config
    private Color _color = Color.Black;
    private float _strength = 0.6f; // 0–1

    private bool _disposed;
    private bool _alwaysEncode = true;
    #endregion

    #region ctor

    public VignetteBlock()
    {
        _inputs = new[] { _inputSocket };
        _outputs = new[] { _outputSocket };
    }

    #endregion

    #region INotifyPropertyChanged

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    #endregion

    #region Basic Properties

    public string Name => "Vignette";

    public string Title
    {
        get => _title;
    }

    public string Content
    {
        get => $"Color: {Color}\nStrength: {Strength}\nRe-encode: {AlwaysEncode}";
    }

    [Category("Layout")]
    [Description("Width of the block node")]
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
    [Description("Height of the block node")]
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

    #region Configuration

    [Category("Configuration")]
    [Description("Vignette color used at the edges. Default is black.")]
    public Color Color
    {
        get => _color;
        set
        {
            if (!_color.Equals(value))
            {
                _color = value;
                OnPropertyChanged(nameof(Color));
            }
        }
    }

    [Category("Configuration")]
    [Description("Vignette strength (0.0–1.0). 0 = no effect, 1 = full effect.")]
    public float Strength
    {
        get => _strength;
        set
        {
            var clamped = Math.Clamp(value, 0f, 1f);
            if (Math.Abs(_strength - clamped) > float.Epsilon)
            {
                _strength = clamped;
                OnPropertyChanged(nameof(Strength));
            }
        }
    }

    [Category("Configuration")]
    [Description("Force re-encoding even when format matches")]
    public bool AlwaysEncode
    {
        get => _alwaysEncode;
        set
        {
            if (_alwaysEncode != value)
            {
                _alwaysEncode = value;
                OnPropertyChanged(nameof(AlwaysEncode));
            }
        }
    }
    #endregion

    #region Sockets

    public IReadOnlyList<Socket> Inputs => _inputs;
    public IReadOnlyList<Socket> Outputs => _outputs;

    #endregion

    #region Execute

    public IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> Execute(
        IDictionary<Socket, IReadOnlyList<IBasicWorkItem>> inputs)
    {
        if (inputs is null) throw new ArgumentNullException(nameof(inputs));

        inputs.TryGetValue(_inputSocket, out var inItems);
        inItems ??= Array.Empty<IBasicWorkItem>();

        var result = new List<IBasicWorkItem>(inItems.Count);

        foreach (var item in inItems)
        {
            var processed = ApplyVignette(item);
            result.Add(processed);
        }

        var readOnly = new ReadOnlyCollection<IBasicWorkItem>(result);

        return new Dictionary<Socket, IReadOnlyList<IBasicWorkItem>>
            {
                { _outputSocket, readOnly }
            };
    }

    public IReadOnlyDictionary<string, IReadOnlyList<IBasicWorkItem>> Execute(
        IDictionary<string, IReadOnlyList<IBasicWorkItem>> inputs)
    {
        if (inputs is null) throw new ArgumentNullException(nameof(inputs));

        inputs.TryGetValue(_inputSocket.Id, out var inItems);
        inItems ??= Array.Empty<IBasicWorkItem>();

        var result = new List<IBasicWorkItem>(inItems.Count);

        foreach (var item in inItems)
        {
            var processed = ApplyVignette(item);
            result.Add(processed);
        }

        var readOnly = new ReadOnlyCollection<IBasicWorkItem>(result);

        return new Dictionary<string, IReadOnlyList<IBasicWorkItem>>
            {
                { _outputSocket.Id, readOnly }
            };
    }

    /// <summary>
    /// Áp vignette lên 1 work item, clone metadata và tạo work item mới.
    /// </summary>
    private IBasicWorkItem ApplyVignette(IBasicWorkItem item)
    {
        if (item is null)
            throw new ArgumentNullException(nameof(item));

        if (!_alwaysEncode)
            return item;

        // Strength = 0 -> no-op, trả về nguyên item
        if (_strength <= 0f)
            return item;

        if (!item.Metadata.TryGetValue("ImageData", out var dataObj) ||
            dataObj is not byte[] bytes ||
            bytes.Length == 0)
        {
            // Không có dữ liệu ảnh -> không làm gì
            return item;
        }

        using var image = Image.Load<Rgba32>(bytes);
        var format = image.Metadata.DecodedImageFormat;
        if (format == null)
        {
            throw new InvalidOperationException($"VignetteBlock: Unsupported or unknown image format.");
        }
        // Map Strength -> GraphicsOptions.BlendPercentage  (0..1)
        var options = new GraphicsOptions
        {
            BlendPercentage = _strength
        };

        // Nếu Strength >= 1 gần như bằng 1 -> không cần options, dùng overload đơn giản cũng được,
        // nhưng để linear, ta luôn dùng GraphicsOptions.
        image.Mutate(ctx =>
        {
            // Nếu Color là black (default) nhưng bạn vẫn muốn dùng explicit color thì giữ như dưới
            ctx.Vignette(options, _color);
        });

        using var ms = new MemoryStream();
        image.Save(ms, format);
        var outBytes = ms.ToArray();

        // Clone metadata để preserve các key khác
        var newMetadata = new Dictionary<string, object>(item.Metadata)
        {
            ["ImageData"] = outBytes,
            ["Width"] = image.Width,
            ["Height"] = image.Height,
            ["VignetteColor"] = _color,
            ["VignetteStrength"] = _strength,
            ["LastOperation"] = "Vignette"
        };

        return new VignetteWorkItem(item.Id, newMetadata);
    }

    #endregion

    #region IDisposable

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            // Hiện tại block không giữ unmanaged resources nên không cần làm gì thêm
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion

    #region Nested WorkItem

    /// <summary>
    /// Work item đơn giản cho Vignette, giữ nguyên Id gốc, cập nhật Metadata.
    /// </summary>
    private sealed class VignetteWorkItem : IBasicWorkItem
    {
        public Guid Id { get; }

        public IDictionary<string, object> Metadata { get; }

        public VignetteWorkItem(Guid id, IDictionary<string, object> metadata)
        {
            Id = id;
            Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
        }
    }

    #endregion
}

