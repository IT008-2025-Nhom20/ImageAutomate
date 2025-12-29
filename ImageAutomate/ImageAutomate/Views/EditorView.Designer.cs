namespace ImageAutomate.Views
{
    partial class EditorView
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
            Menubar = new MenuStrip();
            FileMenu = new ToolStripMenuItem();
            NewGraphMenuItem = new ToolStripMenuItem();
            OpenMenuItem = new ToolStripMenuItem();
            SaveMenuItem = new ToolStripMenuItem();
            SaveAsMenuItem = new ToolStripMenuItem();
            CloseMenuItem = new ToolStripMenuItem();
            ExecuteMenuItem = new ToolStripMenuItem();
            AboutMenuItem = new ToolStripMenuItem();
            HelpMenuItem = new ToolStripMenuItem();
            MainSplit = new SplitContainer();
            Toolbox = new ListBox();
            GraphPanelPropertyGridSplit = new SplitContainer();
            GraphPanel = new ImageAutomate.UI.GraphRenderPanel();
            BlockPropertyGrid = new PropertyGrid();
            Menubar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)MainSplit).BeginInit();
            MainSplit.Panel1.SuspendLayout();
            MainSplit.Panel2.SuspendLayout();
            MainSplit.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)GraphPanelPropertyGridSplit).BeginInit();
            GraphPanelPropertyGridSplit.Panel1.SuspendLayout();
            GraphPanelPropertyGridSplit.Panel2.SuspendLayout();
            GraphPanelPropertyGridSplit.SuspendLayout();
            SuspendLayout();
            // 
            // Menubar
            // 
            Menubar.Items.AddRange(new ToolStripItem[] { FileMenu, ExecuteMenuItem, AboutMenuItem, HelpMenuItem });
            Menubar.Location = new Point(0, 0);
            Menubar.Name = "Menubar";
            Menubar.Size = new Size(800, 24);
            Menubar.TabIndex = 0;
            Menubar.Text = "menuStrip1";
            // 
            // FileMenu
            // 
            FileMenu.DropDownItems.AddRange(new ToolStripItem[] { NewGraphMenuItem, OpenMenuItem, SaveMenuItem, SaveAsMenuItem, CloseMenuItem });
            FileMenu.Name = "FileMenu";
            FileMenu.Size = new Size(37, 20);
            FileMenu.Text = "File";
            // 
            // NewGraphMenuItem
            // 
            NewGraphMenuItem.Name = "NewGraphMenuItem";
            NewGraphMenuItem.Size = new Size(103, 22);
            NewGraphMenuItem.Text = "New";
            NewGraphMenuItem.Click += OnNewMenuItemClick;
            // 
            // OpenMenuItem
            // 
            OpenMenuItem.Name = "OpenMenuItem";
            OpenMenuItem.Size = new Size(103, 22);
            OpenMenuItem.Text = "Open";
            OpenMenuItem.Click += OnOpenMenuItemClick;
            // 
            // SaveMenuItem
            // 
            SaveMenuItem.Name = "SaveMenuItem";
            SaveMenuItem.Size = new Size(103, 22);
            SaveMenuItem.Text = "Save";
            SaveMenuItem.Click += OnSaveMenuItemClick;
            // 
            // SaveAsMenuItem
            // 
            SaveAsMenuItem.Name = "SaveAsMenuItem";
            SaveAsMenuItem.Size = new Size(103, 22);
            SaveAsMenuItem.Text = "Save As...";
            SaveAsMenuItem.Click += OnSaveAsMenuItemClick;
            // 
            // CloseMenuItem
            // 
            CloseMenuItem.Name = "CloseMenuItem";
            CloseMenuItem.Size = new Size(103, 22);
            CloseMenuItem.Text = "Close";
            CloseMenuItem.Click += OnCloseMenuItemClick;
            // 
            // ExecuteMenuItem
            // 
            ExecuteMenuItem.Name = "ExecuteMenuItem";
            ExecuteMenuItem.Size = new Size(59, 20);
            ExecuteMenuItem.Text = "Execute";
            ExecuteMenuItem.Click += OnExecuteMenuItemClick;
            // 
            // AboutMenuItem
            // 
            AboutMenuItem.Name = "AboutMenuItem";
            AboutMenuItem.Size = new Size(52, 20);
            AboutMenuItem.Text = "About";
            AboutMenuItem.Click += OnAboutMenuItemClick;
            // 
            // HelpMenuItem
            // 
            HelpMenuItem.Name = "HelpMenuItem";
            HelpMenuItem.Size = new Size(44, 20);
            HelpMenuItem.Text = "Help";
            HelpMenuItem.Click += OnHelpMenuItemClick;
            // 
            // MainSplit
            // 
            MainSplit.Dock = DockStyle.Fill;
            MainSplit.Location = new Point(0, 24);
            MainSplit.Name = "MainSplit";
            // 
            // MainSplit.Panel1
            // 
            MainSplit.Panel1.Controls.Add(Toolbox);
            // 
            // MainSplit.Panel2
            // 
            MainSplit.Panel2.Controls.Add(GraphPanelPropertyGridSplit);
            MainSplit.Size = new Size(800, 576);
            MainSplit.SplitterDistance = 132;
            MainSplit.TabIndex = 1;
            MainSplit.TabStop = false;
            // 
            // Toolbox
            // 
            Toolbox.Dock = DockStyle.Fill;
            Toolbox.FormattingEnabled = true;
            Toolbox.Location = new Point(0, 0);
            Toolbox.Name = "Toolbox";
            Toolbox.Size = new Size(132, 576);
            Toolbox.TabIndex = 1;
            Toolbox.MouseDown += OnToolboxMouseDown;
            // 
            // GraphPanelPropertyGridSplit
            // 
            GraphPanelPropertyGridSplit.Dock = DockStyle.Fill;
            GraphPanelPropertyGridSplit.Location = new Point(0, 0);
            GraphPanelPropertyGridSplit.Name = "GraphPanelPropertyGridSplit";
            // 
            // GraphPanelPropertyGridSplit.Panel1
            // 
            GraphPanelPropertyGridSplit.Panel1.Controls.Add(GraphPanel);
            // 
            // GraphPanelPropertyGridSplit.Panel2
            // 
            GraphPanelPropertyGridSplit.Panel2.Controls.Add(BlockPropertyGrid);
            GraphPanelPropertyGridSplit.Size = new Size(664, 576);
            GraphPanelPropertyGridSplit.SplitterDistance = 489;
            GraphPanelPropertyGridSplit.TabIndex = 0;
            GraphPanelPropertyGridSplit.TabStop = false;
            // 
            // GraphPanel
            // 
            GraphPanel.AllowDrop = true;
            GraphPanel.BackColor = Color.White;
            GraphPanel.Dock = DockStyle.Fill;
            GraphPanel.Location = new Point(0, 0);
            GraphPanel.Name = "GraphPanel";
            GraphPanel.Size = new Size(489, 576);
            GraphPanel.TabIndex = 2;
            GraphPanel.TabStop = true;
            GraphPanel.SelectedItemChanged += OnGraphSelectedItemChange;
            // 
            // BlockPropertyGrid
            // 
            BlockPropertyGrid.BackColor = SystemColors.Control;
            BlockPropertyGrid.Dock = DockStyle.Fill;
            BlockPropertyGrid.Location = new Point(0, 0);
            BlockPropertyGrid.Name = "BlockPropertyGrid";
            BlockPropertyGrid.Size = new Size(171, 576);
            BlockPropertyGrid.TabIndex = 0;
            BlockPropertyGrid.TabStop = false;
            // 
            // EditorView
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(MainSplit);
            Controls.Add(Menubar);
            Name = "EditorView";
            Size = new Size(800, 600);
            Menubar.ResumeLayout(false);
            Menubar.PerformLayout();
            MainSplit.Panel1.ResumeLayout(false);
            MainSplit.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)MainSplit).EndInit();
            MainSplit.ResumeLayout(false);
            GraphPanelPropertyGridSplit.Panel1.ResumeLayout(false);
            GraphPanelPropertyGridSplit.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)GraphPanelPropertyGridSplit).EndInit();
            GraphPanelPropertyGridSplit.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MenuStrip Menubar;
        private SplitContainer MainSplit;
        private ToolStripMenuItem FileMenu;
        private ToolStripMenuItem NewGraphMenuItem;
        private ToolStripMenuItem OpenMenuItem;
        private ToolStripMenuItem SaveMenuItem;
        private ToolStripMenuItem SaveAsMenuItem;
        private ToolStripMenuItem CloseMenuItem;
        private SplitContainer GraphPanelPropertyGridSplit;
        private PropertyGrid BlockPropertyGrid;
        private UI.GraphRenderPanel GraphPanel;
        private ListBox Toolbox;
        private ToolStripMenuItem ExecuteMenuItem;
        private ToolStripMenuItem AboutMenuItem;
        private ToolStripMenuItem HelpMenuItem;
    }
}
