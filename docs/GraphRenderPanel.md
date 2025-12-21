# GraphRenderPanel

`GraphRenderPanel` is a high-performance, custom-drawn Windows Forms control responsible for visualizing the `PipelineGraph`. It inherits from `System.Windows.Forms.Panel` and uses GDI+ for rendering.

## Overview

This control provides a comprehensive visualization of the image processing workflow, featuring:
*   **Automatic Layout**: Uses MSAGL layered layout to organize nodes.
*   **Navigation**: Pan and Zoom support with "zoom-to-cursor" behavior.
*   **Styling**: Custom-rendered nodes with headers, properties, and socket connections.
*   **Binding**: MVVM-friendly data binding via the `Graph` property.

## API Reference

### Properties

#### Data Binding
*   **`Graph`** (`PipelineGraph?`)
    *   The main data source for the panel. Assigning a `PipelineGraph` instance triggers an automatic layout calculation and repaint.
    *   Default: `null` (renders nothing).

#### Layout Configuration
*   **`ColumnSpacing`** (`double`)
    *   Horizontal distance between graph layers.
    *   Default: `250`.
*   **`NodeSpacing`** (`double`)
    *   Vertical distance between nodes within the same layer.
    *   Default: `30`.

#### Interaction & Behavior
*   **`RenderScale`** (`float`)
    *   The current zoom level of the graph. Can be set programmatically to force a specific zoom.
    *   Default: `1.0`.
*   **`AllowOutOfScreenPan`** (`bool`)
    *   Determines if the user can pan the graph completely out of the viewport.
    *   `true`: Free panning.
    *   `false` (Default): Panning is clamped so that at least a portion of the graph remains visible.

#### Appearance
*   **`SelectedBlockOutlineColor`** (`Color`)
    *   Color of the border highlight for the `CenterNode` (selected node).
    *   Default: `Color.Red`.
*   **`SocketRadius`** (`double`)
    *   Visual size of the input/output connection points.
    *   Default: `6`.

## User Interaction

*   **Pan**: Click and drag with the **Left Mouse Button** to move the camera.
*   **Zoom**: Scroll the **Mouse Wheel** to zoom in and out. The zoom centers on the mouse cursor position.

## Integration Example

```csharp
// 1. Create the Control
var graphPanel = new GraphRenderPanel
{
    Dock = DockStyle.Fill,
    AllowOutOfScreenPan = false
};
this.Controls.Add(graphPanel);

// 2. Prepare the Data
var graph = new PipelineGraph();
var block = new ConvertBlock { Width = 200, Height = 120 };
graph.AddNode(block);

// 3. Bind
graphPanel.Graph = graph;
```

## Customization

The panel uses a strategy pattern for node rendering via the internal `NodeRenderer` class. By default, it uses `OptimizedStrategy` which leverages cached `GraphicsPath` objects for efficient GDI+ drawing.
