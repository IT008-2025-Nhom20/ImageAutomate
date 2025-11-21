using System.ComponentModel;

namespace ConvertBlockPoC;

/// <summary>
/// Unified execution interface for image manipulation blocks.
/// </summary>
public interface IBlock : INotifyPropertyChanged
{
    /// <summary>
    /// Display name of the block.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Description or summary of the block's configuration.
    /// </summary>
    string ConfigurationSummary { get; }
}
