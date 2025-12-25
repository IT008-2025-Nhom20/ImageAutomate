using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Webp;

namespace WebPExtension;

/// <summary>
/// WebP image format implementation for ImageSharp.
/// This class wraps ImageSharp's built-in WebP support and provides
/// integration with ImageAutomate's format registry system.
/// </summary>
public sealed class WebPFormat : IImageFormat
{
    private static readonly Lazy<WebPFormat> _instance = new(() => new WebPFormat());

    /// <summary>
    /// Gets the singleton instance of WebPFormat.
    /// </summary>
    public static WebPFormat Instance => _instance.Value;

    /// <summary>
    /// Initializes a new instance of the WebPFormat class.
    /// </summary>
    public WebPFormat()
    {
    }

    /// <summary>
    /// Gets the name of the format.
    /// </summary>
    public string Name => "WebP";

    /// <summary>
    /// Gets the default MIME type for WebP images.
    /// </summary>
    public string DefaultMimeType => "image/webp";

    /// <summary>
    /// Gets the file extensions associated with WebP format.
    /// </summary>
    public IEnumerable<string> FileExtensions => new[] { "webp" };

    /// <summary>
    /// Gets the MIME types associated with WebP format.
    /// </summary>
    public IEnumerable<string> MimeTypes => new[] { "image/webp" };
}
