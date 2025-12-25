namespace ImageAutomate.Core;

/// <summary>
/// Central registry for image format strategies.
/// Replaces enum-based format handling with extensible registration.
/// </summary>
public interface IImageFormatRegistry
{
    /// <summary>
    /// Registers a format strategy.
    /// </summary>
    /// <param name="formatName">Unique format name (case-insensitive).</param>
    /// <param name="strategy">Format strategy instance.</param>
    void RegisterFormat(string formatName, IImageFormatStrategy strategy);

    /// <summary>
    /// Unregisters a format.
    /// </summary>
    /// <param name="formatName">Format name to unregister.</param>
    /// <returns>True if unregistered, false if not found.</returns>
    bool UnregisterFormat(string formatName);

    /// <summary>
    /// Gets a format strategy by name.
    /// </summary>
    /// <param name="formatName">Format name to look up.</param>
    /// <returns>Format strategy, or null if not found.</returns>
    IImageFormatStrategy? GetFormat(string formatName);

    /// <summary>
    /// Gets all registered format names.
    /// </summary>
    /// <returns>List of registered format names.</returns>
    IReadOnlyList<string> GetRegisteredFormats();

    /// <summary>
    /// Checks if a format is registered.
    /// </summary>
    /// <param name="formatName">Format name to check.</param>
    /// <returns>True if registered, false otherwise.</returns>
    bool IsFormatRegistered(string formatName);
}
