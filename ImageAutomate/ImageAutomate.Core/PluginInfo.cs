using System.Reflection;

namespace ImageAutomate.Core;

/// <summary>
/// Represents metadata and state information for a loaded plugin.
/// </summary>
public class PluginInfo
{
    /// <summary>
    /// Gets the unique name of the plugin.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the full path to the plugin's main assembly file.
    /// </summary>
    public string AssemblyPath { get; }

    /// <summary>
    /// Gets the assembly of the plugin.
    /// </summary>
    public Assembly Assembly { get; }

    /// <summary>
    /// Gets the load context for this plugin.
    /// </summary>
    internal PluginLoadContext LoadContext { get; }

    /// <summary>
    /// Gets the number of instances created from this plugin that are still in use.
    /// </summary>
    public int ActiveInstanceCount { get; internal set; }

    /// <summary>
    /// Gets the timestamp when the plugin was loaded.
    /// </summary>
    public DateTimeOffset LoadedAt { get; }

    /// <summary>
    /// Gets whether the plugin is currently loaded.
    /// </summary>
    public bool IsLoaded { get; internal set; }

    internal PluginInfo(string name, string assemblyPath, Assembly assembly, PluginLoadContext loadContext)
    {
        Name = name;
        AssemblyPath = assemblyPath;
        Assembly = assembly;
        LoadContext = loadContext;
        ActiveInstanceCount = 0;
        LoadedAt = DateTimeOffset.UtcNow;
        IsLoaded = true;
    }

    /// <summary>
    /// Gets all types exported by this plugin.
    /// </summary>
    public IEnumerable<Type> GetExportedTypes()
    {
        try
        {
            return Assembly.GetExportedTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            // Return only the types that were successfully loaded
            return ex.Types.Where(t => t != null).Cast<Type>();
        }
    }

    /// <summary>
    /// Gets all types in this plugin that implement IBlock.
    /// </summary>
    public IEnumerable<Type> GetBlockTypes()
    {
        return GetExportedTypes().Where(t => typeof(IBlock).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface);
    }
}
