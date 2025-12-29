using ImageAutomate.Core;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.ComponentModel;

namespace ImageAutomate.StandardBlocks;

// Enum definition for the block
public enum SwizzleMode
{
    RGB, // No change
    BGR, // Swap Red and Blue
    RBG, // Swap Green and Blue
    GRB, // Swap Red and Green
    GBR, // Cycle Right
    BRG  // Cycle Left
}

public class SwizzleBlock : IBlock
{
    #region Fields

    private readonly IReadOnlyList<Socket> _inputs = [new("Swizzle.In", "Image.In")];
    private readonly IReadOnlyList<Socket> _outputs = [new("Swizzle.Out", "Image.Out")];

    private bool _disposed;

    // Configuration fields
    private SwizzleMode _mode = SwizzleMode.RGB;

    // Layout fields
    private double _x;
    private double _y;
    private int _width;
    private int _height;
    private string _title = "Swizzle";

    #endregion

    public SwizzleBlock()
        : this(200, 100)
    {
    }

    public SwizzleBlock(int width, int height)
    {
        _width = width;
        _height = height;
    }

    #region IBlock basic

    [Browsable(false)]
    public string Name => "Swizzle";

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
    [Description("Selects the channel permutation mode (e.g., BGR swaps Red and Blue).")]
    public SwizzleMode Mode
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

            if (Mode != SwizzleMode.RGB)
            {
                var matrix = GetMatrixForMode(Mode);
                sourceItem.Image.Mutate(x => x.Filter(matrix));
            }

            outputItems.Add(sourceItem);
        }

        return new Dictionary<Socket, IReadOnlyList<IBasicWorkItem>>
            {
                { _outputs[0], outputItems }
            };
    }

    private static ColorMatrix GetMatrixForMode(SwizzleMode mode)
    {
        return mode switch
        {
            SwizzleMode.BGR => new ColorMatrix
            {
                M31 = 1,
                M22 = 1,
                M13 = 1,
                M44 = 1
            },
            SwizzleMode.RBG => new ColorMatrix
            {
                M11 = 1,
                M32 = 1,
                M23 = 1,
                M44 = 1
            },
            SwizzleMode.GRB => new ColorMatrix
            {
                M21 = 1,
                M12 = 1,
                M33 = 1,
                M44 = 1
            },
            SwizzleMode.GBR => new ColorMatrix
            {
                M21 = 1,
                M32 = 1,
                M13 = 1,
                M44 = 1
            },

            SwizzleMode.BRG => new ColorMatrix
            {
                M31 = 1,
                M12 = 1,
                M23 = 1,
                M44 = 1
            },
            _ => new ColorMatrix
            {
                M11 = 1,
                M22 = 1,
                M33 = 1,
                M44 = 1
            }
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