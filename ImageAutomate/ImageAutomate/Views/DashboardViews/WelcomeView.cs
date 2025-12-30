using System.Data;
using System.Diagnostics;

using ImageAutomate.Core;
using ImageAutomate.Dialogs;
using ImageAutomate.Models;
using ImageAutomate.Services;

namespace ImageAutomate.Views.DashboardViews
{
    public partial class WelcomeView : UserControl
    {
        private readonly WorkspaceService _workspaceService = WorkspaceService.Instance;

        // Event to request opening the Editor.
        // The string argument is the file path (optional).
        public event EventHandler<string?>? OpenEditorRequested;

        public WelcomeView()
        {
            InitializeComponent();

            WireEventHandlers();
            RefreshRecentWorkspaces();
        }

        private void WireEventHandlers()
        {
            BlankGraphButton.Click += (s, e) => OpenEditorRequested?.Invoke(this, null);
            LoadGraphButton.Click += BtnLoadGraph_Click;
        }

        /// <summary>
        /// Refreshes the recent workspaces list. Call this when the view becomes visible.
        /// </summary>
        public void RefreshRecentWorkspaces()
        {
            LoadRecentWorkspaces();
        }

        private void LoadRecentWorkspaces()
        {
            RecentPanel.SuspendLayout();
            RecentPanel.Controls.Clear();

            var workspaces = _workspaceService.GetAllWorkspaces();

            // Limit to MaxRecentWorkspaces from configuration
            var recentWorkspaces = workspaces
                .OrderByDescending(w => w.LastModified)
                .Take(UserConfiguration.MaxRecentWorkspaces)
                .ToList();

            if (recentWorkspaces.Count == 0)
            {
                var lblEmpty = new Label
                {
                    Text = "No recent workspaces.",
                    Dock = DockStyle.Top,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Padding = new Padding(10),
                    ForeColor = Color.Gray
                };
                RecentPanel.Controls.Add(lblEmpty);
            }
            else
            {
                foreach (var ws in Enumerable.Reverse(recentWorkspaces))
                {
                    var item = CreateRecentWorkspaceItem(ws);
                    RecentPanel.Controls.Add(item);
                    item.Dock = DockStyle.Top;
                }
            }

            RecentPanel.ResumeLayout();
        }

        private Control CreateRecentWorkspaceItem(WorkspaceInfo ws)
        {
            var btn = new Button
            {
                Text = $"{ws.Name}\n{ws.FilePath}",
                TextAlign = ContentAlignment.MiddleLeft,
                Height = 60,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(0, 0, 0, 5),
                Padding = new Padding(10, 5, 10, 5),
                Tag = ws,
                BackColor = Color.FromArgb(240, 240, 240)
            };

            // Simple styling
            btn.FlatAppearance.BorderSize = 0;

            btn.Click += RecentWorkspaceClick;
            btn.MouseEnter += RecentWorkspaceAnimationMouseEnter;
            btn.MouseLeave += RecentWorkspaceAnimationMouseLeave;

            // Add context menu
            var contextMenu = new ContextMenuStrip();

            var openItem = new ToolStripMenuItem("Open");
            openItem.Click += (s, e) => OpenWorkspace(ws.FilePath);
            contextMenu.Items.Add(openItem);

            contextMenu.Items.Add(new ToolStripSeparator());

            var editItem = new ToolStripMenuItem("Edit...");
            editItem.Click += (s, e) => EditWorkspaceEntry(ws);
            contextMenu.Items.Add(editItem);

            var removeItem = new ToolStripMenuItem("Remove from List");
            removeItem.Click += (s, e) => RemoveWorkspaceFromList(ws);
            contextMenu.Items.Add(removeItem);

            contextMenu.Items.Add(new ToolStripSeparator());

            var showInExplorerItem = new ToolStripMenuItem("Show in File Explorer");
            showInExplorerItem.Click += (s, e) => ShowInFileExplorer(ws.FilePath);
            contextMenu.Items.Add(showInExplorerItem);

            btn.ContextMenuStrip = contextMenu;

            return btn;
        }

        private void RecentWorkspaceClick(object? sender, EventArgs e)
        {
            if (sender is Button btn && btn.Tag is WorkspaceInfo ws)
            {
                OpenWorkspace(ws.FilePath);
            }
        }

        private void RecentWorkspaceAnimationMouseEnter(object? sender, EventArgs e)
        {
            if (sender is Control ctrl)
            {
                ctrl.Cursor = Cursors.Hand;
                ctrl.BackColor = ControlPaint.Light(ctrl.BackColor);
            }
        }

        private void RecentWorkspaceAnimationMouseLeave(object? sender, EventArgs e)
        {
            if (sender is Control ctrl)
            {
                ctrl.Cursor = Cursors.Default;
                ctrl.BackColor = Color.FromArgb(240, 240, 240);
            }
        }

        private void BtnLoadGraph_Click(object? sender, EventArgs e)
        {
            using OpenFileDialog dialog = new()
            {
                Filter = "ImageAutomate Workspace (*.imageautomate;*.json)|*.imageautomate;*.json|All Files (*.*)|*.*",
                Title = "Open Workspace"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                OpenWorkspace(dialog.FileName);
            }
        }

        private void OpenWorkspace(string filePath)
        {
            if (File.Exists(filePath))
            {
                // Update LastOpened
                _workspaceService.UpdateLastOpened(filePath);

                // Request Editor
                OpenEditorRequested?.Invoke(this, filePath);
            }
            else
            {
                MessageBox.Show($"File not found:\n{filePath}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _workspaceService.RemoveWorkspace(filePath);
                LoadRecentWorkspaces();
            }
        }

        private void EditWorkspaceEntry(WorkspaceInfo ws)
        {
            using var dialog = new WorkspaceSaveDialog(
                ws.Name,
                ws.FilePath,
                ws.ThumbnailPath,
                isEditMode: true
            );

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // Load the workspace JSON, update the name, and save it back
                    if (File.Exists(ws.FilePath))
                    {
                        var workspace = Workspace.LoadFromFile(ws.FilePath);
                        workspace.Name = dialog.WorkspaceName;
                        workspace.SaveToFile(ws.FilePath);
                    }

                    // Update the CSV entry with new name and image path
                    _workspaceService.AddOrUpdateWorkspace(
                        ws.FilePath,
                        dialog.WorkspaceName,
                        ws.Description,
                        dialog.ImagePath
                    );

                    // Refresh the list
                    LoadRecentWorkspaces();
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

        private void RemoveWorkspaceFromList(WorkspaceInfo ws)
        {
            var result = MessageBox.Show(
                $"Remove '{ws.Name}' from the recent list?\n\nThe workspace file will not be deleted.",
                "Remove Workspace",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                _workspaceService.RemoveWorkspace(ws.FilePath);
                LoadRecentWorkspaces();
            }
        }

        private void ShowInFileExplorer(string filePath)
        {
            if (File.Exists(filePath))
            {
                Process.Start("explorer.exe", $"/select,\"{filePath}\"");
            }
        }
    }
}