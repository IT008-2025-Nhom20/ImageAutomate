using ImageAutomate.Core;

using SixLabors.ImageSharp.Formats;

namespace ImageAutomate.StandardBlocks;

#region JPEG Format Strategy

/// <summary>
/// JPEG format strategy.
/// </summary>
public sealed class JpegFormatStrategy : IImageFormatStrategy
{
    public string FormatName => "JPEG";
    public string FileExtension => ".jpg";
    public string MimeType => "image/jpeg";

    public IImageEncoder CreateEncoder(object? options = null, bool skipMetadata = false)
    {
        var jpegOptions = options as JpegEncodingOptions;
        if (jpegOptions == null)
            return new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder { SkipMetadata = skipMetadata };

        return new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder
        {
            Quality = jpegOptions.Quality,
            ColorType = jpegOptions.ColorType?.ToImageSharp(),
            Interleaved = jpegOptions.Interleaved,
            SkipMetadata = skipMetadata
        };
    }

    public string GetOptionsSummary(object? options)
    {
        if (options is JpegEncodingOptions opt)
            return $"Quality: {opt.Quality}";
        return "Default";
    }

    public bool IsValidOptionsType(object? options)
    {
        return options == null || options is JpegEncodingOptions;
    }
}

#endregion

#region PNG Format Strategy

/// <summary>
/// PNG format strategy.
/// </summary>
public sealed class PngFormatStrategy : IImageFormatStrategy
{
    public string FormatName => "PNG";
    public string FileExtension => ".png";
    public string MimeType => "image/png";

    public IImageEncoder CreateEncoder(object? options = null, bool skipMetadata = false)
    {
        var pngOptions = options as PngEncodingOptions;
        if (pngOptions == null)
            return new SixLabors.ImageSharp.Formats.Png.PngEncoder { SkipMetadata = skipMetadata };

        return new SixLabors.ImageSharp.Formats.Png.PngEncoder
        {
            CompressionLevel = pngOptions.CompressionLevel.ToImageSharp(),
            ColorType = pngOptions.ColorType?.ToImageSharp(),
            BitDepth = pngOptions.BitDepth?.ToImageSharp(),
            InterlaceMethod = pngOptions.InterlaceMethod?.ToImageSharp(),
            TransparentColorMode = pngOptions.TransparentColorMode.ToImageSharp(),
            Quantizer = pngOptions.Quantizer.CreateQuantizer(),
            SkipMetadata = skipMetadata
        };
    }

    public string GetOptionsSummary(object? options)
    {
        if (options is PngEncodingOptions opt)
            return $"Compression: {opt.CompressionLevel}";
        return "Default";
    }

    public bool IsValidOptionsType(object? options)
    {
        return options == null || options is PngEncodingOptions;
    }
}

#endregion

#region BMP Format Strategy

/// <summary>
/// BMP format strategy.
/// </summary>
public sealed class BmpFormatStrategy : IImageFormatStrategy
{
    public string FormatName => "BMP";
    public string FileExtension => ".bmp";
    public string MimeType => "image/bmp";

    public IImageEncoder CreateEncoder(object? options = null, bool skipMetadata = false)
    {
        var bmpOptions = options as BmpEncodingOptions;
        if (bmpOptions == null)
            return new SixLabors.ImageSharp.Formats.Bmp.BmpEncoder { SkipMetadata = skipMetadata };

        return new SixLabors.ImageSharp.Formats.Bmp.BmpEncoder
        {
            BitsPerPixel = bmpOptions.BitsPerPixel?.ToImageSharp(),
            SupportTransparency = bmpOptions.SupportTransparency,
            Quantizer = bmpOptions.Quantizer.CreateQuantizer(),
            SkipMetadata = skipMetadata
        };
    }

    public string GetOptionsSummary(object? options)
    {
        if (options is BmpEncodingOptions opt)
            return opt.BitsPerPixel.HasValue ? $"{opt.BitsPerPixel}bpp" : "Auto";
        return "Default";
    }

