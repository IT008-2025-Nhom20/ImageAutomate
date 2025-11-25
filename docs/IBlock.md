# IBlock Interface

The `IBlock` interface defines the contract for all graph nodes (blocks) within the ImageAutomate dataflow system. It abstracts the underlying implementation of image manipulation operations, allowing the `PipelineGraph` and `GraphRenderPanel` to handle various block types uniformly.

## Definition

```csharp
public interface IBlock : INotifyPropertyChanged
{
    string Name { get; }
    string ConfigurationSummary { get; }
    double Width { get; }
    double Height { get; }
}
```

## Properties

### `Name`
*   **Type**: `string`
*   **Description**: The display name of the block (e.g., "Convert", "Resize", "Watermark").
*   **Usage**: Displayed as the main label in the block header on the graph.

### `ConfigurationSummary`
*   **Type**: `string`
*   **Description**: A concise summary of the block's current configuration. Multi-line strings are supported.
*   **Usage**: Rendered in the body of the block node to give users quick insight into the block's settings without opening a property editor.

### `Width`
*   **Type**: `double`
*   **Description**: The width of the node in the visual graph.
*   **Usage**: Used by the layout engine (`PipelineGraph`) to allocate space for the node.

### `Height`
*   **Type**: `double`
*   **Description**: The height of the node in the visual graph.
*   **Usage**: Used by the layout engine to allocate space.

## Events

### `PropertyChanged`
*   **Inherited from**: `INotifyPropertyChanged`
*   **Description**: Notifies listeners when a property value changes.
*   **Usage**: Essential for MVVM bindings (e.g., `PropertyGrid`) and potentially for triggering graph updates (though automatic layout refresh on property change is implementation-dependent).

## Implementation Example

```csharp
public class MyCustomBlock : IBlock
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public string Name => "My Block";
    public string ConfigurationSummary => $"Value: {_value}";

    public double Width { get; set; } = 200;
    public double Height { get; set; } = 100;

    private int _value;
    // ... INotifyPropertyChanged implementation ...
}
```
