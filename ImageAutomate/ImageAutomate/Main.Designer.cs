namespace ImageAutomate
{
    partial class Main
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            dashboardView1 = new DashboardView();
            SuspendLayout();
            // 
            // dashboardView1
            // 
            dashboardView1.Dock = DockStyle.Fill;
            dashboardView1.Location = new Point(0, 0);
            dashboardView1.Name = "dashboardView1";
            dashboardView1.Size = new Size(800, 450);
            dashboardView1.TabIndex = 0;
            // 
            // Main
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(dashboardView1);
            Name = "Main";
            Text = "Main";
            ResumeLayout(false);
        }

        #endregion

        private DashboardView dashboardView1;
    }
}