# Architecture

![Architecture Diagram](../assets/architecture_diagram.png)

## Overview

ImageAutomate is designed as a dataflow system where images are processed through a pipeline of blocks.

## Core Components

*   **ImageAutomate.Core**: Defines the core interfaces (`IBlock`, `IWorkItem`, `PipelineGraph`) and data structures.
*   **ImageAutomate.UI**: Provides the visualization components (`GraphRenderPanel`) and UI logic.

## Execution Flow

![Executor Flowchart](../assets/ExecutorFlowchart.jpg)

The execution engine traverses the `PipelineGraph`. Blocks consume `WorkItem`s from their input sockets and produce `WorkItem`s on their output sockets.

## Parallel Processing

![Parallel Mode](../assets/parallel_mode.png)

The system supports parallel execution of blocks where dependencies allow.
