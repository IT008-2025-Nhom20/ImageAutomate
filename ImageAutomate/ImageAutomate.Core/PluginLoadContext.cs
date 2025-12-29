using System.Reflection;
using System.Runtime.Loader;

namespace ImageAutomate.Core;

/// <summary>
/// Custom AssemblyLoadContext for loading plugins in a collectible manner.
/// This allows plugins to be unloaded from memory.
/// </summary>
internal class PluginLoadContext : AssemblyLoadContext
{
    private readonly AssemblyDependencyResolver _resolver;

    public PluginLoadContext(string pluginPath) : base(isCollectible: true)
    {
        _resolver = new AssemblyDependencyResolver(pluginPath);
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        // For shared dependencies (ImageAutomate.Core), we must use the default context's version
        // so that types (like IBlock) are identical across plugin and host application.
        if (assemblyName.Name == "ImageAutomate.Core")
        {
            return null; // Fallback to default load context
        }

        // Also share common dependencies to avoid version conflicts
        if (assemblyName.Name?.StartsWith("System.", StringComparison.Ordinal) == true ||
            assemblyName.Name?.StartsWith("Microsoft.", StringComparison.Ordinal) == true ||
            assemblyName.Name == "netstandard" ||
            assemblyName.Name == "SixLabors.ImageSharp")
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
