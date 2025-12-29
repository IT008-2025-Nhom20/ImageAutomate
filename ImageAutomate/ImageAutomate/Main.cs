using ImageAutomate.Core;
using ImageAutomate.Views;

namespace ImageAutomate
{
    public partial class Main : Form
    {
        private Lazy<EditorView> editorView;
        private bool isEditorLoaded = false;

        private EditorView CreateEditorView()
        {
            var ev = new EditorView()
            {
                Dock = DockStyle.Fill
            };
            ev.CloseRequested += EditorView_CloseRequested;
            return ev;
        }

        public Main()
        {
            InitializeComponent();
            InitializeEvents();

            // Load settings
            UserConfiguration.Load();

            DoubleBuffered = true;

            editorView = new Lazy<EditorView>(CreateEditorView);
        }

        private void InitializeEvents()
        {
            Dashboard.OpenEditorRequested += DashboardView1_OpenEditorRequested;
        }

        private void DashboardView1_OpenEditorRequested(object? sender, string? filePath)
        {
            ShowEditor(filePath);
        }

        private void ShowEditor(string? filePath)
        {
            var ev = editorView.Value;
            if (!isEditorLoaded)
            {
                Controls.Add(ev);
                isEditorLoaded = true;
            }

            ev.BringToFront();
            ev.Focus();

            if (string.IsNullOrEmpty(filePath))
            {
                ev.CreateNewWorkspace();
            }
            else
            {
                ev.LoadWorkspace(filePath);
            }
        }

        private void EditorView_CloseRequested(object? sender, EventArgs e)
        {
            ShowDashboard();
        }

        private void ShowDashboard()
        {
            Dashboard.BringToFront();

            // Refresh workspace lists in dashboard
            Dashboard.GetWelcomeView().RefreshRecentWorkspaces();
            Dashboard.GetWorkspacesView().RefreshWorkspaces();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            UserConfiguration.Save();
        }
    }
}