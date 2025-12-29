using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;

namespace WebPExtension;

/// <summary>
/// Factory class for creating WebP encoders and decoders with ImageAutomate options.
/// This provides the bridge between ImageAutomate's WebPOptions and ImageSharp's WebP support.
/// </summary>
public static class WebPFormatFactory
{
    /// <summary>
    /// Creates a WebP encoder configured with the specified options.
    /// </summary>
    /// <param name="options">WebP encoding options</param>
    /// <returns>A configured WebpEncoder from ImageSharp</returns>
    public static WebpEncoder CreateEncoder(WebPOptions? options = null)
    {
        options ??= new WebPOptions();
        
        return new WebpEncoder
        {
            Quality = (int)Math.Round(options.Quality),
            Method = (WebpEncodingMethod)(int)options.Method,
            FileFormat = options.Lossless 
                ? WebpFileFormatType.Lossless 
                : WebpFileFormatType.Lossy,
            // NearLossless is only enabled in lossless mode with quality < 100
            // It allows some quality loss for better compression in lossless mode
            NearLossless = options.Lossless && options.NearLossless < 100,
            NearLosslessQuality = options.NearLossless,
            UseAlphaCompression = options.UseAlphaCompression
        };
    }

    /// <summary>
    /// Gets the WebP format indicator.
    /// Since ImageSharp's WebpDecoder doesn't have a public constructor in version 3.x,
    /// decoding is handled automatically by ImageSharp's configuration.
    /// </summary>
    /// <returns>The WebP format instance indicating decoding support</returns>
    public static object GetDecoder()
    {
        // ImageSharp's WebpDecoder doesn't have a public constructor in version 3.x
        // The decoder is registered with the Configuration and used automatically
        // We return the WebPFormat instance to indicate WebP support is available
        return WebPFormat.Instance;
    }
}

