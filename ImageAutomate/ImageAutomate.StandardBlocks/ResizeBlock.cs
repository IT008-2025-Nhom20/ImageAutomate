using ImageAutomate.Core;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ImageAutomate.StandardBlocks;

public enum ResizeModeOption
{
    Fixed,
    KeepAspect,
    Fit,
    Fill,
    Pad
}

public enum ResizeResampler
{
    NearestNeighbor,
    Bilinear,
    Bicubic,
    Lanczos2,
    Lanczos3,
    Spline
}
public class ResizeBlock : IBlock
{
    #region Fields

    private readonly Socket _inputSocket = new("Resize.In", "Image.In");
    private readonly Socket _outputSocket = new("Resize.Out", "Image.Out");
    private readonly IReadOnlyList<Socket> _inputs;
    private readonly IReadOnlyList<Socket> _outputs;

    private bool _disposed;

    // UI / basic
    private string _title = "Resize";
    private string _content = "Resize image";

    private int _nodeWidth = 200;
    private int _nodeHeight = 110;

    // Configuration
    private ResizeModeOption _resizeMode = ResizeModeOption.Fit;
    private int? _targetWidth;
    private int? _targetHeight;
    private bool _preserveAspectRatio = true;
    private ResizeResampler _resampler = ResizeResampler.Lanczos3;
    private bool _alwaysEncoder = true;

    private Color _backgroundColor = Color.Transparent;

    #endregion

    #region Ctor

    public ResizeBlock()
    {
        _inputs = new[] { _inputSocket };
        _outputs = new[] { _outputSocket };
    }

    #endregion

    #region IBlock basic properties

    public string Name => "Resize";

    public string Title
    {
        get => _title;
    }

    public string Content
    {
        get
        {
            if (ResizeMode is ResizeModeOption.Fixed)
                return $"Resize mode: {ResizeMode}\n" +
                       $"Width: {TargetWidth}\n" +
                       $"Height: {TargetHeight}\n" +
                       $"Preserve aspect ratio: {PreserveAspectRatio}\n" +
                       $"Resampler: {Resampler}\n" +
                       $"Re-encode: {AlwaysEncoder}";
            else if(ResizeMode is ResizeModeOption.Pad)
                return $"Resize mode: {ResizeMode}\n" +
                       $"Width: {TargetWidth}\n" +
                       $"Height: {TargetHeight}\n" +
                       $"Resampler: {Resampler}\n" +
                       $"Back ground color: {BackgroundColor}\n" +
                       $"Re-encode: {AlwaysEncoder}";
            return $"Resize mode: {ResizeMode}\n" +
                   $"Width: {TargetWidth}\n" +
                   $"Height: {TargetHeight}\n" +
                   $"Resampler: {Resampler}\n" +
                   $"Re-encode: {AlwaysEncoder}";
        }
    }

    #endregion

    #region Layout (node size)

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

    #region Configuration properties

    [Category("Configuration")]
    [Description("Resize mode controlling how the target size is interpreted")]
    public ResizeModeOption ResizeMode
    {
        get => _resizeMode;
        set
        {
            if (_resizeMode != value)
            {
                _resizeMode = value;
                OnPropertyChanged(nameof(ResizeMode));
            }
        }
    }

    [Category("Configuration")]
    [Description("Target width in pixels. Interpretation depends on ResizeMode.")]
    public int? TargetWidth
    {
        get => _targetWidth;
        set
        {
            if (_targetWidth != value)
            {
                if (value.HasValue && value.Value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(TargetWidth), "TargetWidth must be positive.");

                _targetWidth = value;
                OnPropertyChanged(nameof(TargetWidth));
            }
        }
    }

    [Category("Configuration")]
    [Description("Target height in pixels. Interpretation depends on ResizeMode.")]
    public int? TargetHeight
    {
        get => _targetHeight;
        set
        {
            if (_targetHeight != value)
            {
                if (value.HasValue && value.Value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(TargetHeight), "TargetHeight must be positive.");

                _targetHeight = value;
                OnPropertyChanged(nameof(TargetHeight));
            }
        }
    }

    [Category("Configuration")]
    [Description("If true, preserves aspect ratio in Fixed mode.")]
    public bool PreserveAspectRatio
    {
        get => _preserveAspectRatio;
        set
        {
            if (_preserveAspectRatio != value)
            {
                _preserveAspectRatio = value;
                OnPropertyChanged(nameof(PreserveAspectRatio));
            }
        }
    }

    [Category("Configuration")]
    [Description("Resampling kernel used during resize.")]
    public ResizeResampler Resampler
    {
        get => _resampler;
        set
        {
            if (_resampler != value)
            {
                _resampler = value;
                OnPropertyChanged(nameof(Resampler));
            }
        }
    }

