using ImageAutomate.Core;
using ImageAutomate.WebPExtension;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ImageAutomate.WebPExtension.Examples;

/// <summary>
/// Example demonstrating how to use the WebP Format Extension plugin.
/// This is a reference implementation showing the plugin's capabilities.
/// </summary>
public static class WebPExtensionUsageExample
{
    /// <summary>
    /// Example 1: Basic usage with lossy compression.
    /// </summary>
    public static void BasicLossyCompression()
    {
        // Create the extension block
        using var webpExtension = new WebPFormatExtension();

        // Configure for lossy compression with high quality
        webpExtension.Options.Lossless = false;
        webpExtension.Options.Quality = 85f;
        webpExtension.Options.Method = WebPEncodingMethod.BestQuality;

        // Create a sample image and work item
        using var image = new Image<Rgba32>(800, 600);
        var workItem = new WorkItem(image);

        // Prepare inputs
        var inputs = new Dictionary<string, IReadOnlyList<IBasicWorkItem>>
        {
            { "WebP.In", new List<IBasicWorkItem> { workItem } }
        };

        // Execute the block
        var outputs = webpExtension.Execute(inputs);

        // Access the output
        var outputItems = outputs[webpExtension.Outputs[0]];
        var outputWorkItem = outputItems[0] as WorkItem;

        // The output work item now has WebP encoding metadata
        // that can be used by downstream blocks like SaveBlock
    }

    /// <summary>
    /// Example 2: Lossless compression.
    /// </summary>
    public static void LosslessCompression()
    {
        using var webpExtension = new WebPFormatExtension();

        // Configure for lossless compression
        webpExtension.Options.Lossless = true;
        webpExtension.Options.NearLossless = 95; // 95 = near-lossless quality

        // Use the block in your pipeline...
    }

    /// <summary>
    /// Example 3: Batch processing multiple images.
    /// </summary>
    public static void BatchProcessing()
    {
        using var webpExtension = new WebPFormatExtension();
        webpExtension.Options.Quality = 80f;

        // Create multiple work items
        var workItems = new List<IBasicWorkItem>();
        
        for (int i = 0; i < 5; i++)
        {
            var image = new Image<Rgba32>(100, 100);
            workItems.Add(new WorkItem(image));
        }

        var inputs = new Dictionary<string, IReadOnlyList<IBasicWorkItem>>
        {
            { "WebP.In", workItems }
        };

        // Process all images at once
        var outputs = webpExtension.Execute(inputs);
        
        // Cleanup
        foreach (var item in workItems)
        {
            item.Dispose();
        }
    }

    /// <summary>
    /// Example 4: Different encoding methods for speed vs quality tradeoff.
    /// </summary>
    public static void EncodingMethodComparison()
    {
        // Fastest encoding (lower quality)
        using var fastExtension = new WebPFormatExtension();
        fastExtension.Options.Method = WebPEncodingMethod.Fastest;
        fastExtension.Options.Quality = 75f;

        // Best quality encoding (slower)
        using var qualityExtension = new WebPFormatExtension();
        qualityExtension.Options.Method = WebPEncodingMethod.BestQuality;
        qualityExtension.Options.Quality = 90f;

        // Use appropriate extension based on your needs
    }

    /// <summary>
    /// Example 5: Accessing metadata after processing.
    /// </summary>
    public static void MetadataAccess()
    {
        using var webpExtension = new WebPFormatExtension();
        using var image = new Image<Rgba32>(100, 100);
        var workItem = new WorkItem(image);

        var inputs = new Dictionary<string, IReadOnlyList<IBasicWorkItem>>
        {
            { "WebP.In", new List<IBasicWorkItem> { workItem } }
        };

        var outputs = webpExtension.Execute(inputs);
        var outputWorkItem = outputs[webpExtension.Outputs[0]][0] as WorkItem;

        if (outputWorkItem != null)
        {
            // Access the metadata
            var format = outputWorkItem.Metadata["Format"]; // "WebP"
            var encodingOptions = outputWorkItem.Metadata["EncodingOptions"]; // WebpEncoder instance
            var webpOptions = outputWorkItem.Metadata["WebPOptions"] as WebPOptions;

            // Use the metadata as needed
        }
    }
}
