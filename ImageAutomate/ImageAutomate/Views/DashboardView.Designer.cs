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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DashboardView));
            Sidebar = new ImageAutomate.UI.SidebarControl();
            BtnSettings = new ImageAutomate.UI.SidebarButton();
            BtnPlugins = new ImageAutomate.UI.SidebarButton();
            BtnWorkspaces = new ImageAutomate.UI.SidebarButton();
            BtnWelcome = new ImageAutomate.UI.SidebarButton();
            LabelMenu = new ImageAutomate.UI.SidebarLabel();
            ContentPanel = new Panel();
            Sidebar.SuspendLayout();
            SuspendLayout();
            // 
            // Sidebar
            // 
            Sidebar.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            Sidebar.BorderStyle = BorderStyle.FixedSingle;
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
            BtnSettings.CollapsedImage = (Image)resources.GetObject("BtnSettings.CollapsedImage");
            BtnSettings.CollapsedText = "";
            BtnSettings.Dock = DockStyle.Top;
            BtnSettings.ExpandedText = "Settings";
            BtnSettings.Image = (Image)resources.GetObject("BtnSettings.Image");
            BtnSettings.Location = new Point(0, 252);
            BtnSettings.Name = "BtnSettings";
            BtnSettings.Size = new Size(70, 60);
            BtnSettings.TabIndex = 4;
            BtnSettings.UseVisualStyleBackColor = true;
            BtnSettings.Click += BtnSettings_Click;
            // 
            // BtnPlugins
            // 
            BtnPlugins.CollapsedImage = (Image)resources.GetObject("BtnPlugins.CollapsedImage");
            BtnPlugins.CollapsedText = "";
            BtnPlugins.Dock = DockStyle.Top;
            BtnPlugins.ExpandedText = "Plugins";
            BtnPlugins.Image = (Image)resources.GetObject("BtnPlugins.Image");
            BtnPlugins.Location = new Point(0, 192);
            BtnPlugins.Name = "BtnPlugins";
            BtnPlugins.Size = new Size(70, 60);
            BtnPlugins.TabIndex = 3;
            BtnPlugins.UseVisualStyleBackColor = true;
            BtnPlugins.Click += BtnPlugins_Clicked;
            // 
            // BtnWorkspaces
            // 
            BtnWorkspaces.CollapsedImage = (Image)resources.GetObject("BtnWorkspaces.CollapsedImage");
            BtnWorkspaces.CollapsedText = "";
            BtnWorkspaces.Dock = DockStyle.Top;
            BtnWorkspaces.ExpandedText = "Workspaces";
            BtnWorkspaces.Image = (Image)resources.GetObject("BtnWorkspaces.Image");
            BtnWorkspaces.Location = new Point(0, 132);
            BtnWorkspaces.Name = "BtnWorkspaces";
            BtnWorkspaces.Size = new Size(70, 60);
            BtnWorkspaces.TabIndex = 2;
            BtnWorkspaces.UseVisualStyleBackColor = true;
            BtnWorkspaces.Click += BtnWorkspaces_Click;
            // 
            // BtnWelcome
            // 
            BtnWelcome.CollapsedImage = (Image)resources.GetObject("BtnWelcome.CollapsedImage");
            BtnWelcome.CollapsedText = "";
            BtnWelcome.Dock = DockStyle.Top;
            BtnWelcome.ExpandedText = "Home";
            BtnWelcome.Image = (Image)resources.GetObject("BtnWelcome.Image");
            BtnWelcome.Location = new Point(0, 72);
            BtnWelcome.Name = "BtnWelcome";
            BtnWelcome.Size = new Size(70, 60);
            BtnWelcome.TabIndex = 1;
            BtnWelcome.UseVisualStyleBackColor = true;
            BtnWelcome.Click += BtnWelcome_Click;
            // 
            // LabelMenu
            // 
            LabelMenu.CollapsedImage = (Image)resources.GetObject("LabelMenu.CollapsedImage");
            LabelMenu.CollapsedText = "";
            LabelMenu.Dock = DockStyle.Top;
            LabelMenu.ExpandedText = "Menu";
            LabelMenu.Font = new Font("Segoe UI", 15.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            LabelMenu.Image = (Image)resources.GetObject("LabelMenu.Image");
            LabelMenu.Location = new Point(0, 0);
            LabelMenu.Name = "LabelMenu";
            LabelMenu.Size = new Size(70, 72);
            LabelMenu.TabIndex = 0;
            LabelMenu.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // ContentPanel
            // 
            ContentPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            ContentPanel.BorderStyle = BorderStyle.FixedSingle;
            ContentPanel.Location = new Point(72, 0);
            ContentPanel.Name = "ContentPanel";
            ContentPanel.Size = new Size(728, 600);
            ContentPanel.TabIndex = 2;
            // 
            // DashboardView
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(Sidebar);
            Controls.Add(ContentPanel);
            Name = "DashboardView";
            Size = new Size(800, 600);
            Load += OnDashboardLoad;
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
        private Panel ContentPanel;
    }
}