using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using ImageAutomate.Core;
using ImageAutomate.Dialogs;
using ImageAutomate.Models;
using ImageAutomate.Services;
using ImageAutomate.UI;

namespace ImageAutomate.Views.DashboardViews
{
    public partial class WorkspaceView : UserControl
    {
        private readonly WorkspaceService _workspaceService = WorkspaceService.Instance;
        private string? _lastSearchQuery;

        // Event to request the editor.
        public event EventHandler<string?>? OpenEditorRequested;

        public WorkspaceView()
        {
            InitializeComponent();

            // Wire up events
            SearchTextBox.TextChanged += TextBoxSearch_TextChanged;
            NewButton.Click += BtnNew_Click;
            BrowseButton.Click += BtnBrowse_Click;
            ImportButton.Click += BtnImport_Click;

            // Initial load
            LoadWorkspaces();
        }

        private void LoadWorkspaces(string? searchQuery = null)
        {
            WorkspacesPanel.SuspendLayout();
            WorkspacesPanel.Controls.Clear();

            List<WorkspaceInfo> workspaces;
            if (string.IsNullOrWhiteSpace(searchQuery))
            {
                workspaces = _workspaceService.GetAllWorkspaces();
            }
            else
            {
                workspaces = _workspaceService.SearchWorkspaces(searchQuery);
            }

            if (workspaces.Count == 0)
            {
                EmptyLabel.Visible = true;
                EmptyLabel.Text = string.IsNullOrWhiteSpace(searchQuery)
                    ? "No workspaces found. Create a new one!"
                    : "No workspaces match your search.";
            }
            else
            {
                EmptyLabel.Visible = false;

                foreach (var workspaceInfo in workspaces)
                {
                    var card = CreateWorkspaceCard(workspaceInfo);
                    WorkspacesPanel.Controls.Add(card);
                }
            }

            WorkspacesPanel.ResumeLayout();
        }

        private ImageCard CreateWorkspaceCard(WorkspaceInfo workspaceInfo)
        {
            var card = new ImageCard
            {
                Title = workspaceInfo.Name,
                LastModified = workspaceInfo.LastModified,
                CardImage = LoadThumbnail(workspaceInfo.ThumbnailPath),
                Tag = workspaceInfo,
                Margin = new Padding(5)
            };

            card.Click += (s, e) => OpenWorkspace(workspaceInfo);

            // Add context menu for additional actions
            var contextMenu = new ContextMenuStrip();

            var openItem = new ToolStripMenuItem("Open");
            openItem.Click += (s, e) => OpenWorkspace(workspaceInfo);
            contextMenu.Items.Add(openItem);

            contextMenu.Items.Add(new ToolStripSeparator());

            var editItem = new ToolStripMenuItem("Edit...");
            editItem.Click += (s, e) => EditWorkspaceEntry(workspaceInfo);
            contextMenu.Items.Add(editItem);

            var removeItem = new ToolStripMenuItem("Remove from List");
            removeItem.Click += (s, e) => RemoveWorkspaceFromList(workspaceInfo);
            contextMenu.Items.Add(removeItem);

            contextMenu.Items.Add(new ToolStripSeparator());

            var showInExplorerItem = new ToolStripMenuItem("Show in File Explorer");
            showInExplorerItem.Click += (s, e) => ShowInFileExplorer(workspaceInfo.FilePath);
            contextMenu.Items.Add(showInExplorerItem);

            card.ContextMenuStrip = contextMenu;

            return card;
        }

        private Image? LoadThumbnail(string? thumbnailPath)
        {
            if (string.IsNullOrEmpty(thumbnailPath) || !File.Exists(thumbnailPath))
            {
                // Return a default placeholder image
                var placeholder = new Bitmap(200, 200);
                using (var g = Graphics.FromImage(placeholder))
                {
                    g.Clear(Color.LightGray);
                    using (var font = new Font("Segoe UI", 14, FontStyle.Bold))
                    {
                        var text = "No Preview";
                        var size = g.MeasureString(text, font);
                        g.DrawString(text, font, Brushes.DarkGray,
                            (placeholder.Width - size.Width) / 2,
                            (placeholder.Height - size.Height) / 2);
                    }
                }
                return placeholder;
            }

            try
            {
                return Image.FromFile(thumbnailPath);
            }
            catch
            {
                return null;
            }
        }

