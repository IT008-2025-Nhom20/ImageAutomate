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

public enum FlipModeOption
{
    Horizontal,
    Vertical
}
public class FlipBlock : IBlock
{
    #region Fields

    private readonly Socket _inputSocket = new("Flip.In", "Image.In");
    private readonly Socket _outputSocket = new("Flip.Out", "Image.Out");
    private readonly IReadOnlyList<Socket> _inputs;
    private readonly IReadOnlyList<Socket> _outputs;

    private bool _disposed;

    private string _title = "Flip";
    private string _content = "Flip image";

    private int _nodeWidth = 200;
    private int _nodeHeight = 100;

    private FlipModeOption _flipMode = FlipModeOption.Horizontal;
    private bool _alwaysEncode = true;
    #endregion

    #region Ctor

    public FlipBlock()
    {
        _inputs = new[] { _inputSocket };
        _outputs = new[] { _outputSocket };
    }

    #endregion

    #region IBlock basic

    public string Name => "Flip";

    public string Title
    {
        get => _title;
    }

    public string Content
    {
        get => $"Flip direction: {FlipMode}\n Re-encode: {AlwaysEncode}";
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
    [Description("Flip direction: Horizontal or Vertical.")]
    public FlipModeOption FlipMode
    {
        get => _flipMode;
        set
        {
            if (_flipMode != value)
            {
                _flipMode = value;
                OnPropertyChanged(nameof(FlipMode));
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

        inputs.TryGetValue(_inputSocket, out var inItems);
        inItems ??= Array.Empty<IBasicWorkItem>();

        var resultList = new List<IBasicWorkItem>(inItems.Count);

        foreach (var item in inItems)
        {
            var flipped = FlipWorkItem(item);
            if (flipped != null)
                resultList.Add(flipped);
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
            var flipped = FlipWorkItem(item);
            if (flipped != null)
                resultList.Add(flipped);
        }

        var readOnly = new ReadOnlyCollection<IBasicWorkItem>(resultList);

        return new Dictionary<string, IReadOnlyList<IBasicWorkItem>>
            {
                { _outputSocket.Id, readOnly }
            };
    }

    #endregion

    #region Core flip logic

    private IBasicWorkItem? FlipWorkItem(IBasicWorkItem item)
    {
        if (item is null)
            throw new ArgumentNullException(nameof(item));

        if (!_alwaysEncode)
        {
            return item;
        }

        if (!item.Metadata.TryGetValue("ImageData", out var dataObj) ||
            dataObj is not byte[] imageBytes ||
            imageBytes.Length == 0)
        {
            // Không có ảnh, trả nguyên work item
            return item;
        }

        using var image = Image.Load<Rgba32>(imageBytes);

        // Map FlipModeOption -> ImageSharp FlipMode
        var sharpFlipMode = _flipMode == FlipModeOption.Horizontal
            ? SixLabors.ImageSharp.Processing.FlipMode.Horizontal
            : SixLabors.ImageSharp.Processing.FlipMode.Vertical;

        image.Mutate(x => x.Flip(sharpFlipMode));

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
            ["FlippedAtUtc"] = DateTime.UtcNow,
            ["FlipMode"] = _flipMode
        };

        return new FlipBlockWorkItem(newMetadata);
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

    private sealed class FlipBlockWorkItem : IBasicWorkItem
    {
        public Guid Id { get; } = Guid.NewGuid();

        public IDictionary<string, object> Metadata { get; }

        public FlipBlockWorkItem(IDictionary<string, object> metadata)
        {
            Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
        }
    }

    #endregion
}
