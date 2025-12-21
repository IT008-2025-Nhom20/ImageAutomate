namespace ConvertBlockPoC
{
    partial class MainForm
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
            splitContainer1 = new SplitContainer();
            flowLayoutPanel1 = new FlowLayoutPanel();
            butAddSuccessor = new Button();
            butAddPredecessor = new Button();
            butClear = new Button();
            splitContainer2 = new SplitContainer();
            propertyGrid1 = new PropertyGrid();
            graphRenderPanel1 = new GraphRenderPanel();
            butSelectRandom = new Button();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer2).BeginInit();
            splitContainer2.Panel1.SuspendLayout();
            splitContainer2.Panel2.SuspendLayout();
            splitContainer2.SuspendLayout();
            SuspendLayout();
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(0, 0);
            splitContainer1.Name = "splitContainer1";
            splitContainer1.Orientation = Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(flowLayoutPanel1);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(splitContainer2);
            splitContainer1.Size = new Size(800, 450);
            splitContainer1.SplitterDistance = 36;
            splitContainer1.TabIndex = 0;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.Controls.Add(butAddSuccessor);
            flowLayoutPanel1.Controls.Add(butAddPredecessor);
            flowLayoutPanel1.Controls.Add(butSelectRandom);
            flowLayoutPanel1.Controls.Add(butClear);
            flowLayoutPanel1.Dock = DockStyle.Fill;
            flowLayoutPanel1.Location = new Point(0, 0);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(800, 36);
            flowLayoutPanel1.TabIndex = 0;
            // 
            // butAddSuccessor
            // 
            butAddSuccessor.AutoSize = true;
            butAddSuccessor.Location = new Point(3, 3);
            butAddSuccessor.Name = "butAddSuccessor";
            butAddSuccessor.Size = new Size(108, 25);
            butAddSuccessor.TabIndex = 0;
            butAddSuccessor.Text = "Add Successor";
            butAddSuccessor.UseVisualStyleBackColor = true;
            butAddSuccessor.Click += butAddSuccessor_Click;
            // 
            // butAddPredecessor
            // 
            butAddPredecessor.AutoSize = true;
            butAddPredecessor.Location = new Point(117, 3);
            butAddPredecessor.Name = "butAddPredecessor";
            butAddPredecessor.Size = new Size(106, 25);
            butAddPredecessor.TabIndex = 1;
            butAddPredecessor.Text = "Add Predecessor";
            butAddPredecessor.UseVisualStyleBackColor = true;
            butAddPredecessor.Click += butAddPredecessor_Click;
            // 
            // butClear
            // 
            butClear.Location = new Point(331, 3);
            butClear.Name = "butClear";
            butClear.Size = new Size(75, 23);
            butClear.TabIndex = 2;
            butClear.Text = "Clear";
            butClear.UseVisualStyleBackColor = true;
            butClear.Click += butClear_Click;
            // 
            // splitContainer2
            // 
            splitContainer2.Dock = DockStyle.Fill;
            splitContainer2.Location = new Point(0, 0);
            splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            splitContainer2.Panel1.Controls.Add(propertyGrid1);
            // 
            // splitContainer2.Panel2
            // 
            splitContainer2.Panel2.Controls.Add(graphRenderPanel1);
            splitContainer2.Size = new Size(800, 410);
            splitContainer2.SplitterDistance = 234;
            splitContainer2.TabIndex = 0;
            // 
            // propertyGrid1
            // 
            propertyGrid1.BackColor = SystemColors.Control;
            propertyGrid1.Dock = DockStyle.Fill;
            propertyGrid1.Location = new Point(0, 0);
            propertyGrid1.Name = "propertyGrid1";
            propertyGrid1.Size = new Size(234, 410);
            propertyGrid1.TabIndex = 0;
            // 
            // graphRenderPanel1
            // 
            graphRenderPanel1.BackColor = Color.White;
            graphRenderPanel1.ColumnSpacing = 250D;
            graphRenderPanel1.Dock = DockStyle.Fill;
            graphRenderPanel1.Location = new Point(0, 0);
            graphRenderPanel1.Name = "graphRenderPanel1";
            graphRenderPanel1.NodeSpacing = 30D;
            graphRenderPanel1.RenderScale = 1F;
            graphRenderPanel1.SelectedBlockOutlineColor = Color.Red;
            graphRenderPanel1.Size = new Size(562, 410);
            graphRenderPanel1.SocketRadius = 6D;
            graphRenderPanel1.TabIndex = 0;
            // 
            // butSelectRandom
            // 
            butSelectRandom.AutoSize = true;
            butSelectRandom.Location = new Point(229, 3);
            butSelectRandom.Name = "butSelectRandom";
            butSelectRandom.Size = new Size(96, 25);
            butSelectRandom.TabIndex = 3;
            butSelectRandom.Text = "Select Random";
            butSelectRandom.UseVisualStyleBackColor = true;
            butSelectRandom.Click += butSelectRandom_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(splitContainer1);
            Name = "MainForm";
            Text = "MainForm";
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            flowLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel1.PerformLayout();
            splitContainer2.Panel1.ResumeLayout(false);
            splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer2).EndInit();
            splitContainer2.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private SplitContainer splitContainer1;
        private FlowLayoutPanel flowLayoutPanel1;
        private Button butAddSuccessor;
        private Button butAddPredecessor;
        private Button butClear;
        private SplitContainer splitContainer2;
        private PropertyGrid propertyGrid1;
        private GraphRenderPanel graphRenderPanel1;
        private Button butSelectRandom;
    }
}