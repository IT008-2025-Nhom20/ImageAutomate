using System.Collections.Concurrent;
using System.IO.Compression;
using System.Reflection;
using System.Text.Json;

namespace ImageAutomate.Core;

/// <summary>
/// Exception thrown when a plugin operation fails.
/// </summary>
public class PluginException : Exception
{
    public PluginException(string message) : base(message) { }
    public PluginException(string message, Exception innerException) : base(message, innerException) { }

    public PluginException()
    {
    }
}

/// <summary>
/// Manages loading, unloading, and lifecycle of plugins for ImageAutomate.
/// Supports loading plugins from DLL files, directories, and ZIP archives.
/// </summary>
public class PluginLoader
{
    private readonly ConcurrentDictionary<string, PluginInfo> _plugins = new();
    private readonly ConcurrentDictionary<object, string> _instanceToPlugin = new();
    private readonly object _lock = new();

    private readonly IRegistryAccessor _registryAccessor;
    private readonly string _pluginsDirectory;

    /// <summary>
    /// Gets a read-only collection of all loaded plugins.
    /// </summary>
    public IReadOnlyCollection<PluginInfo> LoadedPlugins => _plugins.Values.Where(p => p.IsLoaded).ToList();

    /// <summary>
    /// Initializes a new instance of the PluginLoader.
    /// </summary>
    /// <param name="pluginsDirectory">Directory to search for plugins.</param>
    /// <param name="registryAccessor">Accessor for system registries.</param>
    public PluginLoader(string pluginsDirectory, IRegistryAccessor registryAccessor)
    {
        _pluginsDirectory = pluginsDirectory;
        _registryAccessor = registryAccessor ?? throw new ArgumentNullException(nameof(registryAccessor));
    }

    /// <summary>
    /// Initializes a new instance of the PluginLoader with default registry access.
    /// </summary>
    /// <param name="pluginsDirectory">Directory to search for plugins.</param>
    public PluginLoader(string pluginsDirectory)
        : this(pluginsDirectory, new RegistryAccessorProxy())
    {
    }

    /// <summary>
    /// Loads a plugin from a single DLL file.
    /// </summary>
    /// <param name="dllPath">Path to the plugin DLL file.</param>
    /// <param name="pluginName">Optional custom name for the plugin. If null, uses the assembly name.</param>
    /// <returns>Information about the loaded plugin.</returns>
    /// <exception cref="PluginException">Thrown when the plugin cannot be loaded.</exception>
    public PluginInfo LoadPlugin(string dllPath, string? pluginName = null)
    {
        if (!File.Exists(dllPath))
        {
            throw new PluginException($"Plugin file not found: {dllPath}");
        }

        string fullPath = Path.GetFullPath(dllPath);
        string name = pluginName ?? Path.GetFileNameWithoutExtension(dllPath);

        lock (_lock)
        {
            // Check for name conflicts
            if (_plugins.TryGetValue(name, out var existing) && existing.IsLoaded)
            {
                // If already loaded from same path, return existing
                if (existing.AssemblyPath == fullPath)
                {
                    return existing;
                }

                // Name conflict - append a suffix
                int suffix = 1;
                string originalName = name;
                while (_plugins.ContainsKey(name) && _plugins[name].IsLoaded)
                {
                    name = $"{originalName}_{suffix++}";
                }
            }

            try
            {
                var loadContext = new PluginLoadContext(fullPath);
                var assembly = loadContext.LoadFromAssemblyPath(fullPath);

                // Auto-discover and initialize plugin entry point
                TryInitializePlugin(assembly);

                var pluginInfo = new PluginInfo(name, fullPath, assembly, loadContext);

                _plugins[name] = pluginInfo;
                return pluginInfo;
            }
            catch (Exception ex)
            {
                throw new PluginException($"Failed to load plugin '{name}' from '{dllPath}': {ex.Message}", ex);
            }
        }
    }

