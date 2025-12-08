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
public enum CropModeOption
{
    Rectangle,
    Center,
    Anchor
}

public enum AnchorPositionOption
{
    TopLeft,
    Top,
    TopRight,
    Left,
    Center,
    Right,
    BottomLeft,
    Bottom,
    BottomRight
}
public class CropBlock : IBlock
{
    #region Fields

    private readonly Socket _inputSocket = new("Crop.In", "Image.In");
    private readonly Socket _outputSocket = new("Crop.Out", "Image.Out");
    private readonly IReadOnlyList<Socket> _inputs;
    private readonly IReadOnlyList<Socket> _outputs;

    private bool _disposed;

    private string _title = "Crop";
    private string _content = "Crop image";

    private int _nodeWidth = 200;
    private int _nodeHeight = 110;

    // Configuration
    private CropModeOption _cropMode = CropModeOption.Rectangle;

    private int _x;
    private int _y;
    private int _cropWidth = 100;
    private int _cropHeight = 100;

    private AnchorPositionOption _anchorPosition = AnchorPositionOption.Center;
    private bool _alwaysEncoder = true;
    #endregion

    #region Ctor

    public CropBlock()
    {
        _inputs = new[] { _inputSocket };
        _outputs = new[] { _outputSocket };
    }

    #endregion

    #region IBlock basic

    public string Name => "Crop";

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
    [Description("Crop mode controlling how the crop region is selected.")]
    public CropModeOption CropMode
    {
        get => _cropMode;
        set
        {
            if (_cropMode != value)
            {
                _cropMode = value;
                OnPropertyChanged(nameof(CropMode));
            }
        }
    }

