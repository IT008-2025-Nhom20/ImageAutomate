using ImageAutomate.Core;
using ImageAutomate.Infrastructure;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;

namespace ImageAutomate.StandardBlocks;

/// <summary>
/// A block that saves images to a directory.
/// </summary>
public class SaveBlock : IBlock, IShipmentSink
{
    #region Fields

    private readonly IReadOnlyList<Socket> _inputs = [new("Save.In", "Image.In")];
    private readonly IReadOnlyList<Socket> _outputs = [];

    private bool _disposed;

    private string _outputPath = string.Empty;
    private bool _overwrite = false;
    private bool _createDirectory = true;
    private bool _skipMetadata = false;

    // Layout fields
    private double _x;
    private double _y;
    private int _width;
    private int _height;
    private string _title = "Save";

    #endregion

    public SaveBlock()
        : this(200, 100)
    {
    }

    public SaveBlock(int width, int height)
    {
        _width = width;
        _height = height;
    }

    #region IBlock basic

    /// <inheritdoc />
    [Browsable(false)]
    public string Name => "Save";

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
    public string Content => $"Output path: {OutputPath}\nOverwrite: {Overwrite}\nCreate directory: {CreateDirectory}";

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

    /// <inheritdoc />
    [Browsable(false)]
    public IReadOnlyList<Socket> Inputs => _inputs;
    /// <inheritdoc />
    [Browsable(false)]
    public IReadOnlyList<Socket> Outputs => _outputs;

    #endregion

    #region Configuration

