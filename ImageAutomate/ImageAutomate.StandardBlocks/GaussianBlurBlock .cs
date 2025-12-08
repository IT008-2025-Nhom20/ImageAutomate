using ImageAutomate.Core;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageAutomate.StandardBlocks;

public class GaussianBlurBlock : IBlock
{
    #region Fields

    private readonly Socket _inputSocket = new("GaussianBlur.In", "Image.In");
    private readonly Socket _outputSocket = new("GaussianBlur.Out", "Image.Out");
    private readonly IReadOnlyList<Socket> _inputs;
    private readonly IReadOnlyList<Socket> _outputs;

    private bool _disposed;

    private string _title = "Gaussian Blur";
    private string _content = "Apply Gaussian blur";

    private int _nodeWidth = 220;
    private int _nodeHeight = 110;

    // Configuration
    private float _sigma = 1.0f;      // cường độ blur
    private int? _radius = null;      // bán kính kernel (optional)

    private bool _alwaysEncode = true;
    #endregion

    #region Ctor

    public GaussianBlurBlock()
    {
        _inputs = new[] { _inputSocket };
        _outputs = new[] { _outputSocket };
    }

    #endregion

    #region IBlock basic

    public string Name => "GaussianBlur";

    public string Title
    {
        get => _title;
        set
        {
            if (!string.Equals(_title, value, StringComparison.Ordinal))
            {
                _title = value;
                OnPropertyChanged(nameof(Title));
            }
        }
    }

    public string Content
    {
        get => _content;
        set
        {
            if (!string.Equals(_content, value, StringComparison.Ordinal))
            {
                _content = value;
                OnPropertyChanged(nameof(Content));
            }
        }
    }

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
            // FR: 0.5–25.0 recommended, nhưng cho phép 0.0 = no-op
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

    #region Execute (Socket keyed)

    public IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> Execute(
        IDictionary<Socket, IReadOnlyList<IBasicWorkItem>> inputs)
    {
        if (inputs is null) throw new ArgumentNullException(nameof(inputs));

        inputs.TryGetValue(_inputSocket, out var inItems);
        inItems ??= Array.Empty<IBasicWorkItem>();

        var resultList = new List<IBasicWorkItem>(inItems.Count);

        foreach (var item in inItems)
        {
            var blurred = ApplyGaussianBlur(item);
            if (blurred != null)
                resultList.Add(blurred);
        }

        var readOnly = new ReadOnlyCollection<IBasicWorkItem>(resultList);

        return new Dictionary<Socket, IReadOnlyList<IBasicWorkItem>>
            {
                { _outputSocket, readOnly }
            };
    }

    #endregion

    #region Execute (string keyed)

    public IReadOnlyDictionary<string, IReadOnlyList<IBasicWorkItem>> Execute(
        IDictionary<string, IReadOnlyList<IBasicWorkItem>> inputs)
    {
        if (inputs is null) throw new ArgumentNullException(nameof(inputs));

        inputs.TryGetValue(_inputSocket.Id, out var inItems);
        inItems ??= Array.Empty<IBasicWorkItem>();

        var resultList = new List<IBasicWorkItem>(inItems.Count);

        foreach (var item in inItems)
        {
            var blurred = ApplyGaussianBlur(item);
            if (blurred != null)
                resultList.Add(blurred);
        }

        var readOnly = new ReadOnlyCollection<IBasicWorkItem>(resultList);

        return new Dictionary<string, IReadOnlyList<IBasicWorkItem>>
            {
                { _outputSocket.Id, readOnly }
            };
    }

    #endregion

    #region Core blur logic

    private IBasicWorkItem? ApplyGaussianBlur(IBasicWorkItem item)
    {
        if (item is null)
            throw new ArgumentNullException(nameof(item));

        if (!_alwaysEncode)
            return item;

        // Không có image data → trả nguyên item
        if (!item.Metadata.TryGetValue("ImageData", out var dataObj) ||
            dataObj is not byte[] imageBytes ||
            imageBytes.Length == 0)
        {
            return item;
        }

        // Nếu sigma = 0.0f → no-op
        if (Sigma <= 0.0f)
        {
            return item;
        }

        using var image = Image.Load<Rgba32>(imageBytes);

        // Phiên bản ImageSharp hiện tại chỉ hỗ trợ GaussianBlur(sigma)
        image.Mutate(x => x.GaussianBlur(Sigma));

        var decodedFormat = image.Metadata.DecodedImageFormat
                            ?? SixLabors.ImageSharp.Formats.Png.PngFormat.Instance;

        using var ms = new MemoryStream();
        image.Save(ms, decodedFormat);
        var outBytes = ms.ToArray();

        var newMetadata = new Dictionary<string, object>(item.Metadata)
        {
            ["ImageData"] = outBytes,
            ["Width"] = image.Width,
            ["Height"] = image.Height,
            ["GaussianBlurSigma"] = Sigma,
            ["GaussianBlurRadius"] = Radius,      // chỉ lưu metadata, không ảnh hưởng xử lý
            ["BlurredAtUtc"] = DateTime.UtcNow
        };

        return new GaussianBlurBlockWorkItem(newMetadata);
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

    #region Nested WorkItem

    private sealed class GaussianBlurBlockWorkItem : IBasicWorkItem
    {
        public Guid Id { get; } = Guid.NewGuid();

        public IDictionary<string, object> Metadata { get; }

        public GaussianBlurBlockWorkItem(IDictionary<string, object> metadata)
        {
            Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
        }
    }

    #endregion
}

    