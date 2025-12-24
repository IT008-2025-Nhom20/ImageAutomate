using ImageAutomate.Core;
using System.Collections.ObjectModel;
using System.ComponentModel;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Collections.Immutable;

namespace ImageAutomate.StandardBlocks;

public class LoadBlock : IBlock, IShipmentSource
{
    #region Fields

    private readonly IReadOnlyList<Socket> _inputs = [];
    private readonly IReadOnlyList<Socket> _outputs = [new("Load.out", "Image.out")];

    private string _sourcePath = string.Empty;
    private bool _autoOrient = false;

    private bool disposedValue = false;

    // Shipment state
    private List<string>? _cachedFilePaths;
    private int _currentOffset = 0;

    #endregion

    #region InotifyPropertyChanged

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion

    #region Basic Properties

    public string Name => "Load";

    public string Title => "Load";

    public string Content => $"Path: {SourcePath}\nAuto Orient: {AutoOrient}";
    #endregion

    #region Configuration Propertise
    [Category("Configuration")]
    [Description("File system path to the input image ")]
    public string SourcePath
    {
        get => _sourcePath;
        set
        {
            if (_sourcePath != value)
            {
                _sourcePath = value;
                OnPropertyChanged(nameof(SourcePath));
            }
        }
    }

    [Category("Confiuration")]
    [Description("If true, applies EXIF orientation correction automatically")]
    public bool AutoOrient
    {
        get => _autoOrient;
        set
        {
            if (_autoOrient != value)
            {
                _autoOrient = value;
                OnPropertyChanged(nameof(AutoOrient));
            }
        }
    }

    /// <summary>
    /// Maximum number of images to load per execution (shipment size).
    /// Set by the executor during initialization.
    /// </summary>
    public int MaxShipmentSize { get; set; } = 64;

    #endregion

    #region Socket
    public IReadOnlyList<Socket> Inputs => _inputs;

    public IReadOnlyList<Socket> Outputs => _outputs;
    #endregion

    #region Execute

    public IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> Execute(IDictionary<Socket, IReadOnlyList<IBasicWorkItem>> inputs)
    {
        return Execute(inputs, CancellationToken.None);
    }

    public IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> Execute(IDictionary<Socket, IReadOnlyList<IBasicWorkItem>> inputs, CancellationToken cancellationToken)
    {
        var items = LoadWorkItems(cancellationToken);

        var list = new List<IBasicWorkItem>(items);

        var readOnly = new ReadOnlyCollection<IBasicWorkItem>(list);

        return new Dictionary<Socket, IReadOnlyList<IBasicWorkItem>>
            {
                { _outputs[0], readOnly }
            };
    }

    public IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> Execute(IDictionary<string, IReadOnlyList<IBasicWorkItem>> inputs)
    {
        return Execute(inputs, CancellationToken.None);
    }

    public IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> Execute(IDictionary<string, IReadOnlyList<IBasicWorkItem>> inputs, CancellationToken cancellationToken)
    {
        var items = LoadWorkItems(cancellationToken);

        var list = new List<IBasicWorkItem>(items);

        var readOnly = new ReadOnlyCollection<IBasicWorkItem>(list);

        return new Dictionary<Socket, IReadOnlyList<IBasicWorkItem>>
            {
                { _outputs[0], readOnly }
            };
    }

    #endregion

    private IEnumerable<IBasicWorkItem> LoadWorkItems(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(SourcePath))
            throw new InvalidOperationException("LoadBlock: SourcePath is required.");

        // Initialize file list on first execution
        if (_cachedFilePaths == null)
        {
            if (!Directory.Exists(SourcePath))
                throw new DirectoryNotFoundException($"LoadBlock: directory not found: {SourcePath}");

            _cachedFilePaths = Directory.GetFiles(SourcePath)
                .Where(IsValidImageFile)
                .OrderBy(file => file, StringComparer.OrdinalIgnoreCase)
                .ToList();
            
            _currentOffset = 0;
        }

        // Load next shipment batch
        int remaining = _cachedFilePaths.Count - _currentOffset;
        int batchSize = Math.Min(MaxShipmentSize, remaining);

        for (int i = 0; i < batchSize; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string file = _cachedFilePaths[_currentOffset + i];
            
            Image image = LoadImageFile(file);
            var builder = ImmutableDictionary.CreateBuilder<string, object>();
            builder.Add("BatchFolder", SourcePath);
            builder.Add("FileName", Path.GetFileName(file));
            builder.Add("FullPath", file);
            builder.Add("Format", image.Metadata.DecodedImageFormat?.Name ?? "Unknown");
            builder.Add("ShipmentOffset", _currentOffset);
            builder.Add("ShipmentIndex", i);
            var metadata = builder.ToImmutable();
            WorkItem wi = new(image, metadata);
            yield return wi;
        }

        // Advance offset for next shipment
        _currentOffset += batchSize;
    }

    private bool IsValidImageFile(string path)
    {
        try
        {
            var info = SixLabors.ImageSharp.Image.Identify(path);
            return info != null;
        }
        catch (Exception ex) when (ex is NotSupportedException
                                   || ex is InvalidImageContentException
                                   || ex is UnknownImageFormatException
                                   || ex is ArgumentNullException)
        {
            return false;
        }
    }

    private SixLabors.ImageSharp.Image LoadImageFile(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"LoadBlock: File not found at path '{path}'.", path);

        try
        {
            var image = SixLabors.ImageSharp.Image.Load(path);

            if (AutoOrient)
                image.Mutate(x => x.AutoOrient());

            return image;
        }
        catch (Exception ex) when (ex is IOException
                                   || ex is UnauthorizedAccessException
                                   || ex is UnknownImageFormatException
                                   || ex is InvalidImageContentException)
        {
            throw new InvalidOperationException(
                $"LoadBlock: Failed to load image from file '{path}': {ex.Message}", ex);
        }
    }

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
