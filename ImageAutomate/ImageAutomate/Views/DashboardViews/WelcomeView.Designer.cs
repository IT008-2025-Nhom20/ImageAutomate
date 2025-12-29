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
            ButtonGroupBox = new GroupBox();
            ButtonPanel = new Panel();
            ButtonTable = new TableLayoutPanel();
            BlankGraphButton = new Button();
            LoadGraphButton = new Button();
            RecentGroupBox = new GroupBox();
            RecentPanel = new Panel();
            TitleLabel = new Label();
            SubtitleLabel = new Label();
            MainContentSplit = new SplitContainer();
            ButtonGroupBox.SuspendLayout();
            ButtonPanel.SuspendLayout();
            ButtonTable.SuspendLayout();
            RecentGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)MainContentSplit).BeginInit();
            MainContentSplit.Panel1.SuspendLayout();
            MainContentSplit.Panel2.SuspendLayout();
            MainContentSplit.SuspendLayout();
            SuspendLayout();
            // 
            // ButtonGroupBox
            // 
            ButtonGroupBox.Controls.Add(ButtonPanel);
            ButtonGroupBox.Dock = DockStyle.Top;
            ButtonGroupBox.Location = new Point(0, 0);
            ButtonGroupBox.Name = "ButtonGroupBox";
            ButtonGroupBox.Size = new Size(740, 129);
            ButtonGroupBox.TabIndex = 4;
            ButtonGroupBox.TabStop = false;
            ButtonGroupBox.Text = "Start";
            // 
            // ButtonPanel
            // 
            ButtonPanel.AutoScroll = true;
            ButtonPanel.Controls.Add(ButtonTable);
            ButtonPanel.Dock = DockStyle.Fill;
            ButtonPanel.Location = new Point(3, 19);
            ButtonPanel.Name = "ButtonPanel";
            ButtonPanel.Size = new Size(734, 107);
            ButtonPanel.TabIndex = 2;
            // 
            // ButtonTable
            // 
            ButtonTable.ColumnCount = 2;
            ButtonTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            ButtonTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            ButtonTable.Controls.Add(BlankGraphButton, 0, 0);
            ButtonTable.Controls.Add(LoadGraphButton, 1, 0);
            ButtonTable.Dock = DockStyle.Top;
            ButtonTable.Location = new Point(0, 0);
            ButtonTable.Name = "ButtonTable";
            ButtonTable.RowCount = 1;
            ButtonTable.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            ButtonTable.Size = new Size(734, 100);
            ButtonTable.TabIndex = 3;
            // 
            // BlankGraphButton
            // 
            BlankGraphButton.Dock = DockStyle.Fill;
            BlankGraphButton.Location = new Point(3, 3);
            BlankGraphButton.Name = "BlankGraphButton";
            BlankGraphButton.Size = new Size(361, 94);
            BlankGraphButton.TabIndex = 3;
            BlankGraphButton.Text = "Blank Graph";
            BlankGraphButton.UseVisualStyleBackColor = true;
            // 
            // LoadGraphButton
            // 
            LoadGraphButton.Dock = DockStyle.Fill;
            LoadGraphButton.Location = new Point(370, 3);
            LoadGraphButton.Name = "LoadGraphButton";
            LoadGraphButton.Size = new Size(361, 94);
            LoadGraphButton.TabIndex = 1;
            LoadGraphButton.Text = "Load Graph";
            LoadGraphButton.UseVisualStyleBackColor = true;
            // 
            // RecentGroupBox
            // 
            RecentGroupBox.Controls.Add(RecentPanel);
            RecentGroupBox.Dock = DockStyle.Fill;
            RecentGroupBox.Location = new Point(0, 129);
            RecentGroupBox.Name = "RecentGroupBox";
            RecentGroupBox.Size = new Size(740, 395);
            RecentGroupBox.TabIndex = 5;
            RecentGroupBox.TabStop = false;
            RecentGroupBox.Text = "Recent Workspaces";
            // 
            // RecentPanel
            // 
            RecentPanel.AutoScroll = true;
            RecentPanel.AutoSize = true;
            RecentPanel.Dock = DockStyle.Fill;
            RecentPanel.Location = new Point(3, 19);
            RecentPanel.Name = "RecentPanel";
            RecentPanel.Size = new Size(734, 373);
            RecentPanel.TabIndex = 2;
            // 
            // TitleLabel
            // 
            TitleLabel.Dock = DockStyle.Top;
            TitleLabel.Font = new Font("Segoe UI", 18F, FontStyle.Bold, GraphicsUnit.Point, 0);
            TitleLabel.Location = new Point(0, 0);
            TitleLabel.Name = "TitleLabel";
            TitleLabel.Size = new Size(740, 37);
            TitleLabel.TabIndex = 0;
            TitleLabel.Text = "ImageAutomate";
            // 
            // SubtitleLabel
            // 
            SubtitleLabel.AutoSize = true;
            SubtitleLabel.Dock = DockStyle.Top;
            SubtitleLabel.Location = new Point(0, 37);
            SubtitleLabel.Name = "SubtitleLabel";
            SubtitleLabel.Padding = new Padding(4, 0, 0, 0);
            SubtitleLabel.Size = new Size(200, 15);
            SubtitleLabel.TabIndex = 1;
            SubtitleLabel.Text = "Visual Image Processing Flow Editor";
            // 
            // MainContentSplit
            // 
            MainContentSplit.Dock = DockStyle.Fill;
            MainContentSplit.FixedPanel = FixedPanel.Panel1;
            MainContentSplit.IsSplitterFixed = true;
            MainContentSplit.Location = new Point(0, 0);
            MainContentSplit.Name = "MainContentSplit";
            MainContentSplit.Orientation = Orientation.Horizontal;
            // 
            // MainContentSplit.Panel1
            // 
            MainContentSplit.Panel1.Controls.Add(SubtitleLabel);
            MainContentSplit.Panel1.Controls.Add(TitleLabel);
            // 
            // MainContentSplit.Panel2
            // 
            MainContentSplit.Panel2.Controls.Add(RecentGroupBox);
            MainContentSplit.Panel2.Controls.Add(ButtonGroupBox);
            MainContentSplit.Size = new Size(740, 600);
            MainContentSplit.SplitterDistance = 72;
            MainContentSplit.TabIndex = 1;
            // 
            // WelcomeView
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(MainContentSplit);
            Name = "WelcomeView";
            Size = new Size(740, 600);
            ButtonGroupBox.ResumeLayout(false);
            ButtonPanel.ResumeLayout(false);
            ButtonTable.ResumeLayout(false);
            RecentGroupBox.ResumeLayout(false);
            RecentGroupBox.PerformLayout();
            MainContentSplit.Panel1.ResumeLayout(false);
            MainContentSplit.Panel1.PerformLayout();
            MainContentSplit.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)MainContentSplit).EndInit();
            MainContentSplit.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private GroupBox ButtonGroupBox;
        private Panel ButtonPanel;
        private TableLayoutPanel ButtonTable;
        private Button BlankGraphButton;
        private Button LoadGraphButton;
        private GroupBox RecentGroupBox;
        private Panel RecentPanel;
        private Label TitleLabel;
        private Label SubtitleLabel;
        private SplitContainer MainContentSplit;
    }
}
