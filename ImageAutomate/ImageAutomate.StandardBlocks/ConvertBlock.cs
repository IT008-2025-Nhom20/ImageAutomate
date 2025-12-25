using ImageAutomate.Core;
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
using SixLabors.ImageSharp.Processing.Processors.Dithering;
using SixLabors.ImageSharp.Processing.Processors.Quantization;
using System.Collections.Immutable;
using System.ComponentModel;

namespace ImageAutomate.StandardBlocks;

#region Format Enum

public enum ImageFormat
{
    Unknown = 0,
    Bmp,
    Gif,
    Jpeg,
    Pbm,
    Png,
    Tiff,
    Tga,
    WebP,
    Qoi
}

#endregion

#region Quantizer Enums and Options

public enum QuantizerType
{
    Octree = 0,
    Wu = 1,
    WebSafePalette = 2,
    WernerPalette = 3
}

[TypeConverter(typeof(ExpandableObjectConverter))]
public class QuantizerOptions : INotifyPropertyChanged
{
    private QuantizerType _type = QuantizerType.Octree;
    private int _maxColors = 256;
    private bool _dither = true;
    private float _ditherScale = 1.0f;

    public event PropertyChangedEventHandler? PropertyChanged;

    [Category("Quantizer")]
    [Description("Quantizer algorithm to use for color reduction")]
    [DefaultValue(QuantizerType.Octree)]
    public QuantizerType Type
    {
        get => _type;
        set { if (_type != value) { _type = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Type))); } }
    }

    [Category("Quantizer")]
    [Description("Maximum number of colors in the palette (2-256)")]
    [DefaultValue(256)]
    public int MaxColors
    {
        get => _maxColors;
        set
        {
            value = Math.Clamp(value, 2, 256);
            if (_maxColors != value) { _maxColors = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MaxColors))); }
        }
    }

    [Category("Quantizer")]
    [Description("Apply dithering to reduce color banding")]
    [DefaultValue(true)]
    public bool Dither
    {
        get => _dither;
        set { if (_dither != value) { _dither = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Dither))); } }
    }

    [Category("Quantizer")]
    [Description("Dithering intensity scale (0.0-1.0)")]
    [DefaultValue(1.0f)]
    public float DitherScale
    {
        get => _ditherScale;
        set
        {
            value = Math.Clamp(value, 0f, 1f);
            if (_ditherScale != value) { _ditherScale = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DitherScale))); }
        }
    }

    public IQuantizer CreateQuantizer()
    {
        IDither? ditherAlgorithm = Dither ? QuantizerConstants.DefaultDither : null;
        var opts = new SixLabors.ImageSharp.Processing.Processors.Quantization.QuantizerOptions
        {
            MaxColors = MaxColors,
            Dither = ditherAlgorithm,
            DitherScale = DitherScale
        };
        return Type switch
        {
            QuantizerType.Octree => new OctreeQuantizer(opts),
            QuantizerType.Wu => new WuQuantizer(opts),
            QuantizerType.WebSafePalette => new WebSafePaletteQuantizer(opts),
            QuantizerType.WernerPalette => new WernerPaletteQuantizer(opts),
            _ => new OctreeQuantizer()
        };
    }

    public override string ToString() => $"{Type}, {MaxColors} colors" + (Dither ? ", Dithered" : "");
}

#endregion

#region JPEG Enums

public enum JpegEncodingColor
{
    YCbCrRatio420 = 0,
    YCbCrRatio444 = 1,
    YCbCrRatio422 = 2,
    Luminance = 3,
    Rgb = 4
}

#endregion

#region PBM Enums

public enum PbmColorType
{
    BlackAndWhite = 0,
    Grayscale = 1,
    Rgb = 2
}

public enum PbmComponentType
{
    Byte = 0,
    Short = 1
}

public enum PbmEncoding
{
    Plain = 0,
    Binary = 1
}

#endregion

#region QOI Enums

public enum QoiChannels
{
    Rgb = 3,
    Rgba = 4
}

public enum QoiColorSpace
{
    SrgbWithLinearAlpha = 0,
    Linear = 1
}

#endregion

#region TGA Enums

