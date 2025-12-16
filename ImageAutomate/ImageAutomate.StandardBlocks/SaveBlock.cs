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
using SixLabors.ImageSharp.PixelFormats;
using System.ComponentModel;

namespace ImageAutomate.StandardBlocks;

public class SaveBlock : IBlock
{
    #region Fields

    private readonly Socket _inputSocket = new("Save.In", "Image.In");
    private readonly IReadOnlyList<Socket> _inputs;
    private readonly IReadOnlyList<Socket> _outputs;

    private bool _disposed;

    private string _title = "Save";
    private string _content = "Save image";

    private int _nodeWidth = 200;
    private int _nodeHeight = 100;

    // Configuration
    private string _outputPath = string.Empty;
    private bool _overwrite = false;
    private bool _createDirectory = true;
    private ImageFormat? _encoderFormatOverride = null;

    private JpegEncodingOptions _jpegOptions = new();
    private PngEncodingOptions _pngOptions = new();
    private WebPEncodingOptions _webpOptions = new();
    private TiffEncodingOptions _tiffOptions = new();

    #endregion

    #region Ctor

    public SaveBlock()
    {
        _inputs = new[] { _inputSocket };
        _outputs = Array.Empty<Socket>();

        _jpegOptions.PropertyChanged += Options_OnPropertyChanged;
        _pngOptions.PropertyChanged += Options_OnPropertyChanged;
        _webpOptions.PropertyChanged += Options_OnPropertyChanged;
        _tiffOptions.PropertyChanged += Options_OnPropertyChanged;
    }

    #endregion

    #region IBlock basic

    public string Name => "Save";

    public string Title
    {
        get => _title;
    }

    public string Content
    {
        get => $"Output path: {OutputPath}\nOverwrite: {Overwrite}\nCreate directory: {CreateDirectory}\nTarget format: {EncoderFormat}";
    }

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

    #region Sockets

    public IReadOnlyList<Socket> Inputs => _inputs;
    public IReadOnlyList<Socket> Outputs => _outputs; // sink block: không output

    #endregion

    #region Configuration

    [Category("Configuration")]
    [Description("File path for saving the processed image.")]
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

    [Category("Configuration")]
    [Description("Optional encoder format override. If null, format is inferred from file extension or metadata.")]
    public ImageFormat? EncoderFormat
    {
        get => _encoderFormatOverride;
        set
        {
            if (_encoderFormatOverride != value)
            {
                _encoderFormatOverride = value;
                OnPropertyChanged(nameof(EncoderFormat));
            }
        }
    }

    #endregion

    #region Encoder options

