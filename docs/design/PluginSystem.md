# Plugin System Architecture

## Table of Contents

1. [Architectural Overview](#1-architectural-overview)
2. [Runtime Isolation Model](#2-runtime-isolation-model)
3. [Component Structure](#3-component-structure)
4. [Discovery & Loading Mechanism](#4-discovery--loading-mechanism)
5. [Lifecycle Management](#5-lifecycle-management)
6. [Safety & Consistency](#6-safety--consistency)
7. [Implementation Details](#7-implementation-details)

## 1. Architectural Overview

The **Plugin System** enables the dynamic extension of the ImageAutomate runtime without recompilation or restart. It adheres to a **Shared-Nothing** architecture (at the dependency level) while maintaining a strict contract via the Host Application's Core library.

*   **Design Pattern:** Microkernel / Plugin Pattern.
*   **Isolation Strategy:** `AssemblyLoadContext` (ALC) based isolation.
*   **Contract:** Types defined in `ImageAutomate.Core` (e.g., `IBlock`, `IPluginUnloadable`).
*   **Distribution Unit:** Single Assembly (`.dll`), Directory Package, or Archive (`.zip`).

## 2. Runtime Isolation Model

To prevent "Dependency Hell" (version conflicts between plugin dependencies and host dependencies), the system employs a custom **Assembly Isolation Layer**.

### 2.1. The Load Context Hierarchy

Plugins are loaded into discrete `PluginLoadContext` instances, which inherit from .NET's `AssemblyLoadContext`.

*   **Host Context (Default):** Contains system assemblies, `ImageAutomate.Core`, `SixLabors.ImageSharp`, and the UI shell.
*   **Plugin Context (Collectible):** Dedicated context for each plugin.
    *   **Shared Dependencies:** Requests for "Core" assemblies (Host, `System.*`, `Microsoft.*`, `netstandard`, `SixLabors.ImageSharp`) are delegated to the Host Context.
    *   **Private Dependencies:** Requests for plugin-specific libraries (e.g., specific JSON parsers, third-party CV libraries) are resolved locally within the plugin context.

### 2.2. Collectibility

The `PluginLoadContext` is configured as **Collectible** (`isCollectible: true`). This enables the Garbage Collector (GC) to unload the entire assembly set and reclaim memory once all references to types within that context are released, supporting dynamic "Hot Unload".

## 3. Component Structure

### 3.1. PluginLoader

The **PluginLoader** serves as the central manager (Facade) for the plugin ecosystem.

*   **Responsibilities:**
    *   **Discovery:** Scanning file system locations for valid plugins.
    *   **Context Management:** Instantiating and maintaining lifecycle of `PluginLoadContexts`.
    *   **Reference Tracking:** Tracking active instances via reference counting (`ActiveInstanceCount`) to ensure unload safety.
    *   **Registry:** Maintaining the mapping between loaded Plugins, their Assemblies, and metadata (`PluginInfo`).

### 3.2. PluginInfo

A robust metadata descriptor pattern representing a loaded plugin unit.

*   **Identity:** Name, Version, Path.
*   **Runtime State:** Reference to `Assembly`, `PluginLoadContext`, and `IsLoaded` status.
*   **Usage Statistics:** `ActiveInstanceCount` for soft-unload decisions.
*   **Methods:**
    - `GetExportedTypes()`: Returns all exported types from the plugin assembly.
    - `GetBlockTypes()`: Returns types that implement `IBlock` (non-abstract, non-interface).

### 3.3. PluginLoadContext

The implementation of the isolation mechanism.

*   **Overrides:** `Load(AssemblyName)` to enforce the sharing policy.
*   **Policy:**
    ```csharp
    if (assembly.Name == "ImageAutomate.Core" ||
        assembly.Name.StartsWith("System.") ||
        assembly.Name.StartsWith("Microsoft.") ||
        assembly.Name == "netstandard" ||
        assembly.Name == "SixLabors.ImageSharp")
    {
        return null; // Delegate to Host (Share)
    }
    else
    {
        return LoadFromPath(resolvedPath); // Load Private
    }
    ```

## 4. Discovery & Loading Mechanism

The system supports a hierarchical discovery strategy to accommodate various deployment scenarios.

### 4.1. Discovery Strategies

1.  **Direct Assembly:** Explicit loading of a `.dll`.
2.  **Directory Bundle:** Scanning a directory for a primary assembly matching the directory name, or a `MANIFEST.json`.
3.  **Archive Bundle:** Ephemeral extraction of `.zip` packages to a temporary staging area followed by Directory Bundle loading.

### 4.2. Conflict Resolution

*   **Namespace Collisions:** Handled by ALC isolation (two plugins can define `MyNamespace.MyClass` without conflict).
*   **Name Collisions:** The `PluginLoader` enforces unique logical names. If `MyPlugin` is loaded, a second attempt yields `MyPlugin_1`.

## 5. Lifecycle Management

### 5.1. Instantiation

Code within the Host Application (e.g., the UI or Execution Engine) requests types from the `PluginLoader`.

*   **Type Resolution:** `GetPluginBlockTypes()` reflects over exported types in the isolated context.
*   **Activation:** Standard `Activator.CreateInstance()` creates the object.
*   **Registration:** The Host **MUST** call `RegisterInstance(obj, pluginName)` to increment the reference counter.

### 5.2. Plugin Initialization Convention

Plugins can optionally define a `PluginInitializer` class that implements `IPluginInitializer`:

```csharp
public interface IPluginInitializer
{
    void Initialize(IRegistryAccessor? registryAccessor);
}
```

*   **Discovery:** The `PluginLoader` looks for a type named `PluginInitializer` implementing `IPluginInitializer`.
*   **Invocation:** `Initialize(IRegistryAccessor? accessor)` is called after the assembly is loaded.
*   **Usage:** Allows plugins to register custom schedulers or perform other initialization.

### 5.3. Plugin Unload Notification

Plugins can optionally implement `IPluginUnloadable` to receive unload notifications:

```csharp
public interface IPluginUnloadable
{
    bool OnUnloadRequested();
}
```

*   **Returns:** `true` if the object accepts unload and has cleaned up; `false` if the object rejects unload (e.g., work in progress).
*   **Usage:** Called during soft unload to allow cooperative cleanup.

### 5.4. Unloading (Soft & Hard)

Unloading is a sensitive operation due to the lack of forced memory access revocation in managed code.

1.  **Soft Unload (`TryUnloadPlugin`):**
    *   **Protocol:** Checks `ActiveInstanceCount`. If > 0, it queries instances implementing `IPluginUnloadable.OnUnloadRequested()`.
    *   **Cooperation:** If all instances agree to dispose themselves, the unload proceeds.
    *   **Cleanup:** Polls until `ActiveInstanceCount` drops to zero or timeout occurs.

2.  **Hard Unload (`UnloadPlugin`):**
    *   **Mechanism:** Calls `PluginLoadContext.Unload()`.
    *   **Risk:** If the Host holds strong references to plugin types, the GC cannot reclaim the memory, leading to a memory leak (though the plugin is logically "unloaded").

## 6. Safety & Consistency

### 6.1. IRegistryAccessor

`IRegistryAccessor` is an abstraction layer to allow Core to register schedulers without referencing the Execution assembly:

```csharp
public interface IRegistryAccessor
{
    void RegisterScheduler(string name, Func<object> factory);
}
```

Implemented by `RegistryAccessorProxy` using reflection to avoid circular dependency. Plugins can use this to register custom schedulers:

```csharp
public class PluginInitializer : IPluginInitializer
{
    public void Initialize(IRegistryAccessor? accessor)
    {
        accessor?.RegisterScheduler("MyScheduler", () => new MyScheduler());
    }
}
```

### 6.2. Type Equivalence

To ensure objects passed between Host and Plugin are compatible, `ImageAutomate.Core` must be shared.

*   **Mechanism:** The `PluginLoadContext` explicitly rejects loading `ImageAutomate.Core.dll` from the plugin folder, forcing usage of the Host's loaded version. This ensures `typeof(IBlock)` in Plugin A is identical to `typeof(IBlock)` in Host.

### 6.3. Reference Counting

The system implements a manual Reference Counting (RC) mechanism for high-level safety, distinct from the GC.

*   **Purpose:** Prevents accidental unloading while a Block is executing or displayed in the UI.
*   **Implementation:** `ConcurrentDictionary<object, string>` maps instances to plugin names. `Interlocked` counters track total active objects.

## 7. Implementation Details

### 7.1. Thread Safety

All public API methods on `PluginLoader` are thread-safe, utilizing:
*   `ConcurrentDictionary` for O(1) lookups.
*   `Monitor` (locks) for critical sections involving file I/O and state mutation (Load/Unload operations).

### 7.2. Manifest Schema

For Directory and Archive bundles, a `MANIFEST.json` provides metadata:

```json
{
  "Name": "EdgeDetection",
  "EntryPoint": "EdgeDetect.dll",
  "Version": "1.2.0",
  "Metadata": { ... }
}
```

This decouples the file system structure from the logical plugin identity.
