using System.ComponentModel;

using ImageAutomate.Core;
using ImageAutomate.Infrastructure;

using SixLabors.ImageSharp.Processing.Processors.Dithering;
using SixLabors.ImageSharp.Processing.Processors.Quantization;

namespace ImageAutomate.StandardBlocks;

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

#region Enum Mappers to ImageSharp Types

/// <summary>
/// Maps wrapper enums to ImageSharp enums.
/// Provides compile-time safety when enum values diverge.
/// </summary>
internal static class EnumMappers
{
    // JPEG
    internal static SixLabors.ImageSharp.Formats.Jpeg.JpegEncodingColor ToImageSharp(this JpegEncodingColor value) => value switch
    {
        JpegEncodingColor.YCbCrRatio420 => SixLabors.ImageSharp.Formats.Jpeg.JpegEncodingColor.YCbCrRatio420,
        JpegEncodingColor.YCbCrRatio444 => SixLabors.ImageSharp.Formats.Jpeg.JpegEncodingColor.YCbCrRatio444,
        JpegEncodingColor.YCbCrRatio422 => SixLabors.ImageSharp.Formats.Jpeg.JpegEncodingColor.YCbCrRatio422,
        JpegEncodingColor.Luminance => SixLabors.ImageSharp.Formats.Jpeg.JpegEncodingColor.Luminance,
        JpegEncodingColor.Rgb => SixLabors.ImageSharp.Formats.Jpeg.JpegEncodingColor.Rgb,
        _ => throw new ArgumentOutOfRangeException(nameof(value), $"Unknown JpegEncodingColor: {value}")
    };

    // PNG
    internal static SixLabors.ImageSharp.Formats.Png.PngCompressionLevel ToImageSharp(this PngCompressionLevel value) => value switch
    {
        PngCompressionLevel.NoCompression => SixLabors.ImageSharp.Formats.Png.PngCompressionLevel.Level0,
        PngCompressionLevel.BestSpeed => SixLabors.ImageSharp.Formats.Png.PngCompressionLevel.Level1,
        PngCompressionLevel.Level2 => SixLabors.ImageSharp.Formats.Png.PngCompressionLevel.Level2,
        PngCompressionLevel.Level3 => SixLabors.ImageSharp.Formats.Png.PngCompressionLevel.Level3,
        PngCompressionLevel.Level4 => SixLabors.ImageSharp.Formats.Png.PngCompressionLevel.Level4,
        PngCompressionLevel.Level5 => SixLabors.ImageSharp.Formats.Png.PngCompressionLevel.Level5,
        PngCompressionLevel.DefaultCompression => SixLabors.ImageSharp.Formats.Png.PngCompressionLevel.Level6,
        PngCompressionLevel.Level7 => SixLabors.ImageSharp.Formats.Png.PngCompressionLevel.Level7,
        PngCompressionLevel.Level8 => SixLabors.ImageSharp.Formats.Png.PngCompressionLevel.Level8,
        PngCompressionLevel.BestCompression => SixLabors.ImageSharp.Formats.Png.PngCompressionLevel.Level9,
        _ => throw new ArgumentOutOfRangeException(nameof(value), $"Unknown PngCompressionLevel: {value}")
    };

    internal static SixLabors.ImageSharp.Formats.Png.PngColorType ToImageSharp(this PngColorType value) => value switch
    {
        PngColorType.Grayscale => SixLabors.ImageSharp.Formats.Png.PngColorType.Grayscale,
        PngColorType.GrayscaleWithAlpha => SixLabors.ImageSharp.Formats.Png.PngColorType.GrayscaleWithAlpha,
        PngColorType.Palette => SixLabors.ImageSharp.Formats.Png.PngColorType.Palette,
        PngColorType.Rgb => SixLabors.ImageSharp.Formats.Png.PngColorType.Rgb,
        PngColorType.RgbWithAlpha => SixLabors.ImageSharp.Formats.Png.PngColorType.RgbWithAlpha,
        _ => throw new ArgumentOutOfRangeException(nameof(value), $"Unknown PngColorType: {value}")
    };

