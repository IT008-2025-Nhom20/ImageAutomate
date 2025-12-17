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

The `Execute` method has two overloads that form the core execution protocol:

#### Socket-Keyed Execute (Canonical Protocol)
```csharp
IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> Execute(IDictionary<Socket, IReadOnlyList<IBasicWorkItem>> inputs)
```
*   **Description**: The canonical execution method that accepts Socket-keyed inputs.
*   **Parameters**: `inputs` - Dictionary mapping input sockets to their work items.
*   **Returns**: Dictionary mapping output sockets to their work items.
*   **Implementation Note**: Typically delegates to the string-keyed overload by converting Socket keys to their IDs (via `socket.Id` property access), since retrieving a socket ID is trivial while finding a Socket from an ID requires searching.

#### String-Keyed Execute (Convenience Overload)
```csharp
IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> Execute(IDictionary<string, IReadOnlyList<IBasicWorkItem>> inputs)
```
*   **Description**: Convenience overload that accepts string socket IDs instead of Socket objects.
*   **Parameters**: `inputs` - Dictionary mapping socket IDs to their work items.
*   **Returns**: Dictionary mapping output sockets to their work items.
*   **Implementation Note**: Contains the actual execution logic. Returns Socket-keyed results to maintain a unified return type.

#### Return Type Design
Both overloads return `IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>>` to enable method chaining regardless of input key type. For example:
```csharp
// Chain executions with different key types
var stringKeyedData = new Dictionary<string, IReadOnlyList<IBasicWorkItem>>();
var socketKeyedResult1 = block1.Execute(stringKeyedData);  // string keys in, Socket keys out
var socketKeyedResult2 = block2.Execute(socketKeyedResult1);  // Socket keys in, Socket keys out
```
Both blocks return Socket-keyed results that can be seamlessly passed to subsequent blocks.
