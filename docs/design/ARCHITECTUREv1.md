# Architecture

![Architecture Diagram](../assets/architecture_diagram.png)

## Overview

ImageAutomate is designed as a dataflow system where images are processed through a pipeline of blocks.

## Core Components

*   **ImageAutomate.Core**: Defines the core interfaces (`IBlock`, `IWorkItem`, `PipelineGraph`, `Socket`) and data structures.
*   **ImageAutomate.Execution**: Execution engine and scheduling system.
*   **ImageAutomate.UI**: Provides the visualization components (`GraphRenderPanel`) and UI logic.
*   **ImageAutomate.StandardBlocks**: Built-in image processing blocks.
*   **ImageAutomate.Infrastructure**: Shared infrastructure (e.g., `ImageFormatRegistry`).

## Execution Flow

The execution engine traverses the `PipelineGraph`. Blocks consume `WorkItem`s from their input sockets and produce `WorkItem`s on their output sockets.

## Scheduling System

**Location**: `ImageAutomate.Execution/Scheduling/`

### IScheduler Interface

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

**Note**: All methods take `ExecutionContext context` parameter.

### SchedulerRegistry

Thread-safe registry for scheduler registration:

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

**SimpleDfsScheduler** (registered in `SchedulerFactory` static constructor):

```csharp
static SchedulerFactory()
{
    _registry.RegisterScheduler("SimpleDfs", () => new SimpleDfsScheduler());
}
```

Plugin registration via `IRegistryAccessor`:

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

```csharp
public class ExecutorConfiguration
{
    // String-based (not enum) for extensibility
    public string Mode { get; set; } = "SimpleDfs";

    // Core settings
    public int MaxDegreeOfParallelism { get; set; } = Environment.ProcessorCount;
    public TimeSpan WatchdogTimeout { get; set; } = TimeSpan.FromSeconds(30);
    public bool EnableGcThrottling { get; set; } = true;
    public int MaxShipmentSize { get; set; } = 64;

    // Adaptive mode settings (not fully implemented)
    public int ProfilingWindowSize { get; set; } = 20;
    public double CostEmaAlpha { get; set; } = 0.2;
    public int CriticalPathRecomputeInterval { get; set; } = 10;
    public int BatchSize { get; set; } = 5;
    public double CriticalPathBoost { get; set; } = 1.5;
}
```

## Socket Record

```csharp
public record Socket(string Id, string Name);
```

**Important**: `Socket.Id` is a `string`, NOT a `Guid`.

## Parallel Processing

The system supports parallel execution of blocks where dependencies allow, orchestrated by the scheduling system.
