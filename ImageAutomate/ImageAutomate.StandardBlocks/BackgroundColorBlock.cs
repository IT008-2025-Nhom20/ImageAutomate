using ImageAutomate.Core;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.ComponentModel;

namespace ImageAutomate.StandardBlocks;

public class BackgroundColorBlock : IBlock
{
    #region Fields

    private readonly IReadOnlyList<Socket> _inputs = [new("BackgroundColor.In", "Image.In")];
    private readonly IReadOnlyList<Socket> _outputs = [new("BackgroundColor.Out", "Image.Out")];

    private bool _disposed;

    // Configuration fields
    private Color _color = Color.White; // Default to White

    // Layout fields
    private double _x;
    private double _y;
    private int _width;
    private int _height;
    private string _title = "Background Color";

    #endregion

    public BackgroundColorBlock()
        : this(200, 100)
    {
    }

    public BackgroundColorBlock(int width, int height)
    {
        _width = width;
        _height = height;
    }

    #region IBlock basic

    [Browsable(false)]
    public string Name => "BackgroundColor";

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
    public string Content => $"Color: {Color}";

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
    [Description("The color to apply to the background (transparent areas).")]
    public Color Color
    {
        get => _color;
        set
        {
            if (_color != value)
            {
                _color = value;
                OnPropertyChanged(nameof(Color));
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

            sourceItem.Image.Mutate(x => x.BackgroundColor(Color));

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