namespace ImageAutomate.Views.DashboardViews
{
    partial class PluginsView
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
            PlaceholderLabel = new Label();
            SuspendLayout();
            // 
            // PlaceholderLabel
            // 
            PlaceholderLabel.Dock = DockStyle.Fill;
            PlaceholderLabel.Location = new Point(0, 0);
            PlaceholderLabel.Name = "PlaceholderLabel";
            PlaceholderLabel.Size = new Size(740, 600);
            PlaceholderLabel.TabIndex = 1;
            PlaceholderLabel.Text = "PluginView";
            PlaceholderLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // PluginsView
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(PlaceholderLabel);
            Name = "PluginsView";
            Size = new Size(740, 600);
            ResumeLayout(false);
        }

        #endregion

        private Label PlaceholderLabel;
    }
}
