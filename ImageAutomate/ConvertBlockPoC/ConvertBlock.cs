using System.ComponentModel;

namespace ConvertBlockPoC;

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
    private ImageFormat _targetFormat = ImageFormat.Png;
    private bool _alwaysReEncode = false;
    private JpegEncodingOptions _jpegOptions = new JpegEncodingOptions();
    private PngEncodingOptions _pngOptions = new PngEncodingOptions();
    private BmpEncodingOptions _bmpOptions = new BmpEncodingOptions();
    private GifEncodingOptions _gifOptions = new GifEncodingOptions();
    private TiffEncodingOptions _tiffOptions = new TiffEncodingOptions();
    private TgaEncodingOptions _tgaOptions = new TgaEncodingOptions();
    private WebPEncodingOptions _webpOptions = new WebPEncodingOptions();
    private QoiEncodingOptions _qoiOptions = new QoiEncodingOptions();
    private double _width = 200;
    private double _height = 100;

    public event PropertyChangedEventHandler? PropertyChanged;

    void Options_OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is JpegEncodingOptions)
            OnPropertyChanged(nameof(JpegOptions));
        else if (sender is PngEncodingOptions)
            OnPropertyChanged(nameof(PngOptions));
    }

    public ConvertBlock()
    {
        _jpegOptions.PropertyChanged += Options_OnPropertyChanged;
        _pngOptions.PropertyChanged += Options_OnPropertyChanged;
    }

    [Browsable(false)]
    public string Name => "Convert";

    [Browsable(false)]
    public string ConfigurationSummary
    {
        get
        {
            var opts = TargetFormat switch
            {
                ImageFormat.Jpeg => $"Quality: {JpegOptions.Quality}",
                ImageFormat.Png => $"Compression: {PngOptions.CompressionLevel}",
                _ => "Options: Default"
            };
            return $"Format: {TargetFormat}\nRe-encode: {AlwaysReEncode}\n{opts}";
        }
    }

    [Category("Layout")]
    [Description("Width of the block node")]
    public double Width
    {
        get => _width;
        set
        {
            if (Math.Abs(_width - value) > double.Epsilon)
            {
                _width = value;
                OnPropertyChanged(nameof(Width));
            }
        }
    }

    [Category("Layout")]
    [Description("Height of the block node")]
    public double Height
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
                OnPropertyChanged(nameof(ConfigurationSummary));
            }
        }
    }

    [Category("Configuration")]
    [Description("Force re-encoding even when format matches")]
    public bool AlwaysReEncode
    {
        get => _alwaysReEncode;
        set
        {
            if (_alwaysReEncode != value)
            {
                _alwaysReEncode = value;
                OnPropertyChanged(nameof(AlwaysReEncode));
                OnPropertyChanged(nameof(ConfigurationSummary));
            }
        }
    }

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
            _bmpOptions = value;
            OnPropertyChanged(nameof(BmpOptions));
        }
    }

    [Category("Encoding Options")]
    [Description("BMP encoding parameters")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public GifEncodingOptions GifOptions
    {
        get => _gifOptions;
        set
        {
            _gifOptions = value;
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
            _tiffOptions = value;
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
            _tgaOptions = value; 
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
            _webpOptions = value;
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
            _qoiOptions = value;
            OnPropertyChanged(nameof(QoiOptions));
        }
    }
    [Browsable(false)]
    public bool ShouldSerializeJpegOptions()
    {
        return TargetFormat == ImageFormat.Jpeg;
    }

    [Browsable(false)]
    public bool ShouldSerializePngOptions()
    {
        return TargetFormat == ImageFormat.Png;
    }

    [Browsable(false)]
    public bool ShouldSerializeTiffOptions()
    {
        return TargetFormat == ImageFormat.Tiff;
    }    
    [Browsable(false)]
    public bool ShouldSerializeGifOptions()
    {
        return TargetFormat == ImageFormat.Gif;
    }
    [Browsable(false)]
    public bool ShouldSerializeBmpOptions()
    {
        return TargetFormat == ImageFormat.Bmp;
    }    
    [Browsable(false)]
    public bool ShouldSerializeTgaOptions()
    {
        return TargetFormat == ImageFormat.Tga;
    }
    [Browsable(false)]
    public bool ShouldSerializePbmOptions()
    {
        return TargetFormat == ImageFormat.Pbm;
    }
    [Browsable(false)]
    public bool ShouldSerializeWebPOptions()
    {
        return TargetFormat == ImageFormat.WebP;
    }
    
    [Browsable(false)]
    public bool ShouldSerializeQoiOptions()
    {
        return TargetFormat == ImageFormat.Qoi;
    }
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

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