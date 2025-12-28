using System.Diagnostics;

using ImageAutomate.Execution;
using ImageAutomate.Execution.Scheduling;

namespace ImageAutomate.Views.DashboardViews
{
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();

            Debug.WriteLine("Settings view initialized.");
        }
    }
}
