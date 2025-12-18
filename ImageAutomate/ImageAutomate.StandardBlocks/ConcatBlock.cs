using ImageAutomate.Core;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageAutomate.StandardBlocks;

public enum ConcatMode
{
    Vertical,
    Horizontal
}
public class ConcatBlock
{
    #region Fields
    private readonly IReadOnlyList<Socket> _inputs = [new("FirstImage.In", "Image1.In"), new("SecondImage.In", "Image2.In")];
    private readonly IReadOnlyList<Socket> _outputs = [new("ConCat.Out", "Image.Out")];

    private bool disposedValue;

    private int _nodeWidth = 200;
    private int _nodeHeight = 100;

    private ConcatMode _concatOption = ConcatMode.Horizontal;
    #endregion

    #region IBlock basic
    public string Name => "Concatenate";

    public string Title => "Concatenate";

    public string Content => $"Concatenate mode: {ConcatOption}";

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
    [Description("Concatinating 2 images horizontally or vertically")]
    public ConcatMode ConcatOption
    {
        get => _concatOption;
        set
        {
            if (_concatOption != value)
            {
                _concatOption = value;
                OnPropertyChanged(nameof(ConcatOption));
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
        if (!inputs.TryGetValue(_inputs[0].Id, out var FirstInItems))
            throw new ArgumentException($"Input items not found for the expected input socket {_inputs[0].Id}");

        if (!inputs.TryGetValue(_inputs[1].Id, out var SecondInItems))
            throw new ArgumentException($"Input items not found for the expected input socket {_inputs[1].Id}");
       
        var outputItems = new List<IBasicWorkItem>();

        var workItemList1 = FirstInItems.OfType<WorkItem>();
        var workItemList2 = SecondInItems.OfType<WorkItem>();
        foreach (var (firstItem, secondItem) in workItemList1.Zip(workItemList2))
        {
            Image<Rgba32> concatenated;
            if(ConcatOption == ConcatMode.Horizontal)
            {
                var imageWidth = firstItem.Image.Width + secondItem.Image.Width;
                var imageHeight = Math.Max(firstItem.Image.Height, secondItem.Image.Height);
                
                concatenated = new Image<Rgba32>(imageWidth, imageHeight);
                concatenated.Mutate(x => x.DrawImage(firstItem.Image, new Point(0, 0), 1));
                concatenated.Mutate(x => x.DrawImage(secondItem.Image, new Point(firstItem.Image.Width, 0), 1));
            }
            else
            {
                var imageHeight = firstItem.Image.Height + secondItem.Image.Height;
                var imageWidth = Math.Max(firstItem.Image.Width, secondItem.Image.Width);
                
                concatenated = new Image<Rgba32>(imageWidth, imageHeight);
                concatenated.Mutate(x => x.DrawImage(firstItem.Image, new Point(0, 0), 1));
                concatenated.Mutate(x => x.DrawImage(secondItem.Image, new Point(0, firstItem.Image.Height), 1));
            }

            var concatItem = new WorkItem(concatenated);
            outputItems.Add(concatItem);
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
