namespace ImageAutomate.StandardBlocks;

/// <summary>
/// Specifies the sort order for file loading in LoadBlock.
/// </summary>
public enum FileSortOrder
{
    /// <summary>
    /// No sorting - order may vary between runs (filesystem dependent).
    /// This is the fastest option as it uses files as returned by the filesystem.
    /// </summary>
    None,

    /// <summary>
    /// Standard lexicographic string sort (e.g., file1, file10, file2).
    /// Uses ordinal string comparison.
    /// </summary>
    Lexicographic,

    // /// <summary>
    // /// Natural sort order (e.g., file1, file2, file10) - like Windows Explorer.
    // /// Compares numbers within filenames numerically.
    // /// </summary>
    // Natural
}