    /// <summary>
    /// Gets or sets the output directory path.
    /// </summary>
    [Category("Configuration")]
    [Description("Directory path for saving the processed images.")]
    [Editor("System.Windows.Forms.Design.FolderNameEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    public string OutputPath
    {
        get => _outputPath;
        set
        {
            if (!string.Equals(_outputPath, value, StringComparison.Ordinal))
            {
                _outputPath = value;
                OnPropertyChanged(nameof(OutputPath));
            }
        }
    }

    /// <summary>
    /// Gets or sets whether to overwrite existing files.
    /// </summary>
    [Category("Configuration")]
    [Description("If false, prevent overwrite when file already exists.")]
    public bool Overwrite
    {
        get => _overwrite;
        set
        {
            if (_overwrite != value)
            {
                _overwrite = value;
                OnPropertyChanged(nameof(Overwrite));
            }
        }
    }

    /// <summary>
    /// Gets or sets whether to create the output directory if it doesn't exist.
    /// </summary>
    [Category("Configuration")]
    [Description("If true, automatically creates directories for the OutputPath.")]
    public bool CreateDirectory
    {
        get => _createDirectory;
        set
        {
            if (_createDirectory != value)
            {
                _createDirectory = value;
                OnPropertyChanged(nameof(CreateDirectory));
            }
        }
    }

    /// <summary>
    /// Gets or sets whether to skip metadata when encoding images.
    /// </summary>
    [Category("Configuration")]
    [Description("If true, metadata (EXIF, XMP, etc.) will not be written to output files.")]
    public bool SkipMetadata
    {
        get => _skipMetadata;
        set
        {
            if (_skipMetadata != value)
            {
                _skipMetadata = value;
                OnPropertyChanged(nameof(SkipMetadata));
            }
        }
    }

    #endregion

    #region INotifyPropertyChanged

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Raises the PropertyChanged event.
    /// </summary>
    protected void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    #endregion

    #region Execute

    /// <inheritdoc />
    public IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> Execute(
        IDictionary<Socket, IReadOnlyList<IBasicWorkItem>> inputs)
    {
        return Execute(inputs, CancellationToken.None);
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> Execute(
        IDictionary<Socket, IReadOnlyList<IBasicWorkItem>> inputs, CancellationToken cancellationToken)
    {
        return Execute(inputs.ToDictionary(kvp => kvp.Key.Id, kvp => kvp.Value), cancellationToken);
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> Execute(
        IDictionary<string, IReadOnlyList<IBasicWorkItem>> inputs)
    {
        return Execute(inputs, CancellationToken.None);
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> Execute(
        IDictionary<string, IReadOnlyList<IBasicWorkItem>> inputs, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(inputs, nameof(inputs));
        if (!inputs.TryGetValue(_inputs[0].Id, out var inItems))
            throw new ArgumentException($"Input items not found for the expected input socket {_inputs[0].Id}.", nameof(inputs));

        var workItems = inItems.OfType<WorkItem>().ToList();

        var parallelOptions = new ParallelOptions
        {
            CancellationToken = cancellationToken,
            MaxDegreeOfParallelism = Math.Min(Environment.ProcessorCount, workItems.Count)
        };

        Parallel.ForEach(workItems, parallelOptions, workItem =>
        {
            SaveImage(workItem);
        });

        // Sink block; does not emit new WorkItem list
        return new Dictionary<Socket, IReadOnlyList<IBasicWorkItem>>();
    }

    #endregion

    #region Core saving logic

    private void SaveImage(WorkItem workItem)
    {
        if (string.IsNullOrWhiteSpace(OutputPath))
            throw new InvalidOperationException("SaveBlock: OutputPath is required.");

        // OutputPath is a directory
        var outputDirectory = OutputPath;

        // Create directory if needed
        if (!Directory.Exists(outputDirectory))
        {
            if (CreateDirectory)
            {
                Directory.CreateDirectory(outputDirectory);
            }
            else
            {
                throw new DirectoryNotFoundException(
                    $"SaveBlock: Directory '{outputDirectory}' does not exist.");
            }
        }

        // Get original filename from metadata (set by LoadBlock)
        if (!workItem.Metadata.TryGetValue("FileName", out var fileName) || string.IsNullOrWhiteSpace(fileName?.ToString()))
        {
            throw new InvalidOperationException("SaveBlock: WorkItem metadata does not contain 'FileName'.");
        }

        var finalFileName = fileName.ToString()!;

        // Step 1: If "Format" metadata exists (set by ConvertBlock), override the extension
        if (workItem.Metadata.TryGetValue("Format", out var formatObj)
            && formatObj is string formatStr
            && !string.IsNullOrEmpty(formatStr))
        {
            finalFileName = UpdateFileExtension(finalFileName, formatStr);
        }

        var path = Path.Combine(outputDirectory, finalFileName);

        if (File.Exists(path) && !Overwrite)
        {
            throw new IOException(
                $"SaveBlock: Output file '{path}' already exists.");
        }

        // Step 2 & 3: If "EncodingOptions" and "Format" exist, create encoder and save with it
        // Otherwise, let ImageSharp decide based on file extension
        if (workItem.Metadata.TryGetValue("EncodingOptions", out var encodingOptions) && encodingOptions != null
            && workItem.Metadata.TryGetValue("Format", out var formatObj2) && formatObj2 is string formatName)
        {
            var encoder = CreateEncoder(formatName, encodingOptions);
            workItem.Image.Save(path, encoder);
        }
        else
        {
            // No custom encoder needed - ImageSharp will detect format from file extension
            workItem.Image.Save(path);
        }
    }

    /// <summary>
    /// Updates the file extension based on the target format.
    /// </summary>
    /// <param name="fileName">The file name to update.</param>
    /// <param name="formatName">The format name (e.g., "JPEG", "PNG").</param>
    /// <returns>The file name with the updated extension.</returns>
    private static string UpdateFileExtension(string fileName, string formatName)
    {
        var strategy = ImageFormatRegistry.Instance.GetFormat(formatName);
        if (strategy == null)
        {
            return fileName; // Keep original if unknown
        }

        var nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
        return nameWithoutExt + strategy.FileExtension;
    }

    /// <summary>
    /// Creates an encoder for the specified format with the given options.
    /// </summary>
    /// <param name="formatName">The format name (e.g., "JPEG", "PNG").</param>
    /// <param name="encodingOptions">The encoding options.</param>
    /// <returns>An image encoder.</returns>
    /// <exception cref="InvalidOperationException">Thrown when format is not registered.</exception>
    private IImageEncoder CreateEncoder(string formatName, object encodingOptions)
    {
        var strategy = ImageFormatRegistry.Instance.GetFormat(formatName.ToUpper(CultureInfo.InvariantCulture))
            ?? throw new InvalidOperationException($"Unknown format: {formatName}");
        return strategy.CreateEncoder(encodingOptions, SkipMetadata);
    }

    #endregion

    #region IDisposable

    /// <summary>
    /// Releases unmanaged and - optionally - managed resources.
    /// </summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            _disposed = true;
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion
}
