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

        public DashboardView()
        {
            InitializeComponent();

            workspaceView = new Lazy<WorkspaceView>(() => new WorkspaceView());
            pluginView = new Lazy<PluginsView>(() => new PluginsView());
            settingsView = new Lazy<SettingsView>(() => new SettingsView());
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
            MainSplitter.Panel2.Controls.Clear();
            MainSplitter.Panel2.Controls.Add(view);
            view.Dock = DockStyle.Fill;
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
