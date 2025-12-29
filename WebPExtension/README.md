# ImageAutomate WebP Format Extension

A proper ImageSharp format extension for WebP that integrates with ImageAutomate's format registry system and plugin loader.

## Overview

This extension implements the necessary contracts to make WebP available to `ConvertBlock` and `LoadBlock` through a central format registry. It follows ImageSharp's extension protocol and provides a bridge to ImageAutomate's block system.

## Plugin System Integration

This extension is packaged as a loadable plugin with automatic initialization support.

### Plugin Structure

The plugin includes:
- **MANIFEST.json** - Plugin metadata and configuration
  - Specifies entry point DLL
  - Defines initialization class
  - Provides format metadata
- **WebPPluginInitializer** - Automatic initialization entry point
  - Called by plugin loader on load
  - Handles format registration
  - Provides plugin metadata

### Loading the Plugin

The plugin can be loaded in multiple ways:

**1. Using PluginLoader (Production)**
```csharp
var pluginLoader = new PluginLoader();

// Load from directory (with MANIFEST.json)
var plugin = pluginLoader.LoadPluginFromDirectory("path/to/ImageAutomate.WebPExtension");

// Or load directly from DLL
var plugin = pluginLoader.LoadPlugin("ImageAutomate.WebPExtension.dll");

// Initialize the format extension
WebPPluginInitializer.Initialize(registry);
```

**2. Using Plugin Directory Discovery**
```csharp
var pluginLoader = new PluginLoader();

// Discover and load all plugins from a directory
var plugins = pluginLoader.LoadAllPlugins("./plugins");

// The WebP extension will be automatically discovered if it has MANIFEST.json
```

**3. Manual Initialization (for testing)**
```csharp
using ImageAutomate.WebPExtension;

// Initialize without plugin loader
WebPPluginInitializer.Initialize(registry);
```

### MANIFEST.json

The plugin includes a manifest file that tells the plugin loader how to handle it:

```json
{
  "Name": "ImageAutomate.WebPExtension",
  "EntryPoint": "ImageAutomate.WebPExtension.dll",
  "Version": "1.0.0",
  "Description": "WebP format extension...",
  "Metadata": {
    "FormatName": "WebP",
    "PluginType": "FormatExtension",
    "InitializationClass": "ImageAutomate.WebPExtension.WebPPluginInitializer"
  }
}
```

## Architecture

### Components

1. **WebPFormat** - Implements `IImageFormat` from ImageSharp
   - Singleton instance providing format metadata
   - File extensions: `.webp`
   - MIME type: `image/webp`

2. **WebPFormatFactory** - Factory for encoder/decoder creation
   - Creates `WebpEncoder` from ImageSharp with `WebPOptions`
   - Returns format indicator for decoder (ImageSharp handles automatically)

3. **WebPOptions** - Configuration class for ImageAutomate
   - Lossless/Lossy mode selection
   - Quality control (0-100)
   - Encoding method (Fastest/Default/BestQuality)
   - Near-lossless quality
   - Alpha compression control
   - Implements `INotifyPropertyChanged` for UI binding

4. **WebPFormatRegistration** - Registration utility
   - Registers format with ImageSharp
   - Registers with ImageAutomate's format registry
   - Provides factory methods for encoder/decoder creation

5. **WebPPluginInitializer** - Plugin initialization entry point
   - Automatic initialization when plugin is loaded
   - Provides plugin metadata
   - Thread-safe initialization

6. **CoreImitation** - Simulated format registry interface
   - Defines `IImageFormatRegistry` interface
   - Provides dummy implementation for type checking
   - Located in `ImageAutomate.Core` namespace (as requested)

## Format Registry System

The format registry provides a central mapping of:
- Format Name → IImageFormat
- Format Name → Encoder Factory (with options)
- Format Name → Decoder Factory

This allows blocks like `ConvertBlock` and `LoadBlock` to:
1. Discover available formats
2. Get appropriate encoders with options
3. Get appropriate decoders
4. Map file extensions to formats

## Usage