    internal static SixLabors.ImageSharp.Formats.Png.PngBitDepth ToImageSharp(this PngBitDepth value) => value switch
    {
        PngBitDepth.Bit1 => SixLabors.ImageSharp.Formats.Png.PngBitDepth.Bit1,
        PngBitDepth.Bit2 => SixLabors.ImageSharp.Formats.Png.PngBitDepth.Bit2,
        PngBitDepth.Bit4 => SixLabors.ImageSharp.Formats.Png.PngBitDepth.Bit4,
        PngBitDepth.Bit8 => SixLabors.ImageSharp.Formats.Png.PngBitDepth.Bit8,
        PngBitDepth.Bit16 => SixLabors.ImageSharp.Formats.Png.PngBitDepth.Bit16,
        _ => throw new ArgumentOutOfRangeException(nameof(value), $"Unknown PngBitDepth: {value}")
    };

    internal static SixLabors.ImageSharp.Formats.Png.PngInterlaceMode ToImageSharp(this PngInterlaceMethod value) => value switch
    {
        PngInterlaceMethod.None => SixLabors.ImageSharp.Formats.Png.PngInterlaceMode.None,
        PngInterlaceMethod.Adam7 => SixLabors.ImageSharp.Formats.Png.PngInterlaceMode.Adam7,
        _ => throw new ArgumentOutOfRangeException(nameof(value), $"Unknown PngInterlaceMethod: {value}")
    };

    internal static SixLabors.ImageSharp.Formats.Png.PngTransparentColorMode ToImageSharp(this PngTransparentColorMode value) => value switch
    {
        PngTransparentColorMode.Preserve => SixLabors.ImageSharp.Formats.Png.PngTransparentColorMode.Preserve,
        PngTransparentColorMode.Clear => SixLabors.ImageSharp.Formats.Png.PngTransparentColorMode.Clear,
        _ => throw new ArgumentOutOfRangeException(nameof(value), $"Unknown PngTransparentColorMode: {value}")
    };

    // BMP
    internal static SixLabors.ImageSharp.Formats.Bmp.BmpBitsPerPixel ToImageSharp(this BmpBitsPerPixel value) => value switch
    {
        BmpBitsPerPixel.Pixel1 => SixLabors.ImageSharp.Formats.Bmp.BmpBitsPerPixel.Pixel1,
        BmpBitsPerPixel.Pixel4 => SixLabors.ImageSharp.Formats.Bmp.BmpBitsPerPixel.Pixel4,
        BmpBitsPerPixel.Pixel8 => SixLabors.ImageSharp.Formats.Bmp.BmpBitsPerPixel.Pixel8,
        BmpBitsPerPixel.Pixel16 => SixLabors.ImageSharp.Formats.Bmp.BmpBitsPerPixel.Pixel16,
        BmpBitsPerPixel.Pixel24 => SixLabors.ImageSharp.Formats.Bmp.BmpBitsPerPixel.Pixel24,
        BmpBitsPerPixel.Pixel32 => SixLabors.ImageSharp.Formats.Bmp.BmpBitsPerPixel.Pixel32,
        _ => throw new ArgumentOutOfRangeException(nameof(value), $"Unknown BmpBitsPerPixel: {value}")
    };

    // GIF
    internal static SixLabors.ImageSharp.Formats.Gif.GifColorTableMode ToImageSharp(this GifColorTableMode value) => value switch
    {
        GifColorTableMode.Global => SixLabors.ImageSharp.Formats.Gif.GifColorTableMode.Global,
        GifColorTableMode.Local => SixLabors.ImageSharp.Formats.Gif.GifColorTableMode.Local,
        _ => throw new ArgumentOutOfRangeException(nameof(value), $"Unknown GifColorTableMode: {value}")
    };

    // TIFF
    internal static SixLabors.ImageSharp.Formats.Tiff.Constants.TiffCompression ToImageSharp(this TiffCompression value) => value switch
    {
        TiffCompression.None => SixLabors.ImageSharp.Formats.Tiff.Constants.TiffCompression.None,
        TiffCompression.Ccitt1D => SixLabors.ImageSharp.Formats.Tiff.Constants.TiffCompression.Ccitt1D,
        TiffCompression.PackBits => SixLabors.ImageSharp.Formats.Tiff.Constants.TiffCompression.PackBits,
        TiffCompression.Deflate => SixLabors.ImageSharp.Formats.Tiff.Constants.TiffCompression.Deflate,
        TiffCompression.Lzw => SixLabors.ImageSharp.Formats.Tiff.Constants.TiffCompression.Lzw,
        TiffCompression.CcittGroup3Fax => SixLabors.ImageSharp.Formats.Tiff.Constants.TiffCompression.CcittGroup3Fax,
        TiffCompression.CcittGroup4Fax => SixLabors.ImageSharp.Formats.Tiff.Constants.TiffCompression.CcittGroup4Fax,
        TiffCompression.Jpeg => SixLabors.ImageSharp.Formats.Tiff.Constants.TiffCompression.OldJpeg,
        TiffCompression.Webp => SixLabors.ImageSharp.Formats.Tiff.Constants.TiffCompression.OldJpeg,
        _ => throw new ArgumentOutOfRangeException(nameof(value), $"Unknown TiffCompression: {value}")
    };

