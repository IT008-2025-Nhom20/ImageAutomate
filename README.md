# ImageAutomate

**ImageAutomate** is a modular, node-based image processing framework designed to build flexible execution pipelines. It separates logical graph definition from visualization and execution, allowing for batch processing, high-performance rendering, and easy extensibility via plugins.

## 🏗 Architecture

The system follows a **Core-Periphery** architecture, ensuring that the core logic is decoupled from the user interface and execution details.

The solution is divided into the following key components:

  * **ImageAutomate.Core**: The central contract library defining the topology (`PipelineGraph`), data units (`WorkItem`, `Socket`), and interfaces (`IBlock`). It has no dependencies on UI frameworks.
  * **ImageAutomate.StandardBlocks**: A library of built-in image processing nodes (Resize, Crop, Blur, etc.) implemented using `SixLabors.ImageSharp`.
  * **ImageAutomate.UI**: The visualization layer responsible for rendering the graph using MSAGL and GDI+.
  * **ImageAutomate.Execution**: The engine that validates the topology and orchestrates the batch processing of images.
  * **ImageAutomate.App**: The composition root (WinForms) that wires the components together and handles plugin loading.

### Execution Model

ImageAutomate uses a **Vertical Parallelism** strategy. Data is processed in groups where a specific batch flows through the entire pipeline before the next batch begins. This simplifies state management and resource usage.

## 🧩 Core Concepts

### PipelineGraph

The `PipelineGraph` is the core data structure representing the workflow. It acts as a wrapper around the MSAGL `GeometryGraph`, bridging the application's business logic with the geometric layout engine.

### IBlock

All processing nodes implement the `IBlock` interface. This defines the contract for inputs, outputs, configuration, and execution logic.

  - **Inputs/Outputs**: Defined via `Socket`s, functioning like a directed messaging system.
  - **Configuration**: Blocks expose a `ConfigurationSummary` for quick visual inspection in the graph.

### GraphRenderPanel

The UI is powered by `GraphRenderPanel`, a high-performance custom WinForms control. It features automatic layered layout (via MSAGL), zoom-to-cursor, and custom GDI+ node rendering.

## 📦 Standard Blocks

The project includes a suite of standard blocks for common image manipulation tasks:

**I/O:**

  * `LoadBlock`: Reads images from a directory.
  * `SaveBlock`: Saves processed images to disk (supports PNG, JPG, WebP, etc.).

**Transform:**

  * `ResizeBlock`: Resize images (Fixed, Fit, Fill, Pad modes).
  * `CropBlock`: Crop images (Rectangle, Center, Anchor modes).
  * `FlipBlock`: Horizontal or Vertical flipping.

**Effects:**

  * `GaussianBlurBlock`, `SharpenBlock`, `PixelateBlock`
  * `HueBlock`, `SaturationBlock`, `ContrastBlock`, `VignetteBlock`

**Format:**

  * `ConvertBlock`: Changes image formats (e.g., PNG to JPG) with granular compression settings.

## 🚀 Getting Started

### Prerequisites

  * .NET 9.0 SDK
  * Visual Studio 2022 (or compatible IDE)

### Project Structure

  * `ImageAutomate.Core/`: Interfaces and Graph Data Structures.
  * `ImageAutomate.StandardBlocks/`: Implementation of standard image operations.
  * `ConvertBlockPoC/`: The current visual Proof-of-Concept application.
  * `Stub_Extension_A/`: Example of an external plugin.

### Running the Visual PoC

The `ConvertBlockPoC` project demonstrates the graph rendering and manipulation capabilities.

1.  Open `ImageAutomate.sln`.
2.  Set `ConvertBlockPoC` as the startup project.
3.  Run the application to interact with the node graph visualization.

## 🔌 Extensibility

ImageAutomate is designed for plugins. To create a new block:

1.  Create a new Class Library project.
2.  Reference `ImageAutomate.Core`.
3.  Implement the `IBlock` interface.
4.  Define your `Socket` inputs and outputs.
5.  Implement the `Execute` method using your image processing logic.

*Example (from `Stub_Extension_A`):*

```csharp
public class MyCustomBlock : IBlock
{
    public string Name => "MyCustomBlock";
    // ... Implement properties and Execute logic
}
```

## ✅ TODO

Current roadmap items:

  - [ ] Implement `BeginUpdate` / `EndUpdate` for batch graph modifications.
  - [ ] Improve edge rendering to use the layout engine's calculated curves.
