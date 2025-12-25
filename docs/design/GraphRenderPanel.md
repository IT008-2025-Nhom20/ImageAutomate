# Graph Render Panel Design

## Table of Contents

1. [Architectural Overview](#1-architectural-overview)
2. [Rendering Pipeline](#2-rendering-pipeline)
3. [Coordinate Systems](#3-coordinate-systems)
4. [Interaction Model](#4-interaction-model)
5. [Visual Component System](#5-visual-component-system)
6. [Implementation Details](#6-implementation-details)

## 1. Architectural Overview

The **GraphRenderPanel** is a specialized UI control responsible for the visualization and interactive manipulation of the `PipelineGraph`. It is built upon the Windows Forms `Panel` class, utilizing **GDI+ (System.Drawing)** for high-performance immediate-mode rendering.

*   **Role:** Presentation Layer / View.
*   **Responsibility:** Rendering the graph, handling user input (Mouse/Keyboard), and managing viewport transformations.
*   **Design Pattern:** Model-View-Controller (MVC) - Acts as the View and partially the Controller for the `PipelineGraph` (Model).

## 2. Rendering Pipeline

The rendering system employs a customized **Immediate Mode** loop triggered by standard WinForms paint events.

### 2.1. Double Buffering

To prevent flickering during high-frequency updates (e.g., dragging a node), the panel enables `DoubleBuffered = true` by default.
*   **Mechanism:** GDI+ renders to an off-screen bitmap first, then blits the result to the screen in a single operation.

### 2.2. The Paint Cycle (`OnPaint`)

1.  **State Retrieval:** Accesses the `PipelineGraph` (Model) and `ViewState` (Layout).
2.  **Transform Setup:** Applies the Global Transform Matrix (Pan + Scale) to the `Graphics` context.
3.  **Layer 1: Connections:** Iterates through `Graph.Edges`. Delegates drawing to `NodeRenderer` to draw Bezier curves.
    *   Edges are drawn first so they appear "behind" nodes.
4.  **Layer 2: Interaction Feedback:** Draws transient elements like the "Drag Line" during connection creation.
5.  **Layer 3: Nodes:** Iterates through `Graph.Nodes`. Delegates drawing to `NodeRenderer` for blocks and sockets.
    *   *Ordering:* Iterates in list order, ensuring the "Selected" (last in list) node draws on top.

## 3. Coordinate Systems

The panel manages two distinct coordinate spaces:

1.  **Screen Space:** Physical pixels on the monitor (Mouse events, Control dimensions).
2.  **World Space:** Logical coordinates of the graph canvas (Node positions).

### 3.1. Transformation Matrix

Mapping between spaces is handled via a 2D Affine Transformation Matrix:

$$
\begin{bmatrix} x' \\ y' \\ 1 \end{bmatrix} = \begin{bmatrix} x \\ y \\ 1 \end{bmatrix} \times \begin{bmatrix} S & 0 & 0 \\ 0 & S & 0 \\ T_x & T_y & 1 \end{bmatrix}
$$

Where:
*   $S$: Zoom level (`_renderScale`).
*   $T_x, T_y$: Pan offset (`_panOffset`).

### 3.2. Viewport Management

*   **Panning:** Updates $(T_x, T_y)$ based on mouse deltas.
*   **Zooming:** Updates $S$ based on mouse wheel. Zoom is "anchored" to the mouse cursor position, requiring a simultaneous adjustment of $(T_x, T_y)$ to keep the world point under the cursor stable.

## 4. Interaction Model

The panel implements a finite state machine (implicit in boolean flags) to handle complex mouse interactions.

### 4.1. Input States

1.  **Idle:** Default state. Hover effects active.
2.  **Panning:** Right-mouse drag. Moves the viewport.
3.  **DraggingNode:** Left-mouse drag on a Node body. Updates `ViewState` position.
4.  **Connecting:** Left-mouse drag from a Socket. Renders a live "rubber band" line.

### 4.2. Hit Testing

To determine interaction targets, the panel performs geometric intersection tests in **World Space**.

1.  **Socket Hit Test:** Checks collision with circular regions around input/output ports. Used for initiating connections.
2.  **Node Hit Test:** Checks collision with the node's bounding box. Used for selection and dragging.
3.  **Edge Hit Test:** Checks collision with the Bezier curve of a connection.
    *   Uses a widened invisible `GraphicsPath` (e.g., 10px width) to make selecting thin lines easier for the user.

## 5. Visual Component System

Rendering logic is offloaded to a helper component, the `NodeRenderer`.

### 5.1. NodeRenderer Strategy

*   **Pattern:** Singleton / Flyweight-like resource manager.
*   **Responsibility:** Encapsulates GDI+ primitives (Pens, Brushes, Fonts) to minimize allocation overhead.
*   **Resource Management:** Implements `IDisposable` to properly release unmanaged GDI+ handles.

### 5.2. Visual Style

*   **Nodes:** Rounded rectangles (`GraphicsPath` with arcs).
*   **Headers:** Distinct colored band at the top of the node.
*   **Connections:** Cubic Bezier curves ($C^1$ continuity).
    *   Control points are calculated dynamically based on distance to ensure smooth "S" curves.
*   **Sockets:** Circular ports. Color-coded (Input vs Output) to guide valid connections.

## 6. Implementation Details

### 6.1. Threading

All rendering and interaction occur on the **UI Thread**.
*   **Constraint:** The `PipelineGraph` must not be mutated by background threads while a paint operation is pending.

### 6.2. Dirty Rectangles

Currently, the implementation uses `Invalidate()` (full repaint) for simplicity.