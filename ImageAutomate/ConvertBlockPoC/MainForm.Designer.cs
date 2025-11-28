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
            ListViewItem listViewItem1 = new ListViewItem("");
            ListViewItem listViewItem2 = new ListViewItem("");
            ListViewItem listViewItem3 = new ListViewItem("");
            ListViewItem listViewItem4 = new ListViewItem("");
            splitContainer1 = new SplitContainer();
            flowLayoutPanel1 = new FlowLayoutPanel();
            butAddPredecessor = new Button();
            butAddSuccessor = new Button();
            butSelectRandom = new Button();
            butClear = new Button();
            splitContainer2 = new SplitContainer();
            properties = new PropertyGrid();
            canvas = new GraphRenderPanel();
            listView1 = new ListView();
            columnHeader1 = new ColumnHeader();
            columnHeader2 = new ColumnHeader();
            columnHeader3 = new ColumnHeader();
            columnHeader4 = new ColumnHeader();
            columnHeader5 = new ColumnHeader();
            butZoomIn = new Button();
            butZoomOut = new Button();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer2).BeginInit();
            splitContainer2.Panel1.SuspendLayout();
            splitContainer2.Panel2.SuspendLayout();
            splitContainer2.SuspendLayout();
            canvas.SuspendLayout();
            SuspendLayout();
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.IsSplitterFixed = true;
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
            splitContainer1.Size = new Size(663, 348);
            splitContainer1.SplitterDistance = 32;
            splitContainer1.TabIndex = 0;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.Controls.Add(butAddPredecessor);
            flowLayoutPanel1.Controls.Add(butAddSuccessor);
            flowLayoutPanel1.Controls.Add(butSelectRandom);
            flowLayoutPanel1.Controls.Add(butClear);
            flowLayoutPanel1.Dock = DockStyle.Fill;
            flowLayoutPanel1.Location = new Point(0, 0);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(663, 32);
            flowLayoutPanel1.TabIndex = 0;
            // 
            // butAddPredecessor
            // 
            butAddPredecessor.AutoSize = true;
            butAddPredecessor.Location = new Point(3, 3);
            butAddPredecessor.Name = "butAddPredecessor";
            butAddPredecessor.Size = new Size(106, 25);
            butAddPredecessor.TabIndex = 0;
            butAddPredecessor.Text = "Add Predecessor";
            butAddPredecessor.UseVisualStyleBackColor = true;
            butAddPredecessor.Click += butAddPredecessor_Click;
            // 
            // butAddSuccessor
            // 
            butAddSuccessor.AutoSize = true;
            butAddSuccessor.Location = new Point(115, 3);
            butAddSuccessor.Name = "butAddSuccessor";
            butAddSuccessor.Size = new Size(94, 25);
            butAddSuccessor.TabIndex = 1;
            butAddSuccessor.Text = "Add Successor";
            butAddSuccessor.UseVisualStyleBackColor = true;
            butAddSuccessor.Click += butAddSuccessor_Click;
            // 
            // butSelectRandom
            // 
            butSelectRandom.AutoSize = true;
            butSelectRandom.Location = new Point(215, 3);
            butSelectRandom.Name = "butSelectRandom";
            butSelectRandom.Size = new Size(96, 25);
            butSelectRandom.TabIndex = 2;
            butSelectRandom.Text = "Select Random";
            butSelectRandom.UseVisualStyleBackColor = true;
            butSelectRandom.Click += butSelectRandom_Click;
            // 
            // butClear
            // 
            butClear.Location = new Point(317, 3);
            butClear.Name = "butClear";
            butClear.Size = new Size(75, 23);
            butClear.TabIndex = 3;
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
            splitContainer2.Panel1.Controls.Add(properties);
            // 
            // splitContainer2.Panel2
            // 
            splitContainer2.Panel2.Controls.Add(canvas);
            splitContainer2.Size = new Size(663, 312);
            splitContainer2.SplitterDistance = 220;
            splitContainer2.TabIndex = 0;
            // 
            // properties
            // 
            properties.BackColor = SystemColors.Control;
            properties.Dock = DockStyle.Fill;
            properties.Location = new Point(0, 0);
            properties.Name = "properties";
            properties.Size = new Size(220, 312);
            properties.TabIndex = 0;
            // 
            // canvas
            // 
            canvas.BackColor = Color.White;
            canvas.ColumnSpacing = 250D;
            canvas.Controls.Add(listView1);
            canvas.Controls.Add(butZoomIn);
            canvas.Controls.Add(butZoomOut);
            canvas.Dock = DockStyle.Fill;
            canvas.Location = new Point(0, 0);
            canvas.Name = "canvas";
            canvas.NodeHeight = 100D;
            canvas.NodeWidth = 200D;
            canvas.RenderScale = 1F;
            canvas.SelectedBlockOutlineColor = Color.Red;
            canvas.Size = new Size(439, 312);
            canvas.SocketRadius = 6D;
            canvas.TabIndex = 0;
            // 
            // listView1
            // 
            listView1.Columns.AddRange(new ColumnHeader[] { columnHeader1, columnHeader2, columnHeader3, columnHeader4, columnHeader5 });
            listView1.Items.AddRange(new ListViewItem[] { listViewItem1, listViewItem2, listViewItem3, listViewItem4 });
            listView1.Location = new Point(3, 2);
            listView1.Name = "listView1";
            listView1.Size = new Size(84, 97);
            listView1.TabIndex = 2;
            listView1.UseCompatibleStateImageBehavior = false;
            // 
            // butZoomIn
            // 
            butZoomIn.AutoSize = true;
            butZoomIn.Location = new Point(352, 3);
            butZoomIn.Name = "butZoomIn";
            butZoomIn.Size = new Size(25, 25);
            butZoomIn.TabIndex = 1;
            butZoomIn.Text = "+";
            butZoomIn.UseVisualStyleBackColor = true;
            // 
            // butZoomOut
            // 
            butZoomOut.AutoSize = true;
            butZoomOut.Location = new Point(289, 3);
            butZoomOut.Name = "butZoomOut";
            butZoomOut.Size = new Size(22, 25);
            butZoomOut.TabIndex = 0;
            butZoomOut.Text = "-";
            butZoomOut.UseVisualStyleBackColor = true;
            butZoomOut.Click += butZoomOut_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(663, 348);
            Controls.Add(splitContainer1);
            Name = "MainForm";
            Text = "MainForm";
            Load += Init;
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
            canvas.ResumeLayout(false);
            canvas.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private SplitContainer splitContainer1;
        private FlowLayoutPanel flowLayoutPanel1;
        private SplitContainer splitContainer2;
        private PropertyGrid properties;
        private Button butAddPredecessor;
        private Button butAddSuccessor;
        private Button butSelectRandom;
        private GraphRenderPanel canvas;
        private Button butClear;
        private Button butZoomIn;
        private Button butZoomOut;
        private ListView listView1;
        private ColumnHeader columnHeader1;
        private ColumnHeader columnHeader2;
        private ColumnHeader columnHeader3;
        private ColumnHeader columnHeader4;
        private ColumnHeader columnHeader5;
    }
}