    internal static SixLabors.ImageSharp.Formats.Tiff.TiffBitsPerPixel ToImageSharp(this TiffBitsPerPixel value) => value switch
    {
        TiffBitsPerPixel.Bit1 => SixLabors.ImageSharp.Formats.Tiff.TiffBitsPerPixel.Bit1,
        TiffBitsPerPixel.Bit4 => SixLabors.ImageSharp.Formats.Tiff.TiffBitsPerPixel.Bit4,
        TiffBitsPerPixel.Bit8 => SixLabors.ImageSharp.Formats.Tiff.TiffBitsPerPixel.Bit8,
        TiffBitsPerPixel.Bit24 => SixLabors.ImageSharp.Formats.Tiff.TiffBitsPerPixel.Bit24,
        TiffBitsPerPixel.Bit32 => SixLabors.ImageSharp.Formats.Tiff.TiffBitsPerPixel.Bit32,
        TiffBitsPerPixel.Bit48 => SixLabors.ImageSharp.Formats.Tiff.TiffBitsPerPixel.Bit48,
        TiffBitsPerPixel.Bit64 => SixLabors.ImageSharp.Formats.Tiff.TiffBitsPerPixel.Bit64,
        _ => throw new ArgumentOutOfRangeException(nameof(value), $"Unknown TiffBitsPerPixel: {value}")
    };

    internal static SixLabors.ImageSharp.Compression.Zlib.DeflateCompressionLevel ToImageSharp(this DeflateCompressionLevel value) => value switch
    {
        DeflateCompressionLevel.NoCompression => SixLabors.ImageSharp.Compression.Zlib.DeflateCompressionLevel.Level0,
        DeflateCompressionLevel.BestSpeed => SixLabors.ImageSharp.Compression.Zlib.DeflateCompressionLevel.Level1,
        DeflateCompressionLevel.Level2 => SixLabors.ImageSharp.Compression.Zlib.DeflateCompressionLevel.Level2,
        DeflateCompressionLevel.Level3 => SixLabors.ImageSharp.Compression.Zlib.DeflateCompressionLevel.Level3,
        DeflateCompressionLevel.Level4 => SixLabors.ImageSharp.Compression.Zlib.DeflateCompressionLevel.Level4,
        DeflateCompressionLevel.Level5 => SixLabors.ImageSharp.Compression.Zlib.DeflateCompressionLevel.Level5,
        DeflateCompressionLevel.DefaultCompression => SixLabors.ImageSharp.Compression.Zlib.DeflateCompressionLevel.Level6,
        DeflateCompressionLevel.Level7 => SixLabors.ImageSharp.Compression.Zlib.DeflateCompressionLevel.Level7,
        DeflateCompressionLevel.Level8 => SixLabors.ImageSharp.Compression.Zlib.DeflateCompressionLevel.Level8,
        DeflateCompressionLevel.BestCompression => SixLabors.ImageSharp.Compression.Zlib.DeflateCompressionLevel.Level9,
        _ => throw new ArgumentOutOfRangeException(nameof(value), $"Unknown DeflateCompressionLevel: {value}")
    };

    internal static SixLabors.ImageSharp.Formats.Tiff.Constants.TiffPredictor ToImageSharp(this TiffPredictor value) => value switch
    {
        TiffPredictor.None => SixLabors.ImageSharp.Formats.Tiff.Constants.TiffPredictor.None,
        TiffPredictor.Horizontal => SixLabors.ImageSharp.Formats.Tiff.Constants.TiffPredictor.Horizontal,
        _ => throw new ArgumentOutOfRangeException(nameof(value), $"Unknown TiffPredictor: {value}")
    };

    // TGA
    internal static SixLabors.ImageSharp.Formats.Tga.TgaBitsPerPixel ToImageSharp(this TgaBitsPerPixel value) => value switch
    {
        TgaBitsPerPixel.Pixel8 => SixLabors.ImageSharp.Formats.Tga.TgaBitsPerPixel.Pixel8,
        TgaBitsPerPixel.Pixel16 => SixLabors.ImageSharp.Formats.Tga.TgaBitsPerPixel.Pixel16,
        TgaBitsPerPixel.Pixel24 => SixLabors.ImageSharp.Formats.Tga.TgaBitsPerPixel.Pixel24,
        TgaBitsPerPixel.Pixel32 => SixLabors.ImageSharp.Formats.Tga.TgaBitsPerPixel.Pixel32,
        _ => throw new ArgumentOutOfRangeException(nameof(value), $"Unknown TgaBitsPerPixel: {value}")
    };

