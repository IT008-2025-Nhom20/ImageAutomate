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
using System.Collections.Immutable;
using System.ComponentModel;
using System.Security.Cryptography.X509Certificates;

namespace ImageAutomate.StandardBlocks;

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
    private readonly IReadOnlyList<Socket> _inputs = [new("Convert.In", "Image.Input")];
    private readonly IReadOnlyList<Socket> _outputs = [new("Convert.Out", "Image.Out")];

    private ImageFormat _targetFormat = ImageFormat.Png;
    private bool _alwaysEncode = false;
    private bool disposedValue = false;

    private JpegEncodingOptions _jpegOptions = new JpegEncodingOptions();
    private PngEncodingOptions _pngOptions = new PngEncodingOptions();
    private BmpEncodingOptions _bmpOptions = new BmpEncodingOptions();
    private GifEncodingOptions _gifOptions = new GifEncodingOptions();
    private TiffEncodingOptions _tiffOptions = new TiffEncodingOptions();
    private TgaEncodingOptions _tgaOptions = new TgaEncodingOptions();
    private WebPEncodingOptions _webpOptions = new WebPEncodingOptions();
    private QoiEncodingOptions _qoiOptions = new QoiEncodingOptions();

    #endregion

    public ConvertBlock()
    {
        _jpegOptions.PropertyChanged += Options_OnPropertyChanged;
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
                ImageFormat.Png => (object)PngOptions,
                ImageFormat.Bmp => (object)BmpOptions,
                ImageFormat.Gif => (object)GifOptions,
                ImageFormat.Tiff => (object)TiffOptions,
                ImageFormat.Tga => (object)TgaOptions,
                ImageFormat.WebP => (object)WebPOptions,
                ImageFormat.Qoi => (object)QoiOptions,
                ImageFormat.Unknown => throw new NotImplementedException(),
                ImageFormat.Pbm => throw new NotImplementedException(),
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