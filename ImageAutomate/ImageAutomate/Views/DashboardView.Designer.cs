namespace ImageAutomate
{
    partial class DashboardView
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            Sidebar = new ImageAutomate.UI.SidebarControl();
            BtnSettings = new ImageAutomate.UI.SidebarButton();
            BtnPlugins = new ImageAutomate.UI.SidebarButton();
            BtnWorkspaces = new ImageAutomate.UI.SidebarButton();
            BtnWelcome = new ImageAutomate.UI.SidebarButton();
            LabelMenu = new ImageAutomate.UI.SidebarLabel();
            WelcomeView = new ImageAutomate.Views.DashboardViews.WelcomeView();
            Sidebar.SuspendLayout();
            SuspendLayout();
            // 
            // Sidebar
            // 
            Sidebar.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            Sidebar.Controls.Add(BtnSettings);
            Sidebar.Controls.Add(BtnPlugins);
            Sidebar.Controls.Add(BtnWorkspaces);
            Sidebar.Controls.Add(BtnWelcome);
            Sidebar.Controls.Add(LabelMenu);
            Sidebar.Location = new Point(0, 0);
            Sidebar.Name = "Sidebar";
            Sidebar.Size = new Size(72, 600);
            Sidebar.TabIndex = 1;
            // 
            // BtnSettings
            // 
            BtnSettings.CollapsedText = "S";
            BtnSettings.Dock = DockStyle.Top;
            BtnSettings.ExpandedText = "Settings";
            BtnSettings.Location = new Point(0, 252);
            BtnSettings.Name = "BtnSettings";
            BtnSettings.Size = new Size(72, 60);
            BtnSettings.TabIndex = 4;
            BtnSettings.Text = "S";
            BtnSettings.UseVisualStyleBackColor = true;
            BtnSettings.Click += BtnSettings_Click;
            // 
            // BtnPlugins
            // 
            BtnPlugins.CollapsedText = "P";
            BtnPlugins.Dock = DockStyle.Top;
            BtnPlugins.ExpandedText = "Plugins";
            BtnPlugins.Location = new Point(0, 192);
            BtnPlugins.Name = "BtnPlugins";
            BtnPlugins.Size = new Size(72, 60);
            BtnPlugins.TabIndex = 3;
            BtnPlugins.Text = "P";
            BtnPlugins.UseVisualStyleBackColor = true;
            BtnPlugins.Click += BtnPlugins_Clicked;
            // 
            // BtnWorkspaces
            // 
            BtnWorkspaces.CollapsedText = "Ws";
            BtnWorkspaces.Dock = DockStyle.Top;
            BtnWorkspaces.ExpandedText = "Workspaces";
            BtnWorkspaces.Location = new Point(0, 132);
            BtnWorkspaces.Name = "BtnWorkspaces";
            BtnWorkspaces.Size = new Size(72, 60);
            BtnWorkspaces.TabIndex = 2;
            BtnWorkspaces.Text = "Ws";
            BtnWorkspaces.UseVisualStyleBackColor = true;
            BtnWorkspaces.Click += BtnWorkspaces_Click;
            // 
            // BtnWelcome
            // 
            BtnWelcome.CollapsedText = "W";
            BtnWelcome.Dock = DockStyle.Top;
            BtnWelcome.ExpandedText = "Welcome";
            BtnWelcome.Location = new Point(0, 72);
            BtnWelcome.Name = "BtnWelcome";
            BtnWelcome.Size = new Size(72, 60);
            BtnWelcome.TabIndex = 1;
            BtnWelcome.Text = "W";
            BtnWelcome.UseVisualStyleBackColor = true;
            BtnWelcome.Click += BtnWelcome_Click;
            // 
            // LabelMenu
            // 
            LabelMenu.Dock = DockStyle.Top;
            LabelMenu.Font = new Font("Segoe UI", 15.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            LabelMenu.Location = new Point(0, 0);
            LabelMenu.Name = "LabelMenu";
            LabelMenu.Size = new Size(72, 72);
            LabelMenu.TabIndex = 0;
            LabelMenu.Text = "Menu";
            LabelMenu.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // WelcomeView
            // 
            WelcomeView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            WelcomeView.AutoScroll = true;
            WelcomeView.Location = new Point(72, 0);
            WelcomeView.Name = "WelcomeView";
            WelcomeView.Size = new Size(728, 600);
            WelcomeView.TabIndex = 2;
            // 
            // DashboardView
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(Sidebar);
            Controls.Add(WelcomeView);
            Name = "DashboardView";
            Size = new Size(800, 600);
            Sidebar.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private UI.SidebarControl Sidebar;
        private UI.SidebarButton BtnSettings;
        private UI.SidebarButton BtnPlugins;
        private UI.SidebarButton BtnWorkspaces;
        private UI.SidebarButton BtnWelcome;
        private UI.SidebarLabel LabelMenu;
        private Views.DashboardViews.WelcomeView WelcomeView;
    }
}
