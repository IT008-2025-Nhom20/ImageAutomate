using System.ComponentModel;

using ImageAutomate.Core;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;

namespace ImageAutomate.StandardBlocks;

/// <summary>
/// Specifies the algorithm used for resampling during resize.
/// </summary>
public enum ResizeResampler
{
    NearestNeighbor,
    Bilinear,
    Bicubic,
    Lanczos2,
    Lanczos3,
    Spline
}

/// <summary>
/// Specifies the mode of resizing.
/// </summary>
public enum ResizeModeOption
{
    Fixed,
    KeepAspect,
    Fit,
    Fill,
    Pad
}

/// <summary>
/// A block that resizes an image.
/// </summary>
public class ResizeBlock : IBlock
{
    #region Fields

    private readonly IReadOnlyList<Socket> _inputs = [new("Resize.In", "Image.In")];
    private readonly IReadOnlyList<Socket> _outputs = [new("Resize.Out", "Image.Out")];

    private bool _disposed;

    private ResizeModeOption _resizeMode = ResizeModeOption.Fixed;
    private int? _targetWidth = 100;
    private int? _targetHeight = 100;
    private bool _preserveAspectRatio = true;
    private ResizeResampler _resampler = ResizeResampler.Bicubic;
    private Color _paddingColor = Color.Black;

    private bool _isRelative = false;
    private double _scaleFactor = 1;

    private AnchorPositionOption _anchorPosition = AnchorPositionOption.Center;
    private bool _compand;
    private bool _premultiplyAlpha = true;
    // Layout fields
    private double _x;
    private double _y;
    private int _width;
    private int _height;
    private string _title = "Resize";



    #endregion

    public ResizeBlock()
        : this(200, 100)
    {
    }

    public ResizeBlock(int width, int height)
    {
        _width = width;
        _height = height;
    }

    #region IBlock basic

    /// <inheritdoc />
    [Browsable(false)]
    public string Name => "Resize";

    /// <inheritdoc />
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

    /// <inheritdoc />
    [Browsable(false)]
    public string Content
    {
        get
        {
            if (_isRelative)
            {
                return $"Scale: {ScaleFactor}x\nMode: {ResizeMode}";
            }
            return $"Size: {TargetWidth}x{TargetHeight}\nMode: {ResizeMode}";
        }

    }

    #endregion

    #region Layout Properties

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
    [Browsable(false)]
    public IReadOnlyList<Socket> Inputs => _inputs;
    /// <inheritdoc />
    [Browsable(false)]
    public IReadOnlyList<Socket> Outputs => _outputs;

    #endregion

    #region Configuration

    /// <summary>
    /// Gets or sets the resize mode.
    /// </summary>
    [Category("Configuration")]
    [Description("Resizing logic (Fixed, KeepAspect, Fit, Fill, Pad).")]
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

    /// <summary>
    /// Gets or sets the target width.
    /// </summary>
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

    /// <summary>
    /// Gets or sets the target height.
    /// </summary>
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

    /// <summary>
    /// Gets or sets whether to preserve aspect ratio in Fixed mode.
    /// </summary>
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

    /// <summary>
    /// Gets or sets the resampling algorithm.
    /// </summary>
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

    /// <summary>
    /// Gets or sets the background color for Pad mode.
    /// </summary>
    [Category("Configuration")]
    [Description("Background color used when ResizeMode = Pad.")]
    public Color PaddingColor
    {
        get => _paddingColor;
        set
        {
            if (_paddingColor != value)
            {
                _paddingColor = value;
                OnPropertyChanged(nameof(PaddingColor));
            }
        }
    }
    [Category("Configuration")]
    [Description("If true, resizes based on ScaleFactor relative to input size.")]
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
    [Category("Configuration")]
    [Description("If true, resizes based on ScaleFactor relative to input size.")]
    public double ScaleFactor
    {
        get => _scaleFactor;
        set
        {
            if (_scaleFactor != value)
            {
                _scaleFactor = value;
                OnPropertyChanged(nameof(ScaleFactor));
            }
        }
    }

    /// <summary>
    /// Gets or sets the anchor position.
    /// </summary>
    [Category("Configuration")]
    [Description("Anchor position (used in Pad, Crop, etc.).")]
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

    /// <summary>
    /// Gets or sets whether to compress and expand color (gamma correction) during processing.
    /// </summary>
    [Category("Configuration")]
    [Description("If true, performs operation in linear color space (Compand).")]
    public bool Compand
    {
        get => _compand;
        set
        {
            if (_compand != value)
            {
                _compand = value;
                OnPropertyChanged(nameof(Compand));
            }
        }
    }

