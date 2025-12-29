namespace ImageAutomate.Models
{
    /// <summary>
    /// Represents metadata about a saved workspace.
    /// </summary>
    public class WorkspaceInfo
    {
        /// <summary>
        /// Name of the workspace.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Full file path to the .imageautomate file.
        /// </summary>
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// When the workspace file was last modified.
        /// </summary>
        public DateTime LastModified { get; set; }

        /// <summary>
        /// When the workspace was last opened in the application.
        /// </summary>
        public DateTime LastOpened { get; set; }

        /// <summary>
        /// Optional path to a thumbnail image.
        /// </summary>
        public string? ThumbnailPath { get; set; }

        /// <summary>
        /// Optional description or notes about the workspace.
        /// </summary>
        public string? Description { get; set; }

        public WorkspaceInfo()
        {
        }

        public WorkspaceInfo(string name, string filePath)
        {
            Name = name;
            FilePath = filePath;
            LastModified = DateTime.Now;
            LastOpened = DateTime.Now;
        }
    }
}
