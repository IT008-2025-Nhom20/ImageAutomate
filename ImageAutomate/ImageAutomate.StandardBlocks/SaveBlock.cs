using ImageAutomate.Core;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Pbm;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Qoi;
using SixLabors.ImageSharp.Formats.Tga;
using SixLabors.ImageSharp.Formats.Tiff;
using SixLabors.ImageSharp.Formats.Webp;
using System.ComponentModel;

namespace ImageAutomate.StandardBlocks;

public class SaveBlock : IBlock, IShipmentSink
{
    #region Fields

    private readonly IReadOnlyList<Socket> _inputs = [new("Save.In", "Image.In")];
    private readonly IReadOnlyList<Socket> _outputs = [];

    private bool _disposed;

    private string _outputPath = string.Empty;
    private bool _overwrite = false;
    private bool _createDirectory = true;

    #endregion

    #region IBlock basic

    public string Name => "Save";

    public string Title => "Save";

    public string Content => $"Output path: {OutputPath}\nOverwrite: {Overwrite}\nCreate directory: {CreateDirectory}";

    #endregion

    #region Sockets

    public IReadOnlyList<Socket> Inputs => _inputs;
    public IReadOnlyList<Socket> Outputs => _outputs; // sink block: không output

    #endregion

    #region Configuration

    [Category("Configuration")]
    [Description("Directory path for saving the processed images.")]
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
        if (!inputs.TryGetValue(_inputs[0].Id, out var inItems))
            throw new ArgumentException($"Input items not found for the expected input socket {_inputs[0].Id}.", nameof(inputs));

        foreach (var workItem in inItems.OfType<WorkItem>())
        {
            cancellationToken.ThrowIfCancellationRequested();
            SaveImage(workItem);
        }

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

        var path = Path.Combine(outputDirectory, fileName.ToString()!);

        if (File.Exists(path) && !Overwrite)
        {
            throw new IOException(
                $"SaveBlock: Output file '{path}' already exists.");
        }

        // Determine target format & encoder
        var targetFormat = ResolveTargetFormat(path, workItem.Image);
        var encoder = CreateEncoder(targetFormat);
        using var fs = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.None);
        workItem.Image.Save(fs, encoder);
    }

    static private ImageFormat ResolveTargetFormat(string path, Image image)
    {
        // 1) By Convert-stamped metadata
        // TODO: Implement this
        // 2) By original file extension
        // TODO: Check correctness
        var ext = Path.GetExtension(path)?.ToLowerInvariant();
        if (!string.IsNullOrEmpty(ext))
        {
            return ext switch
            {
                ".jpg" or ".jpeg" => ImageFormat.Jpeg,
                ".png" => ImageFormat.Png,
                ".gif" => ImageFormat.Gif,
                ".bmp" => ImageFormat.Bmp,
                ".pbm" => ImageFormat.Pbm,
                ".tif" or ".tiff" => ImageFormat.Tiff,
                ".tga" => ImageFormat.Tga,
                ".webp" => ImageFormat.WebP,
                ".qoi" => ImageFormat.Qoi,
                _ => ImageFormat.Png
            };
        }
        // 3) Fallback: PNG
        return ImageFormat.Png;
    }

    private IImageEncoder CreateEncoder(ImageFormat format)
    {
        // TODO: Property propagate Metadata encoding intents (currenly only ConvertBlock uses this)
        return format switch
        {
            ImageFormat.Jpeg => new JpegEncoder(),
            ImageFormat.Png => new PngEncoder(),
            ImageFormat.WebP => new WebpEncoder(),
            ImageFormat.Tiff => new TiffEncoder(),
            ImageFormat.Bmp => new BmpEncoder(),
            ImageFormat.Gif => new GifEncoder(),
            ImageFormat.Pbm => new PbmEncoder(),
            ImageFormat.Tga => new TgaEncoder(),
            ImageFormat.Qoi => new QoiEncoder(),
            _ => new PngEncoder()
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