    /// <summary>
    /// Gets or sets whether to premultiply alpha.
    /// </summary>
    [Category("Configuration")]
    [Description("If true, premultiplies alpha component during processing.")]
    public bool PremultiplyAlpha
    {
        get => _premultiplyAlpha;
        set
        {
            if (_premultiplyAlpha != value)
            {
                _premultiplyAlpha = value;
                OnPropertyChanged(nameof(PremultiplyAlpha));
            }
        }
    }
    #endregion

    #region INotifyPropertyChanged

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Raises the PropertyChanged event.
    /// </summary>
    protected void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    #endregion

    #region Execute

    /// <inheritdoc />
    public IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> Execute(IDictionary<Socket, IReadOnlyList<IBasicWorkItem>> inputs)
    {
        return Execute(inputs, CancellationToken.None);
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> Execute(IDictionary<Socket, IReadOnlyList<IBasicWorkItem>> inputs, CancellationToken cancellationToken)
    {
        return Execute(inputs.ToDictionary(kvp => kvp.Key.Id, kvp => kvp.Value), cancellationToken);
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> Execute(IDictionary<string, IReadOnlyList<IBasicWorkItem>> inputs)
    {
        return Execute(inputs, CancellationToken.None);
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> Execute(IDictionary<string, IReadOnlyList<IBasicWorkItem>> inputs, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(inputs, nameof(inputs));
        if (!inputs.TryGetValue(_inputs[0].Id, out var inItems))
            throw new ArgumentException($"Input items not found for the expected input socket {_inputs[0].Id}.", nameof(inputs));

        var outputItems = new List<IBasicWorkItem>();

        foreach (var sourceItem in inItems.OfType<WorkItem>())
        {
            cancellationToken.ThrowIfCancellationRequested();
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
        if (!IsRelative && !TargetWidth.HasValue && !TargetHeight.HasValue)
            throw new InvalidOperationException("ResizeBlock: At least one of TargetWidth or TargetHeight must be specified.");

        var sampler = MapResampler(Resampler);
        var mode = MapResizeMode(_resizeMode, PreserveAspectRatio);

        var targetSize = ComputeTargetSize(srcWidth, srcHeight);

        var options = new ResizeOptions
        {
            Size = targetSize,
            Mode = mode,
            Sampler = sampler,
            PremultiplyAlpha = PremultiplyAlpha,
            Compand = Compand,
            Position = MapAnchorPosition(AnchorPosition)
        };

        if (_resizeMode == ResizeModeOption.Pad)
        {
            options.PadColor = PaddingColor;
        }

        return options;
    }

    private SixLabors.ImageSharp.Size ComputeTargetSize(int srcWidth, int srcHeight)
    {
        int tw, th;

        if (IsRelative)
        {
            tw = (int)Math.Round(srcWidth * ScaleFactor);
            th = (int)Math.Round(srcHeight * ScaleFactor);

            // Ensure at least 1x1
            tw = Math.Max(1, tw);
            th = Math.Max(1, th);
        }
        else
        {
            tw = TargetWidth ?? 0;
            th = TargetHeight ?? 0;
        }

        switch (_resizeMode)
        {
            case ResizeModeOption.Fixed:
                // Fixed: if PreserveAspectRatio = false, stretch;
                // if true, scale by min factor
                if (!PreserveAspectRatio)
                {
                    if (tw <= 0 || th <= 0)
                        throw new InvalidOperationException("ResizeBlock (Fixed): TargetWidth and TargetHeight must be positive.");

                    return new(tw, th);
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
                    return new(rw, rh);
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
                    return new(rw, rh);
                }

            case ResizeModeOption.Fit:
            case ResizeModeOption.Fill:
            case ResizeModeOption.Pad:
                {
                    if (tw <= 0 || th <= 0)
                        throw new InvalidOperationException("ResizeBlock (Fit/Fill/Pad): TargetWidth and TargetHeight must be positive.");

                    return new(tw, th);
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
    static private AnchorPositionMode MapAnchorPosition(AnchorPositionOption position)
    {
        return position switch
        {
            AnchorPositionOption.TopLeft => AnchorPositionMode.TopLeft,
            AnchorPositionOption.Top => AnchorPositionMode.Top,
            AnchorPositionOption.TopRight => AnchorPositionMode.TopRight,
            AnchorPositionOption.Left => AnchorPositionMode.Left,
            AnchorPositionOption.Center => AnchorPositionMode.Center,
            AnchorPositionOption.Right => AnchorPositionMode.Right,
            AnchorPositionOption.BottomLeft => AnchorPositionMode.BottomLeft,
            AnchorPositionOption.Bottom => AnchorPositionMode.Bottom,
            AnchorPositionOption.BottomRight => AnchorPositionMode.BottomRight,
            _ => AnchorPositionMode.Center
        };
    }
    #endregion

    #region IDisposable

    /// <summary>
    /// Releases unmanaged and - optionally - managed resources.
    /// </summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            _disposed = true;
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion
}
