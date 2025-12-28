namespace ImageAutomate.Core;

/// <summary>
/// System-level configuration constants.
/// </summary>
public static class SystemConfiguration
{
    /// <summary>
    /// Gets or sets the URL to the workspace schema JSON file.
    /// Used for workspace serialization and validation.
    /// </summary>
    public static string WorkspaceSchemaUrl { get; set; } =
        "https://raw.githubusercontent.com/IT007-2025-Nhom20/ImageAutomate/project-restructure/docs/workspace-schema.json";
}
