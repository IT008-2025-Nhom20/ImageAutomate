using ImageAutomate.Core;
using System.Collections.ObjectModel;
using System.ComponentModel;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Collections.Immutable;

namespace ImageAutomate.StandardBlocks;

/// <summary>
/// A block that loads images from a directory.
/// </summary>
public class LoadBlock : IBlock, IShipmentSource
{
    #region Fields

    private readonly IReadOnlyList<Socket> _inputs = [];
    private readonly IReadOnlyList<Socket> _outputs = [new("Load.out", "Image.out")];

    private string _sourcePath = string.Empty;
    private bool _autoOrient = false;
    private FileSortOrder _sortOrder = FileSortOrder.Lexicographic;

    private bool disposedValue = false;

    // Layout fields
    private double _x;
    private double _y;
    private int _width;
    private int _height;
    private string _title = "Load";

    #endregion

    public LoadBlock()
        : this(200, 100)
    {
    }

    public LoadBlock(int width, int height)
    {
        _width = width;
        _height = height;
    }

    #region InotifyPropertyChanged

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Raises the PropertyChanged event.
    /// </summary>
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion

    #region Basic Properties

    /// <inheritdoc />
    [Browsable(false)]
    public string Name => "Load";

    /// <inheritdoc />
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

    /// <inheritdoc />
    [Browsable(false)]
    public string Content => $"Path: {SourcePath}\nAuto Orient: {AutoOrient}";
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

    #region Configuration Propertise

    /// <summary>
    /// Gets or sets the source directory path.
    /// </summary>
    [Category("Configuration")]
    [Description("File system path to the input image ")]
    [Editor("System.Windows.Forms.Design.FolderNameEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
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

    /// <summary>
    /// Gets or sets whether to auto-orient images based on EXIF data.
    /// </summary>
    [Category("Configuration")]
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
    /// Gets or sets the sort order for file loading.
    /// </summary>
    [Category("Configuration")]
    [Description("Sort order for loading files: None (no sort), Lexicographic (standard string sort)")]
    public FileSortOrder SortOrder
    {
        get => _sortOrder;
        set
        {
            if (_sortOrder != value)
            {
                _sortOrder = value;
                OnPropertyChanged(nameof(SortOrder));
            }
        }
    }

    /// <summary>
    /// Maximum number of images to load per execution (shipment size).
    /// Set by the executor during initialization.
    /// </summary>
    [Browsable(false)]
    public int MaxShipmentSize { get; set; } = 64;

    /// <summary>
    /// Gets or sets transient data for the current shipment cycle.
    /// Contains the file paths to load in this batch.
    /// Set by ExecutionContext before Execute(), cleared after.
    /// </summary>
    [Browsable(false)]
    public IReadOnlyList<string>? ShipmentData { get; set; }

    private int _maxCount = int.MaxValue;

    /// <summary>
    /// Maximum total number of images to load from the source directory.
    /// Default is int.MaxValue (no limit). Set to a positive value to limit.
    /// </summary>
    [Category("Configuration")]
    [Description("Maximum total number of images to load. Default is unlimited.")]
    public int MaxCount
    {
        get => _maxCount;
        set
        {
            var clamped = Math.Max(1, value);
            if (_maxCount != clamped)
            {
                _maxCount = clamped;
                OnPropertyChanged(nameof(MaxCount));
            }
        }
    }

    #endregion

    #region Socket

    /// <inheritdoc />
    [Browsable(false)]
    public IReadOnlyList<Socket> Inputs => _inputs;
    /// <inheritdoc />
    [Browsable(false)]
    public IReadOnlyList<Socket> Outputs => _outputs;

    #endregion

    #region Execute

    /// <inheritdoc />
    public IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> Execute(IDictionary<Socket, IReadOnlyList<IBasicWorkItem>> inputs)
    {
        return Execute(inputs, CancellationToken.None);
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> Execute(IDictionary<string, IReadOnlyList<IBasicWorkItem>> inputs)
    {
        return Execute(inputs, CancellationToken.None);
    }

    /// <inheritdoc />
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
        if (ShipmentData == null)
            throw new InvalidOperationException("LoadBlock: ShipmentData not set by ExecutionContext.");

        if (string.IsNullOrWhiteSpace(SourcePath))
            throw new InvalidOperationException("LoadBlock: SourcePath is required.");

        // Load the batch of files provided by ExecutionContext
        for (int i = 0; i < ShipmentData.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string file = ShipmentData[i];
            
            Image image = LoadImageFile(file);
            var builder = ImmutableDictionary.CreateBuilder<string, object>();
            builder.Add("BatchFolder", SourcePath);
            builder.Add("FileName", Path.GetFileName(file));
            builder.Add("FullPath", file);
            builder.Add("Format", image.Metadata.DecodedImageFormat?.Name ?? "Unknown");
            builder.Add("ShipmentIndex", i);
            var metadata = builder.ToImmutable();
            WorkItem wi = new(image, metadata);
            yield return wi;
        }
    }

    /// <summary>
    /// Scans the source directory and returns all valid image file paths.
    /// </summary>
    public IReadOnlyList<string> GetShipmentTargets()
    {
        if (string.IsNullOrWhiteSpace(SourcePath))
            throw new InvalidOperationException("LoadBlock: SourcePath is required.");

        if (!Directory.Exists(SourcePath))
            throw new DirectoryNotFoundException($"LoadBlock: directory not found: {SourcePath}");

        // Get all valid image files
        var files = Directory.GetFiles(SourcePath)
            .Where(IsValidImageFile);

        // Apply user's sort preference
        files = SortOrder switch
        {
            FileSortOrder.None => files,
            FileSortOrder.Lexicographic => files.OrderBy(f => f, StringComparer.OrdinalIgnoreCase),
            // FileSortOrder.Natural => files.OrderBy(f => f, new NaturalStringComparer()),
            _ => files
        };

        // Take up to MaxCount and return as list
        return files.Take(MaxCount).ToList();
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

    /// <summary>
    /// Releases unmanaged and - optionally - managed resources.
    /// </summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            disposedValue = true;
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion
}
