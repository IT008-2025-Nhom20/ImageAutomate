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

public class SaturationBlock
{
    #region Fields

    private readonly IReadOnlyList<Socket> _inputs = [new("Saturation.In", "Image.In")];
    private readonly IReadOnlyList<Socket> _outputs = [new("Saturation.Out", "Image.Out")];

    private bool _disposed;

    private string _title = "Saturation";
    private string _content = "Adjust saturation";

    private int _nodeWidth = 200;
    private int _nodeHeight = 100;

    /// <summary>
    /// Saturation factor in [0.0, 3.0].
    /// 1.0 = no change, <1.0 = desaturate, >1.0 = more saturated.
    /// </summary>
    private float _saturation = 1.0f;
    private bool _alwaysEncode = true;
    #endregion

    #region Ctor

    public SaturationBlock()
    {
    }

    #endregion

    #region IBlock basic

    public string Name => "Saturation";

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
        get =>$"Sarutation: {Saturation}\nRe-encode: {AlwaysEncode}";
        
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
    [Description("Saturation factor (0.0–3.0). 1.0 = no change, <1.0 = desaturate, >1.0 = more saturated.")]
    public float Saturation
    {
        get => _saturation;
        set
        {
            var clamped = Math.Clamp(value, 0.0f, 3.0f);
            if (Math.Abs(_saturation - clamped) > float.Epsilon)
            {
                _saturation = clamped;
                OnPropertyChanged(nameof(Saturation));
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
            var adjusted = ApplySaturation(item);
            if (adjusted != null)
                resultList.Add(adjusted);
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
            var adjusted = ApplySaturation(item);
            if (adjusted != null)
                resultList.Add(adjusted);
        }

        var readOnly = new ReadOnlyCollection<IBasicWorkItem>(resultList);

        return new Dictionary<string, IReadOnlyList<IBasicWorkItem>>
            {
                { _outputs[0].Id, readOnly }
            };
    }

    #endregion

    #region Core saturation logic

    private IBasicWorkItem? ApplySaturation(IBasicWorkItem item)
    {
        if (item is null)
            throw new ArgumentNullException(nameof(item));

        if (!_alwaysEncode)
        {
            return item;
        }    

        // Không có ảnh → trả nguyên item
        if (!item.Metadata.TryGetValue("ImageData", out var dataObj) ||
            dataObj is not byte[] imageBytes ||
            imageBytes.Length == 0)
        {
            return item;
        }

        // Saturation = 1.0 → no-op (có thể tối ưu, không re-encode)
        if (Math.Abs(Saturation - 1.0f) < float.Epsilon)
        {
            return item;
        }

        using var image = Image.Load<Rgba32>(imageBytes);

        // Áp dụng saturation theo ImageSharp
        image.Mutate(x => x.Saturate(Saturation));

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
            ["SaturationFactor"] = Saturation,
            ["SaturationAdjustedAtUtc"] = DateTime.UtcNow
        };

        return new SaturationBlockWorkItem(newMetadata);
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

    private sealed class SaturationBlockWorkItem : IBasicWorkItem
    {
        public Guid Id { get; } = Guid.NewGuid();

        public IDictionary<string, object> Metadata { get; }

        public SaturationBlockWorkItem(IDictionary<string, object> metadata)
        {
            Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
        }
    }

    #endregion
}
