# GraphRenderPanel

`GraphRenderPanel` is a custom-drawn Windows Forms control responsible for visualizing and interacting with the `PipelineGraph`. It inherits from `Panel` and uses GDI+ for rendering.

## Overview

*   **Custom Rendering**: Uses GDI+ to draw nodes, sockets, and bezier connections.
*   **Interaction**: Supports dragging nodes, creating connections, panning, and zooming.
*   **Layout Support**: Integrates with `Workspace` to persist layout information.

## Dependencies

*   **ImageAutomate.Core** - Core contracts (IBlock, PipelineGraph, Workspace, Connection, Socket)
*   **Microsoft.Msagl** (1.1.6) - Automatic graph layout library (unused)

## API Reference

### Properties

#### Data Binding
*   **`Workspace`** (`Workspace?`): The workspace containing the graph and view state.
    *   **Note**: The `Graph` is accessed via `Workspace.Graph`, not as a direct property.

#### Node Appearance
*   **`SelectedBlockOutlineColor`** (`Color`): Color of the selection border. Default: `Red`.
*   **`SocketRadius`** (`double`): Size of connection sockets. Default: `6`.

#### Graph Appearance
*   **`RenderScale`** (`float`): Current zoom level. Default: `1.0`.

#### Graph Behavior
*   **`AllowOutOfScreenPan`** (`bool`): Whether the graph can be panned off-screen. Default: `false`.
*   **`AutoSnapZoneWidth`** (`float`): Width of the zone around sockets for easier connection snapping. Default: `20`.

#### Private State (Internal)
*   **`_panOffset`** (`PointF`): Current pan offset (private field, not a public property).
*   **`_isPanning`** (`bool`): Right-click drag state.
*   **`_isDraggingNode`** (`bool`): Node drag state.
*   **`_draggedNode`** (`IBlock?`): Currently dragged block.
*   **`_isConnecting`** (`bool`): Connection drag state.
*   **`_dragStartSocket`** (`SocketHit?`): Connection start point.

### Methods

#### Connection Management
*   **`AddBlockAndConnect(IBlock sourceBlock, Socket sourceSocket, IBlock destBlock, Socket destSocket)`**: Adds two blocks and creates a connection between them.
*   **`AddSuccessor(Socket sourceSocket, IBlock destBlock, Socket destSocket)`**: Connects the selected block's socket to a new downstream block.
*   **`AddPredecessor(IBlock sourceBlock, Socket sourceSocket, Socket destSocket)`**: Connects a new upstream block to the selected block's socket.

#### Deletion
*   **`DeleteBlock(IBlock block)`**: Removes a block from the graph.
*   **`DeleteConnection(Connection connection)`**: Removes a connection from the graph.
*   **`DeleteItem(object item)`**: Handles deletion of both `IBlock` and `Connection` types.
*   **`DeleteSelectedItem()`**: Removes the currently selected block or connection.

#### View Control
*   **`GetViewportCenterWorld()`**: Returns the world coordinates (`PointF`) of the viewport center.
*   **`CenterCameraOnBlock(IBlock block)`**: Centers the view on a specific block.

## Visual Style

*   **Nodes**: Dark gray background (`#3C3C3C`), lighter header (`#505050`), white text
*   **Sockets**: Green for inputs (`#64C864`), red for outputs (`#C86464`), white border
*   **Connections**: Gray (`#969696`) for normal, red for selected, orange dashed for drag
*   **Border**: Gray (`#646464`, 2px), selection color (3px, configurable via `SelectedBlockOutlineColor`)

## User Interaction

*   **Left Click**: Select node/edge.
*   **Left Drag (Node)**: Move node.
*   **Left Drag (Socket)**: Create connection.
*   **Right Drag**: Pan the view.
*   **Mouse Wheel**: Zoom in/out.
*   **Drag & Drop**: Accepts `Type` objects that implement `IBlock` for adding new blocks.

## SocketHit Record

Internal record used for socket hit-testing:

```csharp
public record SocketHit(IBlock Block, Socket Socket, bool IsInput, PointF Position);
```
