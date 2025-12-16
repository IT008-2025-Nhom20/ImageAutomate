using ImageAutomate.Core;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ImageAutomate.StandardBlocks;

internal class ContrastBlock : IBlock
{
    #region Fields

    private readonly Socket _inputSocket = new("Contrast.In", "Image.In");
    private readonly Socket _outputSocket = new("Contrast.Out", "Image.Out");
    private readonly IReadOnlyList<Socket> _inputs;
    private readonly IReadOnlyList<Socket> _outputs;

    private bool _disposed;

    private string _title = "Contrast";
    private string _content = "Adjust image contrast";

    private int _nodeWidth = 200;
    private int _nodeHeight = 100;

    // Configuration: Contrast factor in [0.0, 3.0], default 1.0 = no change
    private float _contrast = 1.0f;
    private bool _alwaysEncoder = true;

    #endregion

    #region Ctor

    public ContrastBlock()
    {
        _inputs = new[] { _inputSocket };
        _outputs = new[] { _outputSocket };
    }

    #endregion

    #region IBlock basic

    public string Name => "Contrast";

    public string Title
    {
        get => _title;
    }

    public string Content
    {
        get => $"Contrast: {Contrast}\nRe-encode: {AlwaysEncode}";
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
    [Description("Contrast factor (0.0–3.0). 1.0 = no change, <1.0 = lower contrast, >1.0 = higher contrast.")]
    public float Contrast
    {
        get => _contrast;
        set
        {
            var clamped = Math.Clamp(value, 0.0f, 3.0f);
            if (Math.Abs(_contrast - clamped) > float.Epsilon)
            {
                _contrast = clamped;
                OnPropertyChanged(nameof(Contrast));
            }
        }
    }

    [Category("Configuration")]
    [Description("Force re-encoding even when format matches")]
    public bool AlwaysEncode
    {
        get => _alwaysEncoder;
        set
        {
            if (_alwaysEncoder != value)
            {
                _alwaysEncoder = value;
                OnPropertyChanged(nameof(AlwaysEncode));
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
            var adjusted = ApplyContrast(item);
            if (adjusted != null)
                resultList.Add(adjusted);
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
            var adjusted = ApplyContrast(item);
            if (adjusted != null)
                resultList.Add(adjusted);
        }

        var readOnly = new ReadOnlyCollection<IBasicWorkItem>(resultList);

        return new Dictionary<string, IReadOnlyList<IBasicWorkItem>>
            {
                { _outputSocket.Id, readOnly }
            };
    }

    #endregion

    #region Core contrast logic

    private IBasicWorkItem? ApplyContrast(IBasicWorkItem item)
    {
        if (item is null)
            throw new ArgumentNullException(nameof(item));

        if (!_alwaysEncoder)
            return item;

        // Lấy image bytes từ metadata
        if (!item.Metadata.TryGetValue("ImageData", out var dataObj) ||
            dataObj is not byte[] imageBytes ||
            imageBytes.Length == 0)
        {
            // Không có ảnh → trả lại item cũ
            return item;
        }

        using var image = Image.Load<Rgba32>(imageBytes);

        // Áp dụng contrast theo ImageSharp
        // Contrast = 1.0 => không thay đổi
        image.Mutate(x => x.Contrast(Contrast));

        // Giữ nguyên format decode nếu có, fallback PNG
        var decodedFormat = image.Metadata.DecodedImageFormat
                            ?? SixLabors.ImageSharp.Formats.Png.PngFormat.Instance;

        using var ms = new MemoryStream();
        image.Save(ms, decodedFormat);
        var outBytes = ms.ToArray();

        // Clone metadata + cập nhật kích thước & timestamp
        var newMetadata = new Dictionary<string, object>(item.Metadata)
        {
            ["ImageData"] = outBytes,
            ["Width"] = image.Width,
            ["Height"] = image.Height,
            ["ContrastAppliedAtUtc"] = DateTime.UtcNow,
            ["Contrast"] = Contrast
        };

        return new ContrastBlockWorkItem(newMetadata);
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

    private sealed class ContrastBlockWorkItem : IBasicWorkItem
    {
        public Guid Id { get; } = Guid.NewGuid();

        public IDictionary<string, object> Metadata { get; }

        public ContrastBlockWorkItem(IDictionary<string, object> metadata)
        {
            Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
        }
    }

    #endregion
}
