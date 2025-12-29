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
using ImageAutomate.Data;
using ImageAutomate.Models;
using ImageAutomate.Services;
using ImageAutomate.UI;

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

            // Initialize service with CSV data context
            var dataContext = new CsvWorkspaceDataContext();

            WireEventHandlers();
            LoadRecentWorkspaces();
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

            foreach (var ws in workspaces)
            {
                Debug.WriteLine($"Workspace: {ws.Name}, LastModified: {ws.LastModified}, FilePath: {ws.FilePath}");
            }

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
                Tag = ws.FilePath,
                BackColor = Color.FromArgb(240, 240, 240)
            };

            // Simple styling
            btn.FlatAppearance.BorderSize = 0;
            //btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(240, 240, 240);

            btn.Click += RecentWorkspaceClick;
            btn.MouseEnter += RecentWorkspaceAnimationMouseEnter;
            btn.MouseLeave += RecentWorkspaceAnimationMouseLeave;

            return btn;
        }

        private void RecentWorkspaceClick(object? sender, EventArgs e)
        {
            if (sender is Button btn && btn.Tag is string filePath)
            {
                OpenWorkspace(filePath);
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
                // Optionally remove from list?
                _workspaceService.RemoveWorkspace(filePath);
                LoadRecentWorkspaces();
            }
        }
    }
}