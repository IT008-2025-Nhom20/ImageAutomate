using ImageAutomate.Core;
using ImageAutomate.Core.Serialization;
using ImageAutomate.StandardBlocks;

using SixLabors.ImageSharp;

namespace ImageAutomate.Execution.Tests;

/// <summary>
/// Tests for serialization of real StandardBlocks implementations.
/// </summary>
public class StandardBlocksSerializationTests
{
    #region LoadBlock Tests

    [Fact]
    public void LoadBlock_Serialization_RoundTrip()
    {
        // Arrange
        var block = new LoadBlock
        {
            SourcePath = "/test/path",
            AutoOrient = true
        };

        // Act
        var dto = BlockSerializer.Serialize(block);
        var deserialized = BlockSerializer.Deserialize(dto) as LoadBlock;

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(block.Name, deserialized.Name);
        Assert.Equal(block.SourcePath, deserialized.SourcePath);
        Assert.Equal(block.AutoOrient, deserialized.AutoOrient);
        // Note: MaxShipmentSize is not serialized as it lacks [Category] attribute
        // and is set by the executor during initialization
    }

    #endregion

    #region BrightnessBlock Tests

    [Fact]
    public void BrightnessBlock_Serialization_RoundTrip()
    {
        // Arrange
        var block = new BrightnessBlock
        {
            Brightness = 1.5f
        };

        // Act
        var dto = BlockSerializer.Serialize(block);
        var deserialized = BlockSerializer.Deserialize(dto) as BrightnessBlock;

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(block.Name, deserialized.Name);
        Assert.Equal(block.Brightness, deserialized.Brightness, precision: 5);
    }

    #endregion

    #region ResizeBlock Tests

    [Fact]
    public void ResizeBlock_Serialization_RoundTrip()
    {
        // Arrange
        var block = new ResizeBlock
        {
            ResizeMode = ResizeModeOption.Fit,
            TargetWidth = 1920,
            TargetHeight = 1080,
            PreserveAspectRatio = true,
            Resampler = ResizeResampler.Lanczos3,
            PaddingColor = Color.White
        };

        // Act
        var dto = BlockSerializer.Serialize(block);
        var deserialized = BlockSerializer.Deserialize(dto) as ResizeBlock;

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(block.Name, deserialized.Name);
        Assert.Equal(block.ResizeMode, deserialized.ResizeMode);
        Assert.Equal(block.TargetWidth, deserialized.TargetWidth);
        Assert.Equal(block.TargetHeight, deserialized.TargetHeight);
        Assert.Equal(block.PreserveAspectRatio, deserialized.PreserveAspectRatio);
        Assert.Equal(block.Resampler, deserialized.Resampler);
    }

    #endregion

    #region ConvertBlock Tests

    [Fact]
    public void ConvertBlock_Serialization_BasicProperties_RoundTrip()
    {
        // Arrange
        var block = new ConvertBlock
        {
            TargetFormat = "JPEG",
            AlwaysEncode = true
        };

        // Act
        var dto = BlockSerializer.Serialize(block);
        var deserialized = BlockSerializer.Deserialize(dto) as ConvertBlock;

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(block.Name, deserialized.Name);
        Assert.Equal(block.TargetFormat, deserialized.TargetFormat);
        Assert.Equal(block.AlwaysEncode, deserialized.AlwaysEncode);
    }

    [Fact]
    public void ConvertBlock_Serialization_JpegOptions_RoundTrip()
    {
        // Arrange
        var block = new ConvertBlock
        {
            TargetFormat = "JPEG",
            JpegOptions = new JpegEncodingOptions { Quality = 90 }
        };

        // Act
        var dto = BlockSerializer.Serialize(block);
        var deserialized = BlockSerializer.Deserialize(dto) as ConvertBlock;

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(90, deserialized.JpegOptions.Quality);
    }

    [Fact]
    public void ConvertBlock_Serialization_PngOptions_RoundTrip()
    {
        // Arrange
        var block = new ConvertBlock
        {
            TargetFormat = "PNG",
            PngOptions = new PngEncodingOptions { CompressionLevel = PngCompressionLevel.BestCompression }
        };

        // Act
        var dto = BlockSerializer.Serialize(block);
        var deserialized = BlockSerializer.Deserialize(dto) as ConvertBlock;

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(PngCompressionLevel.BestCompression, deserialized.PngOptions.CompressionLevel);
    }

    [Fact]
    public void ConvertBlock_Serialization_GifOptions_RoundTrip()
    {
        // Arrange
        var block = new ConvertBlock
        {
            TargetFormat = "GIF",
            GifOptions = new GifEncodingOptions
            {
                ColorTableMode = GifColorTableMode.Local
            }
        };

        // Act
        var dto = BlockSerializer.Serialize(block);
        var deserialized = BlockSerializer.Deserialize(dto) as ConvertBlock;

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(GifColorTableMode.Local, deserialized.GifOptions.ColorTableMode);
    }

    [Fact]
    public void ConvertBlock_Serialization_TiffOptions_RoundTrip()
    {
        // Arrange
        var block = new ConvertBlock
        {
            TargetFormat = "TIFF",
            TiffOptions = new TiffEncodingOptions { Compression = TiffCompression.Deflate }
        };

        // Act
        var dto = BlockSerializer.Serialize(block);
        var deserialized = BlockSerializer.Deserialize(dto) as ConvertBlock;

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(TiffCompression.Deflate, deserialized.TiffOptions.Compression);
    }

