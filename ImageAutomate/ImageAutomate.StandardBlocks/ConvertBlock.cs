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
using System.Collections.ObjectModel;
using System.ComponentModel;


namespace ImageAutomate.StandardBlocks;
public enum ImageFormat
{
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

public enum BmpBitsPerPixel
{
    Pixel24 = 24,
    Pixel32 = 32
}

public enum TiffCompression
{
    None,
    Lzw,
    Ccitt4,
    Rle,
    Zip
}
public class ConvertBlock : IBlock
{
    #region Fields
    private readonly Socket _inputSocket = new("Convert.In", "Image.Input");
    private readonly Socket _outputSocket = new("Convert.Out", "Image.Out");
    private readonly IReadOnlyList<Socket> _inputs;
    private readonly IReadOnlyList<Socket> _outputs;

    private ImageFormat _targetFormat = ImageFormat.Png;
    private bool _alwaysEncoder = false;
    private bool disposedValue = false;

    private JpegEncodingOptions _jpegOptions = new JpegEncodingOptions();
    private PngEncodingOptions _pngOptions = new PngEncodingOptions();
    private BmpEncodingOptions _bmpOptions = new BmpEncodingOptions();
    private GifEncodingOptions _gifOptions = new GifEncodingOptions();
    private TiffEncodingOptions _tiffOptions = new TiffEncodingOptions();
    private TgaEncodingOptions _tgaOptions = new TgaEncodingOptions();
    private WebPEncodingOptions _webpOptions = new WebPEncodingOptions();
    private QoiEncodingOptions _qoiOptions = new QoiEncodingOptions();
   
    private int _width = 200;
    private int _height = 100;

    private string _title = "Convert";
    private string _content = "Convert image format";
    #endregion

    #region Constructor
    public ConvertBlock()
    {
        _inputs = new[] { _inputSocket };
        _outputs = new[] { _outputSocket };

        _jpegOptions.PropertyChanged += Options_OnPropertyChanged;
        _pngOptions.PropertyChanged += Options_OnPropertyChanged;
        _bmpOptions.PropertyChanged += Options_OnPropertyChanged;
        _gifOptions.PropertyChanged += Options_OnPropertyChanged;
        _tiffOptions.PropertyChanged += Options_OnPropertyChanged;
        _tgaOptions.PropertyChanged += Options_OnPropertyChanged;
        _webpOptions.PropertyChanged += Options_OnPropertyChanged;
        _qoiOptions.PropertyChanged += Options_OnPropertyChanged;
    }
    #endregion

    #region Basic Properties
    public string Name => "Convert";

    public string Title 
    { 
        get => _title;
    }
    public string Content
    {
        get
        {
            var optionSummaries= TargetFormat switch
            {
                ImageFormat.Jpeg => $"Quality: {JpegOptions.Quality}",
                ImageFormat.Png => $"Compression: {PngOptions.CompressionLevel}",
                ImageFormat.Bmp => $"BitsPerPixel: {BmpOptions.BitsPerPixel}",
                ImageFormat.Gif => $"ColorPaletteSize: {GifOptions.ColorPaletteSize}\n" +
                                   $"UseDithering: {GifOptions.UseDithering}",
                ImageFormat.Tiff => $"Compression: {TiffOptions.Compression}",
                ImageFormat.Tga => $"Compression: {TgaOptions.Compress}",
                ImageFormat.WebP => $"LossLess: {WebPOptions.Lossless}\n" +
                                    $"Quality: {WebPOptions.Quality}",
                ImageFormat.Qoi => $"Include Alpha: {QoiOptions.IncludeAlpha}",
                _ => "Options: Default"
            };
            return $"Format: {TargetFormat}\nRe-encode: {AlwaysEncode}\nConfiguration:{optionSummaries}";
        }
    }
    
    #endregion

    #region Layout
    [Category("Layout")]
    [Description("Width of the black node")]
    public int Width 
    { 
        get => _width;
        set
        {
            if(Math.Abs(_width - value) > double.Epsilon)
            {
                _width = value;
                OnPropertyChanged(nameof(Width));
            }
        }
    }

