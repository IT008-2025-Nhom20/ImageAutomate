using System.Collections.Generic;
using ImageAutomate.Models;

namespace ImageAutomate.Data
{
    /// <summary>
    /// Interface for workspace metadata persistence.
    /// Provides CRUD operations for workspace information.
    /// </summary>
    public interface IWorkspaceDataContext
    {
        /// <summary>
        /// Retrieves all workspace metadata entries.
        /// </summary>
        List<WorkspaceInfo> GetAll();

        /// <summary>
        /// Adds a new workspace entry.
        /// </summary>
        void Add(WorkspaceInfo workspace);

        /// <summary>
        /// Updates an existing workspace entry.
        /// </summary>
        void Update(WorkspaceInfo workspace);

        /// <summary>
        /// Removes a workspace entry by file path.
        /// </summary>
        void Remove(string filePath);

        /// <summary>
        /// Finds a workspace by file path.
        /// </summary>
        WorkspaceInfo? FindByPath(string filePath);

        /// <summary>
        /// Persists all changes to storage.
        /// </summary>
        void SaveChanges();
    }
}
