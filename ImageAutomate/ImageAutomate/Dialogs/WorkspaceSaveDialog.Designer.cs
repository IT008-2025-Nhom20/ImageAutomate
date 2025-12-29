namespace ImageAutomate.Dialogs
{
    partial class WorkspaceSaveDialog
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
            tableLayoutPanel = new TableLayoutPanel();
            labelName = new Label();
            textBoxName = new TextBox();
            labelFilePath = new Label();
            panelFilePathSelection = new Panel();
            textBoxFilePath = new TextBox();
            buttonBrowseFile = new Button();
            labelImage = new Label();
            panelImageSelection = new Panel();
            textBoxImagePath = new TextBox();
            buttonBrowseImage = new Button();
            pictureBoxPreview = new PictureBox();
            panelButtons = new Panel();
            buttonCancel = new Button();
            buttonOK = new Button();
            tableLayoutPanel.SuspendLayout();
            panelFilePathSelection.SuspendLayout();
            panelImageSelection.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxPreview).BeginInit();
            panelButtons.SuspendLayout();
            SuspendLayout();
            // 
            // tableLayoutPanel
            // 
            tableLayoutPanel.ColumnCount = 2;
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel.Controls.Add(labelName, 0, 0);
            tableLayoutPanel.Controls.Add(textBoxName, 1, 0);
            tableLayoutPanel.Controls.Add(labelFilePath, 0, 1);
            tableLayoutPanel.Controls.Add(panelFilePathSelection, 1, 1);
            tableLayoutPanel.Controls.Add(labelImage, 0, 2);
            tableLayoutPanel.Controls.Add(panelImageSelection, 1, 2);
            tableLayoutPanel.Controls.Add(pictureBoxPreview, 1, 3);
            tableLayoutPanel.Controls.Add(panelButtons, 1, 4);
            tableLayoutPanel.Dock = DockStyle.Fill;
            tableLayoutPanel.Location = new Point(10, 10);
            tableLayoutPanel.Name = "tableLayoutPanel";
            tableLayoutPanel.RowCount = 5;
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 35F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 35F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 35F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 45F));
            tableLayoutPanel.Size = new Size(464, 341);
            tableLayoutPanel.TabIndex = 0;
            // 
            // labelName
            // 
            labelName.Anchor = AnchorStyles.Left;
            labelName.AutoSize = true;
            labelName.Location = new Point(3, 10);
            labelName.Name = "labelName";
            labelName.Size = new Size(42, 15);
            labelName.TabIndex = 0;
            labelName.Text = "Name:";
            // 
            // textBoxName
            // 
            textBoxName.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            textBoxName.Location = new Point(103, 5);
            textBoxName.Name = "textBoxName";
            textBoxName.Size = new Size(358, 23);
            textBoxName.TabIndex = 1;
            // 
            // labelFilePath
            // 
            labelFilePath.Anchor = AnchorStyles.Left;
            labelFilePath.AutoSize = true;
            labelFilePath.Location = new Point(3, 45);
            labelFilePath.Name = "labelFilePath";
            labelFilePath.Size = new Size(55, 15);
            labelFilePath.TabIndex = 2;
            labelFilePath.Text = "Save As:";
            // 
            // panelFilePathSelection
            // 
            panelFilePathSelection.Controls.Add(textBoxFilePath);
            panelFilePathSelection.Controls.Add(buttonBrowseFile);
            panelFilePathSelection.Dock = DockStyle.Fill;
            panelFilePathSelection.Location = new Point(103, 38);
            panelFilePathSelection.Name = "panelFilePathSelection";
            panelFilePathSelection.Size = new Size(358, 29);
            panelFilePathSelection.TabIndex = 3;
            // 
            // textBoxFilePath
            // 
            textBoxFilePath.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            textBoxFilePath.Location = new Point(0, 3);
            textBoxFilePath.Name = "textBoxFilePath";
            textBoxFilePath.Size = new Size(278, 23);
            textBoxFilePath.TabIndex = 0;
            // 
            // buttonBrowseFile
            // 
            buttonBrowseFile.Anchor = AnchorStyles.Right;
            buttonBrowseFile.Location = new Point(284, 2);
            buttonBrowseFile.Name = "buttonBrowseFile";
            buttonBrowseFile.Size = new Size(74, 25);
            buttonBrowseFile.TabIndex = 1;
            buttonBrowseFile.Text = "Browse...";
            buttonBrowseFile.UseVisualStyleBackColor = true;
            // 
            // labelImage
            // 
            labelImage.Anchor = AnchorStyles.Left;
            labelImage.AutoSize = true;
            labelImage.Location = new Point(3, 80);
            labelImage.Name = "labelImage";
            labelImage.Size = new Size(43, 15);
            labelImage.TabIndex = 4;
            labelImage.Text = "Image:";
            // 
            // panelImageSelection
            // 
            panelImageSelection.Controls.Add(textBoxImagePath);
            panelImageSelection.Controls.Add(buttonBrowseImage);
            panelImageSelection.Dock = DockStyle.Fill;
            panelImageSelection.Location = new Point(103, 73);
            panelImageSelection.Name = "panelImageSelection";
            panelImageSelection.Size = new Size(358, 29);
            panelImageSelection.TabIndex = 5;
            // 
            // textBoxImagePath
            // 
            textBoxImagePath.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            textBoxImagePath.Location = new Point(0, 3);
            textBoxImagePath.Name = "textBoxImagePath";
            textBoxImagePath.Size = new Size(278, 23);
            textBoxImagePath.TabIndex = 0;
            // 
            // buttonBrowseImage
            // 
            buttonBrowseImage.Anchor = AnchorStyles.Right;
            buttonBrowseImage.Location = new Point(284, 2);
            buttonBrowseImage.Name = "buttonBrowseImage";
            buttonBrowseImage.Size = new Size(74, 25);
            buttonBrowseImage.TabIndex = 1;
            buttonBrowseImage.Text = "Browse...";
            buttonBrowseImage.UseVisualStyleBackColor = true;
            // 
            // pictureBoxPreview
            // 
            pictureBoxPreview.BackColor = Color.LightGray;
            pictureBoxPreview.BorderStyle = BorderStyle.FixedSingle;
            pictureBoxPreview.Dock = DockStyle.Fill;
            pictureBoxPreview.Location = new Point(103, 108);
            pictureBoxPreview.Name = "pictureBoxPreview";
            pictureBoxPreview.Size = new Size(358, 185);
            pictureBoxPreview.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBoxPreview.TabIndex = 6;
            pictureBoxPreview.TabStop = false;
            // 
            // panelButtons
            // 
            panelButtons.Controls.Add(buttonCancel);
            panelButtons.Controls.Add(buttonOK);
            panelButtons.Dock = DockStyle.Fill;
            panelButtons.Location = new Point(103, 299);
            panelButtons.Name = "panelButtons";
            panelButtons.Size = new Size(358, 39);
            panelButtons.TabIndex = 7;
            // 
            // buttonCancel
            // 
            buttonCancel.Anchor = AnchorStyles.Right;
            buttonCancel.DialogResult = DialogResult.Cancel;
            buttonCancel.Location = new Point(280, 6);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(75, 27);
            buttonCancel.TabIndex = 1;
            buttonCancel.Text = "Cancel";
            buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonOK
            // 
            buttonOK.Anchor = AnchorStyles.Right;
            buttonOK.Location = new Point(199, 6);
            buttonOK.Name = "buttonOK";
            buttonOK.Size = new Size(75, 27);
            buttonOK.TabIndex = 0;
            buttonOK.Text = "Save";
            buttonOK.UseVisualStyleBackColor = true;
            // 
            // WorkspaceSaveDialog
            // 
            AcceptButton = buttonOK;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = buttonCancel;
            ClientSize = new Size(484, 361);
            Controls.Add(tableLayoutPanel);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "WorkspaceSaveDialog";
            Padding = new Padding(10);
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Save Workspace";
            tableLayoutPanel.ResumeLayout(false);
            tableLayoutPanel.PerformLayout();
            panelFilePathSelection.ResumeLayout(false);
            panelFilePathSelection.PerformLayout();
            panelImageSelection.ResumeLayout(false);
            panelImageSelection.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxPreview).EndInit();
            panelButtons.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel tableLayoutPanel;
        private Label labelName;
        private TextBox textBoxName;
        private Label labelFilePath;
        private Panel panelFilePathSelection;
        private TextBox textBoxFilePath;
        private Button buttonBrowseFile;
        private Label labelImage;
        private Panel panelImageSelection;
        private TextBox textBoxImagePath;
        private Button buttonBrowseImage;
        private PictureBox pictureBoxPreview;
        private Panel panelButtons;
        private Button buttonCancel;
        private Button buttonOK;
    }
}