public enum TgaBitsPerPixel
{
    Pixel8 = 8,
    Pixel16 = 16,
    Pixel24 = 24,
    Pixel32 = 32
}

public enum TgaCompression
{
    None = 0,
    RunLength = 1
}

#endregion

#region WebP Enums

public enum WebpFileFormatType
{
    Lossy = 0,
    Lossless = 1
}

public enum WebpEncodingMethod
{
    Fastest = 0,
    Level1 = 1,
    Level2 = 2,
    Level3 = 3,
    Default = 4,
    Level5 = 5,
    BestQuality = 6
}

public enum WebpTransparentColorMode
{
    Preserve = 0,
    Clear = 1
}

#endregion

#region BMP Enums

public enum BmpBitsPerPixel
{
    Pixel1 = 1,
    Pixel2 = 2,
    Pixel4 = 4,
    Pixel8 = 8,
    Pixel16 = 16,
    Pixel24 = 24,
    Pixel32 = 32
}

#endregion

#region GIF Enums

public enum GifColorTableMode
{
    Global = 0,
    Local = 1
}

#endregion

#region PNG Enums

public enum PngBitDepth
{
    Bit1 = 1,
    Bit2 = 2,
    Bit4 = 4,
    Bit8 = 8,
    Bit16 = 16
}

public enum PngColorType
{
    Grayscale = 0,
    Rgb = 2,
    Palette = 3,
    GrayscaleWithAlpha = 4,
    RgbWithAlpha = 6
}

public enum PngCompressionLevel
{
    NoCompression = 0,
    BestSpeed = 1,
    Level2 = 2,
    Level3 = 3,
    Level4 = 4,
    Level5 = 5,
    DefaultCompression = 6,
    Level7 = 7,
    Level8 = 8,
    BestCompression = 9
}

public enum PngFilterMethod
{
    None = 0,
    Sub = 1,
    Up = 2,
    Average = 3,
    Paeth = 4,
    Adaptive = 5
}

public enum PngInterlaceMethod
{
    None = 0,
    Adam7 = 1
}

public enum PngTransparentColorMode
{
    Preserve = 0,
    Clear = 1
}

#endregion

#region TIFF Enums

public enum TiffBitsPerPixel
{
    Bit1 = 1,
    Bit4 = 4,
    Bit8 = 8,
    Bit24 = 24,
    Bit32 = 32,
    Bit48 = 48,
    Bit64 = 64
}

public enum TiffCompression
{
    None = 0,
    Ccitt1D = 1,
    PackBits = 2,
    Deflate = 3,
    Lzw = 4,
    CcittGroup3Fax = 5,
    CcittGroup4Fax = 6,
    Jpeg = 7,
    Webp = 8
}

public enum DeflateCompressionLevel
{
    NoCompression = 0,
    BestSpeed = 1,
    Level2 = 2,
    Level3 = 3,
    Level4 = 4,
    Level5 = 5,
    DefaultCompression = 6,
    Level7 = 7,
    Level8 = 8,
    BestCompression = 9
}

public enum TiffPredictor
{
    None = 0,
    Horizontal = 1
}

public enum TiffPhotometricInterpretation
{
    WhiteIsZero = 0,
    BlackIsZero = 1,
    Rgb = 2,
    PaletteColor = 3,
    YCbCr = 4
}

#endregion

public class ConvertBlock : IBlock
{
    #region Fields
    private readonly IReadOnlyList<Socket> _inputs = [new("Convert.In", "Image.Input")];
    private readonly IReadOnlyList<Socket> _outputs = [new("Convert.Out", "Image.Out")];

    private ImageFormat _targetFormat = ImageFormat.Png;
    private bool _alwaysEncode = false;
    private bool disposedValue = false;

    private JpegEncodingOptions _jpegOptions = new();
    private PbmEncodingOptions _pbmOptions = new();
    private PngEncodingOptions _pngOptions = new();
    private BmpEncodingOptions _bmpOptions = new();
    private GifEncodingOptions _gifOptions = new();
    private TiffEncodingOptions _tiffOptions = new();
    private TgaEncodingOptions _tgaOptions = new();
    private WebPEncodingOptions _webpOptions = new();
    private QoiEncodingOptions _qoiOptions = new();

