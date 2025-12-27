# Workspace

`Workspace` is the top-level container for an ImageAutomate session. It encapsulates the pipeline graph, visual layout state, and project metadata.

## Overview

The `Workspace` class provides a unified interface for managing and persisting the user's work. It stores the graph, view state (zoom/pan), and project metadata.

## Definition

```csharp
public class Workspace
{
    public string Name { get; set; }
    public PipelineGraph Graph { get; set; }
    public double Zoom { get; set; }
    public double PanX { get; set; }
    public double PanY { get; set; }
    public Dictionary<string, object?> Metadata { get; set; }
    public bool IncludeSchemaReference { get; set; }

    // ... methods ...
}
```

## Properties

*   **`Name`**: The name of the workspace (e.g., project name).
*   **`Graph`**: The `PipelineGraph` containing the logic.
*   **`Zoom`**: Current zoom level for the graph view.
*   **`PanX`**: Horizontal pan offset for the graph view.
*   **`PanY`**: Vertical pan offset for the graph view.
*   **`Metadata`**: A dictionary for storing custom user data.
*   **`IncludeSchemaReference`**: If true, includes a `$schema` reference in the JSON output for IDE auto-completion.

## Methods

### Hit Testing
*   **`HitTestNode(double x, double y)`**: Returns the top-most `IBlock` at the specified world coordinates. Useful for mouse interaction.

### Serialization
*   **`ToJson()`**: Serializes the entire workspace to a JSON string.
*   **`FromJson(string json)`**: Static method that deserializes a workspace from a JSON string.
*   **`SaveToFile(string filePath)`**: Helper to save directly to a file.
*   **`LoadFromFile(string filePath)`**: Static helper to load directly from a file.

## Behavior

*   **Auto-Cleanup**: The workspace automatically subscribes to `Graph.OnNodeRemoved` to clean up layout entries when nodes are deleted from the graph.
*   **Embedded Layout**: During serialization, layout information (X, Y, Width, Height) is embedded into the block objects to produce a self-contained JSON structure.
