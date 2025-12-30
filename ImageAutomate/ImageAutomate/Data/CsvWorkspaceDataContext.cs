using System.Globalization;

using ImageAutomate.Models;

using nietras.SeparatedValues;

namespace ImageAutomate.Data
{
    /// <summary>
    /// CSV-based implementation of workspace data storage using SEP library.
    /// Stores workspace metadata in %APPDATA%/ImageAutomate/workspaces.csv
    /// </summary>
    internal sealed class CsvWorkspaceDataContext : IWorkspaceDataContext, IDisposable
    {
        private readonly string _csvFilePath;
        private readonly Lock _lock = new();
        private List<WorkspaceInfo>? _workspaces;
        private bool _disposed;

        /// <summary>
        /// Gets the default CSV file path in %APPDATA%/ImageAutomate/workspaces.csv
        /// </summary>
        public static string DefaultCsvPath
        {
            get
            {
                var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var appFolder = Path.Combine(appDataPath, "ImageAutomate");
                return Path.Combine(appFolder, "workspaces.csv");
            }
        }

        public CsvWorkspaceDataContext() : this(DefaultCsvPath)
        {
        }

        public CsvWorkspaceDataContext(string csvFilePath)
        {
            _csvFilePath = csvFilePath;
        }

        /// <summary>
        /// Ensures the workspaces are loaded from disk (lazy loading).
        /// </summary>
        private void EnsureLoaded()
        {
            if (_workspaces != null)
                return;

            lock (_lock)
            {
                // Double-check after acquiring lock
                _workspaces ??= new List<WorkspaceInfo>();

                var directory = Path.GetDirectoryName(_csvFilePath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                LoadFromFile();
            }
        }

        public List<WorkspaceInfo> GetAll()
        {
            EnsureLoaded();
            lock (_lock)
            {
                return new List<WorkspaceInfo>(_workspaces!);
            }
        }

        public void Add(WorkspaceInfo workspace)
        {
            EnsureLoaded();
            lock (_lock)
            {
                // Remove existing entry with same path
                _workspaces!.RemoveAll(w => w.FilePath.Equals(workspace.FilePath, StringComparison.OrdinalIgnoreCase));
                _workspaces.Add(workspace);
            }
        }

        public void Update(WorkspaceInfo workspace)
        {
            EnsureLoaded();
            lock (_lock)
            {
                var existing = _workspaces!.FirstOrDefault(w => w.FilePath.Equals(workspace.FilePath, StringComparison.OrdinalIgnoreCase));
                if (existing != null)
                {
                    _workspaces!.Remove(existing);
                }
                _workspaces!.Add(workspace);
            }
        }

        public void Remove(string filePath)
        {
            EnsureLoaded();
            lock (_lock)
            {
                _workspaces!.RemoveAll(w => w.FilePath.Equals(filePath, StringComparison.OrdinalIgnoreCase));
            }
        }

        public WorkspaceInfo? FindByPath(string filePath)
        {
            EnsureLoaded();
            lock (_lock)
            {
                return _workspaces!.FirstOrDefault(w => w.FilePath.Equals(filePath, StringComparison.OrdinalIgnoreCase));
            }
        }

        public void SaveChanges()
        {
            EnsureLoaded();
            lock (_lock)
            {
                SaveToFile();
            }
        }

        private void LoadFromFile()
        {
            if (!File.Exists(_csvFilePath))
            {
                return;
            }

            try
            {
                using var reader = Sep.Reader().FromFile(_csvFilePath);
                foreach (var row in reader)
                {
                    var workspace = new WorkspaceInfo
                    {
                        Name = row["Name"].ToString(),
                        FilePath = row["FilePath"].ToString(),
                        LastModified = DateTime.Parse(row["LastModified"].ToString(), CultureInfo.InvariantCulture),
                        LastOpened = DateTime.Parse(row["LastOpened"].ToString(), CultureInfo.InvariantCulture),
                        ThumbnailPath = row["ThumbnailPath"].ToString(),
                        Description = row["Description"].ToString()
                    };

                    // Convert empty strings to null for optional fields
                    if (string.IsNullOrWhiteSpace(workspace.ThumbnailPath))
                        workspace.ThumbnailPath = null;
                    if (string.IsNullOrWhiteSpace(workspace.Description))
                        workspace.Description = null;

                    _workspaces!.Add(workspace);
                }
            }
            catch (Exception ex)
            {
                // If CSV is corrupted, start fresh
                System.Diagnostics.Debug.WriteLine($"Failed to load workspaces CSV: {ex.Message}");
                _workspaces!.Clear();
            }
        }

        private void SaveToFile()
        {
            try
            {
                var directory = Path.GetDirectoryName(_csvFilePath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                using var writer = Sep.Writer().ToFile(_csvFilePath);

                foreach (var workspace in _workspaces!)
                {
                    using var row = writer.NewRow();

                    row["Name"].Set(workspace.Name);
                    row["FilePath"].Set(workspace.FilePath);
                    row["LastModified"].Set(workspace.LastModified.ToString("o", CultureInfo.InvariantCulture));
                    row["LastOpened"].Set(workspace.LastOpened.ToString("o", CultureInfo.InvariantCulture));
                    row["ThumbnailPath"].Set(workspace.ThumbnailPath ?? string.Empty);
                    row["Description"].Set(workspace.Description ?? string.Empty);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save workspaces CSV: {ex.Message}");
                throw;
            }
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            // No unmanaged resources to dispose
        }
    }
}
