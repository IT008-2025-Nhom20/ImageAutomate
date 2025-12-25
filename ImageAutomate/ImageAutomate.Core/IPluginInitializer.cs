namespace ImageAutomate.Core;

/// <summary>
/// Interface for plugin initialization entry points.
/// Plugin authors should implement this interface in a class named "PluginInitializer"
/// to enable automatic discovery and initialization by PluginLoader.
/// </summary>
public interface IPluginInitializer
{
    /// <summary>
    /// Initializes the plugin. Called by PluginLoader after the plugin assembly is loaded.
    /// </summary>
    /// <param name="registryAccessor">Optional accessor to system registries (e.g., scheduler registry).</param>
    void Initialize(IRegistryAccessor? registryAccessor);
}
