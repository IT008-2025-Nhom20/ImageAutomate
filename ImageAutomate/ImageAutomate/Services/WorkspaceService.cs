using ImageAutomate.Data;
using ImageAutomate.Models;

namespace ImageAutomate.Services
{
    /// <summary>
    /// Service for managing workspace metadata and operations.
    /// </summary>
    public class WorkspaceService
    {
        private readonly IWorkspaceDataContext _dataContext;
        private static WorkspaceService? _instance;

        public static WorkspaceService Instance => _instance ??= new WorkspaceService(new CsvWorkspaceDataContext());

        private WorkspaceService(IWorkspaceDataContext dataContext)
        {
            _dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
        }

        /// <summary>
        /// Gets all workspaces, ordered by last opened (most recent first).
        /// </summary>
        public List<WorkspaceInfo> GetAllWorkspaces()
        {
            var workspaces = _dataContext.GetAll();

            // Remove entries where the file no longer exists
            var validWorkspaces = workspaces.Where(w => File.Exists(w.FilePath)).ToList();

            // If we removed any invalid entries, update the data store
            if (validWorkspaces.Count < workspaces.Count)
            {
                foreach (var invalid in workspaces.Except(validWorkspaces))
                {
                    _dataContext.Remove(invalid.FilePath);
                }
                _dataContext.SaveChanges();
            }

            return validWorkspaces.OrderByDescending(w => w.LastOpened).ToList();
        }

        /// <summary>
        /// Gets recent workspaces (limited to a specified count).
        /// </summary>
        public List<WorkspaceInfo> GetRecentWorkspaces(int count = 10)
        {
            return GetAllWorkspaces().Take(count).ToList();
        }

        /// <summary>
        /// Adds or updates a workspace entry.
        /// </summary>
        public void AddOrUpdateWorkspace(string filePath, string? name = null, string? description = null, string? thumbnailPath = null)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Workspace file not found.", filePath);
            }

            var existing = _dataContext.FindByPath(filePath);
            var fileInfo = new FileInfo(filePath);

            if (existing != null)
            {
                existing.Name = name ?? existing.Name;
                existing.LastModified = fileInfo.LastWriteTime;
                existing.LastOpened = DateTime.Now;
                if (description != null)
                    existing.Description = description;
                if (thumbnailPath != null)
                    existing.ThumbnailPath = thumbnailPath;

                _dataContext.Update(existing);
            }
            else
            {
                var workspace = new WorkspaceInfo
                {
                    Name = name ?? Path.GetFileNameWithoutExtension(filePath),
                    FilePath = filePath,
                    LastModified = fileInfo.LastWriteTime,
                    LastOpened = DateTime.Now,
                    Description = description,
                    ThumbnailPath = thumbnailPath
                };

                _dataContext.Add(workspace);
            }

            _dataContext.SaveChanges();
        }

        /// <summary>
        /// Updates the last opened time for a workspace.
        /// </summary>
        public void UpdateLastOpened(string filePath)
        {
            var existing = _dataContext.FindByPath(filePath);
            if (existing != null)
            {
                existing.LastOpened = DateTime.Now;

                // Also update file modified time if it changed
                if (File.Exists(filePath))
                {
                    var fileInfo = new FileInfo(filePath);
                    existing.LastModified = fileInfo.LastWriteTime;
                }

                _dataContext.Update(existing);
                _dataContext.SaveChanges();
            }
        }

        /// <summary>
        /// Removes a workspace from the list.
        /// </summary>
        public void RemoveWorkspace(string filePath)
        {
            _dataContext.Remove(filePath);
            _dataContext.SaveChanges();
        }

        /// <summary>
        /// Searches workspaces by name or description.
        /// </summary>
        public List<WorkspaceInfo> SearchWorkspaces(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return GetAllWorkspaces();
            }

            var lowerQuery = query.ToLowerInvariant();
            return GetAllWorkspaces()
                .Where(w =>
                    w.Name.ToLowerInvariant().Contains(lowerQuery) ||
                    (w.Description?.ToLowerInvariant().Contains(lowerQuery) ?? false))
                .ToList();
        }
    }
}
