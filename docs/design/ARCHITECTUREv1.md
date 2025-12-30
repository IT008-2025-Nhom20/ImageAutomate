# Architecture

![Architecture Diagram](../assets/architecture_diagram.png)

## Overview

ImageAutomate functions as a dataflow system where images are processed through a pipeline of blocks.

## Core Components

*   **ImageAutomate.Core**: Defines the core interfaces (`IBlock`, `IWorkItem`, `PipelineGraph`, `Socket`) and data structures.
*   **ImageAutomate.Execution**: Contains the execution engine and scheduling system.
*   **ImageAutomate.UI**: Provides the visualization components (`GraphRenderPanel`) and UI logic.
*   **ImageAutomate.StandardBlocks**: Contains built-in image processing blocks.
*   **ImageAutomate.Infrastructure**: Provides shared infrastructure (e.g., `ImageFormatRegistry`).

## Execution Flow

The execution engine traverses the `PipelineGraph`. Blocks consume `WorkItem`s from their input sockets and produce `WorkItem`s on their output sockets.

## Scheduling System

**Location**: `ImageAutomate.Execution/Scheduling/`

### IScheduler Interface

The `IScheduler` interface defines the contract for scheduling operations.

```csharp
public interface IScheduler
{
    bool HasPendingWork { get; }

    void Initialize(ExecutionContext context);
    IBlock? TryDequeue(ExecutionContext context);
    void NotifyCompleted(IBlock completedBlock, ExecutionContext context);
    void NotifyBlocked(IBlock blockedBlock, ExecutionContext context);
    void BeginNextShipmentCycle(ExecutionContext context);
}
```

**Note**: All methods accept an `ExecutionContext context` parameter.

### SchedulerRegistry

The `SchedulerRegistry` provides thread-safe registration for schedulers:

```csharp
public class SchedulerRegistry
{
    public void RegisterScheduler<T>(string name, Func<T> factory) where T : IScheduler;
    public void UnregisterScheduler(string name);
    public T CreateScheduler<T>(string name) where T : IScheduler;
    public bool IsRegistered(string name);
    public IReadOnlyList<string> GetRegisteredNames();
}
```

### Built-in Scheduler

**SimpleDfsScheduler** is the default scheduler, registered in the `SchedulerFactory` static constructor:

```csharp
static SchedulerFactory()
{
    _registry.RegisterScheduler("SimpleDfs", () => new SimpleDfsScheduler());
}
```

Plugins can register schedulers via `IRegistryAccessor`:

```csharp
public class PluginInitializer : IPluginInitializer
{
    public void Initialize(IRegistryAccessor? accessor)
    {
        accessor?.RegisterScheduler("MyScheduler", () => new MyScheduler());
    }
}
```

## ExecutorConfiguration

The `ExecutorConfiguration` class holds configuration settings for the execution engine.

```csharp
public class ExecutorConfiguration
{
    // String-based identifier for the execution mode
    public string Mode { get; set; } = "SimpleDfs";

    // Core settings
    public int MaxDegreeOfParallelism { get; set; } = Environment.ProcessorCount;
    public TimeSpan WatchdogTimeout { get; set; } = TimeSpan.FromSeconds(30);
    public bool EnableGcThrottling { get; set; } = true;
    public int MaxShipmentSize { get; set; } = 64;

    // Adaptive mode settings (deferred implementation)
    public int ProfilingWindowSize { get; set; } = 20;
    public double CostEmaAlpha { get; set; } = 0.2;
    public int CriticalPathRecomputeInterval { get; set; } = 10;
    public int BatchSize { get; set; } = 5;
    public double CriticalPathBoost { get; set; } = 1.5;
}
```

## Socket Record

The `Socket` record identifies input and output ports.

```csharp
public record Socket(string Id, string Name);
```

**Note**: `Socket.Id` is a `string`, not a `Guid`.

## Parallel Processing

The system supports parallel execution of blocks where dependencies allow, orchestrated by the scheduling system.