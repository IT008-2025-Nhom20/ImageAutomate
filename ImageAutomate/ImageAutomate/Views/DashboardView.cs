using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using ImageAutomate.Views.DashboardViews;

namespace ImageAutomate
{
    public partial class DashboardView : UserControl
    {
        private Lazy<WorkspaceView> workspaceView;
        private Lazy<PluginsView> pluginView;
        private Lazy<SettingsView> settingsView;
        UserControl currentView;

        public DashboardView()
        {
            InitializeComponent();

            workspaceView = new Lazy<WorkspaceView>(() => new WorkspaceView());
            pluginView = new Lazy<PluginsView>(() => new PluginsView());
            settingsView = new Lazy<SettingsView>(() => new SettingsView());
            currentView = WelcomeView;
        }

        private void Sidebar_NavigationRequested(object? sender, string viewName)
        {
            switch (viewName)
            {
                case "Welcome": SwitchToWelcome(); break;
                case "Workspaces": SwitchToView(GetWorkspacesView()); break;
                case "Plugins": SwitchToView(GetPluginView()); break;
                case "Settings": SwitchToView(GetSettingsView()); break;
            }
        }

        private void SwitchToWelcome()
        {
            SwitchToView(WelcomeView);
        }

        private void SwitchToEditor()
        {
            //if (editorView == null) editorView = new EditorView();
            //SwitchToView(editorView);
        }

        private WorkspaceView GetWorkspacesView()
        {
            return workspaceView.Value;
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
            Controls.Remove(currentView);
            Controls.Add(view);
            currentView = view;
            view.Location = new Point(Sidebar.Width, 0);
            view.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        }

        private void BtnWelcome_Click(object? sender, EventArgs e)
        {
            SwitchToWelcome();
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
    }
}
