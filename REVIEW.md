# Review Report: ImageAutomate.UI & ImageAutomate.Core

## Problematic Code Sections

### `ImageAutomate.UI` - `GraphRenderPanel.cs` & `NodeRenderer.cs`
*   **Issue**: Frequent allocation of GDI+ objects (`Pen`, `Brush`, `Font`, `Matrix`) inside the `OnPaint` loop (and methods called by it like `DrawNode` and `DrawEdge`).
*   **Impact**: High Garbage Collector pressure and potential performance degradation during rendering (low FPS on complex graphs).
*   **Solution**: **Applied.** Cached common `Pen` and `Brush` objects in `NodeRenderer` as `static readonly` fields.

### `ImageAutomate.Core` - `PipelineGraph.cs`
*   **Issue**: `Connect` and `RemoveBlock` methods perform linear scans (`O(N)`) on `_blocks` (List) and `_connections` (List).
*   **Impact**: Performance degradation with a very large number of blocks.
*   **Mitigation**: For typical usage (hundreds of nodes), this is acceptable. For scaling, `HashSet` or Dictionary lookups should be considered.
*   **Issue**: `PipelineGraph` is explicitly not thread-safe, but UI events (`GraphChanged`) might be consumed by background threads if not careful (though typically UI handles this).

## Trivial Optimizations

### 1. Caching GDI+ Resources (Applied)
Modified `NodeRenderer.cs` to use static readonly instances of:
*   `EdgePen`
*   `BgBrush`, `HeaderBrush`, `TextBrush`
*   `BorderPenNormal` (Selected pen is dynamic or cached if color matches)
*   `LabelFont`, `DetailFont`
*   `SocketInputBrush`, `SocketOutputBrush`, `SocketBorderPen`

This significantly reduces allocations per frame.

## Documentation Enhancements

### Method Documentation (Applied)
Updated XML comments in `IBlock.cs` and `PipelineGraph.cs` to focus on the **effect** of the method (what happens) rather than the implementation details.

*   **Example**: `PipelineGraph.RemoveBlock`
    *   *Before*: "Removes the specified block from the collection and disconnects any connections associated with it... If the specified block is the current center block..."
    *   *After*: "Removes a block and all its associated connections from the graph." (Detailed behavior in remarks).

### Directory Cleanup (Applied)
Reorganized `docs/` into:
*   `docs/api/`: Technical documentation for classes (`PipelineGraph.md`, `IBlock.md`, `GraphRenderPanel.md`).
*   `docs/design/`: Architecture documents (`ARCHITECTUREv1.md`).
*   `docs/assets/`: Images.

Updated the Markdown files to reflect the current state of the codebase (e.g., `IBlock` properties, `PipelineGraph` methods).
