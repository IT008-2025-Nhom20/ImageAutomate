namespace ImageAutomate.Execution;

using System.Text.RegularExpressions;

/// <summary>
/// Compares strings using natural sort order (e.g., file1, file2, file10).
/// Numbers within strings are compared numerically rather than lexicographically.
/// </summary>
/// <remarks>
/// This comparer is used for the Natural sort option in LoadBlock.
/// Example: "file1.jpg", "file2.jpg", "file10.jpg" sorts correctly
/// instead of "file1.jpg", "file10.jpg", "file2.jpg" (lexicographic).
/// </remarks>
internal class NaturalStringComparer : IComparer<string>
{
    private static readonly Regex NumberRegex = new(@"(\d+)", RegexOptions.Compiled);

    public int Compare(string? x, string? y)
    {
        // TODO: Implement natural string comparison
        // For now, throw to indicate this feature is not yet implemented
        throw new NotImplementedException(
            "Natural sort order is not yet implemented. " +
            "Please use FileSortOrder.None or FileSortOrder.Lexicographic.");
    }
}