    [Fact]
    public void ConvertBlock_Serialization_WebPOptions_RoundTrip()
    {
        // Arrange
        var block = new ConvertBlock
        {
            TargetFormat = "WebP",
            WebPOptions = new WebPEncodingOptions
            {
                FileFormat = WebpFileFormatType.Lossless,
                Quality = 85
            }
        };

        // Act
        var dto = BlockSerializer.Serialize(block);
        var deserialized = BlockSerializer.Deserialize(dto) as ConvertBlock;

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(WebpFileFormatType.Lossless, deserialized.WebPOptions.FileFormat);
        Assert.Equal(85, deserialized.WebPOptions.Quality);
    }

    #endregion

    #region PipelineGraph with StandardBlocks Tests

    [Fact]
    public void PipelineGraph_WithStandardBlocks_RoundTrip()
    {
        // Arrange
        var graph = new PipelineGraph();
        var loadBlock = new LoadBlock { SourcePath = "/images" };
        var brightnessBlock = new BrightnessBlock { Brightness = 1.2f };
        var convertBlock = new ConvertBlock { TargetFormat = "PNG" };

        graph.AddBlock(loadBlock);
        graph.AddBlock(brightnessBlock);
        graph.AddBlock(convertBlock);

        graph.AddEdge(loadBlock, loadBlock.Outputs[0], brightnessBlock, brightnessBlock.Inputs[0]);
        graph.AddEdge(brightnessBlock, brightnessBlock.Outputs[0], convertBlock, convertBlock.Inputs[0]);

        graph.SelectedItem = brightnessBlock;

        // Act
        var json = graph.ToJson();
        var deserialized = PipelineGraph.FromJson(json);

        // Assert
        Assert.Equal(3, deserialized.Nodes.Count);
        Assert.Equal(2, deserialized.Edges.Count);
        Assert.NotNull(deserialized.SelectedItem);

        var deserializedLoad = deserialized.Nodes[0] as LoadBlock;
        var deserializedBrightness = deserialized.Nodes[1] as BrightnessBlock;
        var deserializedConvert = deserialized.Nodes[2] as ConvertBlock;

        Assert.NotNull(deserializedLoad);
        Assert.NotNull(deserializedBrightness);
        Assert.NotNull(deserializedConvert);

        Assert.Equal("/images", deserializedLoad.SourcePath);
        Assert.Equal(1.2f, deserializedBrightness.Brightness, precision: 5);
        Assert.Equal("PNG", deserializedConvert.TargetFormat);
    }

    [Fact]
    public void Workspace_WithComplexPipeline_SaveAndLoad()
    {
        // Arrange
        var tempFile = Path.Combine(Path.GetTempPath(), $"test_pipeline_{Guid.NewGuid()}.json");
        var workspace = new Workspace(new())
        {
            Name = "Image Processing Pipeline",
            Zoom = 1.25
        };

        var loadBlock = new LoadBlock
        {
            SourcePath = "/input/images",
            AutoOrient = true,
            X = 50,
            Y = 100
        };
        var brightnessBlock = new BrightnessBlock
        {
            Brightness = 1.1f,
            X = 300,
            Y = 100
        };
        var resizeBlock = new ResizeBlock
        {
            ResizeMode = ResizeModeOption.Fit,
            TargetWidth = 1024,
            TargetHeight = 768,
            X = 550,
            Y = 100
        };
        var convertBlock = new ConvertBlock
        {
            TargetFormat = "JPEG",
            JpegOptions = new JpegEncodingOptions { Quality = 85 },
            X = 800,
            Y = 100
        };

        workspace.Graph.AddBlock(loadBlock);
        workspace.Graph.AddBlock(brightnessBlock);
        workspace.Graph.AddBlock(resizeBlock);
        workspace.Graph.AddBlock(convertBlock);

        workspace.Graph.AddEdge(loadBlock, loadBlock.Outputs[0], brightnessBlock, brightnessBlock.Inputs[0]);
        workspace.Graph.AddEdge(brightnessBlock, brightnessBlock.Outputs[0], resizeBlock, resizeBlock.Inputs[0]);
        workspace.Graph.AddEdge(resizeBlock, resizeBlock.Outputs[0], convertBlock, convertBlock.Inputs[0]);

        try
        {
            // Act
            workspace.SaveToFile(tempFile);
            var loaded = Workspace.LoadFromFile(tempFile);

            // Assert
            Assert.Equal(workspace.Name, loaded.Name);
            Assert.NotNull(loaded.Graph);
            Assert.Equal(4, loaded.Graph.Nodes.Count);
            Assert.Equal(3, loaded.Graph.Edges.Count);
            Assert.Equal(1.25, loaded.Zoom);

            var loadedLoadBlock = loaded.Graph.Nodes[0] as LoadBlock;
            var loadedConvertBlock = loaded.Graph.Nodes[3] as ConvertBlock;

            Assert.NotNull(loadedLoadBlock);
            Assert.NotNull(loadedConvertBlock);
            Assert.Equal("/input/images", loadedLoadBlock.SourcePath);
            Assert.Equal("JPEG", loadedConvertBlock.TargetFormat);
            Assert.Equal(85, loadedConvertBlock.JpegOptions.Quality);

            var pos = loaded.Graph.Nodes[0];
            Assert.NotNull(pos);
            Assert.Equal(50, pos.X);
            Assert.Equal(100, pos.Y);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    #endregion

    #region SaveBlock Tests

    [Fact]
    public void SaveBlock_Serialization_RoundTrip()
    {
        // Arrange
        var block = new SaveBlock
        {
            OutputPath = "/output",
            Overwrite = true,
            CreateDirectory = false
        };

        // Act
        var dto = BlockSerializer.Serialize(block);
        var deserialized = BlockSerializer.Deserialize(dto) as SaveBlock;

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(block.OutputPath, deserialized.OutputPath);
        Assert.Equal(block.Overwrite, deserialized.Overwrite);
        Assert.Equal(block.CreateDirectory, deserialized.CreateDirectory);
    }

    #endregion
}
