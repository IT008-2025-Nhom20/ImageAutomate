# PipelineGraph

`PipelineGraph` is the core data structure representing the workflow graph in ImageAutomate. It serves as a wrapper around the MSAGL `GeometryGraph`, bridging the gap between the application's business logic (`IBlock` nodes) and the geometric layout engine.

## Responsibilities

*   **Node Management**: Adds, removes, and retrieves graph nodes associated with `IBlock` instances.
*   **Edge Management**: Connects nodes to define the dataflow execution order.
*   **Layout Integration**: Maintains the mapping between `IBlock` objects and MSAGL `GeomNode` objects, allowing the rendering engine to position visual elements correctly.

## Key Components

### Constructor

```csharp
public PipelineGraph()
```
Initializes a new empty graph.

### Properties

*   **`Nodes`**: `IReadOnlyCollection<GeomNode>` - Returns all nodes in the graph.
*   **`Edges`**: `IReadOnlyCollection<GeomEdge>` - Returns all edges (connections) in the graph.
*   **`CenterNode`**: `GeomNode?` - A reference to a "central" or "selected" node, often used for highlighting or centering the camera.
*   **`GeomGraph`**: `GeometryGraph` - The underlying MSAGL graph object used for layout calculations.

### Methods

#### `AddNode(IBlock block)`
Adds a new node to the graph representing the specified block.
*   **Parameters**: `block` - The block instance to add.
*   **Returns**: The created `GeomNode`.
*   **Behavior**: Uses `block.Width` and `block.Height` to define the node's geometry. If the block already exists in the graph, returns the existing node.

#### `AddEdge(GeomNode from, GeomNode to)`
Creates a directed connection from the source node to the target node.
*   **Parameters**:
    *   `from`: The source node (Output).
    *   `to`: The target node (Input).

#### `RemoveNode(GeomNode node)`
Removes a node and all connected edges from the graph.
*   **Parameters**: `node` - The node to remove.

#### `GetNode(IBlock block)`
Retrieves the `GeomNode` associated with a specific block instance.

### Enumeration

*   **`EnumerateNodesWithBlocks()`**: Helper method that yields tuples of `(GeomNode node, IBlock block)`, facilitating iteration over the graph data.

## Usage

```csharp
var graph = new PipelineGraph();
var blockA = new ConvertBlock();
var blockB = new ResizeBlock();

var nodeA = graph.AddNode(blockA);
var nodeB = graph.AddNode(blockB);

graph.AddEdge(nodeA, nodeB);
```
