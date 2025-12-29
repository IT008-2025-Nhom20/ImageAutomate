using ImageAutomate.Core;
using ImageAutomate.Infrastructure;

namespace ImageAutomate.StandardBlocks;

/// <summary>
/// Initializes the built-in format registry with standard formats.
/// </summary>
public static class FormatRegistryInitializer
{
    /// <summary>
    /// Registers all built-in format strategies with the singleton registry.
    /// </summary>
    /// <param name="registry">Optional registry to initialize. If null, uses the shared singleton.</param>
    public static void InitializeBuiltInFormats(IImageFormatRegistry? registry = null)
    {
        // Use provided registry or default to singleton
        var targetRegistry = registry ?? ImageFormatRegistry.Instance;

        // Register formats - multiple calls are safe (will overwrite/update)
        targetRegistry.RegisterFormat("JPEG", new JpegFormatStrategy());
        targetRegistry.RegisterFormat("PNG", new PngFormatStrategy());
        targetRegistry.RegisterFormat("BMP", new BmpFormatStrategy());
        targetRegistry.RegisterFormat("GIF", new GifFormatStrategy());
        targetRegistry.RegisterFormat("TIFF", new TiffFormatStrategy());
        targetRegistry.RegisterFormat("TGA", new TgaFormatStrategy());
        targetRegistry.RegisterFormat("WebP", new WebPFormatStrategy());
        targetRegistry.RegisterFormat("PBM", new PbmFormatStrategy());
        targetRegistry.RegisterFormat("QOI", new QoiFormatStrategy());
    }
}