    internal static SixLabors.ImageSharp.Formats.Tga.TgaCompression ToImageSharp(this TgaCompression value) => value switch
    {
        TgaCompression.None => SixLabors.ImageSharp.Formats.Tga.TgaCompression.None,
        TgaCompression.RunLength => SixLabors.ImageSharp.Formats.Tga.TgaCompression.RunLength,
        _ => throw new ArgumentOutOfRangeException(nameof(value), $"Unknown TgaCompression: {value}")
    };

    // WebP
    internal static SixLabors.ImageSharp.Formats.Webp.WebpFileFormatType ToImageSharp(this WebpFileFormatType value) => value switch
    {
        WebpFileFormatType.Lossless => SixLabors.ImageSharp.Formats.Webp.WebpFileFormatType.Lossless,
        WebpFileFormatType.Lossy => SixLabors.ImageSharp.Formats.Webp.WebpFileFormatType.Lossy,
        _ => throw new ArgumentOutOfRangeException(nameof(value), $"Unknown WebpFileFormatType: {value}")
    };

    internal static SixLabors.ImageSharp.Formats.Webp.WebpEncodingMethod ToImageSharp(this WebpEncodingMethod value) => value switch
    {
        WebpEncodingMethod.Fastest => SixLabors.ImageSharp.Formats.Webp.WebpEncodingMethod.Level0,
        WebpEncodingMethod.Level1 => SixLabors.ImageSharp.Formats.Webp.WebpEncodingMethod.Level1,
        WebpEncodingMethod.Level2 => SixLabors.ImageSharp.Formats.Webp.WebpEncodingMethod.Level2,
        WebpEncodingMethod.Level3 => SixLabors.ImageSharp.Formats.Webp.WebpEncodingMethod.Level3,
        WebpEncodingMethod.Default => SixLabors.ImageSharp.Formats.Webp.WebpEncodingMethod.Level4,
        WebpEncodingMethod.Level5 => SixLabors.ImageSharp.Formats.Webp.WebpEncodingMethod.Level5,
        WebpEncodingMethod.BestQuality => SixLabors.ImageSharp.Formats.Webp.WebpEncodingMethod.Level6,
        _ => throw new ArgumentOutOfRangeException(nameof(value), $"Unknown WebpEncodingMethod: {value}")
    };

    // PBM
    internal static SixLabors.ImageSharp.Formats.Pbm.PbmColorType ToImageSharp(this PbmColorType value) => value switch
    {
        PbmColorType.BlackAndWhite => SixLabors.ImageSharp.Formats.Pbm.PbmColorType.BlackAndWhite,
        PbmColorType.Grayscale => SixLabors.ImageSharp.Formats.Pbm.PbmColorType.Grayscale,
        PbmColorType.Rgb => SixLabors.ImageSharp.Formats.Pbm.PbmColorType.Rgb,
        _ => throw new ArgumentOutOfRangeException(nameof(value), $"Unknown PbmColorType: {value}")
    };

    internal static SixLabors.ImageSharp.Formats.Pbm.PbmComponentType ToImageSharp(this PbmComponentType value) => value switch
    {
        PbmComponentType.Byte => SixLabors.ImageSharp.Formats.Pbm.PbmComponentType.Byte,
        PbmComponentType.Short => SixLabors.ImageSharp.Formats.Pbm.PbmComponentType.Short,
        _ => throw new ArgumentOutOfRangeException(nameof(value), $"Unknown PbmComponentType: {value}")
    };

    internal static SixLabors.ImageSharp.Formats.Pbm.PbmEncoding ToImageSharp(this PbmEncoding value) => value switch
    {
        PbmEncoding.Plain => SixLabors.ImageSharp.Formats.Pbm.PbmEncoding.Plain,
        PbmEncoding.Binary => SixLabors.ImageSharp.Formats.Pbm.PbmEncoding.Binary,
        _ => throw new ArgumentOutOfRangeException(nameof(value), $"Unknown PbmEncoding: {value}")
    };

