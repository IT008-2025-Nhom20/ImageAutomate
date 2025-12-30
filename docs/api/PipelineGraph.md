# PipelineGraph

`PipelineGraph` is the data structure representing the workflow graph in ImageAutomate. It manages the collection of blocks and the connections between them.

## Responsibilities

*   **Block Management**: Adds and removes `IBlock` instances.
*   **Connection Management**: Connects sockets between blocks (`Connection` records).
*   **State Tracking**: Maintains the `SelectedItem` and raises `OnNodeRemoved` events.
*   **Serialization**: Provides methods to convert the graph to and from JSON.

## Key Components

### Properties

*   **`Nodes`**: `IReadOnlyList<IBlock>` - Returns all blocks in the graph.
*   **`Edges`**: `IReadOnlyList<Connection>` - Returns all connections in the graph.
*   **`SelectedItem`**: `object?` - The currently selected item (Block or Connection).

### Events

*   **`OnNodeRemoved`**: `Action<IBlock>?` - Occurs when a node is removed from the graph.

### Methods

#### `AddBlock(IBlock block)`
Adds a block to the graph if it is not already present.

#### `RemoveNode(IBlock block)`
Removes a block and all its associated connections.

#### `AddEdge(...)`
Establishes a connection between a source block's output socket and a target block's input socket.

**Overloads:**
*   `AddEdge(IBlock source, Socket sourceSocket, IBlock target, Socket targetSocket)`
*   `AddEdge(IBlock source, string sourceSocketId, IBlock target, string targetSocketId)`

#### `RemoveEdge(Connection edge)`
Removes a specific connection.

#### `BringToTop(IBlock block)`
Moves the specified block to the end of the node list.

#### `Clear()`
Resets the graph to an empty state.

### Serialization

#### `ToJson()`
Serializes the current graph structure to a JSON string.

#### `FromJson(string json)`
Static method that creates a new `PipelineGraph` instance from a JSON string.