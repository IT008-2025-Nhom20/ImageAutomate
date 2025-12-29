namespace ImageAutomate.Views.DashboardViews
{
    partial class WorkspaceView
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
            HeaderLabel = new Label();
            SearchTextBox = new TextBox();
            NewButton = new Button();
            BrowseButton = new Button();
            ImportButton = new Button();
            WorkspacesPanel = new FlowLayoutPanel();
            EmptyLabel = new Label();
            SuspendLayout();
            // 
            // HeaderLabel
            // 
            HeaderLabel.AutoSize = true;
            HeaderLabel.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            HeaderLabel.Location = new Point(20, 20);
            HeaderLabel.Name = "HeaderLabel";
            HeaderLabel.Size = new Size(193, 32);
            HeaderLabel.TabIndex = 0;
            HeaderLabel.Text = "My Workspaces";
            // 
            // SearchTextBox
            // 
            SearchTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            SearchTextBox.Font = new Font("Segoe UI", 10F);
            SearchTextBox.Location = new Point(20, 65);
            SearchTextBox.Name = "SearchTextBox";
            SearchTextBox.PlaceholderText = "Search workspaces...";
            SearchTextBox.Size = new Size(330, 25);
            SearchTextBox.TabIndex = 1;
            // 
            // NewButton
            // 
            NewButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            NewButton.Font = new Font("Segoe UI", 9F);
            NewButton.Location = new Point(370, 65);
            NewButton.Name = "NewButton";
            NewButton.Size = new Size(110, 25);
            NewButton.TabIndex = 2;
            NewButton.Text = "Create New";
            NewButton.UseVisualStyleBackColor = true;
            // 
            // BrowseButton
            // 
            BrowseButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            BrowseButton.Font = new Font("Segoe UI", 9F);
            BrowseButton.Location = new Point(490, 65);
            BrowseButton.Name = "BrowseButton";
            BrowseButton.Size = new Size(110, 25);
            BrowseButton.TabIndex = 3;
            BrowseButton.Text = "Browse...";
            BrowseButton.UseVisualStyleBackColor = true;
            // 
            // ImportButton
            // 
            ImportButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            ImportButton.Font = new Font("Segoe UI", 9F);
            ImportButton.Location = new Point(610, 65);
            ImportButton.Name = "ImportButton";
            ImportButton.Size = new Size(110, 25);
            ImportButton.TabIndex = 4;
            ImportButton.Text = "Import...";
            ImportButton.UseVisualStyleBackColor = true;
            // 
            // WorkspacesPanel
            // 
            WorkspacesPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            WorkspacesPanel.AutoScroll = true;
            WorkspacesPanel.Location = new Point(20, 105);
            WorkspacesPanel.Name = "WorkspacesPanel";
            WorkspacesPanel.Padding = new Padding(5);
            WorkspacesPanel.Size = new Size(700, 475);
            WorkspacesPanel.TabIndex = 5;
            // 
            // EmptyLabel
            // 
            EmptyLabel.Anchor = AnchorStyles.None;
            EmptyLabel.AutoSize = true;
            EmptyLabel.Font = new Font("Segoe UI", 11F);
            EmptyLabel.ForeColor = SystemColors.GrayText;
            EmptyLabel.Location = new Point(245, 300);
            EmptyLabel.Name = "EmptyLabel";
            EmptyLabel.Size = new Size(278, 20);
            EmptyLabel.TabIndex = 6;
            EmptyLabel.Text = "No workspaces found. Create a new one!";
            EmptyLabel.Visible = false;
            // 
            // WorkspaceView
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.Control;
            Controls.Add(EmptyLabel);
            Controls.Add(WorkspacesPanel);
            Controls.Add(ImportButton);
            Controls.Add(BrowseButton);
            Controls.Add(NewButton);
            Controls.Add(SearchTextBox);
            Controls.Add(HeaderLabel);
            Name = "WorkspaceView";
            Size = new Size(740, 600);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label HeaderLabel;
        private TextBox SearchTextBox;
        private Button NewButton;
        private Button BrowseButton;
        private Button ImportButton;
        private FlowLayoutPanel WorkspacesPanel;
        private Label EmptyLabel;
    }
}