    /// <summary>
    /// Loads a plugin from a directory containing the plugin DLL and its dependencies.
    /// The directory should contain a DLL with the same name as the directory, or a MANIFEST.json file.
    /// </summary>
    /// <param name="directoryPath">Path to the plugin directory.</param>
    /// <returns>Information about the loaded plugin.</returns>
    /// <exception cref="PluginException">Thrown when the plugin cannot be loaded.</exception>
    public PluginInfo LoadPluginFromDirectory(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
        {
            throw new PluginException($"Plugin directory not found: {directoryPath}");
        }

        string fullPath = Path.GetFullPath(directoryPath);
        string directoryName = new DirectoryInfo(fullPath).Name;

        // Try to find MANIFEST.json first
        string manifestPath = Path.Combine(fullPath, "MANIFEST.json");
        if (File.Exists(manifestPath))
        {
            try
            {
                var manifest = JsonSerializer.Deserialize<PluginManifest>(File.ReadAllText(manifestPath));
                if (manifest != null && !string.IsNullOrEmpty(manifest.EntryPoint))
                {
                    string dllPath = Path.Combine(fullPath, manifest.EntryPoint);
                    if (File.Exists(dllPath))
                    {
                        return LoadPlugin(dllPath, manifest.Name ?? directoryName);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new PluginException($"Failed to parse MANIFEST.json in '{directoryPath}': {ex.Message}", ex);
            }
        }

        // Fallback: Look for DLL with same name as directory
        string expectedDllPath = Path.Combine(fullPath, $"{directoryName}.dll");
        if (File.Exists(expectedDllPath))
        {
            return LoadPlugin(expectedDllPath, directoryName);
        }

        // Look for any DLL in the directory
        var dllFiles = Directory.GetFiles(fullPath, "*.dll");
        if (dllFiles.Length == 1)
        {
            return LoadPlugin(dllFiles[0], directoryName);
        }

        throw new PluginException($"Could not find plugin entry point in directory '{directoryPath}'. " +
            "Expected a MANIFEST.json file, a DLL named '{directoryName}.dll', or a single DLL file.");
    }

    /// <summary>
    /// Loads a plugin from a ZIP archive.
    /// The archive should contain a directory structure with a plugin DLL.
    /// </summary>
    /// <param name="zipPath">Path to the ZIP archive.</param>
    /// <param name="extractToTemp">If true, extracts to a temporary directory. If false, extracts to a permanent location.</param>
    /// <returns>Information about the loaded plugin.</returns>
    /// <exception cref="PluginException">Thrown when the plugin cannot be loaded.</exception>
    public PluginInfo LoadPluginFromZip(string zipPath, bool extractToTemp = true)
    {
        if (!File.Exists(zipPath))
        {
            throw new PluginException($"Plugin ZIP file not found: {zipPath}");
        }

        string extractPath;
        if (extractToTemp)
        {
            extractPath = Path.Combine(Path.GetTempPath(), "ImageAutomate_Plugins", Guid.NewGuid().ToString());
        }
        else
        {
            string pluginName = Path.GetFileNameWithoutExtension(zipPath);
            extractPath = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(zipPath)) ?? ".", pluginName);
        }

        try
        {
            Directory.CreateDirectory(extractPath);
            ZipFile.ExtractToDirectory(zipPath, extractPath, overwriteFiles: true);

            // The ZIP might contain a single root directory or files directly
            var subdirs = Directory.GetDirectories(extractPath);
            if (subdirs.Length == 1 && Directory.GetFiles(extractPath).Length == 0)
            {
                // ZIP contains a single root directory, use it
                return LoadPluginFromDirectory(subdirs[0]);
            }
            else
            {
                // Files are directly in the extraction path
                return LoadPluginFromDirectory(extractPath);
            }
        }
        catch (Exception ex)
        {
            // Clean up on failure
            try
            {
                if (Directory.Exists(extractPath))
                {
                    Directory.Delete(extractPath, recursive: true);
                }
            }
            catch { }

            throw new PluginException($"Failed to load plugin from ZIP '{zipPath}': {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Discovers and loads all valid plugins from a directory.
    /// Supports DLL files, subdirectories with matching DLL names, and ZIP archives.
    /// </summary>
    /// <param name="searchDirectory">Directory to search for plugins.</param>
    /// <param name="searchPattern">Optional search pattern for discovery.</param>
    /// <returns>Collection of loaded plugin information.</returns>
    public IReadOnlyList<PluginInfo> LoadAllPlugins(string searchDirectory, string searchPattern = "*")
    {
        if (!Directory.Exists(searchDirectory))
        {
            throw new PluginException($"Plugin search directory not found: {searchDirectory}");
        }

        var loadedPlugins = new List<PluginInfo>();
        var errors = new List<string>();

        // Find standalone DLL files
        foreach (var dllFile in Directory.GetFiles(searchDirectory, "*.dll", SearchOption.TopDirectoryOnly))
        {
            try
            {
                var pluginInfo = LoadPlugin(dllFile);
                loadedPlugins.Add(pluginInfo);
            }
            catch (Exception ex)
            {
                errors.Add($"{Path.GetFileName(dllFile)}: {ex.Message}");
            }
        }

        // Find directories with matching DLL
        foreach (var dir in Directory.GetDirectories(searchDirectory))
        {
            string dirName = new DirectoryInfo(dir).Name;
            string expectedDll = Path.Combine(dir, $"{dirName}.dll");

            if (File.Exists(expectedDll) || File.Exists(Path.Combine(dir, "MANIFEST.json")))
            {
                try
                {
                    var pluginInfo = LoadPluginFromDirectory(dir);
                    loadedPlugins.Add(pluginInfo);
                }
                catch (Exception ex)
                {
                    errors.Add($"{dirName}/: {ex.Message}");
                }
            }
        }

        // Find ZIP archives
        foreach (var zipFile in Directory.GetFiles(searchDirectory, "*.zip", SearchOption.TopDirectoryOnly))
        {
            try
            {
                var pluginInfo = LoadPluginFromZip(zipFile, extractToTemp: false);
                loadedPlugins.Add(pluginInfo);
            }
            catch (Exception ex)
            {
                errors.Add($"{Path.GetFileName(zipFile)}: {ex.Message}");
            }
        }

        return loadedPlugins;
    }

    /// <summary>
    /// Gets information about a loaded plugin by name.
    /// </summary>
    /// <param name="pluginName">Name of the plugin.</param>
    /// <returns>Plugin information, or null if not found or not loaded.</returns>
    public PluginInfo? GetPlugin(string pluginName)
    {
        if (_plugins.TryGetValue(pluginName, out var plugin) && plugin.IsLoaded)
        {
            return plugin;
        }
        return null;
    }

    /// <summary>
    /// Gets all types exported by a plugin.
    /// </summary>
    /// <param name="pluginName">Name of the plugin.</param>
    /// <returns>Collection of types, or empty if plugin not found.</returns>
    public IEnumerable<Type> GetPluginTypes(string pluginName)
    {
        var plugin = GetPlugin(pluginName);
        return plugin?.GetExportedTypes() ?? Enumerable.Empty<Type>();
    }

    /// <summary>
    /// Gets all block types from a plugin.
    /// </summary>
    /// <param name="pluginName">Name of the plugin.</param>
    /// <returns>Collection of block types.</returns>
    public IEnumerable<Type> GetPluginBlockTypes(string pluginName)
    {
        var plugin = GetPlugin(pluginName);
        return plugin?.GetBlockTypes() ?? Enumerable.Empty<Type>();
    }

    /// <summary>
    /// Registers an instance as being created from a plugin.
    /// This is used for tracking plugin usage.
    /// </summary>
    /// <param name="instance">The instance to register.</param>
    /// <param name="pluginName">Name of the plugin that created the instance.</param>
    public void RegisterInstance(object instance, string pluginName)
    {
        ArgumentNullException.ThrowIfNull(instance);

        lock (_lock)
        {
            if (_plugins.TryGetValue(pluginName, out var plugin))
            {
                _instanceToPlugin[instance] = pluginName;
                plugin.ActiveInstanceCount++;
            }
        }
    }

    /// <summary>
    /// Unregisters an instance, indicating it's no longer in use.
    /// </summary>
    /// <param name="instance">The instance to unregister.</param>
    public void UnregisterInstance(object instance)
    {
        if (instance == null) return;

        lock (_lock)
        {
            if (_instanceToPlugin.TryRemove(instance, out var pluginName))
            {
                if (_plugins.TryGetValue(pluginName, out var plugin))
                {
                    plugin.ActiveInstanceCount = Math.Max(0, plugin.ActiveInstanceCount - 1);
                }
            }
        }
    }

    /// <summary>
    /// Attempts to unload a plugin.
    /// </summary>
    /// <param name="pluginName">Name of the plugin to unload.</param>
    /// <param name="force">If true, unloads even if instances are active (not recommended).</param>
    /// <returns>True if the plugin was unloaded, false otherwise.</returns>
    public bool UnloadPlugin(string pluginName, bool force = false)
    {
        lock (_lock)
        {
            if (!_plugins.TryGetValue(pluginName, out var plugin) || !plugin.IsLoaded)
            {
                return false; // Already unloaded or doesn't exist
            }

            // Check if any instances are active
            if (plugin.ActiveInstanceCount > 0 && !force)
            {
                throw new PluginException(
                    $"Cannot unload plugin '{pluginName}': {plugin.ActiveInstanceCount} instance(s) still in use. " +
                    "Dispose all instances first, or use TryUnloadPlugin for soft unload.");
            }

            // Mark as unloaded before actual unload
            plugin.IsLoaded = false;

            // Attempt to unload the context
            try
            {
                plugin.LoadContext.Unload();
                return true;
            }
            catch (Exception ex)
            {
                // Rollback
                plugin.IsLoaded = true;
                throw new PluginException($"Failed to unload plugin '{pluginName}': {ex.Message}", ex);
            }
        }
    }

    /// <summary>
    /// Attempts to softly unload a plugin by requesting cleanup from active instances.
    /// </summary>
    /// <param name="pluginName">Name of the plugin to unload.</param>
    /// <param name="timeout">Maximum time to wait for instances to clean up.</param>
    /// <returns>True if all instances accepted unload and plugin was unloaded, false otherwise.</returns>
    public bool TryUnloadPlugin(string pluginName, TimeSpan? timeout = null)
    {
        timeout ??= TimeSpan.FromSeconds(5);

        lock (_lock)
        {
            if (!_plugins.TryGetValue(pluginName, out var plugin) || !plugin.IsLoaded)
            {
                return false;
            }

            // Find all active instances from this plugin
            var instances = _instanceToPlugin
                .Where(kvp => kvp.Value == pluginName)
                .Select(kvp => kvp.Key)
                .ToList();

            if (instances.Count == 0)
            {
                // No active instances, safe to unload
                return UnloadPlugin(pluginName, force: false);
            }

            // Request unload from all instances that support it
            var unloadableInstances = instances.OfType<IPluginUnloadable>().ToList();

            // Check if all instances are unloadable
            if (unloadableInstances.Count < instances.Count)
            {
                // Some instances don't implement IPluginUnloadable, cannot safely unload
                return false;
            }

            // Ask all unloadable instances if they can unload
            bool allAccepted = true;
            foreach (var instance in unloadableInstances)
            {
                try
                {
                    if (!instance.OnUnloadRequested())
                    {
                        allAccepted = false;
                        break;
                    }
                }
                catch
                {
                    allAccepted = false;
                    break;
                }
            }

            if (!allAccepted)
            {
                return false;
            }

            // All accepted, now check if they actually cleaned up
            // Give them time to complete cleanup
            var deadline = DateTimeOffset.UtcNow + timeout.Value;
            const int pollIntervalMs = 50;

            while (plugin.ActiveInstanceCount > 0 && DateTimeOffset.UtcNow < deadline)
            {
                Monitor.Exit(_lock);
                try
                {
                    Thread.Sleep(pollIntervalMs);
                }
                finally
                {
                    Monitor.Enter(_lock);
                }
            }

            if (plugin.ActiveInstanceCount > 0)
            {
                return false; // Timeout waiting for cleanup
            }

            // All instances cleaned up, safe to unload
            return UnloadPlugin(pluginName, force: false);
        }
    }

    /// <summary>
    /// Gets the number of active instances for a plugin.
    /// </summary>
    /// <param name="pluginName">Name of the plugin.</param>
    /// <returns>Number of active instances.</returns>
    public int GetActiveInstanceCount(string pluginName)
    {
        if (_plugins.TryGetValue(pluginName, out var plugin))
        {
            return plugin.ActiveInstanceCount;
        }
        return 0;
    }

    /// <summary>
    /// Attempts to discover and initialize the plugin entry point.
    /// </summary>
    /// <param name="assembly">The plugin assembly to search.</param>
    private void TryInitializePlugin(Assembly assembly)
    {
        try
        {
            // Look for class named "PluginInitializer" implementing IPluginInitializer
            var initializerType = assembly.GetTypes()
                .FirstOrDefault(t => t.Name == "PluginInitializer" &&
                                    typeof(IPluginInitializer).IsAssignableFrom(t) &&
                                    !t.IsAbstract &&
                                    !t.IsInterface);

            if (initializerType != null)
            {
                // Use the provided registry accessor
                var initializer = (IPluginInitializer?)Activator.CreateInstance(initializerType);
                initializer?.Initialize(_registryAccessor);
            }
        }
        catch (Exception ex)
        {
            // Log but don't fail plugin load
            Console.Error.WriteLine($"Plugin initialization warning: {ex.Message}");
        }
    }

    /// <summary>
    /// Proxy class that allows Core to access Execution registries via reflection.
    /// This avoids circular dependency between Core and Execution assemblies.
    /// </summary>
    private sealed class RegistryAccessorProxy : IRegistryAccessor
    {
        public void RegisterScheduler(string name, Func<object> factory)
        {
            // Get IScheduler type dynamically
            var iSchedulerType = Type.GetType("ImageAutomate.Execution.Scheduling.IScheduler, ImageAutomate.Execution");
            if (iSchedulerType == null)
            {
                Console.Error.WriteLine("Failed to locate IScheduler type.");
                return;
            }

            // Create Func<IScheduler> type dynamically
            var funcSchedulerType = typeof(Func<>).MakeGenericType(iSchedulerType);
            if (funcSchedulerType == null)
            {
                Console.Error.WriteLine("Failed to create Func<IScheduler> type.");
                return;
            }

            // Create a wrapper delegate that adapts Func<object> to Func<IScheduler>
            var typedFactory = factory;

            CallRegistryMethod(
                "ImageAutomate.Execution.Scheduling.SchedulerRegistry, ImageAutomate.Execution",
                "RegisterScheduler",
                [typeof(string), funcSchedulerType],
                [name, typedFactory]
            );
        }

        public void RegisterImageFormat(string formatName, IImageFormatStrategy strategy)
        {
            CallRegistryMethod(
                "ImageAutomate.Infrastructure.ImageFormatRegistry, ImageAutomate.Infrastructure",
                "RegisterFormat",
                [typeof(string), typeof(IImageFormatStrategy)],
                [formatName, strategy]
            );
        }

        private static void CallRegistryMethod(string fullRegistryTypeName, string methodName, Type[] parameterTypes, object[] parameters)
        {
            var registryType = Type.GetType(fullRegistryTypeName);
            if (registryType == null)
            {
                Console.Error.WriteLine($"Failed to locate {fullRegistryTypeName} type.");
                return;
            }

            var registryProperty = registryType.GetProperty("Instance",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            var registry = registryProperty?.GetValue(null);
            if (registry == null)
            {
                Console.Error.WriteLine($"Failed to access {registryType.Name}.Instance property.");
                return;
            }

            var registerMethod = registry.GetType().GetMethod(methodName, parameterTypes);

            if (registerMethod == null)
            {
                Console.Error.WriteLine($"Failed to find {methodName} method in {registryType.Name}.");
                return;
            }

            registerMethod.Invoke(registry, parameters);
        }
    }
}

/// <summary>
/// Represents the optional MANIFEST.json file for directory-based plugins.
/// </summary>
internal class PluginManifest
{
    public string? Name { get; set; }
    public string? EntryPoint { get; set; }
    public string? Version { get; set; }
    public string? Description { get; set; }
    public string? Author { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
}
