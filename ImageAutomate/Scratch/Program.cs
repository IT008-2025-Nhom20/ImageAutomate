using ImageAutomate.Core;

namespace Scratch;

class Program
{
    // Use the new PluginLoader from ImageAutomate.Core
    static PluginLoader _pluginLoader = new();

    // Registry of constructed instances: Instance Name -> IBlock Instance
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
                var loadedPlugin = _pluginLoader.GetPlugin(plugin);
                string loaded = loadedPlugin != null ? "Yes" : "No";
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

        if (_pluginLoader.GetPlugin(pluginName) != null)
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
            var pluginInfo = _pluginLoader.LoadPlugin(path, pluginName);
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

        try
        {
            bool unloaded = _pluginLoader.UnloadPlugin(pluginName);
            Console.WriteLine($"Unloaded: {pluginName}");
        }
        catch (PluginException ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
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
        string? foundPluginName = null;
        bool typeExistsButNotBlock = false;

        foreach (var plugin in _pluginLoader.LoadedPlugins)
        {
            var types = plugin.GetExportedTypes();
            var type = types.FirstOrDefault(t => t.Name == typeName);
            if (type != null)
            {
                if (typeof(IBlock).IsAssignableFrom(type))
                {
                    foundType = type;
                    foundPluginName = plugin.Name;
                    break;
                }
                else
                {
                    typeExistsButNotBlock = true;
                }
            }
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
                // Register the instance with the plugin loader for tracking
                if (foundPluginName != null)
                {
                    _pluginLoader.RegisterInstance(instance, foundPluginName);
                }
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
            _pluginLoader.UnregisterInstance(instance);
            _instances.Remove(name);
        }
    }
}
