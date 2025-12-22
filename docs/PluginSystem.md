# ImageAutomate Plugin System

## Overview

The ImageAutomate Plugin System provides a flexible mechanism for loading, managing, and unloading plugins at runtime. Plugins can extend ImageAutomate by providing new blocks, ImageSharp format extensions, or other functionality.

## Architecture

The plugin system is built on top of .NET's `AssemblyLoadContext` with collectible assembly loading, allowing plugins to be dynamically loaded and unloaded without restarting the application.

### Key Components

1. **PluginLoader** - Main API for plugin management
2. **PluginLoadContext** - Custom AssemblyLoadContext for isolated plugin loading
3. **PluginInfo** - Metadata and state tracking for loaded plugins
4. **IPluginUnloadable** - Optional interface for plugins to handle unload requests

## Plugin Formats

The system supports three plugin formats:

### 1. Single DLL File
A standalone `.dll` file containing the plugin code.

```csharp
var loader = new PluginLoader();
var plugin = loader.LoadPlugin("path/to/MyPlugin.dll");
```

### 2. Directory-Based Plugin
A directory containing the plugin DLL and its dependencies. The directory name should match the main DLL name (e.g., `MyPlugin/MyPlugin.dll`).

```
MyPlugin/
├── MyPlugin.dll
├── Dependency1.dll
└── Dependency2.dll
```

```csharp
var plugin = loader.LoadPluginFromDirectory("path/to/MyPlugin");
```

### 3. ZIP Archive
A ZIP file containing a directory structure with the plugin DLL and dependencies.

```csharp
var plugin = loader.LoadPluginFromZip("path/to/MyPlugin.zip");
```

## MANIFEST.json (Optional)

Directory-based and ZIP plugins can include an optional `MANIFEST.json` file to specify plugin metadata:

```json
{
  "Name": "MyPlugin",
  "EntryPoint": "MyPlugin.dll",
  "Version": "1.0.0",
  "Description": "Description of the plugin",
  "Author": "Author Name",
  "Metadata": {
    "Website": "https://example.com",
    "License": "MIT"
  }
}
```

## Loading Plugins

### Load a Single Plugin

```csharp
var loader = new PluginLoader();
var plugin = loader.LoadPlugin("path/to/plugin.dll");

Console.WriteLine($"Loaded: {plugin.Name}");
Console.WriteLine($"Block types: {plugin.GetBlockTypes().Count()}");
```

### Load All Plugins from a Directory

```csharp
var plugins = loader.LoadAllPlugins("path/to/plugins");
Console.WriteLine($"Loaded {plugins.Count} plugins");
```

### Get Plugin Information

```csharp
var plugin = loader.GetPlugin("MyPlugin");
if (plugin != null)
{
    var blockTypes = plugin.GetBlockTypes();
    var allTypes = plugin.GetExportedTypes();
}
```

## Creating Plugin Instances

```csharp
// Get a block type from a plugin
var blockTypes = loader.GetPluginBlockTypes("MyPlugin");
var blockType = blockTypes.FirstOrDefault();

if (blockType != null)
{
    var instance = Activator.CreateInstance(blockType) as IBlock;
    
    // Register the instance for usage tracking
    loader.RegisterInstance(instance, "MyPlugin");
    
    // Use the instance...
    
    // When done, unregister it
    instance.Dispose();
    loader.UnregisterInstance(instance);
}
```

## Unloading Plugins

### Hard Unload
Unloads a plugin only if no instances are active. Throws an exception if instances exist.

```csharp
try
{
    loader.UnloadPlugin("MyPlugin");
    Console.WriteLine("Plugin unloaded");
}
catch (PluginException ex)
{
    Console.WriteLine($"Cannot unload: {ex.Message}");
}
```

### Soft Unload
Attempts to gracefully unload by requesting cleanup from active instances.

```csharp
bool unloaded = loader.TryUnloadPlugin("MyPlugin", timeout: TimeSpan.FromSeconds(5));
if (unloaded)
{
    Console.WriteLine("Plugin successfully unloaded");
}
else
{
    Console.WriteLine("Plugin cannot be unloaded at this time");
}
```

## Name Conflict Resolution

If a plugin with the same name is already loaded:
- Loading from the same path returns the existing plugin
- Loading from a different path auto-generates a unique name (e.g., `MyPlugin_1`, `MyPlugin_2`)

```csharp
var plugin1 = loader.LoadPlugin("path1/MyPlugin.dll");  // Name: "MyPlugin"
var plugin2 = loader.LoadPlugin("path2/MyPlugin.dll");  // Name: "MyPlugin_1"
```

## Implementing IPluginUnloadable

Plugins can implement `IPluginUnloadable` to participate in the soft unload process:

```csharp
public class MyBlock : IBlock, IPluginUnloadable
{
    private bool _isProcessing = false;
    
    public bool OnUnloadRequested()
    {
        if (_isProcessing)
        {
            // Reject unload - work in progress
            return false;
        }
        
        // Accept and cleanup
        Cleanup();
        return true;
    }
}
```

## Usage Tracking

The plugin system tracks active instances to prevent unloading plugins that are still in use:

```csharp
// Get active instance count
int count = loader.GetActiveInstanceCount("MyPlugin");
Console.WriteLine($"Active instances: {count}");

// Get all loaded plugins
foreach (var plugin in loader.LoadedPlugins)
{
    Console.WriteLine($"{plugin.Name}: {plugin.ActiveInstanceCount} active instances");
}
```

## Example: Complete Plugin Workflow

```csharp
var loader = new PluginLoader();

// Discover and load plugins
var plugins = loader.LoadAllPlugins("./plugins");

// Find a specific block type
foreach (var plugin in plugins)
{
    var blockType = plugin.GetBlockTypes().FirstOrDefault(t => t.Name == "ImageResizeBlock");
    if (blockType != null)
    {
        // Create and use instance
        var instance = Activator.CreateInstance(blockType) as IBlock;
        loader.RegisterInstance(instance, plugin.Name);
        
        try
        {
            // Use the block...
            instance.Title = "Resize to 800x600";
            var result = instance.Execute(inputs);
        }
        finally
        {
            // Cleanup
            instance.Dispose();
            loader.UnregisterInstance(instance);
        }
        
        break;
    }
}

// Unload unused plugins
foreach (var plugin in loader.LoadedPlugins)
{
    if (plugin.ActiveInstanceCount == 0)
    {
        loader.UnloadPlugin(plugin.Name);
    }
}
```

## Best Practices

1. **Always register instances** - Use `RegisterInstance()` and `UnregisterInstance()` to enable proper usage tracking
2. **Dispose instances** - Always dispose IBlock instances when done to free resources
3. **Use TryUnloadPlugin** - Prefer soft unload over hard unload for better user experience
4. **Shared dependencies** - ImageAutomate.Core and common .NET assemblies are shared between host and plugins
5. **Error handling** - Always wrap plugin operations in try-catch blocks
6. **MANIFEST files** - Use MANIFEST.json for better plugin metadata and documentation

## Thread Safety

The PluginLoader is thread-safe and can be safely used from multiple threads. Internal operations are protected by locks.

## Limitations

1. **Force unload not recommended** - Using `UnloadPlugin(name, force: true)` can cause crashes if instances are still active
2. **GC dependent** - Actual unloading happens during garbage collection
3. **AppDomain limitations** - .NET Core/5+ doesn't support AppDomains, so collectible AssemblyLoadContext is used instead
4. **Shared assemblies** - Types from shared assemblies (like ImageAutomate.Core) must match between host and plugins