    [Category("Configuration")]
    [Description("Background color used when ResizeMode = Pad.")]
    public Color BackgroundColor
    {
        get => _backgroundColor;
        set
        {
            if (_backgroundColor != value)
            {
                _backgroundColor = value;
                OnPropertyChanged(nameof(BackgroundColor));
            }
        }
    }
    [Category("Configuration")]
    [Description("If false, block will NOT resize when input image matches target size or resize is unnecessary.")]
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

    public IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> Execute(IDictionary<Socket, IReadOnlyList<IBasicWorkItem>> inputs)
    {
        if (inputs is null) throw new ArgumentNullException(nameof(inputs));

        inputs.TryGetValue(_inputSocket, out var inItems);
        inItems ??= Array.Empty<IBasicWorkItem>();

        var resultList = new List<IBasicWorkItem>(inItems.Count);

        foreach (var item in inItems)
        {
            var resized = ResizeWorkItem(item);
            if (resized != null)
                resultList.Add(resized);
        }

        var readOnly = new ReadOnlyCollection<IBasicWorkItem>(resultList);

        return new Dictionary<Socket, IReadOnlyList<IBasicWorkItem>>
            {
                { _outputSocket, readOnly }
            };
    }

    #endregion

    #region Execute (string keyed)

    public IReadOnlyDictionary<string, IReadOnlyList<IBasicWorkItem>> Execute(IDictionary<string, IReadOnlyList<IBasicWorkItem>> inputs)
    {
        if (inputs is null) throw new ArgumentNullException(nameof(inputs));

        inputs.TryGetValue(_inputSocket.Id, out var inItems);
        inItems ??= Array.Empty<IBasicWorkItem>();

        var resultList = new List<IBasicWorkItem>(inItems.Count);

        foreach (var item in inItems)
        {
            var resized = ResizeWorkItem(item);
            if (resized != null)
                resultList.Add(resized);
        }

        var readOnly = new ReadOnlyCollection<IBasicWorkItem>(resultList);

        return new Dictionary<string, IReadOnlyList<IBasicWorkItem>>
            {
                { _outputSocket.Id, readOnly }
            };
    }

    #endregion

    #region Core resize logic

    private IBasicWorkItem? ResizeWorkItem(IBasicWorkItem item)
    {
        if (item is null)
            throw new ArgumentNullException(nameof(item));


        if (!AlwaysEncoder)
        {
            return item;
        }

        // 1. Lấy image bytes
        if (!item.Metadata.TryGetValue("ImageData", out var dataObj) ||
            dataObj is not byte[] imageBytes ||
            imageBytes.Length == 0)
        {
            // Không có ảnh → pass-through
            return item;
        }

        // 2. Load ảnh bằng ImageSharp
        using var image = Image.Load<Rgba32>(imageBytes);

        // 3. Tính toán ResizeOptions
        var options = BuildResizeOptions(image.Width, image.Height);

        // 4. Resize
        image.Mutate(x => x.Resize(options));

        // 5. Encode lại, giữ nguyên format (nếu biết), mặc định PNG
        var decodedFormat = image.Metadata.DecodedImageFormat ?? PngFormat.Instance;

        using var ms = new MemoryStream();
        image.Save(ms, decodedFormat);
        var outBytes = ms.ToArray();

        // 6. Cập nhật metadata (giữ nguyên các key khác)
        var newMetadata = new Dictionary<string, object>(item.Metadata)
        {
            ["ImageData"] = outBytes,
            ["Width"] = image.Width,
            ["Height"] = image.Height,
            ["ResizedAtUtc"] = DateTime.UtcNow
        };

        return new ResizeBlockWorkItem(newMetadata);
    }

    private ResizeOptions BuildResizeOptions(int srcWidth, int srcHeight)
    {
        if (srcWidth <= 0 || srcHeight <= 0)
            throw new InvalidOperationException("ResizeBlock: Source image has invalid dimensions.");

        // validate target dims
        if (!TargetWidth.HasValue && !TargetHeight.HasValue)
            throw new InvalidOperationException("ResizeBlock: At least one of TargetWidth or TargetHeight must be specified.");

        var sampler = MapResampler(Resampler);
        var mode = MapResizeMode(_resizeMode, PreserveAspectRatio);

        var targetSize = ComputeTargetSize(srcWidth, srcHeight);

        var options = new ResizeOptions
        {
            Size = targetSize,
            Mode = mode,
            Sampler = sampler,
            PremultiplyAlpha = true,
            Position = AnchorPositionMode.Center
        };

        if (_resizeMode == ResizeModeOption.Pad)
        {
            options.PadColor = BackgroundColor;
        }

        return options;
    }

