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
            MainSplitter = new SplitContainer();
            LeftSidebarPanel = new Panel();
            BtnSettings = new Button();
            BtnPlugins = new Button();
            BtnWorkspaces = new Button();
            BtnWelcome = new Button();
            MenuLabel = new Label();
            WelcomeView = new ImageAutomate.Views.DashboardViews.WelcomeView();
            ((System.ComponentModel.ISupportInitialize)MainSplitter).BeginInit();
            MainSplitter.Panel1.SuspendLayout();
            MainSplitter.Panel2.SuspendLayout();
            MainSplitter.SuspendLayout();
            LeftSidebarPanel.SuspendLayout();
            SuspendLayout();
            // 
            // MainSplitter
            // 
            MainSplitter.Dock = DockStyle.Fill;
            MainSplitter.FixedPanel = FixedPanel.Panel1;
            MainSplitter.IsSplitterFixed = true;
            MainSplitter.Location = new Point(0, 0);
            MainSplitter.Name = "MainSplitter";
            // 
            // MainSplitter.Panel1
            // 
            MainSplitter.Panel1.Controls.Add(LeftSidebarPanel);
            // 
            // MainSplitter.Panel2
            // 
            MainSplitter.Panel2.Controls.Add(WelcomeView);
            MainSplitter.Size = new Size(800, 600);
            MainSplitter.SplitterDistance = 60;
            MainSplitter.TabIndex = 1;
            // 
            // LeftSidebarPanel
            // 
            LeftSidebarPanel.AutoScroll = true;
            LeftSidebarPanel.Controls.Add(BtnSettings);
            LeftSidebarPanel.Controls.Add(BtnPlugins);
            LeftSidebarPanel.Controls.Add(BtnWorkspaces);
            LeftSidebarPanel.Controls.Add(BtnWelcome);
            LeftSidebarPanel.Controls.Add(MenuLabel);
            LeftSidebarPanel.Dock = DockStyle.Fill;
            LeftSidebarPanel.Location = new Point(0, 0);
            LeftSidebarPanel.Margin = new Padding(7);
            LeftSidebarPanel.Name = "LeftSidebarPanel";
            LeftSidebarPanel.Size = new Size(60, 600);
            LeftSidebarPanel.TabIndex = 0;
            // 
            // BtnSettings
            // 
            BtnSettings.Dock = DockStyle.Top;
            BtnSettings.Location = new Point(0, 187);
            BtnSettings.Name = "BtnSettings";
            BtnSettings.Size = new Size(60, 50);
            BtnSettings.TabIndex = 4;
            BtnSettings.Tag = "icon_mode";
            BtnSettings.Text = "Settings";
            BtnSettings.UseVisualStyleBackColor = true;
            BtnSettings.Click += BtnSettings_Click;
            // 
            // BtnPlugins
            // 
            BtnPlugins.Dock = DockStyle.Top;
            BtnPlugins.Location = new Point(0, 137);
            BtnPlugins.Name = "BtnPlugins";
            BtnPlugins.Size = new Size(60, 50);
            BtnPlugins.TabIndex = 3;
            BtnPlugins.Tag = "icon_mode";
            BtnPlugins.Text = "Plugins";
            BtnPlugins.UseVisualStyleBackColor = true;
            BtnPlugins.Click += BtnPlugins_Clicked;
            // 
            // BtnWorkspaces
            // 
            BtnWorkspaces.Dock = DockStyle.Top;
            BtnWorkspaces.Location = new Point(0, 87);
            BtnWorkspaces.Name = "BtnWorkspaces";
            BtnWorkspaces.Size = new Size(60, 50);
            BtnWorkspaces.TabIndex = 2;
            BtnWorkspaces.Tag = "icon_mode";
            BtnWorkspaces.Text = "Workspaces";
            BtnWorkspaces.UseVisualStyleBackColor = true;
            BtnWorkspaces.Click += BtnWorkspaces_Click;
            // 
            // BtnWelcome
            // 
            BtnWelcome.Dock = DockStyle.Top;
            BtnWelcome.Location = new Point(0, 37);
            BtnWelcome.Name = "BtnWelcome";
            BtnWelcome.Size = new Size(60, 50);
            BtnWelcome.TabIndex = 1;
            BtnWelcome.Tag = "icon_mode";
            BtnWelcome.Text = "Welcome";
            BtnWelcome.UseVisualStyleBackColor = true;
            BtnWelcome.Click += this.BtnWelcome_Click;
            // 
            // MenuLabel
            // 
            MenuLabel.Dock = DockStyle.Top;
            MenuLabel.Font = new Font("Segoe UI", 15.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            MenuLabel.Location = new Point(0, 0);
            MenuLabel.Margin = new Padding(5);
            MenuLabel.Name = "MenuLabel";
            MenuLabel.Size = new Size(60, 37);
            MenuLabel.TabIndex = 0;
            MenuLabel.Tag = "icon_mode";
            MenuLabel.Text = "Menu";
            MenuLabel.TextAlign = ContentAlignment.MiddleRight;
            // 
            // WelcomeView
            // 
            WelcomeView.Dock = DockStyle.Fill;
            WelcomeView.Location = new Point(0, 0);
            WelcomeView.Name = "WelcomeView";
            WelcomeView.Size = new Size(736, 600);
            WelcomeView.TabIndex = 0;
            // 
            // DashboardView
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(MainSplitter);
            Name = "DashboardView";
            Size = new Size(800, 600);
            MainSplitter.Panel1.ResumeLayout(false);
            MainSplitter.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)MainSplitter).EndInit();
            MainSplitter.ResumeLayout(false);
            LeftSidebarPanel.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private SplitContainer MainSplitter;
        private Panel LeftSidebarPanel;
        private Button BtnSettings;
        private Button BtnPlugins;
        private Button BtnWorkspaces;
        private Button BtnWelcome;
        private Label MenuLabel;
        private Views.DashboardViews.WelcomeView WelcomeView;
    }
}