    #endregion

    public ConvertBlock()
    {
        _jpegOptions.PropertyChanged += Options_OnPropertyChanged;
        _pbmOptions.PropertyChanged += Options_OnPropertyChanged;
        _pngOptions.PropertyChanged += Options_OnPropertyChanged;
        _bmpOptions.PropertyChanged += Options_OnPropertyChanged;
        _gifOptions.PropertyChanged += Options_OnPropertyChanged;
        _tiffOptions.PropertyChanged += Options_OnPropertyChanged;
        _tgaOptions.PropertyChanged += Options_OnPropertyChanged;
        _webpOptions.PropertyChanged += Options_OnPropertyChanged;
        _qoiOptions.PropertyChanged += Options_OnPropertyChanged;
    }

    #region Basic Properties

    public string Name => "Convert";

    public string Title => "Convert";

    public string Content
    {
        get
        {
            var optionSummaries = TargetFormat switch
            {
                ImageFormat.Jpeg => $"Quality: {JpegOptions.Quality}",
                ImageFormat.Pbm => $"Encoding: {PbmOptions.Encoding}",
                ImageFormat.Png => $"Compression: {PngOptions.CompressionLevel}",
                ImageFormat.Bmp => BmpOptions.BitsPerPixel.HasValue ? $"{BmpOptions.BitsPerPixel}bpp" : "Auto",
                ImageFormat.Gif => $"{GifOptions.Quantizer.MaxColors} colors, {GifOptions.ColorTableMode}",
                ImageFormat.Tiff => $"Compression: {TiffOptions.Compression}",
                ImageFormat.Tga => $"Compression: {TgaOptions.Compression}",
                ImageFormat.WebP => WebPOptions.FileFormat == WebpFileFormatType.Lossless ? "Lossless" : $"Lossy Q{WebPOptions.Quality}",
                ImageFormat.Qoi => QoiOptions.Channels.HasValue ? $"{QoiOptions.Channels}" : "Auto",
                _ => "Options: Default"
            };
            return $"Format: {TargetFormat}\nRe-encode: {AlwaysEncode}\n{optionSummaries}";
        }
    }

    #endregion

    #region Sockets

    public IReadOnlyList<Socket> Inputs => _inputs;
    public IReadOnlyList<Socket> Outputs => _outputs;

    #endregion

    #region Configuration

    [Category("Configuration")]
    [Description("Target image format for conversion")]
    public ImageFormat TargetFormat
    {
        get => _targetFormat;
        set
        {
            if (_targetFormat != value)
            {
                _targetFormat = value;
                OnPropertyChanged(nameof(TargetFormat));
                OnPropertyChanged(nameof(JpegOptions));
                OnPropertyChanged(nameof(PbmOptions));
                OnPropertyChanged(nameof(PngOptions));
                OnPropertyChanged(nameof(BmpOptions));
                OnPropertyChanged(nameof(GifOptions));
                OnPropertyChanged(nameof(TiffOptions));
                OnPropertyChanged(nameof(TgaOptions));
                OnPropertyChanged(nameof(WebPOptions));
                OnPropertyChanged(nameof(QoiOptions));
            }
        }
    }

    [Category("Configuration")]
    [Description("Force re-encoding even when format matches")]
    public bool AlwaysEncode
    {
        get => _alwaysEncode;
        set
        {
            if (_alwaysEncode != value)
            {
                _alwaysEncode = value;
                OnPropertyChanged(nameof(AlwaysEncode));
            }
        }
    }

    #endregion

    #region Enconding Options Properties