### As a Plugin (Recommended)

```csharp
// In application startup
var pluginLoader = new PluginLoader();
var registry = new ImageFormatRegistry();

// Load WebP extension plugin
var webpPlugin = pluginLoader.LoadPluginFromDirectory("./plugins/ImageAutomate.WebPExtension");

// Initialize the extension with registry
WebPPluginInitializer.Initialize(registry);

// Now WebP is available through the registry
var encoder = registry.GetEncoder("WebP", new WebPOptions { Quality = 90 });
```

### Manual Registration

```csharp
using ImageAutomate.WebPExtension;
using ImageAutomate.Core;

// Create or get the format registry
var registry = new ImageFormatRegistry();

// Register WebP format
WebPFormatRegistration.RegisterWebPFormat(registry);
```

### Creating Encoders

```csharp
// With default options
var encoder = WebPFormatRegistration.CreateEncoder();

// With custom options
var options = new WebPOptions
{
    Lossless = false,
    Quality = 85f,
    Method = WebPEncodingMethod.BestQuality
};
var encoder = WebPFormatRegistration.CreateEncoder(options);

// Use the encoder
using var image = Image.Load("input.png");
using var output = File.Create("output.webp");
encoder.Encode(image, output, CancellationToken.None);
```

### Creating Decoders

```csharp
var decoder = WebPFormatRegistration.CreateDecoder();

using var input = File.OpenRead("input.webp");
using var image = decoder.Decode(DecoderOptions.Default, input, CancellationToken.None);
```

### Using with Format Registry

```csharp
// Get encoder from registry
var encoder = registry.GetEncoder("WebP", new WebPOptions { Quality = 90f });

// Get decoder from registry
var decoder = registry.GetDecoder("WebP");

// Check if format is registered
bool isAvailable = registry.IsFormatRegistered("WebP");
```

## Integration with ConvertBlock

Once `ConvertBlock` is updated to use the format registry, it will:

1. Query the registry for available formats
2. Get the appropriate encoder based on target format
3. Apply user-configured options (WebPOptions)
4. Encode the image with the configured encoder

Example future integration:
```csharp
// In ConvertBlock.Execute()
var formatName = TargetFormat.ToString();
var encoder = _formatRegistry.GetEncoder(formatName, _webpOptions);
if (encoder != null)
{
    workItem.Image.Save(stream, encoder);
}
```

## Integration with LoadBlock

`LoadBlock` can use the registry to:

1. Detect format from file extension
2. Get appropriate decoder
3. Decode the image

Example future integration:
```csharp
// In LoadBlock.Execute()
var extension = Path.GetExtension(filePath).TrimStart('.');
var decoder = _formatRegistry.GetDecoder(extension);
if (decoder != null)
{
    using var stream = File.OpenRead(filePath);
    var image = decoder.Decode(DecoderOptions.Default, stream, cancellationToken);
}
```

## WebP Options Reference

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| Lossless | bool | false | Enable lossless compression |
| Quality | float | 75.0 | Quality factor (0-100, lossy mode only) |
| FileFormat | enum | Lossy | Derived from Lossless setting |
| Method | enum | Default | Encoding speed/quality tradeoff |
| NearLossless | int | 100 | Near-lossless quality (0-100, lossless mode only) |
| UseAlphaCompression | bool | true | Enable alpha channel compression |

### Encoding Methods
- **Fastest** (0) - Fast encoding, larger files
- **Default** (4) - Balanced speed and size
- **BestQuality** (6) - Slow encoding, smaller files

## Building

```bash
dotnet build ImageAutomate.WebPExtension.csproj
```

## Dependencies

- .NET 9.0
- SixLabors.ImageSharp 3.1.12
- ImageAutomate.Core (project reference)

## Notes

- **CoreImitation.cs** contains a simulated registry interface to avoid modifying Core
- The actual Core project would need to implement `IImageFormatRegistry` for full integration
- ImageSharp already has WebP support; this extension provides integration with ImageAutomate's architecture
- The format registry pattern allows any format to be added without modifying existing blocks
