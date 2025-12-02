using System.Reflection;
using System.Runtime.Loader;
using ImageAutomate.Core;

namespace Scratch;

// Context to load plugins, allowing unloading
class PluginLoadContext : AssemblyLoadContext
{
    private AssemblyDependencyResolver _resolver;

    public PluginLoadContext(string pluginPath) : base(isCollectible: true)
    {
        _resolver = new AssemblyDependencyResolver(pluginPath);
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        // For shared dependencies (ImageAutomate.Core), we must use the default context's version
        // so that types (like IBlock) are identical.
        if (assemblyName.Name == "ImageAutomate.Core")
        {
            return null; // Fallback to default load context
        }

        string? assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
        if (assemblyPath != null)
        {
            return LoadFromAssemblyPath(assemblyPath);
        }

        return null;
    }

    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
    {
        string? libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
        if (libraryPath != null)
        {
            return LoadUnmanagedDllFromPath(libraryPath);
        }

        return IntPtr.Zero;
    }
}

class Program
{
    // Registry of loaded plugins: Plugin Name -> Context
    static Dictionary<string, PluginLoadContext> _loadedPlugins = new();

    // Registry of constructed instances: Instance Name -> (IBlock Instance, Type Name)
    static Dictionary<string, IBlock> _instances = new();

    // Store discovered plugin paths: Name -> FullPath
    static Dictionary<string, string> _discoveredPaths = new();
    static List<string> _discoveredPlugins = new(); // Keep order