    [Category("Encoding Options")]
    [Description("JPEG encoding parameters")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public JpegEncodingOptions JpegOptions
    {
        get => _jpegOptions;
        set
        {
            // unsubscribe from old options to avoid memory leaks
            if (_jpegOptions != null)
                _jpegOptions.PropertyChanged -= Options_OnPropertyChanged;
            _jpegOptions = value;
            // resubscribe after assignment
            if (_jpegOptions != null)
                _jpegOptions.PropertyChanged += Options_OnPropertyChanged;
            OnPropertyChanged(nameof(JpegOptions));
        }
    }

    [Category("Encoding Options")]
    [Description("PBM encoding parameters")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public PbmEncodingOptions PbmOptions
    {
        get => _pbmOptions;
        set
        {
            if (_pbmOptions != null)
                _pbmOptions.PropertyChanged -= Options_OnPropertyChanged;
            _pbmOptions = value;
            if (_pbmOptions != null)
                _pbmOptions.PropertyChanged += Options_OnPropertyChanged;
            OnPropertyChanged(nameof(PbmOptions));
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
            _pngOptions = value;
            if (_pngOptions != null)
                _pngOptions.PropertyChanged += Options_OnPropertyChanged;
            OnPropertyChanged(nameof(PngOptions));
        }
    }

    [Category("Encoding Options")]
    [Description("BMP encoding parameters")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public BmpEncodingOptions BmpOptions
    {
        get => _bmpOptions;
        set
        {
            if (_bmpOptions != null)
                _bmpOptions.PropertyChanged -= Options_OnPropertyChanged;
            _bmpOptions = value;
            if (_bmpOptions != null)
                _bmpOptions.PropertyChanged += Options_OnPropertyChanged;
            OnPropertyChanged(nameof(BmpOptions));
        }
    }

    [Category("Encoding Options")]
    [Description("GIF encoding parameters")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public GifEncodingOptions GifOptions
    {
        get => _gifOptions;
        set
        {
            if (_gifOptions != null)
                _gifOptions.PropertyChanged -= Options_OnPropertyChanged;
            _gifOptions = value;
            if (_gifOptions != null)
                _gifOptions.PropertyChanged += Options_OnPropertyChanged;
            OnPropertyChanged(nameof(GifOptions));
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
            _tiffOptions = value;
            if (_tiffOptions != null)
                _tiffOptions.PropertyChanged += Options_OnPropertyChanged;
            OnPropertyChanged(nameof(TiffOptions));
        }
    }

    [Category("Encoding Options")]
    [Description("TGA Options")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public TgaEncodingOptions TgaOptions
    {
        get => _tgaOptions;
        set
        {
            if (_tgaOptions != null)
                _tgaOptions.PropertyChanged -= Options_OnPropertyChanged;
            _tgaOptions = value;
            if (_tgaOptions != null)
                _tgaOptions.PropertyChanged += Options_OnPropertyChanged;
            OnPropertyChanged(nameof(TgaOptions));
        }
    }

    [Category("Encoding Options")]
    [Description("TGA Options")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public WebPEncodingOptions WebPOptions
    {
        get => _webpOptions;
        set
        {
            if (_webpOptions != null)
                _webpOptions.PropertyChanged -= Options_OnPropertyChanged;
            _webpOptions = value;
            if (_webpOptions != null)
                _webpOptions.PropertyChanged += Options_OnPropertyChanged;
            OnPropertyChanged(nameof(WebPOptions));
        }
    }
    [Category("Encoding Options")]
    [Description("QOI encoding parameters")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public QoiEncodingOptions QoiOptions
    {
        get => _qoiOptions;
        set
        {
            if (_qoiOptions != null)
                _qoiOptions.PropertyChanged -= Options_OnPropertyChanged;
            _qoiOptions = value;
            if (_qoiOptions != null)
                _qoiOptions.PropertyChanged += Options_OnPropertyChanged;
            OnPropertyChanged(nameof(QoiOptions));
        }
    }

    #endregion

    #region Notify Property Changed

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    void Options_OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is JpegEncodingOptions)
            OnPropertyChanged(nameof(JpegOptions));
        else if (sender is PbmEncodingOptions)
            OnPropertyChanged(nameof(PbmOptions));
        else if (sender is PngEncodingOptions)
            OnPropertyChanged(nameof(PngOptions));
        else if (sender is BmpEncodingOptions)
            OnPropertyChanged(nameof(BmpOptions));
        else if (sender is GifEncodingOptions)
            OnPropertyChanged(nameof(GifOptions));
        else if (sender is TiffEncodingOptions)
            OnPropertyChanged(nameof(TiffOptions));
        else if (sender is TgaEncodingOptions)
            OnPropertyChanged(nameof(TgaOptions));
        else if (sender is WebPEncodingOptions)
            OnPropertyChanged(nameof(WebPOptions));
        else if (sender is QoiEncodingOptions)
            OnPropertyChanged(nameof(QoiOptions));
    }

    #endregion

    #region Execute

    public IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> Execute(IDictionary<Socket, IReadOnlyList<IBasicWorkItem>> inputs)
    {
        return Execute(inputs, CancellationToken.None);
    }

    public IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> Execute(IDictionary<Socket, IReadOnlyList<IBasicWorkItem>> inputs, CancellationToken cancellationToken)
    {
        return Execute(
            inputs.ToDictionary(
                kvp => kvp.Key.Id,
                kvp => kvp.Value
            ),
            cancellationToken
        );
    }

    public IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> Execute(IDictionary<string, IReadOnlyList<IBasicWorkItem>> inputs)
    {
        return Execute(inputs, CancellationToken.None);
    }

    public IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> Execute(IDictionary<string, IReadOnlyList<IBasicWorkItem>> inputs, CancellationToken cancellationToken)
    {
        if (!inputs.TryGetValue(_inputs[0].Id, out var inItems))
            throw new ArgumentException($"Input items not found for the expected input socket {_inputs[0].Id}.", nameof(inputs));

        var outputItems = new List<IBasicWorkItem>();

        foreach (WorkItem sourceItem in inItems.OfType<WorkItem>())
        {
            cancellationToken.ThrowIfCancellationRequested();
            IImmutableDictionary<string, object> metadata = sourceItem.Metadata;
            metadata = metadata.SetItem("Format", TargetFormat.ToString());
            metadata = metadata.SetItem("EncodingOptions", TargetFormat switch
            {
                ImageFormat.Jpeg => (object)JpegOptions,
                ImageFormat.Pbm => (object)PbmOptions,
                ImageFormat.Png => (object)PngOptions,
                ImageFormat.Bmp => (object)BmpOptions,
                ImageFormat.Gif => (object)GifOptions,
                ImageFormat.Tiff => (object)TiffOptions,
                ImageFormat.Tga => (object)TgaOptions,
                ImageFormat.WebP => (object)WebPOptions,
                ImageFormat.Qoi => (object)QoiOptions,
                ImageFormat.Unknown => throw new InvalidOperationException("Cannot convert to Unknown format."),
                _ => null
            } ?? null!);
            outputItems.Add(
                new WorkItem(
                    sourceItem.Image,
                    metadata
                )
            );
        }

        return new Dictionary<Socket, IReadOnlyList<IBasicWorkItem>> { { _outputs[0], outputItems } };
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
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion
}

#region Encoding Classes

[TypeConverter(typeof(ExpandableObjectConverter))]
public class JpegEncodingOptions : INotifyPropertyChanged
{
    private int _quality = 75;
    private JpegEncodingColor? _colorType = null;
    private bool? _interleaved = null;

    public event PropertyChangedEventHandler? PropertyChanged;

    [Category("JPEG")]
    [Description("JPEG quality (1-100, higher = better quality, larger file)")]
    [DefaultValue(75)]
    public int Quality
    {
        get => _quality;
        set
        {
            value = Math.Clamp(value, 1, 100);
            if (_quality != value) { _quality = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Quality))); }
        }
    }

    [Category("JPEG")]
    [Description("Color encoding type (null = auto-detect)")]
    [DefaultValue(null)]
    public JpegEncodingColor? ColorType
    {
        get => _colorType;
        set { if (_colorType != value) { _colorType = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ColorType))); } }
    }

