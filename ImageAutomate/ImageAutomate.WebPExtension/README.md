# ImageAutomate WebP Format Extension

This is a plugin extension for the ImageAutomate image processing framework that provides WebP format encoding capabilities.

## Overview

The WebP Format Extension is a sample plugin that demonstrates how to extend ImageAutomate with new image format support. It provides a block (`WebPFormatExtension`) that can be used in image processing pipelines to convert images to the WebP format with configurable encoding options.

## Features

- **WebP Encoding**: Convert images to WebP format with full control over encoding parameters
- **Lossless and Lossy Modes**: Support for both lossless and lossy compression
- **Configurable Quality**: Adjust quality settings from 0 to 100
- **Encoding Methods**: Choose between fastest, default, and best quality encoding methods
- **Near-Lossless Support**: Fine-tune lossless encoding with near-lossless quality settings
- **Plugin Architecture**: Implements `IBlock` and `IPluginUnloadable` for seamless integration

## Components

### WebPFormatExtension

The main block class that processes images and applies WebP encoding metadata.

**Properties:**
- `Options`: WebPOptions instance containing all encoding configuration

**Sockets:**
- Input: `WebP.In` - Accepts image work items
- Output: `WebP.Out` - Outputs processed work items with WebP encoding metadata

### WebPOptions

Configuration class for WebP encoding parameters.

**Properties:**
- `Lossless` (bool): Enable lossless compression (default: false)
- `Quality` (float): Quality factor 0.0-100.0 (default: 75)
- `FileFormat` (enum): Lossy or Lossless format type
- `Method` (enum): Encoding method - Fastest, Default, or BestQuality
- `NearLossless` (int): Near-lossless quality 0-100 (default: 100)

## Usage

### As a Plugin

1. Build the project:
   ```bash
   dotnet build ImageAutomate.WebPExtension.csproj -c Release
   ```

2. The compiled DLL can be loaded as a plugin in the ImageAutomate application through the Plugin Manager.

3. Once loaded, the WebP Format block will be available in the block palette and can be added to processing pipelines.

### In Code

```csharp
using ImageAutomate.WebPExtension;
using ImageAutomate.Core;

// Create the extension
var webpExtension = new WebPFormatExtension();

// Configure options
webpExtension.Options.Lossless = false;
webpExtension.Options.Quality = 85f;
webpExtension.Options.Method = WebPEncodingMethod.BestQuality;

// Use in a pipeline
var inputs = new Dictionary<string, IReadOnlyList<IBasicWorkItem>>
{
    { "WebP.In", workItems }
};

var outputs = webpExtension.Execute(inputs);
```

## Integration with ConvertBlock

While this extension is currently a standalone plugin, it is designed to be integrated with the `ConvertBlock` in the future. The extension provides:

- Compatible encoding options structure
- Metadata tagging for downstream processing
- Standard ImageSharp WebP encoder configuration

Once the ConvertBlock supports dynamic format extension registration, this plugin can be seamlessly integrated to provide WebP encoding capabilities within the Convert block.

## Testing

The extension includes comprehensive unit tests covering:

- Block creation and properties
- Option configuration and validation
- Work item processing
- Metadata application
- Plugin unloadability

Run tests with:
```bash
dotnet test ImageAutomate.WebPExtension.Tests/ImageAutomate.WebPExtension.Tests.csproj
```

## Dependencies

- .NET 9.0
- ImageAutomate.Core
- SixLabors.ImageSharp 3.1.12

## License

This extension follows the same license as the ImageAutomate project.
