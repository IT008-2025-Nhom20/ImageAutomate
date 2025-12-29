using ImageAutomate.Core;
using ImageAutomate.Infrastructure;

namespace ImageAutomate.StandardBlocks.Tests;

public class StandardBlocksTests : StandardBlockTestBase
{
    // Helper to generate tests for a block
    private async Task TestBlock(IBlock block, string resolution, int count, int batchSize)
    {
        await RunBlockTest(block, resolution, count, batchSize);
    }

    public static IEnumerable<object[]> TestConfigs =>
        new List<object[]>
        {
            // Resolution, Count, Batch
            // Medium Workload
            new object[] { "480p", 20, 5 },
            new object[] { "720p", 20, 5 },
            new object[] { "1080p", 20, 5 },
            new object[] { "2160p", 20, 5 },
            // High Workload
            new object[] { "480p", 100, 10 },
            new object[] { "720p", 100, 10 },
            new object[] { "1080p", 100, 10 },
            new object[] { "2160p", 100, 10 },
        };

    [Theory]
    [MemberData(nameof(TestConfigs))]
    public async Task BrightnessBlock(string res, int count, int batch)
    {
        var block = new BrightnessBlock { Brightness = 1.2f };
        await TestBlock(block, res, count, batch);
    }

    [Theory]
    [MemberData(nameof(TestConfigs))]
    public async Task ContrastBlock(string res, int count, int batch)
    {
        var block = new ContrastBlock { Contrast = 1.2f };
        await TestBlock(block, res, count, batch);
    }

    [Theory]
    [MemberData(nameof(TestConfigs))]
    public async Task ConvertBlock(string res, int count, int batch)
    {
        FormatRegistryInitializer.InitializeBuiltInFormats(ImageFormatRegistry.Instance);
        // Test conversion to JPEG
        var block = new ConvertBlock
        {
            TargetFormat = "JPEG",
            JpegOptions = new JpegEncodingOptions { Quality = 50 }
        };
        await TestBlock(block, res, count, batch);
    }

    [Theory]
    [MemberData(nameof(TestConfigs))]
    public async Task CropBlock(string res, int count, int batch)
    {
        // Crop center
        var block = new CropBlock { CropWidth = 100, CropHeight = 100 };
        await TestBlock(block, res, count, batch);
    }

    [Theory]
    [MemberData(nameof(TestConfigs))]
    public async Task FlipBlock(string res, int count, int batch)
    {
        var block = new FlipBlock { FlipMode = FlipModeOption.Horizontal };
        await TestBlock(block, res, count, batch);
    }

    [Theory]
    [MemberData(nameof(TestConfigs))]
    public async Task GaussianBlurBlock(string res, int count, int batch)
    {
        var block = new GaussianBlurBlock { Sigma = 3f };
        await TestBlock(block, res, count, batch);
    }

    [Theory]
    [MemberData(nameof(TestConfigs))]
    public async Task GrayscaleBlock(string res, int count, int batch)
    {
        var block = new GrayscaleBlock { GrayscaleOption = GrayscaleOptions.Bt709 };
        await TestBlock(block, res, count, batch);
    }

    [Theory]
    [MemberData(nameof(TestConfigs))]
    public async Task HueBlock(string res, int count, int batch)
    {
        var block = new HueBlock { HueShift = 90f };
        await TestBlock(block, res, count, batch);
    }

    [Theory]
    [MemberData(nameof(TestConfigs))]
    public async Task PixelateBlock(string res, int count, int batch)
    {
        var block = new PixelateBlock { Size = 10 };
        await TestBlock(block, res, count, batch);
    }

    [Theory]
    [MemberData(nameof(TestConfigs))]
    public async Task ResizeBlock(string res, int count, int batch)
    {
        var block = new ResizeBlock { TargetWidth = 200, TargetHeight = 200 };
        await TestBlock(block, res, count, batch);
    }

    [Theory]
    [MemberData(nameof(TestConfigs))]
    public async Task SaturationBlock(string res, int count, int batch)
    {
        var block = new SaturationBlock { Saturation = 1.5f };
        await TestBlock(block, res, count, batch);
    }

    [Theory]
    [MemberData(nameof(TestConfigs))]
    public async Task SharpenBlock(string res, int count, int batch)
    {
        var block = new SharpenBlock { Amount = 1.5f };
        await TestBlock(block, res, count, batch);
    }

    [Theory]
    [MemberData(nameof(TestConfigs))]
    public async Task VignetteBlock(string res, int count, int batch)
    {
        var block = new VignetteBlock { Color = SixLabors.ImageSharp.Color.Black };
        await TestBlock(block, res, count, batch);
    }
}
