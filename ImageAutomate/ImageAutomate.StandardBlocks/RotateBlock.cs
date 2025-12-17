using ImageAutomate.Core;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageAutomate.StandardBlocks;

public class RotateBlock : IBlock
{
    #region Fields
    private readonly IReadOnlyList<Socket> _inputs = [new("Rotate.In", "Image.In")];
    private readonly IReadOnlyList<Socket> _outputs = [new("Rotate.Out", "Image.Out")];

    private bool disposedValue;

    private int _nodeWidth = 200;
    private int _nodeHeight = 100;

    private float _degrees = 0;
    #endregion

    #region IBlock basic
    public string Name => "Rotate";

    public string Title => "Rotate";

    public string Content => $"Rotate degree: {Degrees}";

    #endregion

    #region Layout
    [Category("Layout")]
    [Description("Height of the block node")]
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

    #region Socket
    public IReadOnlyList<Socket> Inputs => _inputs;

    public IReadOnlyList<Socket> Outputs => _outputs;
    #endregion

    #region Configuration
    [Category("Configuration")]
    [Description("Degree from 0 to 360")]
    public float Degrees
    {
        get => _degrees;
        set
        {
            if (_degrees != value)
            {
                _degrees = value;
                OnPropertyChanged(nameof(Degrees));
            }
        }
    }
    #endregion

    #region InotifyPropertyChanged
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
            throw new ArgumentException($"Input items not found for the expected input socket {_inputs[0].Id}");

        var outputItems = new List<IBasicWorkItem>();

        foreach (var sourceItem in inItems.OfType<WorkItem>())
        {
            sourceItem.Image.Mutate(x => x.Rotate(Degrees));
            outputItems.Add(sourceItem);
        }
        return new Dictionary<Socket, IReadOnlyList<IBasicWorkItem>>
        {
            {_outputs[0], outputItems }
        };
    }
    #endregion

    #region IDisposable
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    #endregion
}
