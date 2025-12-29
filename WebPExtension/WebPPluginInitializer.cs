using ImageAutomate.Core;

namespace WebPExtension;

/// <summary>
/// Plugin initializer for the WebP format extension.
/// This class provides an entry point for automatic plugin initialization
/// and can be discovered by the plugin loader through the MANIFEST.json file.
/// </summary>
public static class WebPPluginInitializer
{
    private static bool _initialized = false;
    private static readonly object _initLock = new object();

    /// <summary>
    /// Initializes the WebP format extension plugin.
    /// This method is called automatically by the plugin loader if specified in the manifest.
    /// Can also be called manually for explicit initialization.
    /// </summary>
    /// <param name="registry">Optional format registry to register with. If null, only ImageSharp registration is performed.</param>
    /// <returns>True if initialization was successful or already initialized, false otherwise.</returns>
    public static bool Initialize(IImageFormatRegistry? registry = null)
    {
        lock (_initLock)
        {
            if (_initialized)
            {
                return true; // Already initialized
            }

            try
            {
                // Register the WebP format with ImageSharp and the registry
                WebPFormatRegistration.RegisterWebPFormat(registry);
                
                _initialized = true;
                return true;
            }
            catch (Exception ex)
            {
                // Log the full exception for debugging
                Console.Error.WriteLine($"Failed to initialize WebP format extension: {ex.Message}");
                Console.Error.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.Error.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                return false;
            }
        }
    }

    /// <summary>
    /// Gets whether the plugin has been initialized.
    /// </summary>
    public static bool IsInitialized => _initialized;

    /// <summary>
    /// Gets metadata about the WebP format extension.
    /// This can be used by the plugin loader to display information about the plugin.
    /// </summary>
    public static PluginMetadata GetMetadata()
    {
        return new PluginMetadata
        {
            Name = "WebP Format Extension",
            Version = "1.0.0",
            Description = "Provides WebP image format support with configurable encoding options",
            FormatName = "WebP",
            FileExtensions = [".webp"],
            MimeTypes = ["image/webp"],
            SupportsEncoding = true,
            SupportsDecoding = true,
            ConfigurationTypes = [typeof(WebPOptions)]
        };
    }

    /// <summary>
    /// Resets the initialization state. Useful for testing or re-initialization scenarios.
    /// </summary>
    internal static void Reset()
    {
        lock (_initLock)
        {
            _initialized = false;
        }
    }
}

/// <summary>
/// Metadata about a format extension plugin.
/// </summary>
public class PluginMetadata
{
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string FormatName { get; set; } = string.Empty;
    public string[] FileExtensions { get; set; } = Array.Empty<string>();
    public string[] MimeTypes { get; set; } = Array.Empty<string>();
    public bool SupportsEncoding { get; set; }
    public bool SupportsDecoding { get; set; }
    public Type[] ConfigurationTypes { get; set; } = Array.Empty<Type>();
}
