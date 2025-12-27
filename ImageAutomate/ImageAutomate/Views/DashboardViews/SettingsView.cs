using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using ImageAutomate.Core;
using ImageAutomate.Execution;
using ImageAutomate.Execution.Scheduling;

class EmptyScheduler : IScheduler
{
    public bool HasPendingWork => throw new NotImplementedException();

    public void Initialize(ImageAutomate.Execution.ExecutionContext context)
    {
        throw new NotImplementedException();
    }

    public IBlock? TryDequeue(ImageAutomate.Execution.ExecutionContext context)
    {
        throw new NotImplementedException();
    }

    public void NotifyCompleted(IBlock completedBlock, ImageAutomate.Execution.ExecutionContext context)
    {
        throw new NotImplementedException();
    }

    public void NotifyBlocked(IBlock blockedBlock, ImageAutomate.Execution.ExecutionContext context)
    {
        throw new NotImplementedException();
    }

    public void BeginNextShipmentCycle(ImageAutomate.Execution.ExecutionContext context)
    {
        throw new NotImplementedException();
    }
}

namespace ImageAutomate.Views.DashboardViews
{
    public partial class SettingsView : UserControl
    {
        SchedulerRegistry schedulerRegistry = SchedulerFactory.Registry;
        public SettingsView()
        {
            InitializeComponent();

            Debug.WriteLine("Settings view initialized.");

            schedulerRegistry.RegisterScheduler("EmptyScheduler", () => new EmptyScheduler());
        }

        private void SettingsViewLoad(object sender, EventArgs e)
        {
            comboBox1.Items.AddRange([.. schedulerRegistry.GetRegisteredNames()]);
        }
    }
}
