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

public class SharpenBlock
{
    #region Fields

    private readonly IReadOnlyList<Socket> _inputs = [new("Sharpen.In", "Image.In")];
    private readonly IReadOnlyList<Socket> _outputs = [new("Sharpen.Out", "Image.Out")];

    private bool _disposed;

    private string _title = "Sharpen";
    private string _content = "Sharpen image";

    private int _nodeWidth = 200;
    private int _nodeHeight = 100;

    /// <summary>
    /// Sharpen intensity.
    /// 0.0 = no sharpening, 1.0 = normal sharpening, >1.0 = stronger.
    /// Clamped to [0.0, 3.0].
    /// </summary>
    private float _amount = 1.0f;
    private bool _alwaysEncode = true;
    #endregion

    #region Ctor

    public SharpenBlock()
    {
    }

    #endregion

    #region IBlock basic

    public string Name => "Sharpen";

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
        get => $"Amount: {Amount}\nRe-encode: {AlwaysEncode}";
        
        set
        {
            if (!string.Equals(_content, value, StringComparison.Ordinal))
            {
                _content = value;
                OnPropertyChanged(nameof(Content));
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
    [Description("Sharpen intensity. 0.0 = no sharpening, 1.0 = normal sharpening, >1.0 = stronger. Range: 0.0–3.0.")]
    public float Amount
    {
        get => _amount;
        set
        {
            var clamped = Math.Clamp(value, 0.0f, 3.0f);
            if (Math.Abs(_amount - clamped) > float.Epsilon)
            {
                _amount = clamped;
                OnPropertyChanged(nameof(Amount));
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

        inputs.TryGetValue(_inputs[0], out var inItems);
        inItems ??= Array.Empty<IBasicWorkItem>();

        var resultList = new List<IBasicWorkItem>(inItems.Count);

        foreach (var item in inItems)
        {
            var sharpened = ApplySharpen(item);
            if (sharpened != null)
                resultList.Add(sharpened);
        }

        var readOnly = new ReadOnlyCollection<IBasicWorkItem>(resultList);

        return new Dictionary<Socket, IReadOnlyList<IBasicWorkItem>>
            {
                { _outputs[0], readOnly }
            };
    }

    #endregion

    #region Execute (string keyed)

    public IReadOnlyDictionary<string, IReadOnlyList<IBasicWorkItem>> Execute(
        IDictionary<string, IReadOnlyList<IBasicWorkItem>> inputs)
    {
        if (inputs is null) throw new ArgumentNullException(nameof(inputs));

        inputs.TryGetValue(_inputs[0].Id, out var inItems);
        inItems ??= Array.Empty<IBasicWorkItem>();

        var resultList = new List<IBasicWorkItem>(inItems.Count);

        foreach (var item in inItems)
        {
            var sharpened = ApplySharpen(item);
            if (sharpened != null)
                resultList.Add(sharpened);
        }

        var readOnly = new ReadOnlyCollection<IBasicWorkItem>(resultList);

        return new Dictionary<string, IReadOnlyList<IBasicWorkItem>>
            {
                { _outputs[0].Id, readOnly }
            };
    }

    #endregion

    #region Core sharpen logic

    private IBasicWorkItem? ApplySharpen(IBasicWorkItem item)
    {
        if (item is null)
            throw new ArgumentNullException(nameof(item));

        if (!_alwaysEncode)
            return item;

        // Không có ảnh → trả nguyên item
        if (!item.Metadata.TryGetValue("ImageData", out var dataObj) ||
            dataObj is not byte[] imageBytes ||
            imageBytes.Length == 0)
        {
            return item;
        }

        // Amount = 0.0 → no-op
        if (Amount <= 0.0f)
        {
            return item;
        }

        using var image = Image.Load<Rgba32>(imageBytes);

        // Sharpen theo ImageSharp
        image.Mutate(x => x.GaussianSharpen(Amount));

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
            ["SharpenAmount"] = Amount,
            ["SharpenedAtUtc"] = DateTime.UtcNow
        };

        return new SharpenBlockWorkItem(newMetadata);
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

    private sealed class SharpenBlockWorkItem : IBasicWorkItem
    {
        public Guid Id { get; } = Guid.NewGuid();

        public IDictionary<string, object> Metadata { get; }

        public SharpenBlockWorkItem(IDictionary<string, object> metadata)
        {
            Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
        }
    }

    #endregion
}
