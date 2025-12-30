# Infrastructure Standardization Plan

## Executive Summary

This document identifies the current state of lifetime management, module division, and interface design in the ImageAutomate solution. It catalogs problematic patterns and proposes a unified architecture for dependency injection and registry management.

---

## 1. Current State Analysis

### 1.1 Module Overview

The solution consists of 6 projects with the following intended roles:

| Project | Intended Role | Current State |
|---------|---------------|---------------|
| `ImageAutomate.Core` | Shared contracts, interfaces, no external deps | ⚠️ Has ImageSharp dependency; contains plugin system implementation |
| `ImageAutomate.Infrastructure` | Secondary core for ImageSharp-dependent code | ✓ Contains only `ImageFormatRegistry` |
| `ImageAutomate.Execution` | Execution engine, scheduling | ⚠️ Has `StandardBlocks` dependency (circular) |
| `ImageAutomate.StandardBlocks` | Built-in block library | ✓ Correctly depends on Core + Infrastructure |
| `ImageAutomate.UI` | WinForms controls, rendering | ✓ Correctly depends on Core |
| `ImageAutomate` (App) | GUI composition root | ⚠️ Namespace collision with Core |

### 1.2 Dependency Graph

```
                    ┌──────────────────┐
                    │ ImageAutomate    │
                    │ (App)            │
                    └────────┬─────────┘
                             │
    ┌────────────────────────┼────────────────────────┐
    │              │         │         │              │
    ▼              ▼         ▼         ▼              ▼
┌────────┐  ┌──────────┐ ┌──────┐ ┌──────────────┐ ┌──────────────┐
│ Core   │◀─│Execution │ │  UI  │ │Infrastructure│ │StandardBlocks│
└────────┘  └──────────┘ └──────┘ └──────────────┘ └──────────────┘
    ▲              │                     ▲              │
    │              │                     │              │
    └──────────────┴─────────────────────┴──────────────┘
                   (circular via StandardBlocks)
```

---

## 2. Lifetime Management Issues

### 2.1 Identified Patterns

The codebase has **4 distinct lifetime management patterns** running concurrently:

#### Pattern A: Registry + Static Factory (Schedulers)

**Files:**
- [SchedulerFactory.cs](../ImageAutomate/ImageAutomate.Execution/SchedulerFactory.cs)
- [SchedulerRegistry.cs](../ImageAutomate/ImageAutomate.Execution/Scheduling/SchedulerRegistry.cs)

**Description:**
- `SchedulerFactory` is a static class with a private static `SchedulerRegistry` instance
- Built-in schedulers registered in static constructor
- Factory creates **new instances** on each call (transient)
- Plugins register via `IRegistryAccessor` using reflection

**Problems:**
1. Static coupling - no way to inject alternative registries
2. Mixed responsibilities (registry + factory in one place)
3. Creates transient instances but no clear lifecycle

```csharp
// Current
public static class SchedulerFactory
{
    private static readonly SchedulerRegistry _registry = new();
    public static IScheduler CreateScheduler(string mode) => _registry.CreateScheduler(mode);
}
```

#### Pattern B: Singleton Registry (ImageFormatRegistry)

**Files:**
- [ImageFormatRegistry.cs](../ImageAutomate/ImageAutomate.Infrastructure/ImageFormatRegistry.cs)
- [FormatRegistryInitializer.cs](../ImageAutomate/ImageAutomate.StandardBlocks/FormatRegistryInitializer.cs)

**Description:**
- `ImageFormatRegistry.Instance` is a Singleton
- Strategies are stored directly (not factories) - **singleton strategies**
- Initialization done explicitly in `Program.cs`

**Problems:**
1. Strategies are cached forever (implicit singleton lifetime)
2. No factory indirection - can't create per-request strategies
3. Scattered initialization (Program.cs, tests, etc.)
4. Direct static access from `SaveBlock`, `ConvertBlock`

```csharp
// Current
ImageFormatRegistry.Instance.GetFormat("JPEG")  // Called directly from blocks
```