    private Size ComputeTargetSize(int srcWidth, int srcHeight)
    {
        int tw = TargetWidth ?? 0;
        int th = TargetHeight ?? 0;

        switch (_resizeMode)
        {
            case ResizeModeOption.Fixed:
                // Fixed: nếu PreserveAspectRatio = false -> stretch,
                // nếu true -> scale theo min factor.
                if (!PreserveAspectRatio)
                {
                    if (tw <= 0 || th <= 0)
                        throw new InvalidOperationException("ResizeBlock (Fixed): TargetWidth and TargetHeight must be positive.");

                    return new Size(tw, th);
                }
                else
                {
                    // scale theo min factor, không đảm bảo đúng tw x th nhưng giữ ratio
                    if (tw <= 0 && th <= 0)
                        throw new InvalidOperationException("ResizeBlock (Fixed+PreserveAspectRatio): At least one of TargetWidth or TargetHeight must be positive.");

                    double scale;
                    if (tw > 0 && th > 0)
                    {
                        var scaleW = (double)tw / srcWidth;
                        var scaleH = (double)th / srcHeight;
                        scale = Math.Min(scaleW, scaleH);
                    }
                    else if (tw > 0)
                    {
                        scale = (double)tw / srcWidth;
                    }
                    else
                    {
                        scale = (double)th / srcHeight;
                    }

                    int rw = Math.Max(1, (int)Math.Round(srcWidth * scale));
                    int rh = Math.Max(1, (int)Math.Round(srcHeight * scale));
                    return new Size(rw, rh);
                }

            case ResizeModeOption.KeepAspect:
                {
                    // Scale theo 1 chiều, giữ aspect
                    if (tw <= 0 && th <= 0)
                        throw new InvalidOperationException("ResizeBlock (KeepAspect): At least one of TargetWidth or TargetHeight must be positive.");

                    double scale;
                    if (tw > 0 && th > 0)
                    {
                        var scaleW = (double)tw / srcWidth;
                        var scaleH = (double)th / srcHeight;
                        scale = Math.Min(scaleW, scaleH);
                    }
                    else if (tw > 0)
                    {
                        scale = (double)tw / srcWidth;
                    }
                    else
                    {
                        scale = (double)th / srcHeight;
                    }

                    int rw = Math.Max(1, (int)Math.Round(srcWidth * scale));
                    int rh = Math.Max(1, (int)Math.Round(srcHeight * scale));
                    return new Size(rw, rh);
                }

            case ResizeModeOption.Fit:
            case ResizeModeOption.Fill:
            case ResizeModeOption.Pad:
                {
                    if (tw <= 0 || th <= 0)
                        throw new InvalidOperationException("ResizeBlock (Fit/Fill/Pad): TargetWidth and TargetHeight must be positive.");

                    return new Size(tw, th);
                }

            default:
                throw new NotSupportedException($"ResizeBlock: Unsupported ResizeMode '{_resizeMode}'.");
        }
    }

    private IResampler MapResampler(ResizeResampler resampler)
    {
        return resampler switch
        {
            ResizeResampler.NearestNeighbor => KnownResamplers.NearestNeighbor,
            ResizeResampler.Bilinear => KnownResamplers.Triangle,
            ResizeResampler.Bicubic => KnownResamplers.Bicubic,
            ResizeResampler.Lanczos2 => KnownResamplers.Lanczos2,
            ResizeResampler.Lanczos3 => KnownResamplers.Lanczos3,
            ResizeResampler.Spline => KnownResamplers.MitchellNetravali,
            _ => KnownResamplers.Lanczos3
        };
    }

    private SixLabors.ImageSharp.Processing.ResizeMode MapResizeMode(ResizeModeOption mode, bool preserveAspect)
    {
        return mode switch
        {
            ResizeModeOption.Fixed when !preserveAspect => SixLabors.ImageSharp.Processing.ResizeMode.Stretch,
            ResizeModeOption.Fixed when preserveAspect => SixLabors.ImageSharp.Processing.ResizeMode.Max,
            ResizeModeOption.KeepAspect => SixLabors.ImageSharp.Processing.ResizeMode.Max,
            ResizeModeOption.Fit => SixLabors.ImageSharp.Processing.ResizeMode.Max,
            ResizeModeOption.Fill => SixLabors.ImageSharp.Processing.ResizeMode.Crop,
            ResizeModeOption.Pad => SixLabors.ImageSharp.Processing.ResizeMode.Pad,
            _ => SixLabors.ImageSharp.Processing.ResizeMode.Max
        };
    }

    #endregion

    #region IDisposable

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            // hiện chưa giữ resource managed nào đặc biệt
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

    private sealed class ResizeBlockWorkItem : IBasicWorkItem
    {
        public Guid Id { get; } = Guid.NewGuid();

        public IDictionary<string, object> Metadata { get; }

        public ResizeBlockWorkItem(IDictionary<string, object> metadata)
        {
            Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
        }
    }

    #endregion
}