    [Category("JPEG")]
    [Description("Use interleaved encoding (null = auto)")]
    [DefaultValue(null)]
    public bool? Interleaved
    {
        get => _interleaved;
        set { if (_interleaved != value) { _interleaved = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Interleaved))); } }
    }

    public override string ToString() => $"Quality: {Quality}";
}

[TypeConverter(typeof(ExpandableObjectConverter))]
public class PbmEncodingOptions : INotifyPropertyChanged
{
    private PbmColorType _colorType = PbmColorType.Rgb;
    private PbmComponentType? _componentType = null;
    private PbmEncoding _encoding = PbmEncoding.Binary;

    public event PropertyChangedEventHandler? PropertyChanged;

    [Category("PBM")]
    [Description("Color type for PBM output")]
    [DefaultValue(PbmColorType.Rgb)]
    public PbmColorType ColorType
    {
        get => _colorType;
        set { if (_colorType != value) { _colorType = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ColorType))); } }
    }

    [Category("PBM")]
    [Description("Component data type (null = auto)")]
    [DefaultValue(null)]
    public PbmComponentType? ComponentType
    {
        get => _componentType;
        set { if (_componentType != value) { _componentType = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ComponentType))); } }
    }

    [Category("PBM")]
    [Description("Encoding format: Plain (ASCII) or Binary")]
    [DefaultValue(PbmEncoding.Binary)]
    public PbmEncoding Encoding
    {
        get => _encoding;
        set { if (_encoding != value) { _encoding = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Encoding))); } }
    }

    public override string ToString() => $"{ColorType}, {Encoding}";
}

