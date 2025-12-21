# GraphRenderPanel

`GraphRenderPanel` is a high-performance, custom-drawn Windows Forms control responsible for visualizing the `PipelineGraph`. It inherits from `System.Windows.Forms.Panel` and uses GDI+ for rendering, leveraging MSAGL for layout computation.

## Overview

This control provides a comprehensive visualization of the image processing workflow, featuring:
*   **Automatic Layout**: Uses MSAGL layered layout to organize nodes.
*   **Navigation**: Pan and Zoom support with "zoom-to-cursor" behavior.
*   **Styling**: Custom-rendered nodes with headers, properties, and socket connections.
*   **Binding**: Binds to a `PipelineGraph` instance.

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
    *   The current zoom level of the graph.
*   **`AllowOutOfScreenPan`** (`bool`)
    *   Determines if the user can pan the graph completely out of the viewport.

#### Appearance
*   **`SelectedBlockOutlineColor`** (`Color`)
    *   Color of the border highlight for the `Center` block.
    *   Default: `Color.Red`.
*   **`SocketRadius`** (`double`)
    *   Visual size of the input/output connection points.
    *   Default: `6`.

## User Interaction

*   **Pan**: Click and drag with the **Left Mouse Button**.
*   **Zoom**: Scroll the **Mouse Wheel**.
*   **Select**: Click on a node to make it the `Center` of the graph.

## Integration Example

```csharp
// 1. Create the Control
var graphPanel = new GraphRenderPanel
{
    Dock = DockStyle.Fill
};
this.Controls.Add(graphPanel);

// 2. Prepare the Data
var graph = new PipelineGraph();
var block = new ConvertBlock { Width = 200, Height = 120 };
graph.AddBlock(block);

// 3. Bind
graphPanel.Graph = graph;
```

## Customization

The panel uses a strategy pattern for node rendering via the internal `NodeRenderer` class. It uses `OptimizedStrategy` which leverages cached `GraphicsPath` objects.
