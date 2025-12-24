using ImageAutomate.Core;
using ImageAutomate.WebPExtension;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ImageAutomate.WebPExtension.Tests;

/// <summary>
/// Tests for the WebP Format Extension plugin.
/// </summary>
public class WebPFormatExtensionTests
{
    [Fact]
    public void WebPFormatExtension_Should_Have_Correct_Name()
    {
        // Arrange
        using var extension = new WebPFormatExtension();

        // Act & Assert
        Assert.Equal("WebPFormatExtension", extension.Name);
        Assert.Equal("WebP Format", extension.Title);
    }

    [Fact]
    public void WebPFormatExtension_Should_Have_Input_And_Output_Sockets()
    {
        // Arrange
        using var extension = new WebPFormatExtension();

        // Act & Assert
        Assert.Single(extension.Inputs);
        Assert.Single(extension.Outputs);
        Assert.Equal("WebP.In", extension.Inputs[0].Id);
        Assert.Equal("WebP.Out", extension.Outputs[0].Id);
    }

    [Fact]
    public void WebPOptions_Should_Have_Default_Values()
    {
        // Arrange & Act
        var options = new WebPOptions();

        // Assert
        Assert.False(options.Lossless);
        Assert.Equal(75f, options.Quality);
        Assert.Equal(WebPFileFormatType.Lossy, options.FileFormat);
        Assert.Equal(WebPEncodingMethod.Default, options.Method);
        Assert.Equal(100, options.NearLossless);
    }

    [Fact]
    public void WebPOptions_Quality_Should_Be_Clamped()
    {
        // Arrange
        var options = new WebPOptions();

        // Act - test upper bound
        options.Quality = 150f;
        Assert.Equal(100f, options.Quality);

        // Act - test lower bound
        options.Quality = -10f;
        Assert.Equal(0f, options.Quality);

        // Act - test valid value
        options.Quality = 85f;
        Assert.Equal(85f, options.Quality);
    }

    [Fact]
    public void WebPOptions_NearLossless_Should_Be_Clamped()
    {
        // Arrange
        var options = new WebPOptions();

        // Act - test upper bound
        options.NearLossless = 150;
        Assert.Equal(100, options.NearLossless);

        // Act - test lower bound
        options.NearLossless = -10;
        Assert.Equal(0, options.NearLossless);

        // Act - test valid value
        options.NearLossless = 50;
        Assert.Equal(50, options.NearLossless);
    }

    [Fact]
    public void WebPFormatExtension_Execute_Should_Process_WorkItem()
    {
        // Arrange
        using var extension = new WebPFormatExtension();
        using var image = new Image<Rgba32>(100, 100);
        var workItem = new WorkItem(image);
        
        var inputs = new Dictionary<string, IReadOnlyList<IBasicWorkItem>>
        {
            { "WebP.In", new List<IBasicWorkItem> { workItem } }
        };

        // Act
        var outputs = extension.Execute(inputs);

        // Assert
        Assert.Single(outputs);
        Assert.Contains(extension.Outputs[0], outputs.Keys);
        var outputItems = outputs[extension.Outputs[0]];
        Assert.Single(outputItems);
        
        var outputWorkItem = outputItems[0] as WorkItem;
        Assert.NotNull(outputWorkItem);
        Assert.Equal("WebP", outputWorkItem.Metadata["Format"]);
        Assert.True(outputWorkItem.Metadata.ContainsKey("EncodingOptions"));
        Assert.True(outputWorkItem.Metadata.ContainsKey("WebPOptions"));
    }

    [Fact]
    public void WebPFormatExtension_Execute_Should_Process_Multiple_WorkItems()
    {
        // Arrange
        using var extension = new WebPFormatExtension();
        using var image1 = new Image<Rgba32>(100, 100);
        using var image2 = new Image<Rgba32>(200, 200);
        var workItem1 = new WorkItem(image1);
        var workItem2 = new WorkItem(image2);
        
        var inputs = new Dictionary<string, IReadOnlyList<IBasicWorkItem>>
        {
            { "WebP.In", new List<IBasicWorkItem> { workItem1, workItem2 } }
        };

        // Act
        var outputs = extension.Execute(inputs);

        // Assert
        Assert.Single(outputs);
        var outputItems = outputs[extension.Outputs[0]];
        Assert.Equal(2, outputItems.Count);
    }

    [Fact]
    public void WebPFormatExtension_Should_Apply_Lossless_Setting()
    {
        // Arrange
        using var extension = new WebPFormatExtension();
        extension.Options.Lossless = true;
        extension.Options.Quality = 90f;

        using var image = new Image<Rgba32>(100, 100);
        var workItem = new WorkItem(image);
        
        var inputs = new Dictionary<string, IReadOnlyList<IBasicWorkItem>>
        {
            { "WebP.In", new List<IBasicWorkItem> { workItem } }
        };

        // Act
        var outputs = extension.Execute(inputs);

        // Assert
        var outputWorkItem = outputs[extension.Outputs[0]][0] as WorkItem;
        Assert.NotNull(outputWorkItem);
        Assert.True(outputWorkItem.Metadata.ContainsKey("WebPOptions"));
        
        var options = outputWorkItem.Metadata["WebPOptions"] as WebPOptions;
        Assert.NotNull(options);
        Assert.True(options.Lossless);
        Assert.Equal(90f, options.Quality);
    }

    [Fact]
    public void WebPFormatExtension_Should_Implement_IPluginUnloadable()
    {
        // Arrange
        using var extension = new WebPFormatExtension();

        // Act
        var canUnload = extension.OnUnloadRequested();

        // Assert
        Assert.True(canUnload);
    }

    [Fact]
    public void WebPFormatExtension_Content_Should_Reflect_Options()
    {
        // Arrange
        using var extension = new WebPFormatExtension();

        // Act - Lossy mode
        extension.Options.Lossless = false;
        extension.Options.Quality = 85f;
        extension.Options.Method = WebPEncodingMethod.BestQuality;
        var lossyContent = extension.Content;

        // Assert
        Assert.Contains("Lossy", lossyContent);
        Assert.Contains("85", lossyContent);
        Assert.Contains("BestQuality", lossyContent);

        // Act - Lossless mode
        extension.Options.Lossless = true;
        extension.Options.NearLossless = 95;
        var losslessContent = extension.Content;

        // Assert
        Assert.Contains("Lossless", losslessContent);
        Assert.Contains("95", losslessContent);
    }

    [Fact]
    public void WebPOptions_ToString_Should_Return_Correct_String()
    {
        // Arrange
        var options = new WebPOptions();

        // Act - Lossy
        options.Lossless = false;
        options.Quality = 80f;
        options.Method = WebPEncodingMethod.Default;
        var lossyString = options.ToString();

        // Assert
        Assert.Contains("Quality", lossyString);
        Assert.Contains("80", lossyString);

        // Act - Lossless
        options.Lossless = true;
        var losslessString = options.ToString();

        // Assert
        Assert.Equal("Lossless", losslessString);
    }

    [Fact]
    public void WebPFormatExtension_Execute_Should_Throw_On_Missing_Input()
    {
        // Arrange
        using var extension = new WebPFormatExtension();
        var inputs = new Dictionary<string, IReadOnlyList<IBasicWorkItem>>();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => extension.Execute(inputs));
    }
}
