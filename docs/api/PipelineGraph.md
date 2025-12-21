# PipelineGraph

`PipelineGraph` is the core data structure representing the workflow graph in ImageAutomate. It manages the collection of blocks and the connections between them.

## Responsibilities

*   **Block Management**: Adds and removes `IBlock` instances.
*   **Connection Management**: Connects sockets between blocks (`Connection` records).
*   **State Tracking**: Maintains the `Center` block (selected block) and raises `GraphChanged` events.

## Key Components

### Properties

*   **`Blocks`**: `IReadOnlyList<IBlock>` - Returns all blocks in the graph.
*   **`Connections`**: `IReadOnlyList<Connection>` - Returns all connections in the graph.
*   **`Center`**: `IBlock?` - The currently focused or selected block.

### Methods

#### `AddBlock(IBlock block)`
Adds a block to the graph.

#### `RemoveBlock(IBlock block)`
Removes a block and all its associated connections.

#### `Connect(...)`
Establishes a connection between a source block's output socket and a target block's input socket. Ensures that a target socket can only have one incoming connection.

#### `Disconnect(Connection connection)`
Removes a specific connection.

#### `Clear()`
Resets the graph to an empty state.

## Usage

```csharp
var graph = new PipelineGraph();
var blockA = new ConvertBlock();
var blockB = new ResizeBlock();

graph.AddBlock(blockA);
graph.AddBlock(blockB);

// Connect output of A to input of B
graph.Connect(blockA, blockA.Outputs[0], blockB, blockB.Inputs[0]);
```