    [Category("Layout")]
    [Description("Height of the black node")]
    public int Height 
    {
        get => _height; 
        set
        {
            if (Math.Abs(_height - value) > double.Epsilon)
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
                OnPropertyChanged(nameof(PngOptions));
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
        get => _alwaysEncoder;
        set
        {
            if (_alwaysEncoder != value)
            {
                _alwaysEncoder = value;
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

    #region InotifyPropertyChanged
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    void Options_OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is JpegEncodingOptions)
            OnPropertyChanged(nameof(JpegOptions));
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
        if (inputs is null) throw new NotImplementedException();

        inputs.TryGetValue(_inputSocket, out var inItems);
        inItems ??= Array.Empty<IBasicWorkItem>();

        var resultList = new List<IBasicWorkItem>(inItems.Count);
        foreach (var item in inItems)
        {
            var converted = ConvertWorkItem(item);
            if (converted != null)
                resultList.Add(converted);
        }

        var readOnlyResult = new ReadOnlyCollection<IBasicWorkItem>(resultList);
        var dict = new Dictionary<Socket, IReadOnlyList<IBasicWorkItem>>
        {
            {_outputSocket, readOnlyResult}
        };
        return dict;
    }


    public IReadOnlyDictionary<string, IReadOnlyList<IBasicWorkItem>> Execute(IDictionary<string, IReadOnlyList<IBasicWorkItem>> inputs)
    {
        if (inputs is null) throw new ArgumentNullException(nameof(inputs));

        inputs.TryGetValue(_inputSocket.Id, out var inItems);
        inItems ??= Array.Empty<IBasicWorkItem>();

        var resultList = new List<IBasicWorkItem>(inItems.Count);
        foreach (var item in inItems)
        {
            var converted = ConvertWorkItem(item);
            if (converted != null)
                resultList.Add(converted);
        }

        var readOnlyResult = new ReadOnlyCollection<IBasicWorkItem>(resultList);
        var dict = new Dictionary<string, IReadOnlyList<IBasicWorkItem>>
        {
            { _outputSocket.Id, readOnlyResult }
        };

        return dict;
    }
    private IBasicWorkItem? ConvertWorkItem(IBasicWorkItem item)
    {
        if (item is null)
            throw new ArgumentNullException(nameof(item));

        // 1. Lấy ảnh từ metadata
        if (!item.Metadata.TryGetValue("ImageData", out var imageDataObj) ||
            imageDataObj is not byte[] imageBytes ||
            imageBytes.Length == 0)
        {
            // Không có dữ liệu ảnh → trả nguyên item, cho phép pipeline pass-through
            return item;
        }

        // 2. Lấy format hiện tại (nếu có) để quyết định có cần re-encode hay không
        ImageFormat? currentFormat = null;
        if (item.Metadata.TryGetValue("Format", out var fmtObj) && fmtObj is ImageFormat fmtEnum)
        {
            currentFormat = fmtEnum;
        }

        // Nếu không bắt buộc re-encode và format đã trùng target => bỏ qua, trả item gốc
        if (!AlwaysEncode && currentFormat.HasValue && currentFormat.Value == TargetFormat)
        {
            return item;
        }

        try
        {
            // 3. Load ảnh bằng ImageSharp 3.x
            using var image = Image.Load<Rgba32>(imageBytes);

            // 4. Chọn encoder theo TargetFormat + options
            IImageEncoder encoder = CreateEncoderForTargetFormat();

            // 5. Encode lại sang format mới
            using var ms = new MemoryStream();
            image.Save(ms, encoder);
            var convertedBytes = ms.ToArray();

            // 6. Clone metadata cũ và cập nhật
            var newMetadata = new Dictionary<string, object>(item.Metadata)
            {
                ["ImageData"] = convertedBytes,
                ["Format"] = TargetFormat,
                ["ConvertedAtUtc"] = DateTime.UtcNow
            };

            // Giữ lại id cũ hay tạo id mới tuỳ design – ở đây tạo WorkItem mới
            return new BasicWorkItem(newMetadata);
        }
        catch (Exception ex) when (ex is UnknownImageFormatException
                                   || ex is InvalidImageContentException
                                   || ex is NotSupportedException
                                   || ex is IOException)
        {
            
            throw new InvalidOperationException(
                $"ConvertBlock: Failed to convert work item {item.Id} to format {TargetFormat}: {ex.Message}", ex);
        }
    }

    private IImageEncoder CreateEncoderForTargetFormat()
    {
        switch (TargetFormat)
        {
            case ImageFormat.Jpeg:
                return new JpegEncoder
                {
                    Quality = JpegOptions.Quality
                };

            case ImageFormat.Png:
                return new PngEncoder
                {
                    CompressionLevel =
                        (SixLabors.ImageSharp.Formats.Png.PngCompressionLevel)(int)PngOptions.CompressionLevel
                };

            case ImageFormat.Bmp:
                return new BmpEncoder
                {
                    BitsPerPixel = (BmpOptions.BitsPerPixel == BmpBitsPerPixel.Pixel24)
                       ? SixLabors.ImageSharp.Formats.Bmp.BmpBitsPerPixel.Pixel24
                       : SixLabors.ImageSharp.Formats.Bmp.BmpBitsPerPixel.Pixel32
                };

            case ImageFormat.Gif:
                return new GifEncoder();

            case ImageFormat.Tiff:
                return new TiffEncoder
                {
                    Compression =
                        (TiffOptions.Compression == TiffCompression.None) ? SixLabors.ImageSharp.Formats.Tiff.Constants.TiffCompression.None :
                        (TiffOptions.Compression == TiffCompression.Lzw) ? SixLabors.ImageSharp.Formats.Tiff.Constants.TiffCompression.Lzw :
                        (TiffOptions.Compression == TiffCompression.Ccitt4) ? SixLabors.ImageSharp.Formats.Tiff.Constants.TiffCompression.CcittGroup4Fax :
                        (TiffOptions.Compression == TiffCompression.Rle) ? SixLabors.ImageSharp.Formats.Tiff.Constants.TiffCompression.PackBits :
                        SixLabors.ImageSharp.Formats.Tiff.Constants.TiffCompression.Deflate
                };

            case ImageFormat.Tga:
                return new TgaEncoder
                {
                    Compression = TgaOptions.Compress
                        ? TgaCompression.RunLength
                        : TgaCompression.None
                };

            case ImageFormat.WebP:
                return new WebpEncoder
                {
                    Quality = (int)Math.Clamp(WebPOptions.Quality, 0f, 100f),
                    FileFormat = WebPOptions.Lossless
                        ? WebpFileFormatType.Lossless
                        : WebpFileFormatType.Lossy
                };

            case ImageFormat.Qoi:
                return new QoiEncoder
                {
                    Channels = QoiOptions.IncludeAlpha ? QoiChannels.Rgba : QoiChannels.Rgb
                };

            case ImageFormat.Pbm:
                // Không có options riêng, dùng encoder mặc định
                return new PbmEncoder();

            default:
                throw new NotSupportedException($"ConvertBlock: Target format '{TargetFormat}' is not supported.");
        }
    }
    private sealed class BasicWorkItem : IBasicWorkItem
    {
        public Guid Id { get; }
        public IDictionary<string, object> Metadata { get; }

        public BasicWorkItem(IDictionary<string, object> metadata)
        {
            Id = Guid.NewGuid();
            Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
        }
    }

    #endregion

    #region Disposing
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~ConvertBlock()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    #endregion
}
#region Encoding Class
[TypeConverter(typeof(ExpandableObjectConverter))]
public class JpegEncodingOptions : INotifyPropertyChanged
{
    private int _quality = 75;

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

            if (_quality != value)
            {
                _quality = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Quality)));
            }
        }
    }

    public override string ToString()
    {
        return $"Quality: {Quality}";
    }
}

