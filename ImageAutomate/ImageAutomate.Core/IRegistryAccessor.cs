namespace ImageAutomate.Core;

/// <summary>
/// Abstraction layer allowing Core to expose registries without referencing Execution assembly.
/// This avoids circular dependency between Core and Execution.
/// </summary>
public interface IRegistryAccessor
{
    /// <summary>
    /// Registers a custom scheduler factory.
    /// </summary>
    /// <param name="name">Unique name for the scheduler.</param>
    /// <param name="factory">Factory function to create scheduler instances.</param>
    void RegisterScheduler(string name, Func<object> factory);

    /// <summary>
    /// Registers a custom image format strategy.
    /// </summary>
    /// <param name="formatName">Unique format name (case-insensitive).</param>
    /// <param name="strategy">Format strategy instance.</param>
    void RegisterImageFormat(string formatName, IImageFormatStrategy strategy);
}