    static void Main(string[] args)
    {
        Console.WriteLine("ImageAutomate Extension Test CLI");
        Console.WriteLine("Type 'EXIT' or 'DONE' to quit.");

        while (true)
        {
            Console.Write(">> ");
            string? line = Console.ReadLine();
            if (line == null) return; // End of stream
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] parts = line.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            string command = parts[0].ToUpperInvariant();
            string argument = parts.Length > 1 ? parts[1] : "";

            try
            {
                switch (command)
                {
                    case "EXIT":
                    case "DONE":
                        return;

                    case "DISCOVER":
                        HandleDiscover(argument);
                        break;

                    case "LIST":
                        HandleList(argument);
                        break;

                    case "LOAD":
                        HandleLoad(argument);
                        break;

                    case "UNLOAD":
                        HandleUnload(argument);
                        break;

                    case "CONSTRUCT":
                        HandleConstruct(argument);
                        break;

                    case "TITLE":
                        HandleTitle(argument);
                        break;

                    case "CONTENT":
                        HandleContent(argument);
                        break;

                    case "EXEC":
                        HandleExec(argument);
                        break;

                    case "DISPOSE":
                        HandleDispose(argument);
                        break;

                    default:
                        Console.WriteLine($"Unknown command: {command}");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }

    static void HandleDiscover(string path)
    {
        _discoveredPaths.Clear();
        _discoveredPlugins.Clear();

        if (string.IsNullOrWhiteSpace(path))
        {
            path = ".";
        }

        if (!Directory.Exists(path))
        {
            Console.WriteLine($"Error: Directory '{path}' not found.");
            return;
        }

        var dlls = Directory.GetFiles(path, "*.dll", SearchOption.AllDirectories);

        foreach (var dll in dlls)
        {
            var fileInfo = new FileInfo(dll);
            string fileName = Path.GetFileNameWithoutExtension(dll);
            string? parentDir = fileInfo.Directory?.Name;

            bool isValid = false;

            if (string.Equals(parentDir, fileName, StringComparison.OrdinalIgnoreCase))
            {
                isValid = true;
            }
            else if (string.Equals(parentDir, "plugins", StringComparison.OrdinalIgnoreCase))
            {
                isValid = true;
            }

            if (isValid)
            {
                if (!_discoveredPaths.ContainsKey(fileName))
                {
                    _discoveredPaths[fileName] = Path.GetFullPath(dll);
                    _discoveredPlugins.Add(fileName);
                }
            }
        }

        if (_discoveredPlugins.Count > 0)
        {
             Console.WriteLine($"Discovered {_discoveredPlugins.Count} valid plugins.");
        }
    }

    static void HandleList(string arg)
    {
        if (arg.ToUpperInvariant().Contains("INSTANCES"))
        {
            // List instances
            foreach (var kvp in _instances)
            {
                Console.WriteLine($"{kvp.Key}:{kvp.Value.GetType().Name}");
            }
        }
        else
        {
            Console.WriteLine("PLUGINS | LOADED");
            foreach (var plugin in _discoveredPlugins)
            {
                string loaded = _loadedPlugins.ContainsKey(plugin) ? "Yes" : "No";
                Console.WriteLine($"{plugin} | {loaded}");
            }
        }
    }

    static void HandleLoad(string pluginName)
    {
        if (pluginName.EndsWith(".dll"))
        {
            pluginName = Path.GetFileNameWithoutExtension(pluginName);
        }

        if (_loadedPlugins.ContainsKey(pluginName))
        {
            Console.WriteLine($"Loaded: {pluginName}"); // Already loaded
            return;
        }

        if (!_discoveredPaths.ContainsKey(pluginName))
        {
            Console.WriteLine($"Error: Can not find {pluginName}");
            return;
        }

        string path = _discoveredPaths[pluginName];
        try
        {
            var loadContext = new PluginLoadContext(path);
            var assembly = loadContext.LoadFromAssemblyPath(path);
            _loadedPlugins[pluginName] = loadContext;
            Console.WriteLine($"Loaded: {pluginName}");
        }
        catch (Exception ex)
        {
             Console.WriteLine($"Error: Failed to load {pluginName}. {ex.Message}");
        }
    }

    static void HandleUnload(string pluginName)
    {
         if (pluginName.EndsWith(".dll"))
        {
            pluginName = Path.GetFileNameWithoutExtension(pluginName);
        }

        if (_loadedPlugins.TryGetValue(pluginName, out var context))
        {
            context.Unload();
            _loadedPlugins.Remove(pluginName);
            Console.WriteLine($"Unloaded: {pluginName}");
        }
        else
        {
            Console.WriteLine($"Unloaded: {pluginName}");
        }
    }

    static void HandleConstruct(string arg)
    {
        // Format: name:type
        var parts = arg.Split(':');
        if (parts.Length != 2)
        {
            Console.WriteLine("Error: Invalid format. Use name:type");
            return;
        }

        string instanceName = parts[0].Trim();
        string typeName = parts[1].Trim();

        // Search for type in loaded plugins
        Type? foundType = null;
        bool typeExistsButNotBlock = false;

        foreach (var context in _loadedPlugins.Values)
        {
            foreach (var assembly in context.Assemblies)
            {
                var type = assembly.GetTypes().FirstOrDefault(t => t.Name == typeName);
                if (type != null)
                {
                    if (typeof(IBlock).IsAssignableFrom(type))
                    {
                        foundType = type;
                        break;
                    }
                    else
                    {
                        typeExistsButNotBlock = true;
                    }
                }
            }
            if (foundType != null) break;
        }

        if (foundType == null)
        {
             if (typeExistsButNotBlock)
             {
                 Console.WriteLine($"Error: Can not construct instance '{instanceName}' of type '{typeName}'. '{typeName}' is not a valid block.");
             }
             else
             {
                 Console.WriteLine($"Error: Can not construct instance '{instanceName}' of type '{typeName}'. '{typeName}' is not a valid type");
             }
             return;
        }

        try
        {
            var instance = Activator.CreateInstance(foundType) as IBlock;
            if (instance != null)
            {
                _instances[instanceName] = instance;
            }
            else
            {
                 Console.WriteLine($"Error: Failed to instantiate {typeName}");
            }
        }
        catch (Exception ex)
        {
             Console.WriteLine($"Error: {ex.Message}");
        }
    }

    static void HandleTitle(string arg)
    {
        string[] parts = arg.Split(' ', 2);
        string name = parts[0];

        if (!_instances.TryGetValue(name, out var instance))
        {
            Console.WriteLine($"Error: Instance '{name}' does not exist");
            return;
        }

        if (parts.Length == 1)
        {
            Console.WriteLine(instance.Title);
        }
        else
        {
            string newTitle = parts[1].Trim();
            if (newTitle.StartsWith("\"") && newTitle.EndsWith("\""))
            {
                newTitle = newTitle.Substring(1, newTitle.Length - 2);
            }
            instance.Title = newTitle;
        }
    }

    static void HandleContent(string arg)
    {
        string[] parts = arg.Split(' ', 2);
        string name = parts[0];

        if (!_instances.TryGetValue(name, out var instance))
        {
            Console.WriteLine($"Error: Instance '{name}' does not exist");
            return;
        }

        if (parts.Length == 1)
        {
            Console.WriteLine(instance.Content);
        }
        else
        {
            string newContent = parts[1].Trim();
            if (newContent.StartsWith("\"") && newContent.EndsWith("\""))
            {
                newContent = newContent.Substring(1, newContent.Length - 2);
            }
            instance.Content = newContent;
        }
    }

    static void HandleExec(string name)
    {
        if (!_instances.TryGetValue(name, out var instance))
        {
            Console.WriteLine($"Error: Instance '{name}' does not exist");
            return;
        }

        instance.Execute(new Dictionary<Socket, IReadOnlyList<IBasicWorkItem>>());
    }

    static void HandleDispose(string name)
    {
        if (_instances.TryGetValue(name, out var instance))
        {
            instance.Dispose();
            _instances.Remove(name);
        }
    }
}
