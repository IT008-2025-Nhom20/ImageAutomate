using System.ComponentModel;

using ImageAutomate.Core;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace ImageAutomate.StandardBlocks;

/// <summary>
/// Defines how the crop region is determined.
/// </summary>
public enum CropModeOption
{
    Rectangle,
    Center,
    Anchor
}

/// <summary>
/// Defines the anchor point for anchor-based cropping.
/// </summary>
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

/// <summary>
/// A block that crops an image.
/// </summary>
public class CropBlock : IBlock
{
    #region Fields

    private readonly IReadOnlyList<Socket> _inputs = [new("Crop.In", "Image.In")];
    private readonly IReadOnlyList<Socket> _outputs = [new("Crop.Out", "Image.Out")];

    private bool _disposed;

    private CropModeOption _cropMode = CropModeOption.Rectangle;

    private int _cropX;
    private int _cropY;
    private int _cropWidth = 100;
    private int _cropHeight = 100;

    private AnchorPositionOption _anchorPosition = AnchorPositionOption.Center;

    // Layout fields
    private double _layoutX;
    private double _layoutY;
    private int _layoutWidth;
    private int _layoutHeight;
    private string _title = "Crop";

    private bool _isRelative;
    private double _cropXRelative;
    private double _cropYRelative;
    private double _cropWidthRelative = 0.5;
    private double _cropHeightRelative = 0.5;
    #endregion

    public CropBlock()
        : this(200, 100)
    {
    }

    public CropBlock(int width, int height)
    {
        _layoutWidth = width;
        _layoutHeight = height;
    }

    #region IBlock basic

    /// <inheritdoc />
    [Browsable(false)]
    public string Name => "Crop";

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
            if (IsRelative)
            {
                if (CropMode is CropModeOption.Rectangle)
                    return $"Crop Mode: {CropMode} (Relative)\n" +
                           $"Left: {CropXRelative:P0} Top: {CropYRelative:P0}\n" +
                           $"Width: {CropWidthRelative:P0} Height: {CropHeightRelative:P0}\n" +
                           $"Anchor Position: {AnchorPosition}";

                return $"Crop Mode: {CropMode} (Relative)\n" +
                       $"Width: {CropWidthRelative:P0} Height: {CropHeightRelative:P0}\n" +
                       $"Anchor Position: {AnchorPosition}";
            }
            if (CropMode is CropModeOption.Rectangle)
                return $"Crop Mode: {CropMode}\n" +
                       $"Left: {CropX} Top: {CropY}\n" +
                       $"Width: {CropWidth} Height: {CropHeight}\n" +
                       $"Anchor Position: {AnchorPosition}";

