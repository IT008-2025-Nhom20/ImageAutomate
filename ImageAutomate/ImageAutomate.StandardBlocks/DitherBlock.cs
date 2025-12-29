using ImageAutomate.Core;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Dithering;
using System.ComponentModel;

namespace ImageAutomate.StandardBlocks;

public enum DitherMode
{
    FloydSteinberg,
    Atkinson,
    Bayer4x4,
    Bayer8x8
}

public class DitherBlock : IBlock
{
    #region Fields

    private readonly IReadOnlyList<Socket> _inputs = [new("Dither.In", "Image.In")];
    private readonly IReadOnlyList<Socket> _outputs = [new("Dither.Out", "Image.Out")];

    private bool _disposed;

    // Configuration fields
    private DitherMode _mode = DitherMode.FloydSteinberg;
    private float _ditherScale = 1.0f;

    // Layout fields
    private double _x;
    private double _y;
    private int _width;
    private int _height;
    private string _title = "Dither";

    #endregion

    public DitherBlock()
        : this(200, 100)
    {
    }

    public DitherBlock(int width, int height)
    {
        _width = width;
        _height = height;
    }

    #region IBlock basic

    [Browsable(false)]
    public string Name => "Dither";

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

    [Browsable(false)]
    public string Content => $"Mode: {Mode}";

    #endregion

    #region Layout Properties

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

    [Browsable(false)]
    public IReadOnlyList<Socket> Inputs => _inputs;
    [Browsable(false)]
    public IReadOnlyList<Socket> Outputs => _outputs;

    #endregion

    #region Configuration

    [Category("Configuration")]
    [Description("The dithering algorithm to use.")]
    public DitherMode Mode
    {
        get => _mode;
        set
        {
            if (_mode != value)
            {
                _mode = value;
                OnPropertyChanged(nameof(Mode));
            }
        }
    }

    [Category("Configuration")]
    [Description("Scales the dithering matrix. Larger values (>1.0) create larger, blockier dither patterns.")]
    public float DitherScale
    {
        get => _ditherScale;
        set
        {
            var clamped = Math.Max(value, 0.1f);
            if (Math.Abs(_ditherScale - clamped) > float.Epsilon)
            {
                _ditherScale = clamped;
                OnPropertyChanged(nameof(DitherScale));
                OnPropertyChanged(nameof(Content));
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
        ArgumentNullException.ThrowIfNull(inputs, nameof(inputs));
        if (!inputs.TryGetValue(_inputs[0].Id, out var inItems))
            throw new ArgumentException($"Input items not found for the expected input socket {_inputs[0].Id}.", nameof(inputs));

        var outputItems = new List<IBasicWorkItem>();

        // Select the IDitherer based on the Enum
        IDither ditherer = GetDitherer(Mode);

        foreach (var sourceItem in inItems.OfType<WorkItem>())
        {
            cancellationToken.ThrowIfCancellationRequested();

            sourceItem.Image.Mutate(x => x.Dither(ditherer, DitherScale));

            outputItems.Add(sourceItem);
        }

        return new Dictionary<Socket, IReadOnlyList<IBasicWorkItem>>
            {
                { _outputs[0], outputItems }
            };
    }

    private static IDither GetDitherer(DitherMode mode)
    {
        return mode switch
        {
            DitherMode.FloydSteinberg => KnownDitherings.FloydSteinberg,
            DitherMode.Atkinson => KnownDitherings.Atkinson,
            DitherMode.Bayer4x4 => KnownDitherings.Bayer4x4,
            DitherMode.Bayer8x8 => KnownDitherings.Bayer8x8,
            _ => KnownDitherings.FloydSteinberg
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