[TypeConverter(typeof(ExpandableObjectConverter))]
public class PngEncodingOptions : INotifyPropertyChanged
{
    private PngCompressionLevel _compressionLevel = PngCompressionLevel.DefaultCompression;

    public event PropertyChangedEventHandler? PropertyChanged;

    [Category("PNG")]
    [Description("PNG compression level")]
    [DefaultValue(PngCompressionLevel.DefaultCompression)]
    public PngCompressionLevel CompressionLevel
    {
        get => _compressionLevel;
        set
        {
            if (_compressionLevel != value)
            {
                _compressionLevel = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CompressionLevel)));
            }
        }
    }

    public override string ToString()
    {
        return $"Compression: {CompressionLevel}";
    }
}

[TypeConverter(typeof(ExpandableObjectConverter))]
public class BmpEncodingOptions : INotifyPropertyChanged
{
    private BmpBitsPerPixel _bitsPerPixel = BmpBitsPerPixel.Pixel24;

    public event PropertyChangedEventHandler? PropertyChanged;

    [Category("BMP")]
    [Description("Color depth in bits per pixel")]
    [DefaultValue(BmpBitsPerPixel.Pixel24)]
    public BmpBitsPerPixel BitsPerPixel
    {
        get => _bitsPerPixel;
        set
        {
            if (_bitsPerPixel != value)
            {
                _bitsPerPixel = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BitsPerPixel)));
            }
        }
    }

    public override string ToString()
    {
        return $"Depth: {BitsPerPixel} bits";
    }
}