    public bool IsValidOptionsType(object? options)
    {
        return options == null || options is BmpEncodingOptions;
    }
}

#endregion

#region GIF Format Strategy

/// <summary>
/// GIF format strategy.
/// </summary>
public sealed class GifFormatStrategy : IImageFormatStrategy
{
    public string FormatName => "GIF";
    public string FileExtension => ".gif";
    public string MimeType => "image/gif";

    public IImageEncoder CreateEncoder(object? options = null, bool skipMetadata = false)
    {
        var gifOptions = options as GifEncodingOptions;
        if (gifOptions == null)
            return new SixLabors.ImageSharp.Formats.Gif.GifEncoder { SkipMetadata = skipMetadata };

        return new SixLabors.ImageSharp.Formats.Gif.GifEncoder
        {
            ColorTableMode = gifOptions.ColorTableMode.ToImageSharp(),
            Quantizer = gifOptions.Quantizer.CreateQuantizer(),
            SkipMetadata = skipMetadata
        };
    }

    public string GetOptionsSummary(object? options)
    {
        if (options is GifEncodingOptions opt)
            return $"{opt.Quantizer.MaxColors} colors, {opt.ColorTableMode}";
        return "Default";
    }

    public bool IsValidOptionsType(object? options)
    {
        return options == null || options is GifEncodingOptions;
    }
}

#endregion

#region TIFF Format Strategy

/// <summary>
/// TIFF format strategy.
/// </summary>
public sealed class TiffFormatStrategy : IImageFormatStrategy
{
    public string FormatName => "TIFF";
    public string FileExtension => ".tiff";
    public string MimeType => "image/tiff";

    public IImageEncoder CreateEncoder(object? options = null, bool skipMetadata = false)
    {
        var tiffOptions = options as TiffEncodingOptions;
        if (tiffOptions == null)
            return new SixLabors.ImageSharp.Formats.Tiff.TiffEncoder { SkipMetadata = skipMetadata };

        return new SixLabors.ImageSharp.Formats.Tiff.TiffEncoder
        {
            Compression = tiffOptions.Compression?.ToImageSharp(),
            BitsPerPixel = tiffOptions.BitsPerPixel?.ToImageSharp(),
            CompressionLevel = tiffOptions.CompressionLevel.ToImageSharp(),
            HorizontalPredictor = tiffOptions.HorizontalPredictor?.ToImageSharp(),
            Quantizer = tiffOptions.Quantizer.CreateQuantizer(),
            SkipMetadata = skipMetadata
        };
    }

    public string GetOptionsSummary(object? options)
    {
        if (options is TiffEncodingOptions opt)
            return $"Compression: {opt.Compression}";
        return "Default";
    }

    public bool IsValidOptionsType(object? options)
    {
        return options == null || options is TiffEncodingOptions;
    }
}

#endregion

#region TGA Format Strategy

/// <summary>
/// TGA format strategy.
/// </summary>
public sealed class TgaFormatStrategy : IImageFormatStrategy
{
    public string FormatName => "TGA";
    public string FileExtension => ".tga";
    public string MimeType => "image/tga";

    public IImageEncoder CreateEncoder(object? options = null, bool skipMetadata = false)
    {
        var tgaOptions = options as TgaEncodingOptions;
        if (tgaOptions == null)
            return new SixLabors.ImageSharp.Formats.Tga.TgaEncoder { SkipMetadata = skipMetadata };

        return new SixLabors.ImageSharp.Formats.Tga.TgaEncoder
        {
            BitsPerPixel = tgaOptions.BitsPerPixel?.ToImageSharp(),
            Compression = tgaOptions.Compression.ToImageSharp(),
            SkipMetadata = skipMetadata
        };
    }

    public string GetOptionsSummary(object? options)
    {
        if (options is TgaEncodingOptions opt)
            return $"Compression: {opt.Compression}";
        return "Default";
    }

    public bool IsValidOptionsType(object? options)
    {
        return options == null || options is TgaEncodingOptions;
    }
}

