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
            editorView1 = new ImageAutomate.Views.EditorView();
            SuspendLayout();
            // 
            // editorView1
            // 
            editorView1.Dock = DockStyle.Fill;
            editorView1.Location = new Point(0, 0);
            editorView1.Name = "editorView1";
            editorView1.Size = new Size(984, 561);
            editorView1.TabIndex = 0;
            // 
            // Main
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(984, 561);
            Controls.Add(editorView1);
            Name = "Main";
            Text = "Main";
            ResumeLayout(false);
        }

        #endregion

        private Views.EditorView editorView1;
    }
}