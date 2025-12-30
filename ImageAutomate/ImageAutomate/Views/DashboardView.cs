using ImageAutomate.Views.DashboardViews;

namespace ImageAutomate
{
    public partial class DashboardView : UserControl
    {
        private Lazy<WorkspaceView> workspaceView;
        private Lazy<PluginsView> pluginView;
        private Lazy<SettingsView> settingsView;
        private Lazy<WelcomeView> welcomeView;
        UserControl? currentView;

        // Event to request the editor.
        public event EventHandler<string?>? OpenEditorRequested;

        public DashboardView()
        {
            InitializeComponent();

            workspaceView = new Lazy<WorkspaceView>(() =>
            {
                var v = new WorkspaceView();
                v.OpenEditorRequested += WelcomeView_OpenEditorRequested; // Reuse same handler
                return v;
            });

            pluginView = new Lazy<PluginsView>(() => new PluginsView());

            settingsView = new Lazy<SettingsView>(() => new SettingsView());

            welcomeView = new Lazy<WelcomeView>(() =>
            {
                var v = new WelcomeView();
                v.OpenEditorRequested += WelcomeView_OpenEditorRequested;
                return v;
            });
        }

        private void WelcomeView_OpenEditorRequested(object? sender, string? filePath)
        {
            OpenEditorRequested?.Invoke(this, filePath);
        }

        public WelcomeView GetWelcomeView()
        {
            var view = welcomeView.Value;
            // Refresh recent list when showing
            view.RefreshRecentWorkspaces();
            return view;
        }

        public WorkspaceView GetWorkspacesView()
        {
            var view = workspaceView.Value;
            view.RefreshWorkspaces();
            return view;
        }

        private PluginsView GetPluginView()
        {
            return pluginView.Value;
        }

        private SettingsView GetSettingsView()
        {
            return settingsView.Value;
        }

        private void SwitchToView(UserControl view)
        {
            if (ContentPanel.Controls.Contains(view)) return;

            ContentPanel.Controls.Clear();
            currentView = view;
            view.Dock = DockStyle.Fill;
            ContentPanel.Controls.Add(view);
        }

        private void BtnWelcome_Click(object? sender, EventArgs e)
        {
            SwitchToView(GetWelcomeView());
        }

        private void BtnWorkspaces_Click(object? sender, EventArgs e)
        {
            SwitchToView(GetWorkspacesView());
        }

        private void BtnPlugins_Clicked(object? sender, EventArgs e)
        {
            SwitchToView(GetPluginView());
        }

        private void BtnSettings_Click(object? sender, EventArgs e)
        {
            SwitchToView(GetSettingsView());
        }

        private void OnDashboardLoad(object sender, EventArgs e)
        {
            SwitchToView(GetWelcomeView());
        }
    }
}