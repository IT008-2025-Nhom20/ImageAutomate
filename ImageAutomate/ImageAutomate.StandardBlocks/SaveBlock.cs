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
        var targetFormat = ResolveTargetFormat(path, workItem);
        var encoder = CreateEncoder(targetFormat, workItem);
        using var fs = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.None);
        workItem.Image.Save(fs, encoder);
    }

    static private ImageFormat ResolveTargetFormat(string path, WorkItem workItem)
    {
        // 1) By Convert-stamped metadata
        if (workItem.Metadata.TryGetValue("Format", out var formatObj) && formatObj is string formatStr)
        {
            if (Enum.TryParse<ImageFormat>(formatStr, out var format))
            {
                return format;
            }
        }

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

    private IImageEncoder CreateEncoder(ImageFormat format, WorkItem workItem)
    {
        workItem.Metadata.TryGetValue("EncodingOptions", out var options);

        return format switch
        {
            ImageFormat.Jpeg => CreateJpegEncoder(options as JpegEncodingOptions),
            ImageFormat.Pbm => CreatePbmEncoder(options as PbmEncodingOptions),
            ImageFormat.Png => CreatePngEncoder(options as PngEncodingOptions),
            ImageFormat.Bmp => CreateBmpEncoder(options as BmpEncodingOptions),
            ImageFormat.Gif => CreateGifEncoder(options as GifEncodingOptions),
            ImageFormat.Tiff => CreateTiffEncoder(options as TiffEncodingOptions),
            ImageFormat.Tga => CreateTgaEncoder(options as TgaEncodingOptions),
            ImageFormat.WebP => CreateWebpEncoder(options as WebPEncodingOptions),
            ImageFormat.Qoi => CreateQoiEncoder(options as QoiEncodingOptions),
            _ => new PngEncoder()
        };
    }

    private static JpegEncoder CreateJpegEncoder(JpegEncodingOptions? opt)
    {
        if (opt == null) return new JpegEncoder();
        return new JpegEncoder
        {
            Quality = opt.Quality,
            ColorType = opt.ColorType.HasValue ? (SixLabors.ImageSharp.Formats.Jpeg.JpegEncodingColor)opt.ColorType.Value : null,
            Interleaved = opt.Interleaved
        };
    }

    private static PbmEncoder CreatePbmEncoder(PbmEncodingOptions? opt)
    {
        if (opt == null) return new PbmEncoder();
        return new PbmEncoder
        {
            ColorType = (SixLabors.ImageSharp.Formats.Pbm.PbmColorType)opt.ColorType,
            ComponentType = opt.ComponentType.HasValue ? (SixLabors.ImageSharp.Formats.Pbm.PbmComponentType)opt.ComponentType.Value : null,
            Encoding = (SixLabors.ImageSharp.Formats.Pbm.PbmEncoding)opt.Encoding
        };
    }

    private static PngEncoder CreatePngEncoder(PngEncodingOptions? opt)
    {
        if (opt == null) return new PngEncoder();
        return new PngEncoder
        {
            CompressionLevel = (SixLabors.ImageSharp.Formats.Png.PngCompressionLevel)opt.CompressionLevel,
            ColorType = opt.ColorType.HasValue ? (SixLabors.ImageSharp.Formats.Png.PngColorType)opt.ColorType.Value : null,
            BitDepth = opt.BitDepth.HasValue ? (SixLabors.ImageSharp.Formats.Png.PngBitDepth)opt.BitDepth.Value : null,
            InterlaceMethod = opt.InterlaceMethod.HasValue ? (SixLabors.ImageSharp.Formats.Png.PngInterlaceMode)opt.InterlaceMethod.Value : null,
            TransparentColorMode = (SixLabors.ImageSharp.Formats.Png.PngTransparentColorMode)opt.TransparentColorMode,
            Quantizer = opt.Quantizer.CreateQuantizer()
        };
    }

    private static BmpEncoder CreateBmpEncoder(BmpEncodingOptions? opt)
    {
        if (opt == null) return new BmpEncoder();
        return new BmpEncoder
        {
            BitsPerPixel = opt.BitsPerPixel.HasValue ? (SixLabors.ImageSharp.Formats.Bmp.BmpBitsPerPixel)opt.BitsPerPixel.Value : null,
            SupportTransparency = opt.SupportTransparency,
            Quantizer = opt.Quantizer.CreateQuantizer()
        };
    }

    private static GifEncoder CreateGifEncoder(GifEncodingOptions? opt)
    {
        if (opt == null) return new GifEncoder();
        return new GifEncoder
        {
            ColorTableMode = (SixLabors.ImageSharp.Formats.Gif.GifColorTableMode)opt.ColorTableMode,
            Quantizer = opt.Quantizer.CreateQuantizer()
        };
    }

    private static TiffEncoder CreateTiffEncoder(TiffEncodingOptions? opt)
    {
        if (opt == null) return new TiffEncoder();
        return new TiffEncoder
        {
            Compression = opt.Compression.HasValue ? (SixLabors.ImageSharp.Formats.Tiff.Constants.TiffCompression)opt.Compression.Value : null,
            BitsPerPixel = opt.BitsPerPixel.HasValue ? (SixLabors.ImageSharp.Formats.Tiff.TiffBitsPerPixel)opt.BitsPerPixel.Value : null,
            CompressionLevel = (SixLabors.ImageSharp.Compression.Zlib.DeflateCompressionLevel)opt.CompressionLevel,
            HorizontalPredictor = opt.HorizontalPredictor.HasValue ? (SixLabors.ImageSharp.Formats.Tiff.Constants.TiffPredictor)opt.HorizontalPredictor.Value : null,
            Quantizer = opt.Quantizer.CreateQuantizer()
        };
    }

    private static TgaEncoder CreateTgaEncoder(TgaEncodingOptions? opt)
    {
        if (opt == null) return new TgaEncoder();
        return new TgaEncoder
        {
            BitsPerPixel = opt.BitsPerPixel.HasValue ? (SixLabors.ImageSharp.Formats.Tga.TgaBitsPerPixel)opt.BitsPerPixel.Value : null,
            Compression = (SixLabors.ImageSharp.Formats.Tga.TgaCompression)opt.Compression
        };
    }

    private static WebpEncoder CreateWebpEncoder(WebPEncodingOptions? opt)
    {
        if (opt == null) return new WebpEncoder();
        return new WebpEncoder
        {
            FileFormat = (SixLabors.ImageSharp.Formats.Webp.WebpFileFormatType)opt.FileFormat,
            Quality = opt.Quality,
            Method = (SixLabors.ImageSharp.Formats.Webp.WebpEncodingMethod)opt.Method,
            NearLossless = opt.NearLossless,
            NearLosslessQuality = opt.NearLosslessQuality
        };
    }

    private static QoiEncoder CreateQoiEncoder(QoiEncodingOptions? opt)
    {
        if (opt == null) return new QoiEncoder();
        return new QoiEncoder
        {
            Channels = opt.Channels.HasValue ? (SixLabors.ImageSharp.Formats.Qoi.QoiChannels)opt.Channels.Value : null,
            ColorSpace = opt.ColorSpace.HasValue ? (SixLabors.ImageSharp.Formats.Qoi.QoiColorSpace)opt.ColorSpace.Value : null
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
