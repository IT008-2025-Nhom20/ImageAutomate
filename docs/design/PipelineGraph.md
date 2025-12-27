# Pipeline Graph Data Structure

## Table of Contents

1. [Architectural Overview](#1-architectural-overview)
2. [Data Structure Definition](#2-data-structure-definition)
3. [Topology Management](#3-topology-management)
4. [State Separation Model](#4-state-separation-model)
5. [Serialization Strategy](#5-serialization-strategy)
6. [Implementation Details](#6-implementation-details)

## 1. Architectural Overview

The **PipelineGraph** serves as the central data model for the ImageAutomate application, representing the image processing workflow. It acts as a **Directed Acyclic Graph (DAG)** container where nodes represent processing operations (`IBlock`) and edges represent data flow (`Connection`).

*   **Role:** Domain Model / Source of Truth.
*   **Responsibility:** Structural integrity, topology validation, and event notification.
*   **Abstraction:** Decoupled from execution logic (Engine) and presentation logic (UI).

## 2. Data Structure Definition

The graph is implemented as an adjacency list representation, optimized for the sparse connectivity typical of dataflow pipelines.

### 2.1. The Node (`IBlock`)

The primary vertex of the graph.

*   **Storage:** `List<IBlock> _nodes` (exposed as `IReadOnlyList<IBlock>` via `Nodes` property).
*   **Ordering:** The list order implies the **Z-Order** (Rendering Layer) for the UI. The last element is rendered on top ("Painter's Algorithm").
*   **Identity:** Reference equality is used for runtime operations; `string Id` is used for persistence.
*   **Socket Record:**
    ```csharp
    public record Socket(string Id, string Name);
    ```
    **Important**: `Socket.Id` is a `string`, NOT a `Guid`.

### 2.2. The Edge (`Connection`)

The directed link between two vertices.

*   **Storage:** `List<Connection> _edges` (exposed as `IReadOnlyList<Connection>` via `Edges` property).
*   **Definition:**
    ```csharp
    public record Connection(
        IBlock Source,
        Socket SourceSocket,
        IBlock Target,
        Socket TargetSocket
    );
    ```
*   **Semantics:** Represents a "Pipe" transferring `WorkItems` from an Output Port to an Input Port.

## 3. Topology Management

The `PipelineGraph` enforces structural rules to ensure a valid graph state.

### 3.1. Integrity Constraints

*   **Existence:** An Edge cannot exist unless both Source and Target Nodes are present in the `_nodes` list.
*   **Port Validity:** An Edge must reference valid `Socket` instances belonging to the respective Blocks.
*   **Type Safety:** Connections are only valid between Output and Input sockets (enforeced by graph's `AddEdge` method).

### 3.2. Manipulation API

*   **Standard Graph Operations:** `AddBlock`, `RemoveNode`, `AddEdge`, `RemoveEdge`.
*   **Cascading Deletion:** `RemoveNode(block)` automatically identifies and purges all incident Edges (`_edges.RemoveAll(...)`), preventing dangling references.
*   **Layering:** `BringToTop(block)` permutes the `_nodes` list to move the target block to the end, altering render order without changing topology.

### 3.3. Event Model

The graph implements the **Observer Pattern** to notify listeners of structural mutations.

*   **Events:** `OnNodeRemoved`.
*   **Usage:** Used by the `ExecutionEngine` or `GraphRenderPanel` to invalidate caches or UI state when the underlying model changes.

## 4. State Separation Model

To maintain a clean separation of concerns, the `PipelineGraph` distinguishes between **Logical Topology** and **Visual Layout**.

### 4.1. Logical Topology (`PipelineGraph`)

Contains only the semantic information required for execution:
*   Which blocks exist.
*   How they are connected.
*   Block configuration (Parameters).

### 4.2. Visual Layout (`ViewState`)

Contains the presentation data required for the UI, stored in a separate `ViewState` object (managed by `Workspace`).
*   **Position:** `(X, Y)` coordinates on the canvas.
*   **Dimensions:** `(Width, Height)` of the nodes.
*   **Selection State:** Currently selected items.

**Rationale:** This separation allows the Execution Engine to run a graph "headless" without needing to load UI-specific layout data, while allowing the serialization system to merge them for persistence.

## 5. Serialization Strategy

The system uses a **Data Transfer Object (DTO)** pattern for persistence, enabling version tolerance and cleaner JSON output.

### 5.1. DTO Structure

*   **PipelineGraphDto:**
    *   `Blocks`: List of `BlockDto`.
    *   `Connections`: List of `ConnectionDto` (referencing blocks by Index).
    *   `CenterBlockIndex`: Viewport state.

### 5.2. Hybrid Serialization

To handle the separation of Topology and Layout during save/load operations:

*   **Saving (`ToDto`):** The `PipelineGraph` gathers logical data from itself and queries the provided `ViewState` for layout data, merging them into the DTO.
*   **Loading (`FromDto`):** The `PipelineGraph` reconstructs the topology and populates a provided `ViewState` with the extracted layout information.

### 5.3. Connection Resolution

Connections are serialized using **Index-Based Referencing** (Source Block Index / Target Block Index) rather than IDs.
*   **Pros:** Compact JSON, easy to parse.
*   **Cons:** Sensitive to list order preservation (handled by the list-based `_nodes` storage).

## 6. Implementation Details

### 6.1. Selection State

The `PipelineGraph` maintains selection state:
*   **`SelectedItem`** (`object?`): The currently selected item - can be either an `IBlock` or a `Connection` (public property).
*   **Note**: `SelectedBlock` is a private helper property. The public API uses `SelectedItem` which supports polymorphic selection.

### 6.2. Thread Safety

### 6.2. Thread Safety

The `PipelineGraph` is **not thread-safe**.
*   **Constraint:** Modifications must occur on a single thread (typically the UI thread).
*   **Execution:** The Execution Engine creates an internal immutable representation (or handles synchronization) during the run phase.
