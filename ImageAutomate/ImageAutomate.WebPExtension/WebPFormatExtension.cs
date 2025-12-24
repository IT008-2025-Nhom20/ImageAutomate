using System.Collections.Immutable;
using System.ComponentModel;
using ImageAutomate.Core;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;

namespace ImageAutomate.WebPExtension;

/// <summary>
/// WebP Format Extension Block for converting images to WebP format.
/// This block can be used as a plugin to provide WebP encoding capabilities.
/// </summary>
public class WebPFormatExtension : IBlock, IPluginUnloadable
{
    #region Fields

    private readonly IReadOnlyList<Socket> _inputs = [new("WebP.In", "Image.In")];
    private readonly IReadOnlyList<Socket> _outputs = [new("WebP.Out", "Image.Out")];

    private WebPOptions _options = new WebPOptions();
    private bool _disposed = false;

    private int _width = 200;
    private int _height = 100;

    #endregion

    #region Constructor

    public WebPFormatExtension()
    {
        _options.PropertyChanged += Options_OnPropertyChanged;
    }

    #endregion

    #region Basic Properties

    public string Name => "WebPFormatExtension";

    public string Title => "WebP Format";

    public string Content
    {
        get
        {
            var optionsSummary = Options.Lossless
                ? $"Mode: Lossless\nNear Lossless: {Options.NearLossless}"
                : $"Mode: Lossy\nQuality: {Options.Quality}\nMethod: {Options.Method}";

            return $"WebP Encoder\n{optionsSummary}";
        }
    }

    #endregion

    #region Layout

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

    #region Sockets

    public IReadOnlyList<Socket> Inputs => _inputs;
    public IReadOnlyList<Socket> Outputs => _outputs;

    #endregion

    #region Configuration

    [Category("Configuration")]
    [Description("WebP encoding options")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public WebPOptions Options
    {
        get => _options;
        set
        {
            if (_options != null)
                _options.PropertyChanged -= Options_OnPropertyChanged;

            _options = value ?? new WebPOptions();

            if (_options != null)
                _options.PropertyChanged += Options_OnPropertyChanged;

            OnPropertyChanged(nameof(Options));
        }
    }

    #endregion

    #region INotifyPropertyChanged

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void Options_OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        OnPropertyChanged(nameof(Options));
    }

    #endregion

    #region Execute

    public IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> Execute(
        IDictionary<Socket, IReadOnlyList<IBasicWorkItem>> inputs)
    {
        return Execute(inputs, CancellationToken.None);
    }

    public IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> Execute(
        IDictionary<Socket, IReadOnlyList<IBasicWorkItem>> inputs, 
        CancellationToken cancellationToken)
    {
        return Execute(
            inputs.ToDictionary(kvp => kvp.Key.Id, kvp => kvp.Value),
            cancellationToken
        );
    }

    public IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> Execute(
        IDictionary<string, IReadOnlyList<IBasicWorkItem>> inputs)
    {
        return Execute(inputs, CancellationToken.None);
    }

    public IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> Execute(
        IDictionary<string, IReadOnlyList<IBasicWorkItem>> inputs, 
        CancellationToken cancellationToken)
    {
        if (!inputs.TryGetValue(_inputs[0].Id, out var inItems))
            throw new ArgumentException(
                $"Input items not found for the expected input socket {_inputs[0].Id}.", 
                nameof(inputs));

        var outputItems = new List<IBasicWorkItem>();

        foreach (WorkItem sourceItem in inItems.OfType<WorkItem>())
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Create metadata with WebP format information
            IImmutableDictionary<string, object> metadata = sourceItem.Metadata;
            metadata = metadata.SetItem("Format", "WebP");
            metadata = metadata.SetItem("EncodingOptions", CreateEncoder());
            metadata = metadata.SetItem("WebPOptions", Options);

            outputItems.Add(new WorkItem(sourceItem.Image, metadata));
        }

        return new Dictionary<Socket, IReadOnlyList<IBasicWorkItem>> 
        { 
            { _outputs[0], outputItems } 
        };
    }

    #endregion

    #region WebP Encoding

    /// <summary>
    /// Creates a WebP encoder based on the current options.
    /// </summary>
    private WebpEncoder CreateEncoder()
    {
        var encoder = new WebpEncoder
        {
            Quality = (int)Options.Quality,
            Method = (WebpEncodingMethod)(int)Options.Method,
            FileFormat = Options.Lossless 
                ? WebpFileFormatType.Lossless 
                : WebpFileFormatType.Lossy,
            NearLossless = Options.NearLossless < 100,
            NearLosslessQuality = Options.NearLossless
        };

        return encoder;
    }

    #endregion

    #region IDisposable

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Unsubscribe from events
                if (_options != null)
                    _options.PropertyChanged -= Options_OnPropertyChanged;
            }

            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion

    #region IPluginUnloadable

    public bool OnUnloadRequested()
    {
        // Allow unloading if not disposed
        return !_disposed;
    }

    #endregion
}
