using ImageAutomate.Core;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.ComponentModel;

namespace ImageAutomate.StandardBlocks;

public class CropBlock : IBlock
{
    #region Fields

    private readonly IReadOnlyList<Socket> _inputs = [new("Crop.In", "Image.In")];
    private readonly IReadOnlyList<Socket> _outputs = [new("Crop.Out", "Image.Out")];

    private bool _disposed;

    private int _nodeWidth = 200;
    private int _nodeHeight = 110;

    private int _x;
    private int _y;
    private int _cropWidth = 100;
    private int _cropHeight = 100;

    #endregion

    #region IBlock basic

    public string Name => "Crop";

    public string Title => "Crop";

    public string Content
    {
        get 
        {
            return $"Widht: {CropWidth}\n" +
                   $"Height: {CropHeight}";
        }
    }
    // eliminate anchorposition and cropmodeoption
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
        return Execute(inputs.ToDictionary(kvp => kvp.Key.Id, kvp => kvp.Value));
    }

    public IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> Execute(
        IDictionary<string, IReadOnlyList<IBasicWorkItem>> inputs)
    {
        if (!inputs.TryGetValue(_inputs[0].Id, out var inItems))
            throw new ArgumentException($"Input items not found for the expected input socket {_inputs[0].Id}.", nameof(inputs)); 

        var outputItems = new List<IBasicWorkItem>();

        foreach (var sourceItem in inItems.OfType<WorkItem>())
        {
            sourceItem.Image.Mutate(x => x.Crop(CropWidth, CropHeight));
            outputItems.Add(sourceItem);
        }
        
        return new Dictionary<Socket, IReadOnlyList<IBasicWorkItem>>
        {
            { _outputs[0], outputItems }
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