    [Category("Encoding Options")]
    [Description("JPEG encoding parameters")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public JpegEncodingOptions JpegOptions
    {
        get => _jpegOptions;
        set
        {
            if (_jpegOptions != null)
                _jpegOptions.PropertyChanged -= Options_OnPropertyChanged;

            _jpegOptions = value ?? new JpegEncodingOptions();

            _jpegOptions.PropertyChanged += Options_OnPropertyChanged;
            OnPropertyChanged(nameof(JpegOptions));
        }
    }

    [Category("Encoding Options")]
    [Description("PNG encoding parameters")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public PngEncodingOptions PngOptions
    {
        get => _pngOptions;
        set
        {
            if (_pngOptions != null)
                _pngOptions.PropertyChanged -= Options_OnPropertyChanged;

            _pngOptions = value ?? new PngEncodingOptions();
            _pngOptions.PropertyChanged += Options_OnPropertyChanged;
            OnPropertyChanged(nameof(PngOptions));
        }
    }

    [Category("Encoding Options")]
    [Description("WebP encoding parameters")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public WebPEncodingOptions WebPOptions
    {
        get => _webpOptions;
        set
        {
            if (_webpOptions != null)
                _webpOptions.PropertyChanged -= Options_OnPropertyChanged;

            _webpOptions = value ?? new WebPEncodingOptions();
            _webpOptions.PropertyChanged += Options_OnPropertyChanged;
            OnPropertyChanged(nameof(WebPOptions));
        }
    }

    [Category("Encoding Options")]
    [Description("TIFF encoding parameters")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public TiffEncodingOptions TiffOptions
    {
        get => _tiffOptions;
        set
        {
            if (_tiffOptions != null)
                _tiffOptions.PropertyChanged -= Options_OnPropertyChanged;

            _tiffOptions = value ?? new TiffEncodingOptions();
            _tiffOptions.PropertyChanged += Options_OnPropertyChanged;
            OnPropertyChanged(nameof(TiffOptions));
        }
    }

    #endregion

    #region INotifyPropertyChanged

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private void Options_OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is JpegEncodingOptions)
            OnPropertyChanged(nameof(JpegOptions));
        else if (sender is PngEncodingOptions)
            OnPropertyChanged(nameof(PngOptions));
        else if (sender is WebPEncodingOptions)
            OnPropertyChanged(nameof(WebPOptions));
        else if (sender is TiffEncodingOptions)
            OnPropertyChanged(nameof(TiffOptions));
    }

    #endregion

    #region Execute (Socket keyed)

    public IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> Execute(
        IDictionary<Socket, IReadOnlyList<IBasicWorkItem>> inputs)
    {
        if (inputs is null) throw new ArgumentNullException(nameof(inputs));

        inputs.TryGetValue(_inputSocket, out var inItems);
        inItems ??= Array.Empty<IBasicWorkItem>();

        foreach (var item in inItems)
        {
            SaveWorkItem(item);
        }

        // Sink block: không phát tiếp work item
        return new Dictionary<Socket, IReadOnlyList<IBasicWorkItem>>();
    }

    #endregion

    #region Execute (string keyed)

    public IReadOnlyDictionary<string, IReadOnlyList<IBasicWorkItem>> Execute(
        IDictionary<string, IReadOnlyList<IBasicWorkItem>> inputs)
    {
        if (inputs is null) throw new ArgumentNullException(nameof(inputs));

        inputs.TryGetValue(_inputSocket.Id, out var inItems);
        inItems ??= Array.Empty<IBasicWorkItem>();

        foreach (var item in inItems)
        {
            SaveWorkItem(item);
        }

        return new Dictionary<string, IReadOnlyList<IBasicWorkItem>>();
    }

    #endregion

    #region Core saving logic

    private void SaveWorkItem(IBasicWorkItem item)
    {
        if (item is null)
            throw new ArgumentNullException(nameof(item));

        if (string.IsNullOrWhiteSpace(OutputPath))
            throw new InvalidOperationException("SaveBlock: OutputPath is required.");

        if (!item.Metadata.TryGetValue("ImageData", out var dataObj) ||
            dataObj is not byte[] imageBytes ||
            imageBytes.Length == 0)
        {
            throw new InvalidOperationException(
                $"SaveBlock: Work item '{item.Id}' does not contain valid ImageData.");
        }

        // Resolve final path (for now: dùng OutputPath trực tiếp)
        var path = OutputPath;

        // Directory handling
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir))
        {
            if (!Directory.Exists(dir))
            {
                if (CreateDirectory)
                {
                    Directory.CreateDirectory(dir);
                }
                else
                {
                    throw new DirectoryNotFoundException(
                        $"SaveBlock: Directory '{dir}' does not exist and CreateDirectory = false.");
                }
            }
        }

        // Overwrite handling
        if (File.Exists(path) && !Overwrite)
        {
            throw new IOException(
                $"SaveBlock: Output file '{path}' already exists and Overwrite = false.");
        }

        // Determine target format & encoder
        var targetFormat = ResolveTargetFormat(path, item);
        var encoder = CreateEncoder(targetFormat);

        // Decode & re-encode với encoder đã chọn
        using var image = Image.Load<Rgba32>(imageBytes);
        using var fs = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.None);
        image.Save(fs, encoder);

        // Update metadata
        item.Metadata["SavedPath"] = path;
        item.Metadata["SavedAtUtc"] = DateTime.UtcNow;
        item.Metadata["Format"] = targetFormat;
    }

    private ImageFormat ResolveTargetFormat(string path, IBasicWorkItem item)
    {
        // 1) Ưu tiên EncoderFormat override
        if (EncoderFormat.HasValue)
            return EncoderFormat.Value;

        // 2) Theo extension file
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

        // 3) Theo metadata Format nếu có
        if (item.Metadata.TryGetValue("Format", out var fmtObj) && fmtObj is ImageFormat fmtEnum)
            return fmtEnum;

        // 4) Fallback: PNG
        return ImageFormat.Png;
    }

    private IImageEncoder CreateEncoder(ImageFormat format)
    {
        return format switch
        {
            ImageFormat.Jpeg => new JpegEncoder
            {
                Quality = JpegOptions.Quality
            },

            ImageFormat.Png => new PngEncoder
            {
                CompressionLevel =
                    (SixLabors.ImageSharp.Formats.Png.PngCompressionLevel)(int)PngOptions.CompressionLevel
            },

            ImageFormat.WebP => new WebpEncoder
            {
                Quality = (int)Math.Clamp(WebPOptions.Quality, 0f, 100f),
                FileFormat = WebPOptions.Lossless
                    ? WebpFileFormatType.Lossless
                    : WebpFileFormatType.Lossy
            },

            ImageFormat.Tiff => new TiffEncoder
            {
                Compression =
                        (TiffOptions.Compression == TiffCompression.None) ? SixLabors.ImageSharp.Formats.Tiff.Constants.TiffCompression.None :
                        (TiffOptions.Compression == TiffCompression.Lzw) ? SixLabors.ImageSharp.Formats.Tiff.Constants.TiffCompression.Lzw :
                        (TiffOptions.Compression == TiffCompression.Ccitt4) ? SixLabors.ImageSharp.Formats.Tiff.Constants.TiffCompression.CcittGroup4Fax :
                        (TiffOptions.Compression == TiffCompression.Rle) ? SixLabors.ImageSharp.Formats.Tiff.Constants.TiffCompression.PackBits :
                        SixLabors.ImageSharp.Formats.Tiff.Constants.TiffCompression.Deflate
            },

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
