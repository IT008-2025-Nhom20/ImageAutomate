using ImageAutomate.Core;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using System.ComponentModel;

namespace ImageAutomate.StandardBlocks;


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
    private int _targetWidth = 0;
    private int _targetHeight = 0;
    private ResizeResampler _resampler = ResizeResampler.Lanczos3;

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
            return $"Width: {TargetWidth}\n" +
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
    [Description("Target width in pixels. Interpretation depends on ResizeMode.")]
    public int TargetWidth
    {
        get => _targetWidth;
        set
        {
            if (_targetWidth != value)
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(TargetWidth), "TargetWidth must be positive.");

                _targetWidth = value;
                OnPropertyChanged(nameof(TargetWidth));
            }
        }
    }

    [Category("Configuration")]
    [Description("Target height in pixels. Interpretation depends on ResizeMode.")]
    public int TargetHeight
    {
        get => _targetHeight;
        set
        {
            if (_targetHeight != value)
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(TargetHeight), "TargetHeight must be positive.");

                _targetHeight = value;
                OnPropertyChanged(nameof(TargetHeight));
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
            var resamplerOptions = MapResampler(Resampler);
            sourceItem.Image.Mutate(x => x.Resize(TargetWidth, TargetHeight, resamplerOptions));
            outputItems.Add(sourceItem);
        }

        return new Dictionary<Socket, IReadOnlyList<IBasicWorkItem>>
            {
                { _outputs[0], outputItems }
            };
    }

    #endregion

    #region Resize Option Builder

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
