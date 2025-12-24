# Workspace Serialization Example

This document demonstrates how to use the new serialization features for PipelineGraph.

## Basic PipelineGraph Serialization

```csharp
using ImageAutomate.Core;
using ImageAutomate.StandardBlocks;

// Create a pipeline
var graph = new PipelineGraph();
var loadBlock = new LoadBlock { SourcePath = "/input" };
var brightnessBlock = new BrightnessBlock { Bright = 1.2f };
var saveBlock = new SaveBlock { OutputPath = "/output" };

graph.AddBlock(loadBlock);
graph.AddBlock(brightnessBlock);
graph.AddBlock(saveBlock);

graph.Connect(loadBlock, loadBlock.Outputs[0], brightnessBlock, brightnessBlock.Inputs[0]);
graph.Connect(brightnessBlock, brightnessBlock.Outputs[0], saveBlock, saveBlock.Inputs[0]);

// Serialize to JSON
string json = graph.ToJson();

// Deserialize from JSON
PipelineGraph loadedGraph = PipelineGraph.FromJson(json);
```

## Workspace Serialization with ViewState

```csharp
using ImageAutomate.Core;
using ImageAutomate.StandardBlocks;

// Create a workspace
var workspace = new Workspace
{
    Name = "My Image Processing Project",
    Graph = new PipelineGraph()
};

// Add blocks
var loadBlock = new LoadBlock { SourcePath = "/images" };
var convertBlock = new ConvertBlock 
{ 
    TargetFormat = ImageFormat.Jpeg,
    JpegOptions = new JpegEncodingOptions { Quality = 90 }
};

workspace.Graph.AddBlock(loadBlock);
workspace.Graph.AddBlock(convertBlock);
workspace.Graph.Connect(loadBlock, loadBlock.Outputs[0], convertBlock, convertBlock.Inputs[0]);

// Set view state (for UI)
workspace.ViewState.SetBlockPosition(loadBlock, new Position(100, 100));
workspace.ViewState.SetBlockPosition(convertBlock, new Position(400, 100));
workspace.ViewState.Zoom = 1.5;

// Add metadata
workspace.Metadata["Author"] = "John Doe";
workspace.Metadata["CreatedDate"] = DateTime.Now.ToString("o");

// Save to file
workspace.SaveToFile("my-project.json");

// Load from file
Workspace loadedWorkspace = Workspace.LoadFromFile("my-project.json");
```

## JSON Structure

The serialized JSON has the following structure:

```json
{
  "version": "1.0",
  "name": "My Image Processing Project",
  "graph": {
    "blocks": [
      {
        "blockType": "LoadBlock",
        "assemblyQualifiedName": "ImageAutomate.StandardBlocks.LoadBlock, ...",
        "width": 200,
        "height": 100,
        "properties": {
          "SourcePath": "/images",
          "AutoOrient": true
        },
        "inputs": [],
        "outputs": [
          { "id": "Load.out", "name": "Image.out" }
        ]
      },
      {
        "blockType": "ConvertBlock",
        "assemblyQualifiedName": "ImageAutomate.StandardBlocks.ConvertBlock, ...",
        "width": 200,
        "height": 100,
        "properties": {
          "TargetFormat": "Jpeg",
          "AlwaysEncode": false,
          "JpegOptions": {
            "Quality": 90
          }
        },
        "inputs": [
          { "id": "Convert.In", "name": "Image.Input" }
        ],
        "outputs": [
          { "id": "Convert.Out", "name": "Image.Out" }
        ]
      }
    ],
    "connections": [
      {
        "sourceBlockIndex": 0,
        "sourceSocketId": "Load.out",
        "targetBlockIndex": 1,
        "targetSocketId": "Convert.In"
      }
    ],
    "centerBlockIndex": null
  },
  "viewState": {
    "blockPositions": {
      "0": { "x": 100, "y": 100 },
      "1": { "x": 400, "y": 100 }
    },
    "zoom": 1.5,
    "panX": 0.0,
    "panY": 0.0
  },
  "metadata": {
    "Author": "John Doe",
    "CreatedDate": "2024-01-01T12:00:00Z"
  }
}
```

## Features

### Supported Block Types

All StandardBlocks are fully serializable, including:
- LoadBlock (with SourcePath, AutoOrient)
- SaveBlock (with OutputPath, Overwrite, CreateDirectory)
- BrightnessBlock, ContrastBlock, SaturationBlock, HueBlock
- ResizeBlock (with ResizeMode, TargetWidth/Height, Resampler, etc.)
- ConvertBlock (with all encoding options for JPEG, PNG, GIF, TIFF, WebP, QOI, etc.)
- CropBlock, FlipBlock, GrayscaleBlock, PixelateBlock
- GaussianBlurBlock, SharpenBlock, VignetteBlock

### Encoding Options

ConvertBlock's encoding options are fully preserved:
- **JPEG**: Quality (1-100)
- **PNG**: CompressionLevel (0-9)
- **GIF**: UseDithering, ColorPaletteSize (2-256)
- **TIFF**: Compression (None, Lzw, Ccitt4, Rle, Zip)
- **TGA**: Compress (boolean)
- **WebP**: Lossless, Quality
- **QOI**: IncludeAlpha

### Property Serialization

Only properties marked with `[Category]` attribute are serialized. This ensures:
- Configuration properties are saved
- Implementation details are excluded
- Runtime state is not persisted

Properties without `[Category]` (like `MaxShipmentSize` on LoadBlock) are not serialized as they are set by the executor at runtime.
