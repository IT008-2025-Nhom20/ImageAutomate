# IBlock Interface

The `IBlock` interface defines the contract for all graph nodes (blocks) within the ImageAutomate dataflow system. It abstracts the underlying implementation of image manipulation operations, allowing the `PipelineGraph` and `GraphRenderPanel` to handle various block types uniformly.

## Definition

```csharp
public interface IBlock : INotifyPropertyChanged, IDisposable
{
    string Name { get; }
    string Title { get; }
    string Content { get; }
    int Width { get; set; }
    int Height { get; set; }

    IReadOnlyList<Socket> Inputs { get; }
    IReadOnlyList<Socket> Outputs { get; }

    IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> Execute(IDictionary<Socket, IReadOnlyList<IBasicWorkItem>> inputs);
    IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> Execute(IDictionary<string, IReadOnlyList<IBasicWorkItem>> inputs);
}
```

## Properties

### `Name`
*   **Type**: `string`
*   **Description**: The internal or immutable name of the block type (e.g., "ConvertBlock").
*   **Usage**: Used for identification.

### `Title`
*   **Type**: `string`
*   **Description**: The display header of the block.
*   **Usage**: Displayed as the main label in the block header on the graph.

### `Content`
*   **Type**: `string`
*   **Description**: The display content of the block.
*   **Usage**: Rendered in the body of the block node to give users quick insight into the block's settings.

### `Width` / `Height`
*   **Type**: `int`
*   **Description**: Dimensions of the node in the visual graph.
*   **Usage**: Used by the layout engine (`GraphRenderPanel` and MSAGL) to allocate space.

### `Inputs` / `Outputs`
*   **Type**: `IReadOnlyList<Socket>`
*   **Description**: Collections of input and output sockets.
*   **Usage**: Defines the connectivity points for the block.

## Methods

### `Execute`
*   **Description**: Executes the block's operation on the provided inputs.
*   **Returns**: A dictionary of results mapped to output sockets.
