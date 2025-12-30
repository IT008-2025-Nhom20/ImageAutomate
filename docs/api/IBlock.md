# IBlock Interface

The `IBlock` interface defines the contract for all graph nodes (blocks) within the ImageAutomate dataflow system. It abstracts the underlying implementation of image manipulation operations, allowing the `PipelineGraph` and `GraphRenderPanel` to handle various block types uniformly.

## Socket Record

```csharp
public record Socket(string Id, string Name);
```

**Note**: `Socket.Id` is a `string`, identifying the socket within the block.

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

### `Title`
*   **Type**: `string`
*   **Description**: The display header of the block.

### `Content`
*   **Type**: `string`
*   **Description**: The display content of the block.
*   **Usage**: Rendered in the body of the block node to show settings summary.

### `Inputs` / `Outputs`
*   **Type**: `IReadOnlyList<Socket>`
*   **Description**: Collections of input and output sockets.

### Layout Properties
*   **`X`**: `double` - X position in graph coordinates
*   **`Y`**: `double` - Y position in graph coordinates
*   **`Width`**: `int` - Width of the block
*   **`Height`**: `int` - Height of the block

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
*   **Returns**: Dictionary mapping output sockets to their work items.

#### String-Keyed Execute (Convenience Overload)
```csharp
IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> Execute(
    IDictionary<string, IReadOnlyList<IBasicWorkItem>> inputs);
IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> Execute(
    IDictionary<string, IReadOnlyList<IBasicWorkItem>> inputs,
    CancellationToken cancellationToken);
```
*   **Description**: Convenience overloads that accept string socket IDs.
*   **Returns**: Dictionary mapping output sockets to their work items.