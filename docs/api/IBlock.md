# IBlock Interface

The `IBlock` interface defines the contract for all graph nodes (blocks) within the ImageAutomate dataflow system. It abstracts the underlying implementation of image manipulation operations, allowing the `PipelineGraph` and `GraphRenderPanel` to handle various block types uniformly.

## Socket Record

```csharp
public record Socket(string Id, string Name);
```

**Important**: `Socket.Id` is a `string`, NOT a `Guid`. Sockets are identified by their string IDs within blocks.

## Definition

```csharp
public interface IBlock : INotifyPropertyChanged, IDisposable
{
    string Name { get; }
    string Title { get; }
    string Content { get; }

    IReadOnlyList<Socket> Inputs { get; }
    IReadOnlyList<Socket> Outputs { get; }

    IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> Execute(IDictionary<Socket, IReadOnlyList<IBasicWorkItem>> inputs);
    IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> Execute(IDictionary<Socket, IReadOnlyList<IBasicWorkItem>> inputs, CancellationToken cancellationToken);
    IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> Execute(IDictionary<string, IReadOnlyList<IBasicWorkItem>> inputs);
    IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> Execute(IDictionary<string, IReadOnlyList<IBasicWorkItem>> inputs, CancellationToken cancellationToken);
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

### `Inputs` / `Outputs`
*   **Type**: `IReadOnlyList<Socket>`
*   **Description**: Collections of input and output sockets (using `Socket` record with `string Id`).
*   **Usage**: Defines the connectivity points for the block.

### Layout Properties
*   **`X`**: `double` - X position in graph coordinates
*   **`Y`**: `double` - Y position in graph coordinates
*   **`Width`**: `int` - Width of the block (settable)
*   **`Height`**: `int` - Height of the block (settable)

## Methods

### `Execute`

The `Execute` method has four overloads. All overloads require an `inputs` parameter.

#### Socket-Keyed Execute
```csharp
IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> Execute(
    IDictionary<Socket, IReadOnlyList<IBasicWorkItem>> inputs);
IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> Execute(
    IDictionary<Socket, IReadOnlyList<IBasicWorkItem>> inputs,
    CancellationToken cancellationToken);
```
*   **Description**: Primary execution methods that accept Socket-keyed inputs.
*   **Parameters**:
    *   `inputs`: Dictionary mapping input sockets to their work items (required).
    *   `cancellationToken`: Token to observe for cancellation requests (optional).
*   **Returns**: Dictionary mapping output sockets to their work items.

#### String-Keyed Execute (Convenience Overload)
```csharp
IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> Execute(
    IDictionary<string, IReadOnlyList<IBasicWorkItem>> inputs);
IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> Execute(
    IDictionary<string, IReadOnlyList<IBasicWorkItem>> inputs,
    CancellationToken cancellationToken);
```
*   **Description**: Convenience overloads that accept string socket IDs instead of Socket objects.
*   **Parameters**:
    *   `inputs`: Dictionary mapping socket IDs to their work items (required).
    *   `cancellationToken`: Token to observe for cancellation requests (optional).
*   **Returns**: Dictionary mapping output sockets to their work items.
