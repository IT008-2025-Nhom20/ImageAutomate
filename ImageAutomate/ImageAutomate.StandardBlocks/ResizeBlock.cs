using ImageAutomate.Core;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
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

    private readonly IReadOnlyList<Socket> _inputs = [new("Resize.In", "Image.In")];
    private readonly IReadOnlyList<Socket> _outputs = [new("Resize.Out", "Image.Out")];

    private bool _disposed;

    private int _nodeWidth = 200;
    private int _nodeHeight = 110;

    // Configuration
    private ResizeModeOption _resizeMode = ResizeModeOption.Fit;
    private int? _targetWidth;
    private int? _targetHeight;
    private bool _preserveAspectRatio = true;
    private ResizeResampler _resampler = ResizeResampler.Lanczos3;
    private Color _backgroundColor = Color.Transparent;

    #endregion

    #region IBlock basic properties

    public string Name => "Resize";

    public string Title
    {
        get => "Resize";
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
                       $"Resampler: {Resampler}\n";
            else if (ResizeMode is ResizeModeOption.Pad)
                return $"Resize mode: {ResizeMode}\n" +
                       $"Width: {TargetWidth}\n" +
                       $"Height: {TargetHeight}\n" +
                       $"Resampler: {Resampler}\n" +
                       $"Back ground color: {BackgroundColor}\n";
            return $"Resize mode: {ResizeMode}\n" +
                   $"Width: {TargetWidth}\n" +
                   $"Height: {TargetHeight}\n" +
                   $"Resampler: {Resampler}\n";
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

    #endregion

    #region INotifyPropertyChanged

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    #endregion

    #region Execute

    public IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> Execute(IDictionary<Socket, IReadOnlyList<IBasicWorkItem>> inputs)
    {
        return Execute(inputs.ToDictionary(kvp => kvp.Key.Id, kvp => kvp.Value));
    }

    public IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> Execute(IDictionary<string, IReadOnlyList<IBasicWorkItem>> inputs)
    {
        if (!inputs.TryGetValue(_inputs[0].Id, out var inItems))
            throw new ArgumentException($"Input items not found for the expected input socket {_inputs[0].Id}.", nameof(inputs));

        var outputItems = new List<IBasicWorkItem>();

        foreach (var sourceItem in inItems.OfType<WorkItem>())
        {
            var resizeOptions = BuildResizeOptions(sourceItem.Image.Width, sourceItem.Image.Height);
            sourceItem.Image.Mutate(x => x.Resize(resizeOptions));
            outputItems.Add(sourceItem);
        }

        return new Dictionary<Socket, IReadOnlyList<IBasicWorkItem>>
            {
                { _outputs[0], outputItems }
            };
    }

    #endregion

    #region Resize Option Builder

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
                // Fixed: if PreserveAspectRatio = false, stretch;
                // if true, scale by min factor
                if (!PreserveAspectRatio)
                {
                    if (tw <= 0 || th <= 0)
                        throw new InvalidOperationException("ResizeBlock (Fixed): TargetWidth and TargetHeight must be positive.");

                    return new Size(tw, th);
                }
                else
                {
                    // Scale by min factor, doesn't guarantee exact tw x th but preserves ratio
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
                    // Scale in one dimension, preserve aspect ratio
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

    static private IResampler MapResampler(ResizeResampler resampler)
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

    static private ResizeMode MapResizeMode(ResizeModeOption mode, bool preserveAspect)
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
