using System.Runtime.InteropServices;

using ImageAutomate.Core;
using ImageAutomate.Execution.Scheduling;

namespace ImageAutomate.Views.DashboardViews
{
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();

            LoadConfigurationToUI();
            WireEventHandlers();
        }

        /// <summary>
        /// Loads configuration values from UserConfiguration into UI controls.
        /// </summary>
        private void LoadConfigurationToUI()
        {
            // Application Settings
            MaxRecentWorkspacesValue.Value = UserConfiguration.MaxRecentWorkspaces;

            // Execution Settings
            ExecutionModeComboBox.SelectedItem = UserConfiguration.Mode;
            MaxDegreeOfParallelismValue.Value = UserConfiguration.MaxDegreeOfParallelism;
            EnableGcThrottlingValue.Checked = UserConfiguration.EnableGcThrottling;
            WatchdogTimeoutValue.Value = UserConfiguration.WatchdogTimeoutSeconds;
            MaxShipmentSizeValue.Value = UserConfiguration.MaxShipmentSize;
            ProfilingWindowSizeValue.Value = UserConfiguration.ProfilingWindowSize;
            CostEmaAlphaValue.Value = (decimal)UserConfiguration.CostEmaAlpha;
            CriticalPathRecomputeIntervalValue.Value = UserConfiguration.CriticalPathRecomputeInterval;
            BatchSizeValue.Value = UserConfiguration.BatchSize;
            CriticalPathBoostValue.Value = (decimal)UserConfiguration.CriticalPathBoost;

            // Editor Settings
            SelectedBlockOutlineColorButton.SelectedColor = UserConfiguration.SelectedBlockOutlineColor;
            SocketRadiusValue.Value = UserConfiguration.SocketRadius;
            RenderScaleValue.Value = (decimal)UserConfiguration.RenderScale;
            AllowOutOfScreenPanValue.Checked = UserConfiguration.AllowOutOfScreenPan;
            AutoSnapZoneWidthValue.Value = UserConfiguration.AutoSnapZoneWidth;

            // Theme Settings
            HoveredBlockOutlineColorButton.SelectedColor = UserConfiguration.HoveredBlockOutline.Color;
            DefaultNodeColorColorButton.SelectedColor = UserConfiguration.DefaultNodeColor;
            HoveredNodeColorColorButton.SelectedColor = UserConfiguration.HoveredNodeColor;
            DisabledNodeColorColorButton.SelectedColor = UserConfiguration.DisabledNodeColor;
            SuccessColorColorButton.SelectedColor = UserConfiguration.SuccessColor;
            ErrorColorColorButton.SelectedColor = UserConfiguration.ErrorColor;
            SelectedBlockOutlineColorButton2.SelectedColor = UserConfiguration.SelectedBlockOutline.Color;
            TextColorColorButton.SelectedColor = UserConfiguration.TextColor;
            BorderOutlineColorButton.SelectedColor = UserConfiguration.BorderOutline.Color;
            SocketConnectionColorColorButton.SelectedColor = UserConfiguration.SocketConnectionColor.Color;
            NodeWidthValue.Value = UserConfiguration.NodeWidth;
            NodeBorderWidthValue.Value = UserConfiguration.NodeBorderWidth;
            NodeSpacingValue.Value = UserConfiguration.NodeSpacing;
        }

        /// <summary>
        /// Wires up event handlers for all UI controls.
        /// </summary>
        private void WireEventHandlers()
        {
            // Application Settings
            MaxRecentWorkspacesValue.ValueChanged += (s, e) =>
            {
                UserConfiguration.MaxRecentWorkspaces = (int)MaxRecentWorkspacesValue.Value;
            };

            // Execution Settings
            ExecutionModeComboBox.SelectedIndexChanged += (s, e) =>
            {
                UserConfiguration.Mode = ExecutionModeComboBox.SelectedItem?.ToString() ?? "SimpleDfs";
            };

            MaxDegreeOfParallelismValue.ValueChanged += (s, e) =>
            {
                UserConfiguration.MaxDegreeOfParallelism = (int)MaxDegreeOfParallelismValue.Value;
            };

            EnableGcThrottlingValue.CheckedChanged += (s, e) =>
            {
                UserConfiguration.EnableGcThrottling = EnableGcThrottlingValue.Checked;
            };

            WatchdogTimeoutValue.ValueChanged += (s, e) =>
            {
                UserConfiguration.WatchdogTimeoutSeconds = (int)WatchdogTimeoutValue.Value;
            };

            MaxShipmentSizeValue.ValueChanged += (s, e) =>
            {
                UserConfiguration.MaxShipmentSize = (int)MaxShipmentSizeValue.Value;
            };

            ProfilingWindowSizeValue.ValueChanged += (s, e) =>
            {
                UserConfiguration.ProfilingWindowSize = (int)ProfilingWindowSizeValue.Value;
            };

            CostEmaAlphaValue.ValueChanged += (s, e) =>
            {
                UserConfiguration.CostEmaAlpha = (double)CostEmaAlphaValue.Value;
            };

            CriticalPathRecomputeIntervalValue.ValueChanged += (s, e) =>
            {
                UserConfiguration.CriticalPathRecomputeInterval = (int)CriticalPathRecomputeIntervalValue.Value;
            };

            BatchSizeValue.ValueChanged += (s, e) =>
            {
                UserConfiguration.BatchSize = (int)BatchSizeValue.Value;
            };

            CriticalPathBoostValue.ValueChanged += (s, e) =>
            {
                UserConfiguration.CriticalPathBoost = (double)CriticalPathBoostValue.Value;
            };

            // Editor Settings
            SelectedBlockOutlineColorButton.SelectedColorChanged += (s, e) =>
            {
                UserConfiguration.SelectedBlockOutlineColor = SelectedBlockOutlineColorButton.SelectedColor;
            };

            SocketRadiusValue.ValueChanged += (s, e) =>
            {
                UserConfiguration.SocketRadius = (int)SocketRadiusValue.Value;
            };

            RenderScaleValue.ValueChanged += (s, e) =>
            {
                UserConfiguration.RenderScale = (float)RenderScaleValue.Value;
            };

            AllowOutOfScreenPanValue.CheckedChanged += (s, e) =>
            {
                UserConfiguration.AllowOutOfScreenPan = AllowOutOfScreenPanValue.Checked;
            };

            AutoSnapZoneWidthValue.ValueChanged += (s, e) =>
            {
                UserConfiguration.AutoSnapZoneWidth = (int)AutoSnapZoneWidthValue.Value;
            };

            // Theme Settings
            HoveredBlockOutlineColorButton.SelectedColorChanged += (s, e) =>
            {
                UserConfiguration.HoveredBlockOutline.Color = HoveredBlockOutlineColorButton.SelectedColor;
            };

            DefaultNodeColorColorButton.SelectedColorChanged += (s, e) =>
            {
                UserConfiguration.DefaultNodeColor = DefaultNodeColorColorButton.SelectedColor;
            };

            HoveredNodeColorColorButton.SelectedColorChanged += (s, e) =>
            {
                UserConfiguration.HoveredNodeColor = HoveredNodeColorColorButton.SelectedColor;
            };

            DisabledNodeColorColorButton.SelectedColorChanged += (s, e) =>
            {
                UserConfiguration.DisabledNodeColor = DisabledNodeColorColorButton.SelectedColor;
            };

            SuccessColorColorButton.SelectedColorChanged += (s, e) =>
            {
                UserConfiguration.SuccessColor = SuccessColorColorButton.SelectedColor;
            };

            ErrorColorColorButton.SelectedColorChanged += (s, e) =>
            {
                UserConfiguration.ErrorColor = ErrorColorColorButton.SelectedColor;
            };

            SelectedBlockOutlineColorButton2.SelectedColorChanged += (s, e) =>
            {
                UserConfiguration.SelectedBlockOutline.Color = SelectedBlockOutlineColorButton2.SelectedColor;
            };

            TextColorColorButton.SelectedColorChanged += (s, e) =>
            {
                UserConfiguration.TextColor = TextColorColorButton.SelectedColor;
            };

            BorderOutlineColorButton.SelectedColorChanged += (s, e) =>
            {
                UserConfiguration.BorderOutline.Color = BorderOutlineColorButton.SelectedColor;
            };

            SocketConnectionColorColorButton.SelectedColorChanged += (s, e) =>
            {
                UserConfiguration.SocketConnectionColor.Color = SocketConnectionColorColorButton.SelectedColor;
            };

            NodeWidthValue.ValueChanged += (s, e) =>
            {
                UserConfiguration.NodeWidth = (int)NodeWidthValue.Value;
            };

            NodeBorderWidthValue.ValueChanged += (s, e) =>
            {
                UserConfiguration.NodeBorderWidth = (int)NodeBorderWidthValue.Value;
            };

            NodeSpacingValue.ValueChanged += (s, e) =>
            {
                UserConfiguration.NodeSpacing = (int)NodeSpacingValue.Value;
            };

            // Buttons
            BtnSave.Click += (s, e) =>
            {
                UserConfiguration.Save();
                MessageBox.Show("Settings saved successfully.", "Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            BtnReload.Click += (s, e) =>
            {
                UserConfiguration.Load();
                LoadConfigurationToUI();
                MessageBox.Show("Settings reloaded from disk.", "Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            BtnSetToDefault.Click += (s, e) =>
            {
                var result = MessageBox.Show(
                    "Are you sure you want to reset all settings to their default values?",
                    "Reset Settings",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    UserConfiguration.ResetToDefaults();
                    LoadConfigurationToUI();
                }
            };
        }

        private void HoveredBlockOutlineLabel_Click(object sender, EventArgs e)
        {

        }

        private void OnSettingsViewLoad(object sender, EventArgs e)
        {
            ExecutionModeComboBox.Items.AddRange(SchedulerRegistry.Instance.GetRegisteredSchedulers().ToArray());
            ExecutionModeComboBox.SelectedItem = UserConfiguration.Mode;
        }
    }

    /// <summary>
    /// NumericUpDown that only processes MouseWheel when focused, ensuring parent panels can scroll.
    /// </summary>
    public class NoScrollNumericUpDown : NumericUpDown
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x020A && !this.Focused) // WM_MOUSEWHEEL = 0x020A
            {
                // Forward message to parent to allow container scrolling
                if (this.Parent != null)
                {
                    SendMessage(this.Parent.Handle, m.Msg, m.WParam, m.LParam);
                }
                // Do NOT call base.WndProc, effectively ignoring the event for this control
                return;
            }
            base.WndProc(ref m);
        }
    }

    /// <summary>
    /// ComboBox that only processes MouseWheel when focused, ensuring parent panels can scroll.
    /// </summary>
    public class NoScrollComboBox : ComboBox
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x020A && !this.Focused) // WM_MOUSEWHEEL = 0x020A
            {
                if (this.Parent != null)
                {
                    SendMessage(this.Parent.Handle, m.Msg, m.WParam, m.LParam);
                }
                return;
            }
            base.WndProc(ref m);
        }
    }
}