[TypeConverter(typeof(ExpandableObjectConverter))]
public class QoiEncodingOptions : INotifyPropertyChanged
{
    private QoiChannels? _channels = null;
    private QoiColorSpace? _colorSpace = null;

    public event PropertyChangedEventHandler? PropertyChanged;

    [Category("QOI")]
    [Description("Number of color channels (null = auto-detect)")]
    [DefaultValue(null)]
    public QoiChannels? Channels
    {
        get => _channels;
        set { if (_channels != value) { _channels = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Channels))); } }
    }

    [Category("QOI")]
    [Description("Color space (null = auto)")]
    [DefaultValue(null)]
    public QoiColorSpace? ColorSpace
    {
        get => _colorSpace;
        set { if (_colorSpace != value) { _colorSpace = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ColorSpace))); } }
    }

    public override string ToString() => Channels.HasValue ? $"Channels: {Channels}" : "Auto";
}

[TypeConverter(typeof(ExpandableObjectConverter))]
public class TgaEncodingOptions : INotifyPropertyChanged
{
    private TgaBitsPerPixel? _bitsPerPixel = null;
    private TgaCompression _compression = TgaCompression.RunLength;

    public event PropertyChangedEventHandler? PropertyChanged;

    [Category("TGA")]
    [Description("Bits per pixel (null = auto)")]
    [DefaultValue(null)]
    public TgaBitsPerPixel? BitsPerPixel
    {
        get => _bitsPerPixel;
        set { if (_bitsPerPixel != value) { _bitsPerPixel = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BitsPerPixel))); } }
    }

    [Category("TGA")]
    [Description("Compression mode")]
    [DefaultValue(TgaCompression.RunLength)]
    public TgaCompression Compression
    {
        get => _compression;
        set { if (_compression != value) { _compression = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Compression))); } }
    }

    public override string ToString() => $"Compression: {Compression}";
}

[TypeConverter(typeof(ExpandableObjectConverter))]
public class WebPEncodingOptions : INotifyPropertyChanged
{
    private WebpFileFormatType _fileFormat = WebpFileFormatType.Lossy;
    private int _quality = 75;
    private WebpEncodingMethod _method = WebpEncodingMethod.Default;
    private bool _nearLossless = true;
    private int _nearLosslessQuality = 100;

    public event PropertyChangedEventHandler? PropertyChanged;

    [Category("WebP")]
    [Description("File format: Lossy or Lossless")]
    [DefaultValue(WebpFileFormatType.Lossy)]
    public WebpFileFormatType FileFormat
    {
        get => _fileFormat;
        set { if (_fileFormat != value) { _fileFormat = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FileFormat))); } }
    }

    [Category("WebP")]
    [Description("Quality (0-100, for lossy: compression quality, for lossless: effort)")]
    [DefaultValue(75)]
    public int Quality
    {
        get => _quality;
        set
        {
            value = Math.Clamp(value, 0, 100);
            if (_quality != value) { _quality = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Quality))); }
        }
    }

    [Category("WebP")]
    [Description("Encoding method: speed vs quality trade-off (0=fast, 6=best)")]
    [DefaultValue(WebpEncodingMethod.Default)]
    public WebpEncodingMethod Method
    {
        get => _method;
        set { if (_method != value) { _method = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Method))); } }
    }

    [Category("WebP")]
    [Description("Enable near-lossless mode for lossless encoding")]
    [DefaultValue(true)]
    public bool NearLossless
    {
        get => _nearLossless;
        set { if (_nearLossless != value) { _nearLossless = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NearLossless))); } }
    }

    [Category("WebP")]
    [Description("Near-lossless quality (0-100, 100 = maximum quality)")]
    [DefaultValue(100)]
    public int NearLosslessQuality
    {
        get => _nearLosslessQuality;
        set
        {
            value = Math.Clamp(value, 0, 100);
            if (_nearLosslessQuality != value) { _nearLosslessQuality = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NearLosslessQuality))); }
        }
    }

    public override string ToString() => FileFormat == WebpFileFormatType.Lossless ? "Lossless" : $"Lossy Q{Quality}";
}