[TypeConverter(typeof(ExpandableObjectConverter))]
public class GifEncodingOptions : INotifyPropertyChanged
{
    private bool _useDithering = true;
    private int _colorPaletteSize = 256;

    public event PropertyChangedEventHandler? PropertyChanged;

    [Category("GIF")]
    [Description("Use dithering to smooth color gradients")]
    [DefaultValue(true)]
    public bool UseDithering
    {
        get => _useDithering;
        set
        {
            if (_useDithering != value)
            {
                _useDithering = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UseDithering)));
            }
        }
    }

    [Category("GIF")]
    [Description("Number of colors in palette (2-256)")]
    [DefaultValue(256)]
    public int ColorPaletteSize
    {
        get => _colorPaletteSize;
        set
        {
            if (value < 2) value = 2;
            if (value > 256) value = 256;
            if (_colorPaletteSize != value)
            {
                _colorPaletteSize = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ColorPaletteSize)));
            }
        }
    }

    public override string ToString() => $"Colors: {ColorPaletteSize}, Dither: {UseDithering}";
}

[TypeConverter(typeof(ExpandableObjectConverter))]
public class TiffEncodingOptions : INotifyPropertyChanged
{
    private TiffCompression _compression = TiffCompression.Lzw;

    public event PropertyChangedEventHandler? PropertyChanged;

    [Category("TIFF")]
    [Description("Compression scheme (LZW is standard for lossless)")]
    [DefaultValue(TiffCompression.Lzw)]
    public TiffCompression Compression
    {
        get => _compression;
        set
        {
            if (_compression != value)
            {
                _compression = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Compression)));
            }
        }
    }
    public override string ToString() => $"Compression: {Compression}";
}

[TypeConverter(typeof(ExpandableObjectConverter))]
public class TgaEncodingOptions : INotifyPropertyChanged
{
    private bool _compress = true;

    public event PropertyChangedEventHandler? PropertyChanged;

    [Category("TGA")]
    [Description("Use RLE compression")]
    [DefaultValue(true)]
    public bool Compress
    {
        get => _compress;
        set { if (_compress != value) { _compress = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Compress))); } }
    }
    public override string ToString() => $"RLE: {Compress}";
}

[TypeConverter(typeof(ExpandableObjectConverter))]
public class WebPEncodingOptions : INotifyPropertyChanged
{
    private bool _lossless = false;
    private float _quality = 75f;

    public event PropertyChangedEventHandler? PropertyChanged;

    [Category("WebP")]
    [Description("Use lossless compression (ignores Quality if true)")]
    [DefaultValue(false)]
    public bool Lossless
    {
        get => _lossless;
        set
        {
            if (_lossless != value)
            {
                _lossless = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Lossless)));
            }
        }
    }

    [Category("WebP")]
    [Description("Quality factor (0.0 to 100.0)")]
    [DefaultValue(75f)]
    public float Quality
    {
        get => _quality;
        set
        {
            if (value < 0) value = 0;
            if (value > 100) value = 100;

            if (_quality != value)
            {
                _quality = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Quality)));
            }
        }
    }

    public override string ToString() => Lossless ? "Lossless" : $"Quality: {Quality}";
}

[TypeConverter(typeof(ExpandableObjectConverter))]
public class QoiEncodingOptions : INotifyPropertyChanged
{
    private bool _includeAlpha = true;

    public event PropertyChangedEventHandler? PropertyChanged;

    [Category("QOI")]
    [Description("Include alpha (transparency) channel if present")]
    [DefaultValue(true)]
    public bool IncludeAlpha
    {
        get => _includeAlpha;
        set
        {
            if (_includeAlpha != value)
            {
                _includeAlpha = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IncludeAlpha)));
            }
        }
    }

    public override string ToString() => _includeAlpha ? "Format: RGBA (with Alpha)" : "Format: RGB (No Alpha)";
}
#endregion
