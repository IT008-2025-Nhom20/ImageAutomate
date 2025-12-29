using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using ImageAutomate.Core;

namespace WebPExtension;

/// <summary>
/// Handles registration of the WebP format extension with ImageSharp and ImageAutomate Core.
/// </summary>
public static class WebPFormatRegistration
{
    private static volatile bool _isRegistered = false;
    private static readonly object _lock = new object();

    /// <summary>
    /// Registers the WebP format with ImageSharp's configuration and ImageAutomate's format registry.
    /// This method should be called once during application startup.
    /// </summary>
    /// <param name="registry">The ImageAutomate format registry (optional, for future use)</param>
    public static void RegisterWebPFormat(IImageFormatRegistry? registry = null)
    {
        lock (_lock)
        {
            if (_isRegistered)
                return;

            // Step 1: Register with ImageSharp's global configuration
            // ImageSharp already has WebP support built-in, but we ensure our custom format is recognized
            RegisterWithImageSharp();

            // Step 2: Register with ImageAutomate's format registry (if provided)
            if (registry != null)
            {
                RegisterWithImageAutomate(registry);
            }

            _isRegistered = true;
        }
    }

    /// <summary>
    /// Registers the WebP format with ImageSharp's configuration.
    /// </summary>
    private static void RegisterWithImageSharp()
    {
        // ImageSharp 3.x has WebP support built-in, so we just need to ensure
        // our configuration is aligned with the default configuration.
        // The Configuration.Default already includes WebP format.
        
        // For custom configurations, you would do:
        // var config = new Configuration();
        // config.ImageFormatsManager.AddImageFormat(WebPFormat.Instance);
        
        // Since we're using the default configuration, WebP is already available.
    }

    /// <summary>
    /// Registers the WebP format with ImageAutomate's format registry.
    /// This allows ConvertBlock and LoadBlock to recognize and use WebP through the registry.
    /// </summary>
    private static void RegisterWithImageAutomate(IImageFormatRegistry registry)
    {
        // Note: ImageSharp's WebpDecoder doesn't have a public constructor in 3.x
        // The decoder is handled automatically by ImageSharp's configuration
        // We register the encoder factory and indicate decoding support through the format
        registry.RegisterFormat<WebPFormat, WebpEncoder, object, WebPOptions>(
            "WebP",
            encoderFactory: (options) => WebPFormatFactory.CreateEncoder(options),
            decoderFactory: () => WebPFormatFactory.GetDecoder()
        );
    }

    /// <summary>
    /// Creates a WebP encoder with the specified options.
    /// </summary>
    /// <param name="options">WebP encoding options</param>
    /// <returns>A configured WebP encoder</returns>
    public static WebpEncoder CreateEncoder(WebPOptions? options = null)
    {
        return WebPFormatFactory.CreateEncoder(options);
    }

    /// <summary>
    /// Gets the WebP decoder support indicator.
    /// Note: In ImageSharp 3.x, WebP decoding is handled automatically by the configuration.
    /// </summary>
    /// <returns>Format indicator for WebP decoding support</returns>
    public static object GetDecoder()
    {
        return WebPFormatFactory.GetDecoder();
    }

    /// <summary>
    /// Gets whether the WebP format has been registered.
    /// </summary>
    public static bool IsRegistered => _isRegistered;
}