            return $"Crop Mode: {CropMode}\n" +
                   $"Width: {CropWidth} Height: {CropHeight}\n" +
                   $"Anchor Position: {AnchorPosition}";
        }
    }

    #endregion

    #region Layout Properties

    /// <inheritdoc />
    [Category("Layout")]
    public double X
    {
        get => _layoutX;
        set
        {
            if (Math.Abs(_layoutX - value) > double.Epsilon)
            {
                _layoutX = value;
                OnPropertyChanged(nameof(X));
            }
        }
    }

    /// <inheritdoc />
    [Category("Layout")]
    public double Y
    {
        get => _layoutY;
        set
        {
            if (Math.Abs(_layoutY - value) > double.Epsilon)
            {
                _layoutY = value;
                OnPropertyChanged(nameof(Y));
            }
        }
    }

    /// <inheritdoc />
    [Category("Layout")]
    public int Width
    {
        get => _layoutWidth;
        set
        {
            if (_layoutWidth != value)
            {
                _layoutWidth = value;
                OnPropertyChanged(nameof(Width));
            }
        }
    }

    /// <inheritdoc />
    [Category("Layout")]
    public int Height
    {
        get => _layoutHeight;
        set
        {
            if (_layoutHeight != value)
            {
                _layoutHeight = value;
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
    /// Gets or sets the crop mode.
    /// </summary>
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

    /// <summary>
    /// Gets or sets the X coordinate for Rectangle mode.
    /// </summary>
    [Category("Configuration")]
    [Description("Left coordinate (X) of crop origin in pixels (Rectangle mode).")]
    public int CropX
    {
        get => _cropX;
        set
        {
            if (_cropX != value)
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(CropX), "CropX must be non-negative.");

                _cropX = value;
                OnPropertyChanged(nameof(CropX));
            }
        }
    }

    /// <summary>
    /// Gets or sets the Y coordinate for Rectangle mode.
    /// </summary>
    [Category("Configuration")]
    [Description("Top coordinate (Y) of crop origin in pixels (Rectangle mode).")]
    public int CropY
    {
        get => _cropY;
        set
        {
            if (_cropY != value)
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(CropY), "CropY must be non-negative.");

                _cropY = value;
                OnPropertyChanged(nameof(CropY));
            }
        }
    }

    /// <summary>
    /// Gets or sets the width of the cropped area.
    /// </summary>
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

    /// <summary>
    /// Gets or sets the height of the cropped area.
    /// </summary>
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

    /// <summary>
    /// Gets or sets the anchor position for Anchor mode.
    /// </summary>
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
    /// <summary>
    /// Gets or sets a value indicating whether crop parameters are relative (0.0-1.0) or absolute pixels.
    /// </summary>
    [Category("Region Configuration")]
    [Description("If true, uses relative values (0.0-1.0) instead of pixels.")]
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

    /// <summary>
    /// Gets or sets the relative X coordinate (0.0-1.0).
    /// </summary>
    [Category("Region Configuration")]
    [Description("Relative Left coordinate (X) of crop origin (0.0-1.0).")]
    public double CropXRelative
    {
        get => _cropXRelative;
        set
        {
            if (Math.Abs(_cropXRelative - value) > double.Epsilon)
            {
                if (value < 0.0 || value > 1.0)
                    throw new ArgumentOutOfRangeException(nameof(CropXRelative), "Relative X must be between 0.0 and 1.0.");

                _cropXRelative = value;
                OnPropertyChanged(nameof(CropXRelative));
            }
        }
    }

    /// <summary>
    /// Gets or sets the relative Y coordinate (0.0-1.0).
    /// </summary>
    [Category("Region Configuration")]
    [Description("Relative Top coordinate (Y) of crop origin (0.0-1.0).")]
    public double CropYRelative
    {
        get => _cropYRelative;
        set
        {
            if (Math.Abs(_cropYRelative - value) > double.Epsilon)
            {
                if (value < 0.0 || value > 1.0)
                    throw new ArgumentOutOfRangeException(nameof(CropYRelative), "Relative Y must be between 0.0 and 1.0.");

                _cropYRelative = value;
                OnPropertyChanged(nameof(CropYRelative));
            }
        }
    }

    /// <summary>
    /// Gets or sets the relative width (0.0-1.0).
    /// </summary>
    [Category("Region Configuration")]
    [Description("Relative crop width (0.0-1.0).")]
    public double CropWidthRelative
    {
        get => _cropWidthRelative;
        set
        {
            if (Math.Abs(_cropWidthRelative - value) > double.Epsilon)
            {
                if (value <= 0.0 || value > 1.0)
                    throw new ArgumentOutOfRangeException(nameof(CropWidthRelative), "Relative Width must be between 0.0 and 1.0.");

                _cropWidthRelative = value;
                OnPropertyChanged(nameof(CropWidthRelative));
            }
        }
    }

    /// <summary>
    /// Gets or sets the relative height (0.0-1.0).
    /// </summary>
    [Category("Region Configuration")]
    [Description("Relative crop height (0.0-1.0).")]
    public double CropHeightRelative
    {
        get => _cropHeightRelative;
        set
        {
            if (Math.Abs(_cropHeightRelative - value) > double.Epsilon)
            {
                if (value <= 0.0 || value > 1.0)
                    throw new ArgumentOutOfRangeException(nameof(CropHeightRelative), "Relative Height must be between 0.0 and 1.0.");

                _cropHeightRelative = value;
                OnPropertyChanged(nameof(CropHeightRelative));
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
    public IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> Execute(
        IDictionary<Socket, IReadOnlyList<IBasicWorkItem>> inputs)
    {
        return Execute(inputs, CancellationToken.None);
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> Execute(
        IDictionary<Socket, IReadOnlyList<IBasicWorkItem>> inputs, CancellationToken cancellationToken)
    {
        return Execute(inputs.ToDictionary(kvp => kvp.Key.Id, kvp => kvp.Value), cancellationToken);
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> Execute(
        IDictionary<string, IReadOnlyList<IBasicWorkItem>> inputs)
    {
        return Execute(inputs, CancellationToken.None);
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> Execute(
        IDictionary<string, IReadOnlyList<IBasicWorkItem>> inputs, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(inputs, nameof(inputs));
        if (!inputs.TryGetValue(_inputs[0].Id, out var inItems))
            throw new ArgumentException($"Input items not found for the expected input socket {_inputs[0].Id}.", nameof(inputs));

        var outputItems = new List<IBasicWorkItem>();

        foreach (var sourceItem in inItems.OfType<WorkItem>())
        {
            cancellationToken.ThrowIfCancellationRequested();
            var rect = BuildCropRegion(sourceItem.Image.Width, sourceItem.Image.Height);

            sourceItem.Image.Mutate(x => x.Crop(rect));

            outputItems.Add(sourceItem);
        }

        return new Dictionary<Socket, IReadOnlyList<IBasicWorkItem>>
        {
            { _outputs[0], outputItems }
        };
    }

    #endregion

    #region Crop Region Builders

    private Rectangle BuildCropRegion(int sourceWidth, int sourceHeight)
    {
        return CropMode switch
        {
            CropModeOption.Rectangle => BuildRectangleCropRegion(sourceWidth, sourceHeight),
            CropModeOption.Center => BuildCenteredCropRegion(sourceWidth, sourceHeight),
            CropModeOption.Anchor => BuildAnchorCropRegion(sourceWidth, sourceHeight),
            _ => throw new NotSupportedException($"CropBlock: Unsupported CropMode '{CropMode}'."),
        };
    }
    private static Rectangle Clamp(Rectangle rect, int imageWidth, int imageHeight)
    {
        var imageRect = new Rectangle(0, 0, imageWidth, imageHeight);
        rect.Intersect(imageRect);

        if (rect.Width <= 0 || rect.Height <= 0)
        {
            return new Rectangle(0, 0, 1, 1);
        }

        return rect;
    }
    private Rectangle BuildRectangleCropRegion(int sourceWidth, int sourceHeight)
    {
        int x, y, w, h;

        if (IsRelative)
        {
            x = (int)(CropXRelative * sourceWidth);
            y = (int)(CropYRelative * sourceHeight);
            w = (int)(CropWidthRelative * sourceWidth);
            h = (int)(CropHeightRelative * sourceHeight);
        }
        else
        {
            x = CropX;
            y = CropY;
            w = CropWidth;
            h = CropHeight;
        }

        return Clamp(new Rectangle(x, y, w, h), sourceWidth, sourceHeight);
    }

    private Rectangle BuildCenteredCropRegion(int srcWidth, int srcHeight)
    {
        int w, h;

        if (IsRelative)
        {
            w = (int)(CropWidthRelative * srcWidth);
            h = (int)(CropHeightRelative * srcHeight);
        }
        else
        {
            w = CropWidth;
            h = CropHeight;
        }

        var x = (srcWidth - w) / 2;
        var y = (srcHeight - h) / 2;

        return Clamp(new Rectangle(x, y, w, h), srcWidth, srcHeight);
    }

    private Rectangle BuildAnchorCropRegion(int srcWidth, int srcHeight)
    {
        int w, h;

        if (IsRelative)
        {
            w = (int)(CropWidthRelative * srcWidth);
            h = (int)(CropHeightRelative * srcHeight);
        }
        else
        {
            w = CropWidth;
            h = CropHeight;
        }

        int x = 0, y = 0;

        switch (AnchorPosition)
        {
            case AnchorPositionOption.TopLeft:
                x = 0;
                y = 0;
                break;

            case AnchorPositionOption.Top:
                x = (srcWidth - w) / 2;
                y = 0;
                break;

            case AnchorPositionOption.TopRight:
                x = srcWidth - w;
                y = 0;
                break;

            case AnchorPositionOption.Left:
                x = 0;
                y = (srcHeight - h) / 2;
                break;

            case AnchorPositionOption.Center:
                x = (srcWidth - w) / 2;
                y = (srcHeight - h) / 2;
                break;

            case AnchorPositionOption.Right:
                x = srcWidth - w;
                y = (srcHeight - h) / 2;
                break;

            case AnchorPositionOption.BottomLeft:
                x = 0;
                y = srcHeight - h;
                break;

            case AnchorPositionOption.Bottom:
                x = (srcWidth - w) / 2;
                y = srcHeight - h;
                break;

            case AnchorPositionOption.BottomRight:
                x = srcWidth - w;
                y = srcHeight - h;
                break;

            default:
                throw new NotSupportedException($"CropBlock: Unsupported AnchorPosition '{AnchorPosition}'.");
        }

        return Clamp(new Rectangle(x, y, w, h), srcWidth, srcHeight);
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