        private void OpenWorkspace(WorkspaceInfo workspaceInfo)
        {
            try
            {
                if (!File.Exists(workspaceInfo.FilePath))
                {
                    MessageBox.Show(
                        $"Workspace file not found:\n{workspaceInfo.FilePath}",
                        "File Not Found",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    // Remove from list
                    _workspaceService.RemoveWorkspace(workspaceInfo.FilePath);
                    LoadWorkspaces(_lastSearchQuery);
                    return;
                }

                var workspace = Workspace.LoadFromFile(workspaceInfo.FilePath);

                // Update last opened time
                _workspaceService.UpdateLastOpened(workspaceInfo.FilePath);

                // Raise event to notify parent (DashboardView) to switch to EditorView
                // We pass the file path instead of the loaded object, letting the Editor load it fresh.
                OpenEditorRequested?.Invoke(this, workspaceInfo.FilePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to open workspace:\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void RemoveWorkspaceFromList(WorkspaceInfo workspaceInfo)
        {
            var result = MessageBox.Show(
                $"Remove '{workspaceInfo.Name}' from the list?\n\nThe workspace file will not be deleted.",
                "Remove Workspace",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                _workspaceService.RemoveWorkspace(workspaceInfo.FilePath);
                LoadWorkspaces(_lastSearchQuery);
            }
        }

        private void ShowInFileExplorer(string filePath)
        {
            if (File.Exists(filePath))
            {
                Process.Start("explorer.exe", $"/select,\"{filePath}\"");
            }
        }

        private void EditWorkspaceEntry(WorkspaceInfo workspaceInfo)
        {
            using var dialog = new WorkspaceSaveDialog(
                workspaceInfo.Name,
                workspaceInfo.FilePath,
                workspaceInfo.ThumbnailPath,
                isEditMode: true
            );

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // Load the workspace JSON, update the name, and save it back
                    if (File.Exists(workspaceInfo.FilePath))
                    {
                        var workspace = Workspace.LoadFromFile(workspaceInfo.FilePath);
                        workspace.Name = dialog.WorkspaceName;
                        workspace.SaveToFile(workspaceInfo.FilePath);
                    }

                    // Update the CSV entry with new name and image path
                    _workspaceService.AddOrUpdateWorkspace(
                        workspaceInfo.FilePath,
                        dialog.WorkspaceName,
                        workspaceInfo.Description,
                        dialog.ImagePath
                    );

                    // Refresh the list
                    LoadWorkspaces(_lastSearchQuery);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Failed to update workspace:\n{ex.Message}",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

        private void TextBoxSearch_TextChanged(object? sender, EventArgs e)
        {
            _lastSearchQuery = SearchTextBox.Text;
            LoadWorkspaces(_lastSearchQuery);
        }

        private void BtnNew_Click(object? sender, EventArgs e)
        {
            // Create a new blank workspace
            var workspace = new Workspace(new PipelineGraph())
            {
                Name = "Untitled Workspace"
            };

            // Show save dialog
            using SaveFileDialog dialog = new()
            {
                Filter = "ImageAutomate Workspace (*.imageautomate)|*.imageautomate|JSON File (*.json)|*.json|All Files (*.*)|*.*",
                DefaultExt = "imageautomate",
                Title = "Save New Workspace"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    workspace.Name = Path.GetFileNameWithoutExtension(dialog.FileName);
                    workspace.SaveToFile(dialog.FileName);

                    // Add to recent workspaces
                    _workspaceService.AddOrUpdateWorkspace(dialog.FileName, workspace.Name);

                    // Reload list
                    LoadWorkspaces(_lastSearchQuery);

                    // Open the new workspace
                    OpenEditorRequested?.Invoke(this, dialog.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Failed to create workspace:\n{ex.Message}",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

        private void BtnBrowse_Click(object? sender, EventArgs e)
        {
            using OpenFileDialog dialog = new()
            {
                Filter = "ImageAutomate Workspace (*.imageautomate;*.json)|*.imageautomate;*.json|All Files (*.*)|*.*",
                Title = "Open Workspace"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var workspace = Workspace.LoadFromFile(dialog.FileName);

                    // Add to recent workspaces
                    _workspaceService.AddOrUpdateWorkspace(dialog.FileName, workspace.Name);

                    // Reload list
                    LoadWorkspaces(_lastSearchQuery);

                    // Open the workspace
                    OpenEditorRequested?.Invoke(this, dialog.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Failed to open workspace:\n{ex.Message}",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

        private void BtnImport_Click(object? sender, EventArgs e)
        {
            using OpenFileDialog dialog = new()
            {
                Filter = "ImageAutomate Workspace (*.imageautomate;*.json)|*.imageautomate;*.json|All Files (*.*)|*.*",
                Title = "Import Workspaces",
                Multiselect = true
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                int importedCount = 0;
                int failedCount = 0;

                foreach (var filePath in dialog.FileNames)
                {
                    try
                    {
                        var workspace = Workspace.LoadFromFile(filePath);

                        // Add to workspaces list (without opening)
                        _workspaceService.AddOrUpdateWorkspace(filePath, workspace.Name);
                        importedCount++;
                    }
                    catch (Exception)
                    {
                        failedCount++;
                    }
                }

                // Reload list
                LoadWorkspaces(_lastSearchQuery);

                // Show summary
                if (failedCount == 0)
                {
                    MessageBox.Show(
                        $"Successfully imported {importedCount} workspace(s).",
                        "Import Complete",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show(
                        $"Imported {importedCount} workspace(s).\n{failedCount} file(s) could not be imported.",
                        "Import Complete",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
            }
        }

        /// <summary>
        /// Refreshes the workspace list. Call this when returning from EditorView.
        /// </summary>
        public void RefreshWorkspaces()
        {
            LoadWorkspaces(_lastSearchQuery);
        }
    }
}