    // QOI
    internal static SixLabors.ImageSharp.Formats.Qoi.QoiChannels ToImageSharp(this QoiChannels value) => value switch
    {
        QoiChannels.Rgb => SixLabors.ImageSharp.Formats.Qoi.QoiChannels.Rgb,
        QoiChannels.Rgba => SixLabors.ImageSharp.Formats.Qoi.QoiChannels.Rgba,
        _ => throw new ArgumentOutOfRangeException(nameof(value), $"Unknown QoiChannels: {value}")
    };

    internal static SixLabors.ImageSharp.Formats.Qoi.QoiColorSpace ToImageSharp(this QoiColorSpace value) => value switch
    {
        QoiColorSpace.SrgbWithLinearAlpha => SixLabors.ImageSharp.Formats.Qoi.QoiColorSpace.SrgbWithLinearAlpha,
        QoiColorSpace.Linear => SixLabors.ImageSharp.Formats.Qoi.QoiColorSpace.AllChannelsLinear,
        _ => throw new ArgumentOutOfRangeException(nameof(value), $"Unknown QoiColorSpace: {value}")
    };
}

#endregion

public class ConvertBlock : IBlock
{
    #region Fields
    public static IReadOnlyList<string> SupportedFormats
        => ImageFormatRegistry.Instance.GetRegisteredFormats();

    private readonly IReadOnlyList<Socket> _inputs = [new("Convert.In", "Image.Input")];
    private readonly IReadOnlyList<Socket> _outputs = [new("Convert.Out", "Image.Out")];

    private string _targetFormat = "PNG";
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

    // Layout fields
    private double _x;
    private double _y;
    private int _width;
    private int _height;
    private string _title = "Convert";

    #endregion

    public ConvertBlock()
        : this(200, 100)
    {
    }

    public ConvertBlock(int width, int height)
    {
        _width = width;
        _height = height;
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

    [Browsable(false)]
    public string Name => "Convert";

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

    [Browsable(false)]
    public string Content
    {
        get
        {
            var strategy = ImageFormatRegistry.Instance.GetFormat(TargetFormat);
            var options = GetOptionsForFormat(TargetFormat);
            var optionSummary = strategy?.GetOptionsSummary(options) ?? "Default";
            return $"Format: {TargetFormat}\nRe-encode: {AlwaysEncode}\n{optionSummary}";
        }
    }

    /// <summary>
    /// Gets the encoding options for the specified format.
    /// </summary>
    private object? GetOptionsForFormat(string formatName)
    {
        return formatName.ToUpperInvariant() switch
        {
            "JPEG" => JpegOptions,
            "PNG" => PngOptions,
            "BMP" => BmpOptions,
            "GIF" => GifOptions,
            "TIFF" => TiffOptions,
            "TGA" => TgaOptions,
            "WEBP" => WebPOptions,
            "PBM" => PbmOptions,
            "QOI" => QoiOptions,
            _ => null
        };
    }

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

    [Browsable(false)]
    public IReadOnlyList<Socket> Inputs => _inputs;
    [Browsable(false)]
    public IReadOnlyList<Socket> Outputs => _outputs;

    #endregion

    #region Configuration

    [Category("Configuration")]
    [Description("Target image format for conversion")]
    [TypeConverter(typeof(ImageFormatConverter))]
    public string TargetFormat
    {
        get => _targetFormat;
        set
        {
            if (_targetFormat != value)
            {
                _targetFormat = value;
                OnPropertyChanged(nameof(TargetFormat));
                OnPropertyChanged(nameof(Content));
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
        ArgumentNullException.ThrowIfNull(inputs, nameof(inputs));
        if (!inputs.TryGetValue(_inputs[0].Id, out var inItems))
            throw new ArgumentException($"Input items not found for the expected input socket {_inputs[0].Id}.", nameof(inputs));

        var outputItems = new List<IBasicWorkItem>();

        foreach (WorkItem sourceItem in inItems.OfType<WorkItem>())
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Mutate metadata in-place and return same WorkItem (no image cloning needed)
            sourceItem.Metadata = sourceItem.Metadata
                .SetItem("Format", TargetFormat)
                .SetItem("EncodingOptions", GetOptionsForFormat(TargetFormat) ?? null!);

            outputItems.Add(sourceItem);
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

public class ImageFormatConverter : StringConverter
{
    public override bool GetStandardValuesSupported(ITypeDescriptorContext? context) => true;
    public override bool GetStandardValuesExclusive(ITypeDescriptorContext? context) => true;

    public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext? context)
    {
        return new StandardValuesCollection((System.Collections.ICollection)ConvertBlock.SupportedFormats);
    }
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
