namespace ImageAutomate.Views.DashboardViews
{
    partial class WelcomeView
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
            RightContentSplit = new SplitContainer();
            LabelSub = new Label();
            LabelMain = new Label();
            RecentGroup = new GroupBox();
            PanelRecentGroup = new Panel();
            PanelRecent5 = new Panel();
            PanelRecent4 = new Panel();
            PanelRecent3 = new Panel();
            PanelRecent2 = new Panel();
            PanelRecent1 = new Panel();
            ButtonGroup = new GroupBox();
            PanelStartGroup = new Panel();
            ButtonTable = new TableLayoutPanel();
            BtnBlankGraph = new Button();
            BtnLoadGraph = new Button();
            ((System.ComponentModel.ISupportInitialize)RightContentSplit).BeginInit();
            RightContentSplit.Panel1.SuspendLayout();
            RightContentSplit.Panel2.SuspendLayout();
            RightContentSplit.SuspendLayout();
            RecentGroup.SuspendLayout();
            PanelRecentGroup.SuspendLayout();
            ButtonGroup.SuspendLayout();
            PanelStartGroup.SuspendLayout();
            ButtonTable.SuspendLayout();
            SuspendLayout();
            // 
            // RightContentSplit
            // 
            RightContentSplit.Dock = DockStyle.Fill;
            RightContentSplit.FixedPanel = FixedPanel.Panel1;
            RightContentSplit.IsSplitterFixed = true;
            RightContentSplit.Location = new Point(0, 0);
            RightContentSplit.Name = "RightContentSplit";
            RightContentSplit.Orientation = Orientation.Horizontal;
            // 
            // RightContentSplit.Panel1
            // 
            RightContentSplit.Panel1.Controls.Add(LabelSub);
            RightContentSplit.Panel1.Controls.Add(LabelMain);
            // 
            // RightContentSplit.Panel2
            // 
            RightContentSplit.Panel2.Controls.Add(RecentGroup);
            RightContentSplit.Panel2.Controls.Add(ButtonGroup);
            RightContentSplit.Size = new Size(740, 600);
            RightContentSplit.SplitterDistance = 72;
            RightContentSplit.TabIndex = 1;
            // 
            // LabelSub
            // 
            LabelSub.AutoSize = true;
            LabelSub.Dock = DockStyle.Top;
            LabelSub.Location = new Point(0, 37);
            LabelSub.Name = "LabelSub";
            LabelSub.Size = new Size(196, 15);
            LabelSub.TabIndex = 1;
            LabelSub.Text = "Visual Image Processing Flow Editor";
            // 
            // LabelMain
            // 
            LabelMain.Dock = DockStyle.Top;
            LabelMain.Font = new Font("Segoe UI", 18F, FontStyle.Bold, GraphicsUnit.Point, 0);
            LabelMain.Location = new Point(0, 0);
            LabelMain.Name = "LabelMain";
            LabelMain.Size = new Size(740, 37);
            LabelMain.TabIndex = 0;
            LabelMain.Text = "ImageAutomate";
            // 
            // RecentGroup
            // 
            RecentGroup.Controls.Add(PanelRecentGroup);
            RecentGroup.Dock = DockStyle.Top;
            RecentGroup.Location = new Point(0, 129);
            RecentGroup.Name = "RecentGroup";
            RecentGroup.Size = new Size(740, 129);
            RecentGroup.TabIndex = 5;
            RecentGroup.TabStop = false;
            RecentGroup.Text = "Recent Workspaces";
            // 
            // PanelRecentGroup
            // 
            PanelRecentGroup.AutoScroll = true;
            PanelRecentGroup.Controls.Add(PanelRecent5);
            PanelRecentGroup.Controls.Add(PanelRecent4);
            PanelRecentGroup.Controls.Add(PanelRecent3);
            PanelRecentGroup.Controls.Add(PanelRecent2);
            PanelRecentGroup.Controls.Add(PanelRecent1);
            PanelRecentGroup.Dock = DockStyle.Fill;
            PanelRecentGroup.Location = new Point(3, 19);
            PanelRecentGroup.Name = "PanelRecentGroup";
            PanelRecentGroup.Size = new Size(734, 107);
            PanelRecentGroup.TabIndex = 2;
            // 
            // PanelRecent5
            // 
            PanelRecent5.AutoSize = true;
            PanelRecent5.Dock = DockStyle.Left;
            PanelRecent5.Location = new Point(0, 0);
            PanelRecent5.Name = "PanelRecent5";
            PanelRecent5.Size = new Size(0, 107);
            PanelRecent5.TabIndex = 6;
            // 
            // PanelRecent4
            // 
            PanelRecent4.AutoSize = true;
            PanelRecent4.Dock = DockStyle.Left;
            PanelRecent4.Location = new Point(0, 0);
            PanelRecent4.Name = "PanelRecent4";
            PanelRecent4.Size = new Size(0, 107);
            PanelRecent4.TabIndex = 5;
            // 
            // PanelRecent3
            // 
            PanelRecent3.AutoSize = true;
            PanelRecent3.Dock = DockStyle.Left;
            PanelRecent3.Location = new Point(0, 0);
            PanelRecent3.Name = "PanelRecent3";
            PanelRecent3.Size = new Size(0, 107);
            PanelRecent3.TabIndex = 4;
            // 
            // PanelRecent2
            // 
            PanelRecent2.AutoSize = true;
            PanelRecent2.Dock = DockStyle.Left;
            PanelRecent2.Location = new Point(0, 0);
            PanelRecent2.Name = "PanelRecent2";
            PanelRecent2.Size = new Size(0, 107);
            PanelRecent2.TabIndex = 3;
            // 
            // PanelRecent1
            // 
            PanelRecent1.AutoSize = true;
            PanelRecent1.Dock = DockStyle.Left;
            PanelRecent1.Location = new Point(0, 0);
            PanelRecent1.Name = "PanelRecent1";
            PanelRecent1.Size = new Size(0, 107);
            PanelRecent1.TabIndex = 2;
            // 
            // ButtonGroup
            // 
            ButtonGroup.Controls.Add(PanelStartGroup);
            ButtonGroup.Dock = DockStyle.Top;
            ButtonGroup.Location = new Point(0, 0);
            ButtonGroup.Name = "ButtonGroup";
            ButtonGroup.Size = new Size(740, 129);
            ButtonGroup.TabIndex = 4;
            ButtonGroup.TabStop = false;
            ButtonGroup.Text = "Start";
            // 
            // PanelStartGroup
            // 
            PanelStartGroup.AutoScroll = true;
            PanelStartGroup.Controls.Add(ButtonTable);
            PanelStartGroup.Dock = DockStyle.Fill;
            PanelStartGroup.Location = new Point(3, 19);
            PanelStartGroup.Name = "PanelStartGroup";
            PanelStartGroup.Size = new Size(734, 107);
            PanelStartGroup.TabIndex = 2;
            // 
            // ButtonTable
            // 
            ButtonTable.ColumnCount = 2;
            ButtonTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            ButtonTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            ButtonTable.Controls.Add(BtnBlankGraph, 0, 0);
            ButtonTable.Controls.Add(BtnLoadGraph, 1, 0);
            ButtonTable.Dock = DockStyle.Top;
            ButtonTable.Location = new Point(0, 0);
            ButtonTable.Name = "ButtonTable";
            ButtonTable.RowCount = 1;
            ButtonTable.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            ButtonTable.Size = new Size(734, 100);
            ButtonTable.TabIndex = 3;
            // 
            // BtnBlankGraph
            // 
            BtnBlankGraph.Dock = DockStyle.Fill;
            BtnBlankGraph.Location = new Point(3, 3);
            BtnBlankGraph.Name = "BtnBlankGraph";
            BtnBlankGraph.Size = new Size(361, 94);
            BtnBlankGraph.TabIndex = 3;
            BtnBlankGraph.Text = "Blank Graph";
            BtnBlankGraph.UseVisualStyleBackColor = true;
            // 
            // BtnLoadGraph
            // 
            BtnLoadGraph.Dock = DockStyle.Fill;
            BtnLoadGraph.Location = new Point(370, 3);
            BtnLoadGraph.Name = "BtnLoadGraph";
            BtnLoadGraph.Size = new Size(361, 94);
            BtnLoadGraph.TabIndex = 1;
            BtnLoadGraph.Text = "Load Graph";
            BtnLoadGraph.UseVisualStyleBackColor = true;
            // 
            // WelcomeView
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(RightContentSplit);
            Name = "WelcomeView";
            Size = new Size(740, 600);
            RightContentSplit.Panel1.ResumeLayout(false);
            RightContentSplit.Panel1.PerformLayout();
            RightContentSplit.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)RightContentSplit).EndInit();
            RightContentSplit.ResumeLayout(false);
            RecentGroup.ResumeLayout(false);
            PanelRecentGroup.ResumeLayout(false);
            PanelRecentGroup.PerformLayout();
            ButtonGroup.ResumeLayout(false);
            PanelStartGroup.ResumeLayout(false);
            ButtonTable.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private SplitContainer RightContentSplit;
        private Label LabelSub;
        private Label LabelMain;
        private GroupBox RecentGroup;
        private Panel PanelRecentGroup;
        private Panel PanelRecent5;
        private Panel PanelRecent4;
        private Panel PanelRecent3;
        private Panel PanelRecent2;
        private Panel PanelRecent1;
        private GroupBox ButtonGroup;
        private Panel PanelStartGroup;
        private TableLayoutPanel ButtonTable;
        private Button BtnBlankGraph;
        private Button BtnLoadGraph;
    }
}
