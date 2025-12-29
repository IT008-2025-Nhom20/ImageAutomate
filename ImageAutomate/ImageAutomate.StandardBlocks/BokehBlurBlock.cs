using ImageAutomate.Core;
using SixLabors.ImageSharp.Processing;
using System.ComponentModel;

namespace ImageAutomate.StandardBlocks;

public class BokehBlurBlock : IBlock
{
    #region Fields

    private readonly IReadOnlyList<Socket> _inputs = [new("BokehBlur.In", "Image.In")];
    private readonly IReadOnlyList<Socket> _outputs = [new("BokehBlur.Out", "Image.Out")];

    private bool _disposed;

    // Configuration fields
    private int _radius = 32;       // Size of the blur
    private int _components = 6;    // Number of aperture blades (e.g., 6 = Hexagon)
    private float _gamma = 3.0f;    // Highlight intensity

    // Layout fields
    private double _x;
    private double _y;
    private int _width;
    private int _height;
    private string _title = "Bokeh Blur";

    #endregion

    public BokehBlurBlock()
        : this(220, 140) // Taller to accommodate 3 properties in UI if needed
    {
    }

    public BokehBlurBlock(int width, int height)
    {
        _width = width;
        _height = height;
    }

    #region IBlock basic

    [Browsable(false)]
    public string Name => "BokehBlur";

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
    public string Content => $"R:{Radius}\nC:{Components}\nG:{Gamma:F1}";

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

    [Browsable(false)]
    public IReadOnlyList<Socket> Inputs => _inputs;
    [Browsable(false)]
    public IReadOnlyList<Socket> Outputs => _outputs;

    #endregion

    #region Configuration

    [Category("Configuration")]
    [Description("The radius of the blur kernel.")]
    public int Radius
    {
        get => _radius;
        set
        {
            // Radius must be >= 1 for Bokeh
            var clamped = Math.Max(value, 1);
            if (_radius != clamped)
            {
                _radius = clamped;
                OnPropertyChanged(nameof(Radius));
                OnPropertyChanged(nameof(Content));
            }
        }
    }

    [Category("Configuration")]
    [Description("The number of components (aperture blades). E.g., 6 for hexagonal bokeh.")]
    public int Components
    {
        get => _components;
        set
        {
            var clamped = Math.Max(value, 1);
            if (_components != clamped)
            {
                _components = clamped;
                OnPropertyChanged(nameof(Components));
                OnPropertyChanged(nameof(Content));
            }
        }
    }

    [Category("Configuration")]
    [Description("The gamma strength (highlight intensity).")]
    public float Gamma
    {
        get => _gamma;
        set
        {
            // Prevent negative or zero gamma
            var clamped = Math.Max(value, 0.1f);
            if (Math.Abs(_gamma - clamped) > float.Epsilon)
            {
                _gamma = clamped;
                OnPropertyChanged(nameof(Gamma));
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

        foreach (var sourceItem in inItems.OfType<WorkItem>())
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (Radius > 0)
            {
                sourceItem.Image.Mutate(x => x.BokehBlur(Radius, Components, Gamma));
            }

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