#endregion

#region WebP Format Strategy

/// <summary>
/// WebP format strategy.
/// </summary>
public sealed class WebPFormatStrategy : IImageFormatStrategy
{
    public string FormatName => "WebP";
    public string FileExtension => ".webp";
    public string MimeType => "image/webp";

    public IImageEncoder CreateEncoder(object? options = null, bool skipMetadata = false)
    {
        var webpOptions = options as WebPEncodingOptions;
        if (webpOptions == null)
            return new SixLabors.ImageSharp.Formats.Webp.WebpEncoder { SkipMetadata = skipMetadata };

        return new SixLabors.ImageSharp.Formats.Webp.WebpEncoder
        {
            FileFormat = webpOptions.FileFormat.ToImageSharp(),
            Quality = webpOptions.Quality,
            Method = webpOptions.Method.ToImageSharp(),
            NearLossless = webpOptions.NearLossless,
            NearLosslessQuality = webpOptions.NearLosslessQuality,
            SkipMetadata = skipMetadata
        };
    }

    public string GetOptionsSummary(object? options)
    {
        if (options is WebPEncodingOptions opt)
            return opt.FileFormat == WebpFileFormatType.Lossless ? "Lossless" : $"Lossy Q{opt.Quality}";
        return "Default";
    }

    public bool IsValidOptionsType(object? options)
    {
        return options == null || options is WebPEncodingOptions;
    }
}

#endregion

#region PBM Format Strategy

/// <summary>
/// PBM format strategy.
/// </summary>
public sealed class PbmFormatStrategy : IImageFormatStrategy
{
    public string FormatName => "PBM";
    public string FileExtension => ".pbm";
    public string MimeType => "image/x-portable-bitmap";

    public IImageEncoder CreateEncoder(object? options = null, bool skipMetadata = false)
    {
        var pbmOptions = options as PbmEncodingOptions;
        if (pbmOptions == null)
            return new SixLabors.ImageSharp.Formats.Pbm.PbmEncoder { SkipMetadata = skipMetadata };

        return new SixLabors.ImageSharp.Formats.Pbm.PbmEncoder
        {
            ColorType = pbmOptions.ColorType.ToImageSharp(),
            ComponentType = pbmOptions.ComponentType?.ToImageSharp(),
            Encoding = pbmOptions.Encoding.ToImageSharp(),
            SkipMetadata = skipMetadata
        };
    }

    public string GetOptionsSummary(object? options)
    {
        if (options is PbmEncodingOptions opt)
            return $"Encoding: {opt.Encoding}";
        return "Default";
    }

    public bool IsValidOptionsType(object? options)
    {
        return options == null || options is PbmEncodingOptions;
    }
}

#endregion

#region QOI Format Strategy

/// <summary>
/// QOI format strategy.
/// </summary>
public sealed class QoiFormatStrategy : IImageFormatStrategy
{
    public string FormatName => "QOI";
    public string FileExtension => ".qoi";
    public string MimeType => "image/qoi";

    public IImageEncoder CreateEncoder(object? options = null, bool skipMetadata = false)
    {
        var qoiOptions = options as QoiEncodingOptions;
        if (qoiOptions == null)
            return new SixLabors.ImageSharp.Formats.Qoi.QoiEncoder { SkipMetadata = skipMetadata };

        return new SixLabors.ImageSharp.Formats.Qoi.QoiEncoder
        {
            Channels = qoiOptions.Channels?.ToImageSharp(),
            ColorSpace = qoiOptions.ColorSpace?.ToImageSharp(),
            SkipMetadata = skipMetadata
        };
    }

    public string GetOptionsSummary(object? options)
    {
        if (options is QoiEncodingOptions opt)
            return opt.Channels.HasValue ? $"{opt.Channels}" : "Auto";
        return "Default";
    }

    public bool IsValidOptionsType(object? options)
    {
        return options == null || options is QoiEncodingOptions;
    }
}

#endregion