    [Category("Configuration")]
    [Description("Left coordinate (X) of crop origin in pixels (Rectangle mode).")]
    public int X
    {
        get => _x;
        set
        {
            if (_x != value)
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(X), "X must be non-negative.");

                _x = value;
                OnPropertyChanged(nameof(X));
            }
        }
    }

    [Category("Configuration")]
    [Description("Top coordinate (Y) of crop origin in pixels (Rectangle mode).")]
    public int Y
    {
        get => _y;
        set
        {
            if (_y != value)
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(Y), "Y must be non-negative.");

                _y = value;
                OnPropertyChanged(nameof(Y));
            }
        }
    }

    [Category("Configuration")]
    [Description("Crop width in pixels.")]
    public int CropWidth
    {
        get => _cropWidth;
        set
        {
            if (_cropWidth != value)
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(CropWidth), "CropWidth must be positive.");

                _cropWidth = value;
                OnPropertyChanged(nameof(CropWidth));
            }
        }
    }

    [Category("Configuration")]
    [Description("Crop height in pixels.")]
    public int CropHeight
    {
        get => _cropHeight;
        set
        {
            if (_cropHeight != value)
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(CropHeight), "CropHeight must be positive.");

                _cropHeight = value;
                OnPropertyChanged(nameof(CropHeight));
            }
        }
    }

    [Category("Configuration")]
    [Description("Anchor position for Anchor crop mode.")]
    public AnchorPositionOption AnchorPosition
    {
        get => _anchorPosition;
        set
        {
            if (_anchorPosition != value)
            {
                _anchorPosition = value;
                OnPropertyChanged(nameof(AnchorPosition));
            }
        }
    }

    [Category("Configuration")]
    [Description("Force re-encoding even when format matches")]
    public bool AlwaysEncoder
    {
        get => _alwaysEncoder;
        set
        {
            if (_alwaysEncoder != value)
            {
                _alwaysEncoder = value;
                OnPropertyChanged(nameof(AlwaysEncoder));
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
            var cropped = CropWorkItem(item);
            if (cropped != null)
                resultList.Add(cropped);
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
            var cropped = CropWorkItem(item);
            if (cropped != null)
                resultList.Add(cropped);
        }

        var readOnly = new ReadOnlyCollection<IBasicWorkItem>(resultList);

        return new Dictionary<string, IReadOnlyList<IBasicWorkItem>>
            {
                { _outputSocket.Id, readOnly }
            };
    }

    #endregion

    #region Core crop logic

    private IBasicWorkItem? CropWorkItem(IBasicWorkItem item)
    {
        if (item is null)
            throw new ArgumentNullException(nameof(item));

        if (!_alwaysEncoder)
            return item;

        if (!item.Metadata.TryGetValue("ImageData", out var dataObj) ||
            dataObj is not byte[] imageBytes ||
            imageBytes.Length == 0)
        {
            // Không có ảnh → pass-through
            return item;
        }

        using var image = Image.Load<Rgba32>(imageBytes);
        var srcW = image.Width;
        var srcH = image.Height;

        // Tính rectangle crop
        var rect = BuildCropRectangle(srcW, srcH);

        // Validate trong bounds
        if (rect.X < 0 || rect.Y < 0 ||
            rect.Right > srcW || rect.Bottom > srcH)
        {
            throw new InvalidOperationException(
                $"CropBlock: Crop rectangle {rect} is outside image bounds ({srcW}x{srcH}).");
        }

        // Thực hiện crop
        image.Mutate(x => x.Crop(rect));

        // Giữ nguyên format decode nếu có, fallback PNG
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
            ["CroppedAtUtc"] = DateTime.UtcNow
        };

        return new CropBlockWorkItem(newMetadata);
    }

    private Rectangle BuildCropRectangle(int srcWidth, int srcHeight)
    {
        switch (CropMode)
        {
            case CropModeOption.Rectangle:
                return BuildRectangleMode(srcWidth, srcHeight);

            case CropModeOption.Center:
                return BuildCenterMode(srcWidth, srcHeight);

            case CropModeOption.Anchor:
                return BuildAnchorMode(srcWidth, srcHeight);

            default:
                throw new NotSupportedException($"CropBlock: Unsupported CropMode '{CropMode}'.");
        }
    }

    private Rectangle BuildRectangleMode(int srcWidth, int srcHeight)
    {
        if (CropWidth <= 0 || CropHeight <= 0)
            throw new InvalidOperationException("CropBlock (Rectangle): CropWidth and CropHeight must be positive.");

        // X, Y đã validate >= 0 ở setter
        if (X >= srcWidth || Y >= srcHeight)
            throw new InvalidOperationException("CropBlock (Rectangle): X/Y start outside image bounds.");

        return new Rectangle(X, Y, CropWidth, CropHeight);
    }

    private Rectangle BuildCenterMode(int srcWidth, int srcHeight)
    {
        if (CropWidth <= 0 || CropHeight <= 0)
            throw new InvalidOperationException("CropBlock (Center): CropWidth and CropHeight must be positive.");

        if (CropWidth > srcWidth || CropHeight > srcHeight)
            throw new InvalidOperationException(
                $"CropBlock (Center): Crop size {CropWidth}x{CropHeight} exceeds image bounds {srcWidth}x{srcHeight}.");

        var x = (srcWidth - CropWidth) / 2;
        var y = (srcHeight - CropHeight) / 2;

        return new Rectangle(x, y, CropWidth, CropHeight);
    }

    private Rectangle BuildAnchorMode(int srcWidth, int srcHeight)
    {
        if (CropWidth <= 0 || CropHeight <= 0)
            throw new InvalidOperationException("CropBlock (Anchor): CropWidth and CropHeight must be positive.");

        if (CropWidth > srcWidth || CropHeight > srcHeight)
            throw new InvalidOperationException(
                $"CropBlock (Anchor): Crop size {CropWidth}x{CropHeight} exceeds image bounds {srcWidth}x{srcHeight}.");

        int x = 0, y = 0;

        switch (AnchorPosition)
        {
            case AnchorPositionOption.TopLeft:
                x = 0;
                y = 0;
                break;

            case AnchorPositionOption.Top:
                x = (srcWidth - CropWidth) / 2;
                y = 0;
                break;

            case AnchorPositionOption.TopRight:
                x = srcWidth - CropWidth;
                y = 0;
                break;

            case AnchorPositionOption.Left:
                x = 0;
                y = (srcHeight - CropHeight) / 2;
                break;

            case AnchorPositionOption.Center:
                x = (srcWidth - CropWidth) / 2;
                y = (srcHeight - CropHeight) / 2;
                break;

            case AnchorPositionOption.Right:
                x = srcWidth - CropWidth;
                y = (srcHeight - CropHeight) / 2;
                break;

            case AnchorPositionOption.BottomLeft:
                x = 0;
                y = srcHeight - CropHeight;
                break;

            case AnchorPositionOption.Bottom:
                x = (srcWidth - CropWidth) / 2;
                y = srcHeight - CropHeight;
                break;

            case AnchorPositionOption.BottomRight:
                x = srcWidth - CropWidth;
                y = srcHeight - CropHeight;
                break;

            default:
                throw new NotSupportedException($"CropBlock: Unsupported AnchorPosition '{AnchorPosition}'.");
        }

        return new Rectangle(x, y, CropWidth, CropHeight);
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

    private sealed class CropBlockWorkItem : IBasicWorkItem
    {
        public Guid Id { get; } = Guid.NewGuid();

        public IDictionary<string, object> Metadata { get; }

        public CropBlockWorkItem(IDictionary<string, object> metadata)
        {
            Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
        }
    }

    #endregion
}