[TypeConverter(typeof(ExpandableObjectConverter))]
public class BmpEncodingOptions : INotifyPropertyChanged
{
    private BmpBitsPerPixel? _bitsPerPixel = null;
    private bool _supportTransparency = false;
    private QuantizerOptions _quantizer = new();

    public event PropertyChangedEventHandler? PropertyChanged;

    [Category("BMP")]
    [Description("Color depth in bits per pixel (null = auto)")]
    [DefaultValue(null)]
    public BmpBitsPerPixel? BitsPerPixel
    {
        get => _bitsPerPixel;
        set { if (_bitsPerPixel != value) { _bitsPerPixel = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BitsPerPixel))); } }
    }

    [Category("BMP")]
    [Description("Support transparency (32-bit only)")]
    [DefaultValue(false)]
    public bool SupportTransparency
    {
        get => _supportTransparency;
        set { if (_supportTransparency != value) { _supportTransparency = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SupportTransparency))); } }
    }

    [Category("BMP Quantizer")]
    [Description("Quantizer options for indexed color modes")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public QuantizerOptions Quantizer
    {
        get => _quantizer;
        set { _quantizer = value ?? new(); PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Quantizer))); }
    }

    public override string ToString() => BitsPerPixel.HasValue ? $"{BitsPerPixel}bpp" : "Auto";
}

[TypeConverter(typeof(ExpandableObjectConverter))]
public class GifEncodingOptions : INotifyPropertyChanged
{
    private GifColorTableMode _colorTableMode = GifColorTableMode.Global;
    private QuantizerOptions _quantizer = new();

    public event PropertyChangedEventHandler? PropertyChanged;

    [Category("GIF")]
    [Description("Color table mode: Global (shared palette) or Local (per-frame)")]
    [DefaultValue(GifColorTableMode.Global)]
    public GifColorTableMode ColorTableMode
    {
        get => _colorTableMode;
        set { if (_colorTableMode != value) { _colorTableMode = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ColorTableMode))); } }
    }

    [Category("GIF Quantizer")]
    [Description("Quantizer options for color reduction and dithering")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public QuantizerOptions Quantizer
    {
        get => _quantizer;
        set { _quantizer = value ?? new(); PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Quantizer))); }
    }

    public override string ToString() => $"{ColorTableMode}, {Quantizer.MaxColors} colors";
}

[TypeConverter(typeof(ExpandableObjectConverter))]
public class PngEncodingOptions : INotifyPropertyChanged
{
    private PngCompressionLevel _compressionLevel = PngCompressionLevel.DefaultCompression;
    private PngColorType? _colorType = null;
    private PngBitDepth? _bitDepth = null;
    private PngInterlaceMethod? _interlaceMethod = null;
    private PngTransparentColorMode _transparentColorMode = PngTransparentColorMode.Preserve;
    private QuantizerOptions _quantizer = new();

    public event PropertyChangedEventHandler? PropertyChanged;

