using ImageAutomate.Core;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageAutomate.StandardBlocks;

public enum ColorBlindnessOptions
{
    Achromatomaly,
    Achromatopsia,
    Deuteranomaly,
    Deuteranopia,
    Protanomaly,
    Protanopia,
    Tritanomaly,
    Tritanopia
}

public class ColorBlindnessBlock : IBlock
{
    #region Field
    private readonly IReadOnlyList<Socket> _inputs = [new("ColorBlindness.In", "Image.In")];
    private readonly IReadOnlyList<Socket> _outputs = [new("ColorBlindness.Out", "Image.Out")];

    private int _nodeWidth = 200;
    private int _nodeHeight = 100;

    private ColorBlindnessOptions _options = ColorBlindnessOptions.Deuteranopia;
    
    private bool disposedValue;
    #endregion

    #region IBlock Basic
    public string Name => "ColorBlindness";

    public string Title => "ColorBlindness";

    public string Content => throw new NotImplementedException();
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

    #region Configuration
    public ColorBlindnessOptions Options
    {
        get => _options;
        set
        {
            if (_options != value)
            {
                _options = value;
                OnPropertyChanged(nameof(Options));
            }
        }
    }
    #endregion

    #region Socket
    public IReadOnlyList<Socket> Inputs => _inputs;

    public IReadOnlyList<Socket> Outputs => _outputs;
    #endregion

    #region InotifyPropertyChanged
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string PropertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
    #endregion

    #region Execute
    public IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> Execute(IDictionary<Socket, IReadOnlyList<IBasicWorkItem>> inputs)
    {
        return Execute(inputs.ToDictionary(kvp => kvp.Key.Id, kvp => kvp.Value));
    }

    public IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> Execute(IDictionary<string, IReadOnlyList<IBasicWorkItem>> inputs)
    {
        if (!inputs.TryGetValue(_inputs[0].Id, out var inItems))
            throw new ArgumentException($"Input items not found for the expected input socket{_inputs[0].Id}");
        var outputItems = new List<WorkItem>();

        ColorBlindnessMode colorOption = Options switch
        {
            ColorBlindnessOptions.Achromatomaly => ColorBlindnessMode.Achromatomaly,
            ColorBlindnessOptions.Achromatopsia => ColorBlindnessMode.Achromatopsia,
            ColorBlindnessOptions.Deuteranomaly => ColorBlindnessMode.Deuteranomaly,
            ColorBlindnessOptions.Deuteranopia => ColorBlindnessMode.Deuteranopia,
            ColorBlindnessOptions.Tritanomaly => ColorBlindnessMode.Tritanomaly,
            ColorBlindnessOptions.Tritanopia => ColorBlindnessMode.Tritanopia,
            ColorBlindnessOptions.Protanomaly => ColorBlindnessMode.Protanomaly,
            _ => ColorBlindnessMode.Protanopia
        };

        foreach (var sourceItem in inItems.OfType<WorkItem>())
        {
            sourceItem.Image.Mutate(x => x.ColorBlindness(colorOption));
            outputItems.Add(sourceItem);
        }
        return new Dictionary<Socket, IReadOnlyList<IBasicWorkItem>>
        {
            {_outputs[0], outputItems }
        };
    }
    #endregion

    #region Disposing
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
