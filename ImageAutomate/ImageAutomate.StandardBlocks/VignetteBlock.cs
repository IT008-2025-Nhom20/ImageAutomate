using ImageAutomate.Core;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.ComponentModel;

namespace ImageAutomate.StandardBlocks;

public class VignetteBlock : IBlock
{
    #region Fields

    private readonly IReadOnlyList<Socket> _inputs = [new("Vignette.In", "Image.In")];
    private readonly IReadOnlyList<Socket> _outputs = [new("Vignette.Out", "Image.Out")];

    private int _width = 200;
    private int _height = 100;

    private Color _color = Color.Black;
    private float _strength = 0.6f;
    private bool _disposed;

    #endregion

    #region INotifyPropertyChanged

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    #endregion

    #region Basic Properties

    public string Name => "Vignette";

    public string Title => "Vignette";

    public string Content => $"Color: {Color}\nStrength: {Strength}";

    [Category("Layout")]
    [Description("Width of the block node")]
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
    [Description("Height of the block node")]
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

    #region Configuration

    [Category("Configuration")]
    [Description("Vignette color used at the edges. Default is black.")]
    public Color Color
    {
        get => _color;
        set
        {
            if (!_color.Equals(value))
            {
                _color = value;
                OnPropertyChanged(nameof(Color));
            }
        }
    }

    [Category("Configuration")]
    [Description("Vignette strength (0.0–1.0). 0 = no effect, 1 = full effect.")]
    public float Strength
    {
        get => _strength;
        set
        {
            var clamped = Math.Clamp(value, 0f, 1f);
            if (Math.Abs(_strength - clamped) > float.Epsilon)
            {
                _strength = clamped;
                OnPropertyChanged(nameof(Strength));
            }
        }
    }

    #endregion

    #region Sockets

    public IReadOnlyList<Socket> Inputs => _inputs;
    public IReadOnlyList<Socket> Outputs => _outputs;

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
            if (_strength > 0f)
            {
                sourceItem.Image.Mutate(
                    x => x.Vignette(
                        new GraphicsOptions
                        {
                            BlendPercentage = _strength
                        },
                        _color
                    )
                );
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