#### Pattern C: Lazy Singleton Service (WorkspaceService)

**Files:**
- [WorkspaceService.cs](../ImageAutomate/ImageAutomate/Services/WorkspaceService.cs)

**Description:**
- `WorkspaceService.Instance` with lazy initialization
- Internally creates `CsvWorkspaceDataContext` dependency
- Acts as singleton for workspace metadata

**Problems:**
1. Hidden dependency construction
2. Not testable (hardcoded `CsvWorkspaceDataContext`)
3. Static access throughout app

```csharp
// Current
public static WorkspaceService Instance => _instance ??= new WorkspaceService(new CsvWorkspaceDataContext());
```

#### Pattern D: Plugin System (Manual Reference Counting)

**Files:**
- [PluginLoader.cs](../ImageAutomate/ImageAutomate.Core/PluginLoader.cs)
- [PluginInfo.cs](../ImageAutomate/ImageAutomate.Core/PluginInfo.cs)
- [RegistryAccessorProxy](../ImageAutomate/ImageAutomate.Core/PluginLoader.cs#L556)

**Description:**
- `PluginLoader` manages plugin lifecycles with manual reference counting
- `ActiveInstanceCount` tracking per plugin
- `IPluginUnloadable` for graceful shutdown
- Uses reflection-based `RegistryAccessorProxy` to bridge Core → Execution/Infrastructure

**Problems:**
1. Manual reference counting is error-prone
2. `RegistryAccessorProxy` uses reflection (fragile, no compile-time safety)
3. No instance created by PluginLoader is tracked for disposal
4. Plugin blocks are transient but their strategies might be singleton

#### Pattern E: Static Renderer Instance (NodeRenderer)

**Files:**
- [NodeRenderer.cs](../ImageAutomate/ImageAutomate.UI/NodeRenderer.cs)

**Description:**
- `NodeRenderer.Instance` is a public static readonly field
- Holds GDI+ resources (pens, brushes, fonts)
- Implements `IDisposable` but never disposed

**Problems:**
1. Never disposed (resource leak on app exit)
2. Static instance prevents configuration
3. Used directly from `GraphRenderPanel`

---

## 3. Module Division Issues

### 3.1 Core Has Too Many Responsibilities

`ImageAutomate.Core` currently contains:

| Category | Items | Should Be In |
|----------|-------|--------------|
| Contracts | `IBlock`, `Socket`, `IWorkItem` | Core ✓ |
| Data Structures | `PipelineGraph`, `Workspace` | Core ✓ |
| **Plugin System** | `PluginLoader`, `PluginInfo`, `PluginLoadContext` | New: `ImageAutomate.Plugins` |
| **Serialization** | `BlockSerializer`, `SerializationDto` | New: `ImageAutomate.Serialization` |
| **ImageSharp Dependency** | `IImageFormatStrategy` (uses `IImageEncoder`) | Infrastructure |

**Problem:** Core depends on `SixLabors.ImageSharp` which breaks the "no external deps" principle.

### 3.2 Namespace Collision

**Critical Issue:** The GUI app has a folder `ImageAutomate/Core/` with:
- [UserConfiguration.cs](../ImageAutomate/ImageAutomate/Core/UserConfiguration.cs) using `namespace ImageAutomate.Core`

This collides with the `ImageAutomate.Core` project namespace, causing:
1. Ambiguous type resolution
2. Confusing IntelliSense
3. Potential assembly loading issues

### 3.3 Execution → StandardBlocks Circular Dependency

**Dependency Chain:**
```
Execution → StandardBlocks (for block types in tests/initialization)
StandardBlocks → Infrastructure → Core
```

The `.csproj` shows:
```xml
<!-- ImageAutomate.Execution.csproj -->
<ProjectReference Include="..\ImageAutomate.StandardBlocks\ImageAutomate.StandardBlocks.csproj" />
```

**Question:** Why does Execution need StandardBlocks?

---

## 4. Missing Interface Abstractions

### 4.1 Concrete Types Without Interfaces

| Class | Module | Should Have Interface |
|-------|--------|----------------------|
| `GraphValidator` | Execution | `IGraphValidator` ✓ (exists, good) |
| `GraphExecutor` | Execution | `IGraphExecutor` ✓ (exists, good) |
| `SchedulerFactory` | Execution | `ISchedulerFactory` ✗ |
| `SchedulerRegistry` | Execution | `ISchedulerRegistry` ✗ |
| `ImageFormatRegistry` | Infrastructure | `IImageFormatRegistry` ✓ (exists, good) |
| `WorkspaceService` | App | `IWorkspaceService` ✗ |
| `CsvWorkspaceDataContext` | App | `IWorkspaceDataContext` ✓ (exists, good) |
| `NodeRenderer` | UI | `INodeRenderer` ✗ |
| `PluginLoader` | Core | `IPluginLoader` ✗ |
| `UserConfiguration` | App | Should not exist as static |
| `EditorConfiguration` | UI | Should be injectable |

### 4.2 Static Classes That Should Be Injected

| Static Class | Current Access | Recommended |
|--------------|----------------|-------------|
| `SchedulerFactory` | `SchedulerFactory.CreateScheduler()` | Inject `ISchedulerFactory` |
| `ImageFormatRegistry.Instance` | Direct static access | Inject `IImageFormatRegistry` |
| `WorkspaceService.Instance` | Direct static access | Inject `IWorkspaceService` |
| `NodeRenderer.Instance` | Direct static access | Inject `INodeRenderer` |
| `UserConfiguration` | Static properties | Inject `IUserConfiguration` |
| `EditorConfiguration` | Static properties | Inject `IEditorConfiguration` |

---

## 5. Problematic Items Catalog

### 5.1 Priority 1 - Breaking Issues

| ID | Issue | Location | Impact |
|----|-------|----------|--------|
| P1-1 | Namespace collision `ImageAutomate.Core` | `ImageAutomate/Core/` folder | Type resolution confusion |
| P1-2 | Core depends on ImageSharp | `IImageFormatStrategy` | Breaks contract-only principle |
| P1-3 | Execution → StandardBlocks dependency | `Execution.csproj` | Circular dependency |

### 5.2 Priority 2 - Architectural Debt

| ID | Issue | Location | Impact |
|----|-------|----------|--------|
| P2-1 | SchedulerFactory is static | `SchedulerFactory.cs` | Not testable, not extensible |
| P2-2 | ImageFormatRegistry is singleton | `ImageFormatRegistry.cs` | Strategies have undefined lifetime |
| P2-3 | WorkspaceService singleton | `WorkspaceService.cs` | Hidden dependencies |
| P2-4 | RegistryAccessorProxy uses reflection | `PluginLoader.cs` | Fragile, runtime errors |
| P2-5 | NodeRenderer never disposed | `NodeRenderer.cs` | Resource leak |
| P2-6 | Plugin system in Core | `PluginLoader.cs` | Core is too heavy |

### 5.3 Priority 3 - Code Quality

| ID | Issue | Location | Impact |
|----|-------|----------|--------|
| P3-1 | No ISchedulerRegistry interface | `SchedulerRegistry.cs` | Hard to test/mock |
| P3-2 | No ISchedulerFactory interface | `SchedulerFactory.cs` | Hard to test/mock |
| P3-3 | No INodeRenderer interface | `NodeRenderer.cs` | Hard to test/mock |
| P3-4 | No IPluginLoader interface | `PluginLoader.cs` | Hard to test/mock |
| P3-5 | UserConfiguration is static | `UserConfiguration.cs` | Hard to test |
| P3-6 | EditorConfiguration is static | `EditorConfiguration.cs` | Hard to configure per-instance |
| P3-7 | Format strategies stored directly | `ImageFormatRegistry.cs` | No factory pattern |

---

## 6. Proposed Architecture

### 6.1 Module Restructuring

```
ImageAutomate.Contracts       (NEW - pure interfaces, no deps)
├── IBlock.cs
├── IScheduler.cs
├── IWorkItem.cs
├── Socket.cs
└── PipelineGraph.cs

ImageAutomate.Core            (data structures, depends on Contracts)
├── Workspace.cs
├── WorkItem.cs
└── SystemConfiguration.cs

ImageAutomate.Plugins         (NEW - plugin system)
├── IPluginLoader.cs
├── PluginLoader.cs
├── PluginInfo.cs
└── PluginLoadContext.cs

ImageAutomate.Infrastructure  (ImageSharp integration)
├── IImageFormatRegistry.cs
├── ImageFormatRegistry.cs
└── IImageFormatStrategy.cs

ImageAutomate.Execution       (no StandardBlocks dependency)
├── ISchedulerFactory.cs
├── ISchedulerRegistry.cs
├── SchedulerRegistry.cs
├── SchedulerFactory.cs
└── ...

ImageAutomate.StandardBlocks  (no changes)

ImageAutomate.UI              (no changes)

ImageAutomate (App)
├── Composition/              (NEW - DI setup)
│   └── ServiceCollectionExtensions.cs
├── Configuration/            (RENAMED from Core)
│   └── UserConfiguration.cs  (→ IUserConfiguration)
├── Services/
│   └── WorkspaceService.cs   (→ IWorkspaceService)
└── ...
```

### 6.2 Lifetime Strategy

Adopt a consistent **Factory + Registry** pattern with clear lifetimes:

| Component | Lifetime | Access Pattern |
|-----------|----------|----------------|
| `ISchedulerRegistry` | Singleton | Injected |
| `ISchedulerFactory` | Singleton | Injected |
| `IScheduler` | **Transient** (per execution) | Created by factory |
| `IImageFormatRegistry` | Singleton | Injected |
| `IImageFormatStrategy` | **Transient** (per use) | Created by factory |
| `IWorkspaceService` | Singleton | Injected |
| `INodeRenderer` | Singleton | Injected |
| `IPluginLoader` | Singleton | Injected |
| `IUserConfiguration` | Singleton | Injected |

### 6.3 Interface Standardization

All registries should implement a common pattern:

```csharp
public interface INamedRegistry<TFactory> where TFactory : Delegate
{
    void Register(string name, TFactory factory);
    bool Unregister(string name);
    TFactory? GetFactory(string name);
    bool IsRegistered(string name);
    IReadOnlyList<string> GetRegisteredNames();
}

public interface ISchedulerRegistry : INamedRegistry<Func<IScheduler>>
{
}

public interface IImageFormatRegistry : INamedRegistry<Func<IImageFormatStrategy>>
{
    // Additional format-specific methods
    IImageFormatStrategy? GetFormat(string formatName);
}
```

### 6.4 Dependency Injection Composition Root

```csharp
// ImageAutomate/Composition/ServiceCollectionExtensions.cs
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddImageAutomate(this IServiceCollection services)
    {
        // Registries (Singleton)
        services.AddSingleton<ISchedulerRegistry, SchedulerRegistry>();
        services.AddSingleton<IImageFormatRegistry, ImageFormatRegistry>();
        
        // Factories (Singleton)
        services.AddSingleton<ISchedulerFactory, SchedulerFactory>();
        
        // Executors (Transient - new per execution)
        services.AddTransient<IGraphExecutor, GraphExecutor>();
        services.AddTransient<IGraphValidator, GraphValidator>();
        
        // Services (Singleton)
        services.AddSingleton<IWorkspaceService, WorkspaceService>();
        services.AddSingleton<IPluginLoader, PluginLoader>();
        services.AddSingleton<INodeRenderer, NodeRenderer>();
        
        // Configuration (Singleton)
        services.AddSingleton<IUserConfiguration, UserConfiguration>();
        
        return services;
    }
}
```

---

## 7. Migration Plan

### Phase 1: Fix Breaking Issues (Priority 1)

1. **Rename namespace collision**
   - Rename `ImageAutomate/Core/` folder to `ImageAutomate/Configuration/`
   - Change namespace from `ImageAutomate.Core` to `ImageAutomate.Configuration`

2. **Move ImageSharp types from Core**
   - Move `IImageFormatStrategy` to Infrastructure
   - Update all references

3. **Remove Execution → StandardBlocks dependency**
   - Identify why this dependency exists
   - Move shared types to Core if needed

### Phase 2: Introduce Interfaces (Priority 2-3)

4. **Create missing interfaces**
   - `ISchedulerFactory`, `ISchedulerRegistry`
   - `IWorkspaceService`
   - `INodeRenderer`
   - `IPluginLoader`
   - `IUserConfiguration`, `IEditorConfiguration`

5. **Update implementations**
   - Make registries non-static, implement interfaces
   - Remove `.Instance` pattern from `ImageFormatRegistry`
   - Remove static accessor from `WorkspaceService`

### Phase 3: Implement DI (Optional)

6. **Add Microsoft.Extensions.DependencyInjection**
   - Create `ServiceCollectionExtensions`
   - Update `Program.cs` to use DI container
   - Update all classes to receive dependencies via constructor

7. **Update tests**
   - Use mock implementations where needed
   - Remove direct static access

---

## 8. Decision Points

### 8.1 Do We Need Full DI Container?

**Option A: Microsoft.Extensions.DependencyInjection**
- Pros: Industry standard, well-supported, testable
- Cons: Learning curve, adds dependency, more boilerplate

**Option B: Manual Composition Root**
- Pros: Simpler, fewer dependencies, explicit
- Cons: More manual wiring, no automatic lifetime management

**Recommendation:** Start with manual composition root, add DI container if complexity grows.

### 8.2 How to Handle Plugin Registry Access?

**Current:** Reflection-based `RegistryAccessorProxy`

**Option A: Keep reflection but add interface**
- Pros: Backward compatible
- Cons: Still fragile

**Option B: Pass registries directly to PluginLoader**
- Pros: Type-safe, no reflection
- Cons: Tighter coupling

**Option C: Event-based registration**
- Pros: Decoupled
- Cons: More complex

**Recommendation:** Option B with interfaces

### 8.3 Should Strategies Be Factories?

**Current:** `ImageFormatRegistry` stores strategy instances directly

**Option A: Keep storing instances (singleton strategies)**
- Pros: Simple, no breaking change
- Cons: Can't customize per-request

**Option B: Store factories, create strategies per-request**
- Pros: More flexible, stateless
- Cons: Breaking change for plugins

**Recommendation:** Keep instances for now (Option A), document that strategies should be stateless.

---

## 9. Appendix: File Index

### Files Requiring Changes

| File | Changes Needed |
|------|----------------|
| `ImageAutomate/Core/UserConfiguration.cs` | Rename namespace |
| `ImageAutomate.Execution/SchedulerFactory.cs` | Add interface, make non-static |
| `ImageAutomate.Execution/Scheduling/SchedulerRegistry.cs` | Add interface |
| `ImageAutomate.Infrastructure/ImageFormatRegistry.cs` | Remove static Instance |
| `ImageAutomate/Services/WorkspaceService.cs` | Add interface, remove Instance |
| `ImageAutomate.UI/NodeRenderer.cs` | Add interface, fix disposal |
| `ImageAutomate.Core/PluginLoader.cs` | Add interface, fix RegistryAccessorProxy |
| `ImageAutomate.UI/EditorConfiguration.cs` | Consider making injectable |

### New Files Needed

| File | Purpose |
|------|---------|
| `ImageAutomate.Execution/ISchedulerFactory.cs` | Interface for scheduler factory |
| `ImageAutomate.Execution/ISchedulerRegistry.cs` | Interface for scheduler registry |
| `ImageAutomate/Services/IWorkspaceService.cs` | Interface for workspace service |
| `ImageAutomate.UI/INodeRenderer.cs` | Interface for node renderer |
| `ImageAutomate.Core/IPluginLoader.cs` | Interface for plugin loader |
| `ImageAutomate/Configuration/IUserConfiguration.cs` | Interface for user config |
