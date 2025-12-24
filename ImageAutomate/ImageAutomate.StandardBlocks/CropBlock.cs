using ImageAutomate.Core;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.ComponentModel;

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

    private readonly IReadOnlyList<Socket> _inputs = [new("Crop.In", "Image.In")];
    private readonly IReadOnlyList<Socket> _outputs = [new("Crop.Out", "Image.Out")];

    private bool _disposed;

    private CropModeOption _cropMode = CropModeOption.Rectangle;

    private int _x;
    private int _y;
    private int _cropWidth = 100;
    private int _cropHeight = 100;

    private AnchorPositionOption _anchorPosition = AnchorPositionOption.Center;
    #endregion

    #region IBlock basic

    public string Name => "Crop";

    public string Title => "Crop";

    public string Content
    {
        get 
        {
            if (CropMode is CropModeOption.Rectangle)
                return $"Crop Mode: {CropMode}\n" +
                       $"Left: {X} Top: {Y}\n" +
                       $"Width: {CropWidth} Height: {CropHeight}\n" +
                       $"Anchor Position: {AnchorPosition}";

            return $"Crop Mode: {CropMode}\n" +
                   $"Width: {CropWidth} Height: {CropHeight}\n" +
                   $"Anchor Position: {AnchorPosition}";
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

    #endregion

    #region INotifyPropertyChanged

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    #endregion

    #region Execute

    public IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> Execute(
        IDictionary<Socket, IReadOnlyList<IBasicWorkItem>> inputs)
    {
        return Execute(inputs, CancellationToken.None);
    }

    public IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> Execute(
        IDictionary<Socket, IReadOnlyList<IBasicWorkItem>> inputs, CancellationToken cancellationToken)
    {
        return Execute(inputs.ToDictionary(kvp => kvp.Key.Id, kvp => kvp.Value), cancellationToken);
    }

    public IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> Execute(
        IDictionary<string, IReadOnlyList<IBasicWorkItem>> inputs)
    {
        return Execute(inputs, CancellationToken.None);
    }

    public IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> Execute(
        IDictionary<string, IReadOnlyList<IBasicWorkItem>> inputs, CancellationToken cancellationToken)
    {
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

    private Rectangle BuildRectangleCropRegion(int sourceWidth, int sourceHeight)
    {
        if (CropWidth <= 0 || CropHeight <= 0)
            throw new InvalidOperationException("CropBlock (Rectangle): CropWidth and CropHeight must be positive.");

        if (X >= sourceWidth || Y >= sourceHeight)
            throw new InvalidOperationException("CropBlock (Rectangle): X/Y start outside image bounds.");

        return new Rectangle(X, Y, CropWidth, CropHeight);
    }

    private Rectangle BuildCenteredCropRegion(int srcWidth, int srcHeight)
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

    private Rectangle BuildAnchorCropRegion(int srcWidth, int srcHeight)
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
}
