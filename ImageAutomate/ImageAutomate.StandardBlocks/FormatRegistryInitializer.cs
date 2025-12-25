using ImageAutomate.Core;

namespace ImageAutomate.StandardBlocks;

/// <summary>
/// Initializes the built-in format registry with standard formats.
/// </summary>
public static class FormatRegistryInitializer
{
    private static bool _initialized = false;
    private static readonly object _lock = new();

    /// <summary>
    /// Registers all built-in format strategies with the registry.
    /// </summary>
    /// <param name="registry">The format registry to initialize.</param>
    /// <exception cref="ArgumentNullException">Thrown when registry is null.</exception>
    public static void InitializeBuiltInFormats(IImageFormatRegistry registry)
    {
        if (registry == null)
        {
            throw new ArgumentNullException(nameof(registry));
        }

        lock (_lock)
        {
            if (_initialized)
            {
                return;
            }

            registry.RegisterFormat("JPEG", new JpegFormatStrategy());
            registry.RegisterFormat("PNG", new PngFormatStrategy());
            registry.RegisterFormat("BMP", new BmpFormatStrategy());
            registry.RegisterFormat("GIF", new GifFormatStrategy());
            registry.RegisterFormat("TIFF", new TiffFormatStrategy());
            registry.RegisterFormat("TGA", new TgaFormatStrategy());
            registry.RegisterFormat("WebP", new WebPFormatStrategy());
            registry.RegisterFormat("PBM", new PbmFormatStrategy());
            registry.RegisterFormat("QOI", new QoiFormatStrategy());

            _initialized = true;
        }
    }
}