    [Category("PNG")]
    [Description("Compression level (0=none, 9=max)")]
    [DefaultValue(PngCompressionLevel.DefaultCompression)]
    public PngCompressionLevel CompressionLevel
    {
        get => _compressionLevel;
        set { if (_compressionLevel != value) { _compressionLevel = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CompressionLevel))); } }
    }

    [Category("PNG")]
    [Description("Color type (null = auto-detect)")]
    [DefaultValue(null)]
    public PngColorType? ColorType
    {
        get => _colorType;
        set { if (_colorType != value) { _colorType = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ColorType))); } }
    }

    [Category("PNG")]
    [Description("Bit depth per channel (null = auto)")]
    [DefaultValue(null)]
    public PngBitDepth? BitDepth
    {
        get => _bitDepth;
        set { if (_bitDepth != value) { _bitDepth = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BitDepth))); } }
    }

    [Category("PNG")]
    [Description("Interlacing method (null = none)")]
    [DefaultValue(null)]
    public PngInterlaceMethod? InterlaceMethod
    {
        get => _interlaceMethod;
        set { if (_interlaceMethod != value) { _interlaceMethod = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(InterlaceMethod))); } }
    }

    [Category("PNG")]
    [Description("How to handle transparent pixels")]
    [DefaultValue(PngTransparentColorMode.Preserve)]
    public PngTransparentColorMode TransparentColorMode
    {
        get => _transparentColorMode;
        set { if (_transparentColorMode != value) { _transparentColorMode = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TransparentColorMode))); } }
    }

    [Category("PNG Quantizer")]
    [Description("Quantizer options for indexed color mode")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public QuantizerOptions Quantizer
    {
        get => _quantizer;
        set { _quantizer = value ?? new(); PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Quantizer))); }
    }

    public override string ToString() => $"Compression: {CompressionLevel}";
}

[TypeConverter(typeof(ExpandableObjectConverter))]
public class TiffEncodingOptions : INotifyPropertyChanged
{
    private TiffCompression? _compression = TiffCompression.Lzw;
    private TiffBitsPerPixel? _bitsPerPixel = null;
    private DeflateCompressionLevel _compressionLevel = DeflateCompressionLevel.DefaultCompression;
    private TiffPredictor? _horizontalPredictor = null;
    private QuantizerOptions _quantizer = new();

    public event PropertyChangedEventHandler? PropertyChanged;

    [Category("TIFF")]
    [Description("Compression algorithm")]
    [DefaultValue(TiffCompression.Lzw)]
    public TiffCompression? Compression
    {
        get => _compression;
        set { if (_compression != value) { _compression = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Compression))); } }
    }

    [Category("TIFF")]
    [Description("Bits per pixel (null = auto)")]
    [DefaultValue(null)]
    public TiffBitsPerPixel? BitsPerPixel
    {
        get => _bitsPerPixel;
        set { if (_bitsPerPixel != value) { _bitsPerPixel = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BitsPerPixel))); } }
    }

    [Category("TIFF")]
    [Description("Deflate compression level (for Deflate compression)")]
    [DefaultValue(DeflateCompressionLevel.DefaultCompression)]
    public DeflateCompressionLevel CompressionLevel
    {
        get => _compressionLevel;
        set { if (_compressionLevel != value) { _compressionLevel = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CompressionLevel))); } }
    }

    [Category("TIFF")]
    [Description("Horizontal predictor for better compression")]
    [DefaultValue(null)]
    public TiffPredictor? HorizontalPredictor
    {
        get => _horizontalPredictor;
        set { if (_horizontalPredictor != value) { _horizontalPredictor = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HorizontalPredictor))); } }
    }

    [Category("TIFF Quantizer")]
    [Description("Quantizer options for indexed color modes")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public QuantizerOptions Quantizer
    {
        get => _quantizer;
        set { _quantizer = value ?? new(); PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Quantizer))); }
    }

    public override string ToString() => $"Compression: {Compression}";
}

#endregion

public static class ImageSharpExtensions
{
    public static ImageFormat ToSimpleFormat(this IImageFormat format)
    {
        return format switch
        {
            BmpFormat => ImageFormat.Bmp,
            GifFormat => ImageFormat.Gif,
            JpegFormat => ImageFormat.Jpeg,
            PbmFormat => ImageFormat.Pbm,
            PngFormat => ImageFormat.Png,
            TiffFormat => ImageFormat.Tiff,
            TgaFormat => ImageFormat.Tga,
            WebpFormat => ImageFormat.WebP,
            QoiFormat => ImageFormat.Qoi,
            _ => ImageFormat.Unknown
        };
    }
}