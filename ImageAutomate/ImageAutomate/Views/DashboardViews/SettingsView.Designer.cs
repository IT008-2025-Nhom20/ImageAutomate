namespace ImageAutomate.Views.DashboardViews
{
    partial class SettingsView
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
            MainScrollPanel = new Panel();
            ThemeGroupBox = new GroupBox();
            ThemeTable = new TableLayoutPanel();
            NodeWidthLabel = new Label();
            NodeWidthValue = new NoScrollNumericUpDown();
            NodeBorderWidthLabel = new Label();
            NodeBorderWidthValue = new NoScrollNumericUpDown();
            NodeSpacingLabel = new Label();
            NodeSpacingValue = new NoScrollNumericUpDown();
            TextColorLabel = new Label();
            TextColorColorButton = new ImageAutomate.UI.ColorDialogButton();
            DisabledNodeColorLabel = new Label();
            DisabledNodeColorColorButton = new ImageAutomate.UI.ColorDialogButton();
            SuccessColorLabel = new Label();
            SuccessColorColorButton = new ImageAutomate.UI.ColorDialogButton();
            ErrorColorLabel = new Label();
            ErrorColorColorButton = new ImageAutomate.UI.ColorDialogButton();
            HoveredNodeColorLabel = new Label();
            HoveredNodeColorColorButton = new ImageAutomate.UI.ColorDialogButton();
            DefaultNodeColorLabel = new Label();
            DefaultNodeColorColorButton = new ImageAutomate.UI.ColorDialogButton();
            SelectedBlockOutlineLabel = new Label();
            SelectedBlockOutlineColorButton2 = new ImageAutomate.UI.ColorDialogButton();
            HoveredBlockOutlineLabel = new Label();
            HoveredBlockOutlineColorButton = new ImageAutomate.UI.ColorDialogButton();
            BorderOutlineLabel = new Label();
            BorderOutlineColorButton = new ImageAutomate.UI.ColorDialogButton();
            SocketConnectionColorLabel = new Label();
            SocketConnectionColorColorButton = new ImageAutomate.UI.ColorDialogButton();
            EditorGroupBox = new GroupBox();
            EditorTable = new TableLayoutPanel();
            AutoSnapZoneWidthLabel = new Label();
            AutoSnapZoneWidthValue = new NoScrollNumericUpDown();
            RenderScaleLabel = new Label();
            RenderScaleValue = new NoScrollNumericUpDown();
            SocketRadiusLabel = new Label();
            SocketRadiusValue = new NoScrollNumericUpDown();
            AllowOutOfScreenPanLabel = new Label();
            AllowOutOfScreenPanValue = new CheckBox();
            SelectedBlockOutlineColorLabel = new Label();
            SelectedBlockOutlineColorButton = new ImageAutomate.UI.ColorDialogButton();
            ExecutionGroupBox = new GroupBox();
            ExecutionTable = new TableLayoutPanel();
            WatchdogTimeoutLabel = new Label();
            WatchdogTimeoutValue = new NoScrollNumericUpDown();
            MaxShipmentSizeLabel = new Label();
            MaxShipmentSizeValue = new NoScrollNumericUpDown();
            ProfilingWindowSizeLabel = new Label();
            ProfilingWindowSizeValue = new NoScrollNumericUpDown();
            CostEmaAlphaLabel = new Label();
            CostEmaAlphaValue = new NoScrollNumericUpDown();
            CriticalPathRecomputeIntervalLabel = new Label();
            CriticalPathRecomputeIntervalValue = new NoScrollNumericUpDown();
            BatchSizeLabel = new Label();
            BatchSizeValue = new NoScrollNumericUpDown();
            CriticalPathBoostLabel = new Label();
            CriticalPathBoostValue = new NoScrollNumericUpDown();
            ExecutionModeLabel = new Label();
            ExecutionModeComboBox = new NoScrollComboBox();
            MaxDegreeOfParallelismLabel = new Label();
            MaxDegreeOfParallelismValue = new NoScrollNumericUpDown();
            EnableGcThrottlingLabel = new Label();
            EnableGcThrottlingValue = new CheckBox();
            ApplicationGroupBox = new GroupBox();
            ApplicationTable = new TableLayoutPanel();
            MaxRecentWorkspacesLabel = new Label();
            MaxRecentWorkspacesValue = new NoScrollNumericUpDown();
            BtnSave = new Button();
            BtnReload = new Button();
            BtnSetToDefault = new Button();
            BottomPanel = new Panel();
            MainScrollPanel.SuspendLayout();
            ThemeGroupBox.SuspendLayout();
            ThemeTable.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)NodeWidthValue).BeginInit();
            ((System.ComponentModel.ISupportInitialize)NodeBorderWidthValue).BeginInit();
            ((System.ComponentModel.ISupportInitialize)NodeSpacingValue).BeginInit();
            EditorGroupBox.SuspendLayout();
            EditorTable.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)AutoSnapZoneWidthValue).BeginInit();
            ((System.ComponentModel.ISupportInitialize)RenderScaleValue).BeginInit();
            ((System.ComponentModel.ISupportInitialize)SocketRadiusValue).BeginInit();
            ExecutionGroupBox.SuspendLayout();
            ExecutionTable.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)WatchdogTimeoutValue).BeginInit();
            ((System.ComponentModel.ISupportInitialize)MaxShipmentSizeValue).BeginInit();
            ((System.ComponentModel.ISupportInitialize)ProfilingWindowSizeValue).BeginInit();
            ((System.ComponentModel.ISupportInitialize)CostEmaAlphaValue).BeginInit();
            ((System.ComponentModel.ISupportInitialize)CriticalPathRecomputeIntervalValue).BeginInit();
            ((System.ComponentModel.ISupportInitialize)BatchSizeValue).BeginInit();
            ((System.ComponentModel.ISupportInitialize)CriticalPathBoostValue).BeginInit();
            ((System.ComponentModel.ISupportInitialize)MaxDegreeOfParallelismValue).BeginInit();
            ApplicationGroupBox.SuspendLayout();
            ApplicationTable.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)MaxRecentWorkspacesValue).BeginInit();
            BottomPanel.SuspendLayout();
            SuspendLayout();
            // 
            // MainScrollPanel
            // 
            MainScrollPanel.AutoScroll = true;
            MainScrollPanel.Controls.Add(ThemeGroupBox);
            MainScrollPanel.Controls.Add(EditorGroupBox);
            MainScrollPanel.Controls.Add(ExecutionGroupBox);
            MainScrollPanel.Controls.Add(ApplicationGroupBox);
            MainScrollPanel.Dock = DockStyle.Fill;
            MainScrollPanel.Location = new Point(0, 0);
            MainScrollPanel.Name = "MainScrollPanel";
            MainScrollPanel.Padding = new Padding(0, 0, 0, 50);
            MainScrollPanel.Size = new Size(740, 550);
            MainScrollPanel.TabIndex = 2;
            // 
            // ThemeGroupBox
            // 
            ThemeGroupBox.AutoSize = true;
            ThemeGroupBox.Controls.Add(ThemeTable);
            ThemeGroupBox.Dock = DockStyle.Top;
            ThemeGroupBox.Location = new Point(0, 546);
            ThemeGroupBox.Name = "ThemeGroupBox";
            ThemeGroupBox.Size = new Size(723, 412);
            ThemeGroupBox.TabIndex = 3;
            ThemeGroupBox.TabStop = false;
            ThemeGroupBox.Text = "Theme";
            // 
            // ThemeTable
            // 
            ThemeTable.AutoSize = true;
            ThemeTable.ColumnCount = 2;
            ThemeTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200F));
            ThemeTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            ThemeTable.Controls.Add(NodeWidthLabel, 0, 10);
            ThemeTable.Controls.Add(NodeWidthValue, 1, 10);
            ThemeTable.Controls.Add(NodeBorderWidthLabel, 0, 9);
            ThemeTable.Controls.Add(NodeBorderWidthValue, 1, 9);
            ThemeTable.Controls.Add(NodeSpacingLabel, 0, 8);
            ThemeTable.Controls.Add(NodeSpacingValue, 1, 8);
            ThemeTable.Controls.Add(TextColorLabel, 0, 7);
            ThemeTable.Controls.Add(TextColorColorButton, 1, 7);
            ThemeTable.Controls.Add(DisabledNodeColorLabel, 0, 6);
            ThemeTable.Controls.Add(DisabledNodeColorColorButton, 1, 6);
            ThemeTable.Controls.Add(SuccessColorLabel, 0, 5);
            ThemeTable.Controls.Add(SuccessColorColorButton, 1, 5);
            ThemeTable.Controls.Add(ErrorColorLabel, 0, 4);
            ThemeTable.Controls.Add(ErrorColorColorButton, 1, 4);
            ThemeTable.Controls.Add(HoveredNodeColorLabel, 0, 3);
            ThemeTable.Controls.Add(HoveredNodeColorColorButton, 1, 3);
            ThemeTable.Controls.Add(DefaultNodeColorLabel, 0, 2);
            ThemeTable.Controls.Add(DefaultNodeColorColorButton, 1, 2);
            ThemeTable.Controls.Add(SelectedBlockOutlineLabel, 0, 1);
            ThemeTable.Controls.Add(SelectedBlockOutlineColorButton2, 1, 1);
            ThemeTable.Controls.Add(HoveredBlockOutlineLabel, 0, 0);
            ThemeTable.Controls.Add(HoveredBlockOutlineColorButton, 1, 0);
            ThemeTable.Controls.Add(BorderOutlineLabel, 0, 11);
            ThemeTable.Controls.Add(BorderOutlineColorButton, 1, 11);
            ThemeTable.Controls.Add(SocketConnectionColorLabel, 0, 12);
            ThemeTable.Controls.Add(SocketConnectionColorColorButton, 1, 12);
            ThemeTable.Dock = DockStyle.Fill;
            ThemeTable.Location = new Point(3, 19);
            ThemeTable.Name = "ThemeTable";
            ThemeTable.RowCount = 13;
            ThemeTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            ThemeTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            ThemeTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            ThemeTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            ThemeTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            ThemeTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            ThemeTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            ThemeTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            ThemeTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            ThemeTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            ThemeTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            ThemeTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            ThemeTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            ThemeTable.Size = new Size(717, 390);
            ThemeTable.TabIndex = 0;
            // 
            // NodeWidthLabel
            // 
            NodeWidthLabel.AutoSize = true;
            NodeWidthLabel.Dock = DockStyle.Fill;
            NodeWidthLabel.Location = new Point(3, 300);
            NodeWidthLabel.Name = "NodeWidthLabel";
            NodeWidthLabel.Size = new Size(194, 30);
            NodeWidthLabel.TabIndex = 23;
            NodeWidthLabel.Text = "Node Width:";
            NodeWidthLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // NodeWidthValue
            // 
            NodeWidthValue.Dock = DockStyle.Fill;
            NodeWidthValue.Location = new Point(203, 303);
            NodeWidthValue.Minimum = new decimal(new int[] { 10, 0, 0, 0 });
            NodeWidthValue.Name = "NodeWidthValue";
            NodeWidthValue.Size = new Size(511, 23);
            NodeWidthValue.TabIndex = 22;
            NodeWidthValue.Value = new decimal(new int[] { 10, 0, 0, 0 });
            // 
            // NodeBorderWidthLabel
            // 
            NodeBorderWidthLabel.AutoSize = true;
            NodeBorderWidthLabel.Dock = DockStyle.Fill;
            NodeBorderWidthLabel.Location = new Point(3, 270);
            NodeBorderWidthLabel.Name = "NodeBorderWidthLabel";
            NodeBorderWidthLabel.Size = new Size(194, 30);
            NodeBorderWidthLabel.TabIndex = 21;
            NodeBorderWidthLabel.Text = "Node Border Width:";
            NodeBorderWidthLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // NodeBorderWidthValue
            // 
            NodeBorderWidthValue.Dock = DockStyle.Fill;
            NodeBorderWidthValue.Location = new Point(203, 273);
            NodeBorderWidthValue.Maximum = new decimal(new int[] { 20, 0, 0, 0 });
            NodeBorderWidthValue.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            NodeBorderWidthValue.Name = "NodeBorderWidthValue";
            NodeBorderWidthValue.Size = new Size(511, 23);
            NodeBorderWidthValue.TabIndex = 20;
            NodeBorderWidthValue.Value = new decimal(new int[] { 8, 0, 0, 0 });
            // 
            // NodeSpacingLabel
            // 
            NodeSpacingLabel.AutoSize = true;
            NodeSpacingLabel.Dock = DockStyle.Fill;
            NodeSpacingLabel.Location = new Point(3, 240);
            NodeSpacingLabel.Name = "NodeSpacingLabel";
            NodeSpacingLabel.Size = new Size(194, 30);
            NodeSpacingLabel.TabIndex = 19;
            NodeSpacingLabel.Text = "Node Spacing:";
            NodeSpacingLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // NodeSpacingValue
            // 
            NodeSpacingValue.Dock = DockStyle.Fill;
            NodeSpacingValue.Location = new Point(203, 243);
            NodeSpacingValue.Minimum = new decimal(new int[] { 5, 0, 0, 0 });
            NodeSpacingValue.Name = "NodeSpacingValue";
            NodeSpacingValue.Size = new Size(511, 23);
            NodeSpacingValue.TabIndex = 18;
            NodeSpacingValue.Value = new decimal(new int[] { 25, 0, 0, 0 });
            // 
            // TextColorLabel
            // 
            TextColorLabel.AutoSize = true;
            TextColorLabel.Dock = DockStyle.Fill;
            TextColorLabel.Location = new Point(3, 210);
            TextColorLabel.Name = "TextColorLabel";
            TextColorLabel.Size = new Size(194, 30);
            TextColorLabel.TabIndex = 17;
            TextColorLabel.Text = "Text Color:";
            TextColorLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // TextColorColorButton
            // 
            TextColorColorButton.Dock = DockStyle.Fill;
            TextColorColorButton.Location = new Point(203, 213);
            TextColorColorButton.Name = "TextColorColorButton";
            TextColorColorButton.Size = new Size(511, 24);
            TextColorColorButton.TabIndex = 16;
            TextColorColorButton.Text = "Color";
            TextColorColorButton.UseVisualStyleBackColor = true;
            // 
            // DisabledNodeColorLabel
            // 
            DisabledNodeColorLabel.AutoSize = true;
            DisabledNodeColorLabel.Dock = DockStyle.Fill;
            DisabledNodeColorLabel.Location = new Point(3, 180);
            DisabledNodeColorLabel.Name = "DisabledNodeColorLabel";
            DisabledNodeColorLabel.Size = new Size(194, 30);
            DisabledNodeColorLabel.TabIndex = 15;
            DisabledNodeColorLabel.Text = "Disabled Node Color:";
            DisabledNodeColorLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // DisabledNodeColorColorButton
            // 
            DisabledNodeColorColorButton.Dock = DockStyle.Fill;
            DisabledNodeColorColorButton.Location = new Point(203, 183);
            DisabledNodeColorColorButton.Name = "DisabledNodeColorColorButton";
            DisabledNodeColorColorButton.Size = new Size(511, 24);
            DisabledNodeColorColorButton.TabIndex = 14;
            DisabledNodeColorColorButton.Text = "Color";
            DisabledNodeColorColorButton.UseVisualStyleBackColor = true;
            // 
            // SuccessColorLabel
            // 
            SuccessColorLabel.AutoSize = true;
            SuccessColorLabel.Dock = DockStyle.Fill;
            SuccessColorLabel.Location = new Point(3, 150);
            SuccessColorLabel.Name = "SuccessColorLabel";
            SuccessColorLabel.Size = new Size(194, 30);
            SuccessColorLabel.TabIndex = 13;
            SuccessColorLabel.Text = "Success Color:";
            SuccessColorLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // SuccessColorColorButton
            // 
            SuccessColorColorButton.Dock = DockStyle.Fill;
            SuccessColorColorButton.Location = new Point(203, 153);
            SuccessColorColorButton.Name = "SuccessColorColorButton";
            SuccessColorColorButton.Size = new Size(511, 24);
            SuccessColorColorButton.TabIndex = 12;
            SuccessColorColorButton.Text = "Color";
            SuccessColorColorButton.UseVisualStyleBackColor = true;
            // 
            // ErrorColorLabel
            // 
            ErrorColorLabel.AutoSize = true;
            ErrorColorLabel.Dock = DockStyle.Fill;
            ErrorColorLabel.Location = new Point(3, 120);
            ErrorColorLabel.Name = "ErrorColorLabel";
            ErrorColorLabel.Size = new Size(194, 30);
            ErrorColorLabel.TabIndex = 11;
            ErrorColorLabel.Text = "Error Color:";
            ErrorColorLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // ErrorColorColorButton
            // 
            ErrorColorColorButton.Dock = DockStyle.Fill;
            ErrorColorColorButton.Location = new Point(203, 123);
            ErrorColorColorButton.Name = "ErrorColorColorButton";
            ErrorColorColorButton.Size = new Size(511, 24);
            ErrorColorColorButton.TabIndex = 10;
            ErrorColorColorButton.Text = "Color";
            ErrorColorColorButton.UseVisualStyleBackColor = true;
            // 
            // HoveredNodeColorLabel
            // 
            HoveredNodeColorLabel.AutoSize = true;
            HoveredNodeColorLabel.Dock = DockStyle.Fill;
            HoveredNodeColorLabel.Location = new Point(3, 90);
            HoveredNodeColorLabel.Name = "HoveredNodeColorLabel";
            HoveredNodeColorLabel.Size = new Size(194, 30);
            HoveredNodeColorLabel.TabIndex = 9;
            HoveredNodeColorLabel.Text = "Hovered Node Color:";
            HoveredNodeColorLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // HoveredNodeColorColorButton
            // 
            HoveredNodeColorColorButton.Dock = DockStyle.Fill;
            HoveredNodeColorColorButton.Location = new Point(203, 93);
            HoveredNodeColorColorButton.Name = "HoveredNodeColorColorButton";
            HoveredNodeColorColorButton.Size = new Size(511, 24);
            HoveredNodeColorColorButton.TabIndex = 8;
            HoveredNodeColorColorButton.Text = "Color";
            HoveredNodeColorColorButton.UseVisualStyleBackColor = true;
            // 
            // DefaultNodeColorLabel
            // 
            DefaultNodeColorLabel.AutoSize = true;
            DefaultNodeColorLabel.Dock = DockStyle.Fill;
            DefaultNodeColorLabel.Location = new Point(3, 60);
            DefaultNodeColorLabel.Name = "DefaultNodeColorLabel";
            DefaultNodeColorLabel.Size = new Size(194, 30);
            DefaultNodeColorLabel.TabIndex = 7;
            DefaultNodeColorLabel.Text = "Default Node Color:";
            DefaultNodeColorLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // DefaultNodeColorColorButton
            // 
            DefaultNodeColorColorButton.Dock = DockStyle.Fill;
            DefaultNodeColorColorButton.Location = new Point(203, 63);
            DefaultNodeColorColorButton.Name = "DefaultNodeColorColorButton";
            DefaultNodeColorColorButton.Size = new Size(511, 24);
            DefaultNodeColorColorButton.TabIndex = 6;
            DefaultNodeColorColorButton.Text = "Color";
            DefaultNodeColorColorButton.UseVisualStyleBackColor = true;
            // 
            // SelectedBlockOutlineLabel
            // 
            SelectedBlockOutlineLabel.AutoSize = true;
            SelectedBlockOutlineLabel.Dock = DockStyle.Fill;
            SelectedBlockOutlineLabel.Location = new Point(3, 30);
            SelectedBlockOutlineLabel.Name = "SelectedBlockOutlineLabel";
            SelectedBlockOutlineLabel.Size = new Size(194, 30);
            SelectedBlockOutlineLabel.TabIndex = 5;
            SelectedBlockOutlineLabel.Text = "Selected Block Outline:";
            SelectedBlockOutlineLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // SelectedBlockOutlineColorButton2
            // 
            SelectedBlockOutlineColorButton2.Dock = DockStyle.Fill;
            SelectedBlockOutlineColorButton2.Location = new Point(203, 33);
            SelectedBlockOutlineColorButton2.Name = "SelectedBlockOutlineColorButton2";
            SelectedBlockOutlineColorButton2.Size = new Size(511, 24);
            SelectedBlockOutlineColorButton2.TabIndex = 4;
            SelectedBlockOutlineColorButton2.Text = "Color";
            SelectedBlockOutlineColorButton2.UseVisualStyleBackColor = true;
            // 
            // HoveredBlockOutlineLabel
            // 
            HoveredBlockOutlineLabel.AutoSize = true;
            HoveredBlockOutlineLabel.Dock = DockStyle.Fill;
            HoveredBlockOutlineLabel.Location = new Point(3, 0);
            HoveredBlockOutlineLabel.Name = "HoveredBlockOutlineLabel";
            HoveredBlockOutlineLabel.Size = new Size(194, 30);
            HoveredBlockOutlineLabel.TabIndex = 3;
            HoveredBlockOutlineLabel.Text = "Hovered Block Outline:";
            HoveredBlockOutlineLabel.TextAlign = ContentAlignment.MiddleLeft;
            HoveredBlockOutlineLabel.Click += HoveredBlockOutlineLabel_Click;
            // 
            // HoveredBlockOutlineColorButton
            // 
            HoveredBlockOutlineColorButton.Dock = DockStyle.Fill;
            HoveredBlockOutlineColorButton.Location = new Point(203, 3);
            HoveredBlockOutlineColorButton.Name = "HoveredBlockOutlineColorButton";
            HoveredBlockOutlineColorButton.Size = new Size(511, 24);
            HoveredBlockOutlineColorButton.TabIndex = 2;
            HoveredBlockOutlineColorButton.Text = "Color";
            HoveredBlockOutlineColorButton.UseVisualStyleBackColor = true;
            // 
            // BorderOutlineLabel
            // 
            BorderOutlineLabel.AutoSize = true;
            BorderOutlineLabel.Dock = DockStyle.Fill;
            BorderOutlineLabel.Location = new Point(3, 330);
            BorderOutlineLabel.Name = "BorderOutlineLabel";
            BorderOutlineLabel.Size = new Size(194, 30);
            BorderOutlineLabel.TabIndex = 25;
            BorderOutlineLabel.Text = "Border Outline:";
            BorderOutlineLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // BorderOutlineColorButton
            // 
            BorderOutlineColorButton.Dock = DockStyle.Fill;
            BorderOutlineColorButton.Location = new Point(203, 333);
            BorderOutlineColorButton.Name = "BorderOutlineColorButton";
            BorderOutlineColorButton.Size = new Size(511, 24);
            BorderOutlineColorButton.TabIndex = 24;
            BorderOutlineColorButton.Text = "Color";
            BorderOutlineColorButton.UseVisualStyleBackColor = true;
            // 
            // SocketConnectionColorLabel
            // 
            SocketConnectionColorLabel.AutoSize = true;
            SocketConnectionColorLabel.Dock = DockStyle.Fill;
            SocketConnectionColorLabel.Location = new Point(3, 360);
            SocketConnectionColorLabel.Name = "SocketConnectionColorLabel";
            SocketConnectionColorLabel.Size = new Size(194, 30);
            SocketConnectionColorLabel.TabIndex = 27;
            SocketConnectionColorLabel.Text = "Socket Connection Color:";
            SocketConnectionColorLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // SocketConnectionColorColorButton
            // 
            SocketConnectionColorColorButton.Dock = DockStyle.Fill;
            SocketConnectionColorColorButton.Location = new Point(203, 363);
            SocketConnectionColorColorButton.Name = "SocketConnectionColorColorButton";
            SocketConnectionColorColorButton.Size = new Size(511, 24);
            SocketConnectionColorColorButton.TabIndex = 26;
            SocketConnectionColorColorButton.Text = "Color";
            SocketConnectionColorColorButton.UseVisualStyleBackColor = true;
            // 
            // EditorGroupBox
            // 
            EditorGroupBox.AutoSize = true;
            EditorGroupBox.Controls.Add(EditorTable);
            EditorGroupBox.Dock = DockStyle.Top;
            EditorGroupBox.Location = new Point(0, 374);
            EditorGroupBox.Name = "EditorGroupBox";
            EditorGroupBox.Size = new Size(723, 172);
            EditorGroupBox.TabIndex = 2;
            EditorGroupBox.TabStop = false;
            EditorGroupBox.Text = "Editor";
            // 
            // EditorTable
            // 
            EditorTable.AutoSize = true;
            EditorTable.ColumnCount = 2;
            EditorTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200F));
            EditorTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            EditorTable.Controls.Add(AutoSnapZoneWidthLabel, 0, 4);
            EditorTable.Controls.Add(AutoSnapZoneWidthValue, 1, 4);
            EditorTable.Controls.Add(RenderScaleLabel, 0, 3);
            EditorTable.Controls.Add(RenderScaleValue, 1, 3);
            EditorTable.Controls.Add(SocketRadiusLabel, 0, 2);
            EditorTable.Controls.Add(SocketRadiusValue, 1, 2);
            EditorTable.Controls.Add(AllowOutOfScreenPanLabel, 0, 1);
            EditorTable.Controls.Add(AllowOutOfScreenPanValue, 1, 1);
            EditorTable.Controls.Add(SelectedBlockOutlineColorLabel, 0, 0);
            EditorTable.Controls.Add(SelectedBlockOutlineColorButton, 1, 0);
            EditorTable.Dock = DockStyle.Fill;
            EditorTable.Location = new Point(3, 19);
            EditorTable.Name = "EditorTable";
            EditorTable.RowCount = 5;
            EditorTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            EditorTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            EditorTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            EditorTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            EditorTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            EditorTable.Size = new Size(717, 150);
            EditorTable.TabIndex = 0;
            // 
            // AutoSnapZoneWidthLabel
            // 
            AutoSnapZoneWidthLabel.AutoSize = true;
            AutoSnapZoneWidthLabel.Dock = DockStyle.Fill;
            AutoSnapZoneWidthLabel.Location = new Point(3, 120);
            AutoSnapZoneWidthLabel.Name = "AutoSnapZoneWidthLabel";
            AutoSnapZoneWidthLabel.Size = new Size(194, 30);
            AutoSnapZoneWidthLabel.TabIndex = 5;
            AutoSnapZoneWidthLabel.Text = "Auto Snap Zone Width:";
            AutoSnapZoneWidthLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // AutoSnapZoneWidthValue
            // 
            AutoSnapZoneWidthValue.Dock = DockStyle.Fill;
            AutoSnapZoneWidthValue.Location = new Point(203, 123);
            AutoSnapZoneWidthValue.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            AutoSnapZoneWidthValue.Name = "AutoSnapZoneWidthValue";
            AutoSnapZoneWidthValue.Size = new Size(511, 23);
            AutoSnapZoneWidthValue.TabIndex = 4;
            AutoSnapZoneWidthValue.Value = new decimal(new int[] { 20, 0, 0, 0 });
            // 
            // RenderScaleLabel
            // 
            RenderScaleLabel.AutoSize = true;
            RenderScaleLabel.Dock = DockStyle.Fill;
            RenderScaleLabel.Location = new Point(3, 90);
            RenderScaleLabel.Name = "RenderScaleLabel";
            RenderScaleLabel.Size = new Size(194, 30);
            RenderScaleLabel.TabIndex = 3;
            RenderScaleLabel.Text = "Render Scale:";
            RenderScaleLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // RenderScaleValue
            // 
            RenderScaleValue.DecimalPlaces = 2;
            RenderScaleValue.Dock = DockStyle.Fill;
            RenderScaleValue.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            RenderScaleValue.Location = new Point(203, 93);
            RenderScaleValue.Maximum = new decimal(new int[] { 5, 0, 0, 0 });
            RenderScaleValue.Minimum = new decimal(new int[] { 1, 0, 0, 65536 });
            RenderScaleValue.Name = "RenderScaleValue";
            RenderScaleValue.Size = new Size(511, 23);
            RenderScaleValue.TabIndex = 2;
            RenderScaleValue.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // SocketRadiusLabel
            // 
            SocketRadiusLabel.AutoSize = true;
            SocketRadiusLabel.Dock = DockStyle.Fill;
            SocketRadiusLabel.Location = new Point(3, 60);
            SocketRadiusLabel.Name = "SocketRadiusLabel";
            SocketRadiusLabel.Size = new Size(194, 30);
            SocketRadiusLabel.TabIndex = 1;
            SocketRadiusLabel.Text = "Socket Radius:";
            SocketRadiusLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // SocketRadiusValue
            // 
            SocketRadiusValue.Dock = DockStyle.Fill;
            SocketRadiusValue.Location = new Point(203, 63);
            SocketRadiusValue.Maximum = new decimal(new int[] { 20, 0, 0, 0 });
            SocketRadiusValue.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            SocketRadiusValue.Name = "SocketRadiusValue";
            SocketRadiusValue.Size = new Size(511, 23);
            SocketRadiusValue.TabIndex = 0;
            SocketRadiusValue.Value = new decimal(new int[] { 6, 0, 0, 0 });
            // 
            // AllowOutOfScreenPanLabel
            // 
            AllowOutOfScreenPanLabel.AutoSize = true;
            AllowOutOfScreenPanLabel.Dock = DockStyle.Fill;
            AllowOutOfScreenPanLabel.Location = new Point(3, 30);
            AllowOutOfScreenPanLabel.Name = "AllowOutOfScreenPanLabel";
            AllowOutOfScreenPanLabel.Size = new Size(194, 30);
            AllowOutOfScreenPanLabel.TabIndex = 7;
            AllowOutOfScreenPanLabel.Text = "Allow Out-of-Screen Pan:";
            AllowOutOfScreenPanLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // AllowOutOfScreenPanValue
            // 
            AllowOutOfScreenPanValue.AutoSize = true;
            AllowOutOfScreenPanValue.Dock = DockStyle.Fill;
            AllowOutOfScreenPanValue.Location = new Point(203, 33);
            AllowOutOfScreenPanValue.Name = "AllowOutOfScreenPanValue";
            AllowOutOfScreenPanValue.Size = new Size(511, 24);
            AllowOutOfScreenPanValue.TabIndex = 6;
            AllowOutOfScreenPanValue.UseVisualStyleBackColor = true;
            // 
            // SelectedBlockOutlineColorLabel
            // 
            SelectedBlockOutlineColorLabel.AutoSize = true;
            SelectedBlockOutlineColorLabel.Dock = DockStyle.Fill;
            SelectedBlockOutlineColorLabel.Location = new Point(3, 0);
            SelectedBlockOutlineColorLabel.Name = "SelectedBlockOutlineColorLabel";
            SelectedBlockOutlineColorLabel.Size = new Size(194, 30);
            SelectedBlockOutlineColorLabel.TabIndex = 9;
            SelectedBlockOutlineColorLabel.Text = "Selected Block Outline Color:";
            SelectedBlockOutlineColorLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // SelectedBlockOutlineColorButton
            // 
            SelectedBlockOutlineColorButton.Dock = DockStyle.Fill;
            SelectedBlockOutlineColorButton.Location = new Point(203, 3);
            SelectedBlockOutlineColorButton.Name = "SelectedBlockOutlineColorButton";
            SelectedBlockOutlineColorButton.Size = new Size(511, 24);
            SelectedBlockOutlineColorButton.TabIndex = 8;
            SelectedBlockOutlineColorButton.Text = "Color";
            SelectedBlockOutlineColorButton.UseVisualStyleBackColor = true;
            // 
            // ExecutionGroupBox
            // 
            ExecutionGroupBox.AutoSize = true;
            ExecutionGroupBox.Controls.Add(ExecutionTable);
            ExecutionGroupBox.Dock = DockStyle.Top;
            ExecutionGroupBox.Location = new Point(0, 52);
            ExecutionGroupBox.Name = "ExecutionGroupBox";
            ExecutionGroupBox.Size = new Size(723, 322);
            ExecutionGroupBox.TabIndex = 1;
            ExecutionGroupBox.TabStop = false;
            ExecutionGroupBox.Text = "Execution";
            // 
            // ExecutionTable
            // 
            ExecutionTable.AutoSize = true;
            ExecutionTable.ColumnCount = 2;
            ExecutionTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200F));
            ExecutionTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            ExecutionTable.Controls.Add(WatchdogTimeoutLabel, 0, 3);
            ExecutionTable.Controls.Add(WatchdogTimeoutValue, 1, 3);
            ExecutionTable.Controls.Add(MaxShipmentSizeLabel, 0, 4);
            ExecutionTable.Controls.Add(MaxShipmentSizeValue, 1, 4);
            ExecutionTable.Controls.Add(ProfilingWindowSizeLabel, 0, 5);
            ExecutionTable.Controls.Add(ProfilingWindowSizeValue, 1, 5);
            ExecutionTable.Controls.Add(CostEmaAlphaLabel, 0, 6);
            ExecutionTable.Controls.Add(CostEmaAlphaValue, 1, 6);
            ExecutionTable.Controls.Add(CriticalPathRecomputeIntervalLabel, 0, 7);
            ExecutionTable.Controls.Add(CriticalPathRecomputeIntervalValue, 1, 7);
            ExecutionTable.Controls.Add(BatchSizeLabel, 0, 8);
            ExecutionTable.Controls.Add(BatchSizeValue, 1, 8);
            ExecutionTable.Controls.Add(CriticalPathBoostLabel, 0, 9);
            ExecutionTable.Controls.Add(CriticalPathBoostValue, 1, 9);
            ExecutionTable.Controls.Add(ExecutionModeLabel, 0, 0);
            ExecutionTable.Controls.Add(ExecutionModeComboBox, 1, 0);
            ExecutionTable.Controls.Add(MaxDegreeOfParallelismLabel, 0, 1);
            ExecutionTable.Controls.Add(MaxDegreeOfParallelismValue, 1, 1);
            ExecutionTable.Controls.Add(EnableGcThrottlingLabel, 0, 2);
            ExecutionTable.Controls.Add(EnableGcThrottlingValue, 1, 2);
            ExecutionTable.Dock = DockStyle.Fill;
            ExecutionTable.Location = new Point(3, 19);
            ExecutionTable.Name = "ExecutionTable";
            ExecutionTable.RowCount = 10;
            ExecutionTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            ExecutionTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            ExecutionTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            ExecutionTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            ExecutionTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            ExecutionTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            ExecutionTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            ExecutionTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            ExecutionTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            ExecutionTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            ExecutionTable.Size = new Size(717, 300);
            ExecutionTable.TabIndex = 0;
            // 
            // WatchdogTimeoutLabel
            // 
            WatchdogTimeoutLabel.AutoSize = true;
            WatchdogTimeoutLabel.Dock = DockStyle.Fill;
            WatchdogTimeoutLabel.Location = new Point(3, 90);
            WatchdogTimeoutLabel.Name = "WatchdogTimeoutLabel";
            WatchdogTimeoutLabel.Size = new Size(194, 30);
            WatchdogTimeoutLabel.TabIndex = 9;
            WatchdogTimeoutLabel.Text = "Watchdog Timeout (seconds):";
            WatchdogTimeoutLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // WatchdogTimeoutValue
            // 
            WatchdogTimeoutValue.Dock = DockStyle.Fill;
            WatchdogTimeoutValue.Location = new Point(203, 93);
            WatchdogTimeoutValue.Maximum = new decimal(new int[] { 300, 0, 0, 0 });
            WatchdogTimeoutValue.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            WatchdogTimeoutValue.Name = "WatchdogTimeoutValue";
            WatchdogTimeoutValue.Size = new Size(511, 23);
            WatchdogTimeoutValue.TabIndex = 8;
            WatchdogTimeoutValue.Value = new decimal(new int[] { 30, 0, 0, 0 });
            // 
            // MaxShipmentSizeLabel
            // 
            MaxShipmentSizeLabel.AutoSize = true;
            MaxShipmentSizeLabel.Dock = DockStyle.Fill;
            MaxShipmentSizeLabel.Location = new Point(3, 120);
            MaxShipmentSizeLabel.Name = "MaxShipmentSizeLabel";
            MaxShipmentSizeLabel.Size = new Size(194, 30);
            MaxShipmentSizeLabel.TabIndex = 7;
            MaxShipmentSizeLabel.Text = "Max Shipment Size:";
            MaxShipmentSizeLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // MaxShipmentSizeValue
            // 
            MaxShipmentSizeValue.Dock = DockStyle.Fill;
            MaxShipmentSizeValue.Location = new Point(203, 123);
            MaxShipmentSizeValue.Maximum = new decimal(new int[] { 512, 0, 0, 0 });
            MaxShipmentSizeValue.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            MaxShipmentSizeValue.Name = "MaxShipmentSizeValue";
            MaxShipmentSizeValue.Size = new Size(511, 23);
            MaxShipmentSizeValue.TabIndex = 6;
            MaxShipmentSizeValue.Value = new decimal(new int[] { 64, 0, 0, 0 });
            // 
            // ProfilingWindowSizeLabel
            // 
            ProfilingWindowSizeLabel.AutoSize = true;
            ProfilingWindowSizeLabel.Dock = DockStyle.Fill;
            ProfilingWindowSizeLabel.Location = new Point(3, 150);
            ProfilingWindowSizeLabel.Name = "ProfilingWindowSizeLabel";
            ProfilingWindowSizeLabel.Size = new Size(194, 30);
            ProfilingWindowSizeLabel.TabIndex = 5;
            ProfilingWindowSizeLabel.Text = "Profiling Window Size:";
            ProfilingWindowSizeLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // ProfilingWindowSizeValue
            // 
            ProfilingWindowSizeValue.Dock = DockStyle.Fill;
            ProfilingWindowSizeValue.Location = new Point(203, 153);
            ProfilingWindowSizeValue.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            ProfilingWindowSizeValue.Name = "ProfilingWindowSizeValue";
            ProfilingWindowSizeValue.Size = new Size(511, 23);
            ProfilingWindowSizeValue.TabIndex = 4;
            ProfilingWindowSizeValue.Value = new decimal(new int[] { 20, 0, 0, 0 });
            // 
            // CostEmaAlphaLabel
            // 
            CostEmaAlphaLabel.AutoSize = true;
            CostEmaAlphaLabel.Dock = DockStyle.Fill;
            CostEmaAlphaLabel.Location = new Point(3, 180);
            CostEmaAlphaLabel.Name = "CostEmaAlphaLabel";
            CostEmaAlphaLabel.Size = new Size(194, 30);
            CostEmaAlphaLabel.TabIndex = 3;
            CostEmaAlphaLabel.Text = "Cost EMA Alpha:";
            CostEmaAlphaLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // CostEmaAlphaValue
            // 
            CostEmaAlphaValue.DecimalPlaces = 2;
            CostEmaAlphaValue.Dock = DockStyle.Fill;
            CostEmaAlphaValue.Increment = new decimal(new int[] { 1, 0, 0, 131072 });
            CostEmaAlphaValue.Location = new Point(203, 183);
            CostEmaAlphaValue.Maximum = new decimal(new int[] { 1, 0, 0, 0 });
            CostEmaAlphaValue.Minimum = new decimal(new int[] { 1, 0, 0, 131072 });
            CostEmaAlphaValue.Name = "CostEmaAlphaValue";
            CostEmaAlphaValue.Size = new Size(511, 23);
            CostEmaAlphaValue.TabIndex = 2;
            CostEmaAlphaValue.Value = new decimal(new int[] { 2, 0, 0, 65536 });
            // 
            // CriticalPathRecomputeIntervalLabel
            // 
            CriticalPathRecomputeIntervalLabel.AutoSize = true;
            CriticalPathRecomputeIntervalLabel.Dock = DockStyle.Fill;
            CriticalPathRecomputeIntervalLabel.Location = new Point(3, 210);
            CriticalPathRecomputeIntervalLabel.Name = "CriticalPathRecomputeIntervalLabel";
            CriticalPathRecomputeIntervalLabel.Size = new Size(194, 30);
            CriticalPathRecomputeIntervalLabel.TabIndex = 1;
            CriticalPathRecomputeIntervalLabel.Text = "Critical Path Recompute Interval:";
            CriticalPathRecomputeIntervalLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // CriticalPathRecomputeIntervalValue
            // 
            CriticalPathRecomputeIntervalValue.Dock = DockStyle.Fill;
            CriticalPathRecomputeIntervalValue.Location = new Point(203, 213);
            CriticalPathRecomputeIntervalValue.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            CriticalPathRecomputeIntervalValue.Name = "CriticalPathRecomputeIntervalValue";
            CriticalPathRecomputeIntervalValue.Size = new Size(511, 23);
            CriticalPathRecomputeIntervalValue.TabIndex = 0;
            CriticalPathRecomputeIntervalValue.Value = new decimal(new int[] { 10, 0, 0, 0 });
            // 
            // BatchSizeLabel
            // 
            BatchSizeLabel.AutoSize = true;
            BatchSizeLabel.Dock = DockStyle.Fill;
            BatchSizeLabel.Location = new Point(3, 240);
            BatchSizeLabel.Name = "BatchSizeLabel";
            BatchSizeLabel.Size = new Size(194, 30);
            BatchSizeLabel.TabIndex = 25;
            BatchSizeLabel.Text = "Batch Size:";
            BatchSizeLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // BatchSizeValue
            // 
            BatchSizeValue.Dock = DockStyle.Fill;
            BatchSizeValue.Location = new Point(203, 243);
            BatchSizeValue.Maximum = new decimal(new int[] { 50, 0, 0, 0 });
            BatchSizeValue.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            BatchSizeValue.Name = "BatchSizeValue";
            BatchSizeValue.Size = new Size(511, 23);
            BatchSizeValue.TabIndex = 24;
            BatchSizeValue.Value = new decimal(new int[] { 5, 0, 0, 0 });
            // 
            // CriticalPathBoostLabel
            // 
            CriticalPathBoostLabel.AutoSize = true;
            CriticalPathBoostLabel.Dock = DockStyle.Fill;
            CriticalPathBoostLabel.Location = new Point(3, 270);
            CriticalPathBoostLabel.Name = "CriticalPathBoostLabel";
            CriticalPathBoostLabel.Size = new Size(194, 30);
            CriticalPathBoostLabel.TabIndex = 23;
            CriticalPathBoostLabel.Text = "Critical Path Boost:";
            CriticalPathBoostLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // CriticalPathBoostValue
            // 
            CriticalPathBoostValue.DecimalPlaces = 2;
            CriticalPathBoostValue.Dock = DockStyle.Fill;
            CriticalPathBoostValue.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            CriticalPathBoostValue.Location = new Point(203, 273);
            CriticalPathBoostValue.Maximum = new decimal(new int[] { 10, 0, 0, 0 });
            CriticalPathBoostValue.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            CriticalPathBoostValue.Name = "CriticalPathBoostValue";
            CriticalPathBoostValue.Size = new Size(511, 23);
            CriticalPathBoostValue.TabIndex = 22;
            CriticalPathBoostValue.Value = new decimal(new int[] { 15, 0, 0, 65536 });
            // 
            // ExecutionModeLabel
            // 
            ExecutionModeLabel.AutoSize = true;
            ExecutionModeLabel.Dock = DockStyle.Fill;
            ExecutionModeLabel.Location = new Point(3, 0);
            ExecutionModeLabel.Name = "ExecutionModeLabel";
            ExecutionModeLabel.Size = new Size(194, 30);
            ExecutionModeLabel.TabIndex = 15;
            ExecutionModeLabel.Text = "Mode:";
            ExecutionModeLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // ExecutionModeComboBox
            // 
            ExecutionModeComboBox.Dock = DockStyle.Fill;
            ExecutionModeComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            ExecutionModeComboBox.FormattingEnabled = true;
            ExecutionModeComboBox.Location = new Point(203, 3);
            ExecutionModeComboBox.Name = "ExecutionModeComboBox";
            ExecutionModeComboBox.Size = new Size(511, 23);
            ExecutionModeComboBox.TabIndex = 14;
            // 
            // MaxDegreeOfParallelismLabel
            // 
            MaxDegreeOfParallelismLabel.AutoSize = true;
            MaxDegreeOfParallelismLabel.Dock = DockStyle.Fill;
            MaxDegreeOfParallelismLabel.Location = new Point(3, 30);
            MaxDegreeOfParallelismLabel.Name = "MaxDegreeOfParallelismLabel";
            MaxDegreeOfParallelismLabel.Size = new Size(194, 30);
            MaxDegreeOfParallelismLabel.TabIndex = 13;
            MaxDegreeOfParallelismLabel.Text = "Max Degree of Parallelism:";
            MaxDegreeOfParallelismLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // MaxDegreeOfParallelismValue
            // 
            MaxDegreeOfParallelismValue.Dock = DockStyle.Fill;
            MaxDegreeOfParallelismValue.Location = new Point(203, 33);
            MaxDegreeOfParallelismValue.Maximum = new decimal(new int[] { 128, 0, 0, 0 });
            MaxDegreeOfParallelismValue.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            MaxDegreeOfParallelismValue.Name = "MaxDegreeOfParallelismValue";
            MaxDegreeOfParallelismValue.Size = new Size(511, 23);
            MaxDegreeOfParallelismValue.TabIndex = 12;
            MaxDegreeOfParallelismValue.Value = new decimal(new int[] { 8, 0, 0, 0 });
            // 
            // EnableGcThrottlingLabel
            // 
            EnableGcThrottlingLabel.AutoSize = true;
            EnableGcThrottlingLabel.Dock = DockStyle.Fill;
            EnableGcThrottlingLabel.Location = new Point(3, 60);
            EnableGcThrottlingLabel.Name = "EnableGcThrottlingLabel";
            EnableGcThrottlingLabel.Size = new Size(194, 30);
            EnableGcThrottlingLabel.TabIndex = 11;
            EnableGcThrottlingLabel.Text = "Enable GC Throttling:";
            EnableGcThrottlingLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // EnableGcThrottlingValue
            // 
            EnableGcThrottlingValue.AutoSize = true;
            EnableGcThrottlingValue.Dock = DockStyle.Fill;
            EnableGcThrottlingValue.Location = new Point(203, 63);
            EnableGcThrottlingValue.Name = "EnableGcThrottlingValue";
            EnableGcThrottlingValue.Size = new Size(511, 24);
            EnableGcThrottlingValue.TabIndex = 10;
            EnableGcThrottlingValue.UseVisualStyleBackColor = true;
            // 
            // ApplicationGroupBox
            // 
            ApplicationGroupBox.AutoSize = true;
            ApplicationGroupBox.Controls.Add(ApplicationTable);
            ApplicationGroupBox.Dock = DockStyle.Top;
            ApplicationGroupBox.Location = new Point(0, 0);
            ApplicationGroupBox.Name = "ApplicationGroupBox";
            ApplicationGroupBox.Size = new Size(723, 52);
            ApplicationGroupBox.TabIndex = 0;
            ApplicationGroupBox.TabStop = false;
            ApplicationGroupBox.Text = "Application";
            // 
            // ApplicationTable
            // 
            ApplicationTable.AutoSize = true;
            ApplicationTable.ColumnCount = 2;
            ApplicationTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200F));
            ApplicationTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            ApplicationTable.Controls.Add(MaxRecentWorkspacesLabel, 0, 0);
            ApplicationTable.Controls.Add(MaxRecentWorkspacesValue, 1, 0);
            ApplicationTable.Dock = DockStyle.Fill;
            ApplicationTable.Location = new Point(3, 19);
            ApplicationTable.Name = "ApplicationTable";
            ApplicationTable.RowCount = 1;
            ApplicationTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            ApplicationTable.Size = new Size(717, 30);
            ApplicationTable.TabIndex = 0;
            // 
            // MaxRecentWorkspacesLabel
            // 
            MaxRecentWorkspacesLabel.AutoSize = true;
            MaxRecentWorkspacesLabel.Dock = DockStyle.Fill;
            MaxRecentWorkspacesLabel.Location = new Point(3, 0);
            MaxRecentWorkspacesLabel.Name = "MaxRecentWorkspacesLabel";
            MaxRecentWorkspacesLabel.Size = new Size(194, 30);
            MaxRecentWorkspacesLabel.TabIndex = 0;
            MaxRecentWorkspacesLabel.Text = "Max Recent Workspaces:";
            MaxRecentWorkspacesLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // MaxRecentWorkspacesValue
            // 
            MaxRecentWorkspacesValue.Dock = DockStyle.Fill;
            MaxRecentWorkspacesValue.Location = new Point(203, 3);
            MaxRecentWorkspacesValue.Maximum = new decimal(new int[] { 50, 0, 0, 0 });
            MaxRecentWorkspacesValue.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            MaxRecentWorkspacesValue.Name = "MaxRecentWorkspacesValue";
            MaxRecentWorkspacesValue.Size = new Size(511, 23);
            MaxRecentWorkspacesValue.TabIndex = 1;
            MaxRecentWorkspacesValue.Value = new decimal(new int[] { 10, 0, 0, 0 });
            // 
            // BtnSave
            // 
            BtnSave.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            BtnSave.Location = new Point(631, 10);
            BtnSave.Name = "BtnSave";
            BtnSave.Size = new Size(100, 30);
            BtnSave.TabIndex = 0;
            BtnSave.Text = "Save";
            BtnSave.UseVisualStyleBackColor = true;
            // 
            // BtnReload
            // 
            BtnReload.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            BtnReload.Location = new Point(525, 10);
            BtnReload.Name = "BtnReload";
            BtnReload.Size = new Size(100, 30);
            BtnReload.TabIndex = 1;
            BtnReload.Text = "Reload";
            BtnReload.UseVisualStyleBackColor = true;
            // 
            // BtnSetToDefault
            // 
            BtnSetToDefault.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            BtnSetToDefault.Location = new Point(419, 10);
            BtnSetToDefault.Name = "BtnSetToDefault";
            BtnSetToDefault.Size = new Size(100, 30);
            BtnSetToDefault.TabIndex = 2;
            BtnSetToDefault.Text = "Set to Default";
            BtnSetToDefault.UseVisualStyleBackColor = true;
            // 
            // BottomPanel
            // 
            BottomPanel.Controls.Add(BtnSetToDefault);
            BottomPanel.Controls.Add(BtnReload);
            BottomPanel.Controls.Add(BtnSave);
            BottomPanel.Dock = DockStyle.Bottom;
            BottomPanel.Location = new Point(0, 550);
            BottomPanel.Name = "BottomPanel";
            BottomPanel.Size = new Size(740, 50);
            BottomPanel.TabIndex = 1;
            // 
            // SettingsView
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(MainScrollPanel);
            Controls.Add(BottomPanel);
            Name = "SettingsView";
            Size = new Size(740, 600);
            Load += OnSettingsViewLoad;
            MainScrollPanel.ResumeLayout(false);
            MainScrollPanel.PerformLayout();
            ThemeGroupBox.ResumeLayout(false);
            ThemeGroupBox.PerformLayout();
            ThemeTable.ResumeLayout(false);
            ThemeTable.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)NodeWidthValue).EndInit();
            ((System.ComponentModel.ISupportInitialize)NodeBorderWidthValue).EndInit();
            ((System.ComponentModel.ISupportInitialize)NodeSpacingValue).EndInit();
            EditorGroupBox.ResumeLayout(false);
            EditorGroupBox.PerformLayout();
            EditorTable.ResumeLayout(false);
            EditorTable.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)AutoSnapZoneWidthValue).EndInit();
            ((System.ComponentModel.ISupportInitialize)RenderScaleValue).EndInit();
            ((System.ComponentModel.ISupportInitialize)SocketRadiusValue).EndInit();
            ExecutionGroupBox.ResumeLayout(false);
            ExecutionGroupBox.PerformLayout();
            ExecutionTable.ResumeLayout(false);
            ExecutionTable.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)WatchdogTimeoutValue).EndInit();
            ((System.ComponentModel.ISupportInitialize)MaxShipmentSizeValue).EndInit();
            ((System.ComponentModel.ISupportInitialize)ProfilingWindowSizeValue).EndInit();
            ((System.ComponentModel.ISupportInitialize)CostEmaAlphaValue).EndInit();
            ((System.ComponentModel.ISupportInitialize)CriticalPathRecomputeIntervalValue).EndInit();
            ((System.ComponentModel.ISupportInitialize)BatchSizeValue).EndInit();
            ((System.ComponentModel.ISupportInitialize)CriticalPathBoostValue).EndInit();
            ((System.ComponentModel.ISupportInitialize)MaxDegreeOfParallelismValue).EndInit();
            ApplicationGroupBox.ResumeLayout(false);
            ApplicationGroupBox.PerformLayout();
            ApplicationTable.ResumeLayout(false);
            ApplicationTable.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)MaxRecentWorkspacesValue).EndInit();
            BottomPanel.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel MainScrollPanel;
        private GroupBox ThemeGroupBox;
        private TableLayoutPanel ThemeTable;
        private Label NodeBorderWidthLabel;
        private NoScrollNumericUpDown NodeBorderWidthValue;
        private Label NodeSpacingLabel;
        private NoScrollNumericUpDown NodeSpacingValue;
        private Label TextColorLabel;
        private UI.ColorDialogButton TextColorColorButton;
        private Label DisabledNodeColorLabel;
        private UI.ColorDialogButton DisabledNodeColorColorButton;
        private Label SuccessColorLabel;
        private UI.ColorDialogButton SuccessColorColorButton;
        private Label ErrorColorLabel;
        private UI.ColorDialogButton ErrorColorColorButton;
        private Label HoveredNodeColorLabel;
        private UI.ColorDialogButton HoveredNodeColorColorButton;
        private Label DefaultNodeColorLabel;
        private UI.ColorDialogButton DefaultNodeColorColorButton;
        private Label SelectedBlockOutlineLabel;
        private UI.ColorDialogButton SelectedBlockOutlineColorButton2;
        private Label HoveredBlockOutlineLabel;
        private UI.ColorDialogButton HoveredBlockOutlineColorButton;
        private Label BorderOutlineLabel;
        private UI.ColorDialogButton BorderOutlineColorButton;
        private Label SocketConnectionColorLabel;
        private UI.ColorDialogButton SocketConnectionColorColorButton;
        private GroupBox EditorGroupBox;
        private TableLayoutPanel EditorTable;
        private Label AutoSnapZoneWidthLabel;
        private NoScrollNumericUpDown AutoSnapZoneWidthValue;
        private Label RenderScaleLabel;
        private NoScrollNumericUpDown RenderScaleValue;
        private Label SocketRadiusLabel;
        private NoScrollNumericUpDown SocketRadiusValue;
        private Label AllowOutOfScreenPanLabel;
        private CheckBox AllowOutOfScreenPanValue;
        private Label SelectedBlockOutlineColorLabel;
        private UI.ColorDialogButton SelectedBlockOutlineColorButton;
        private GroupBox ExecutionGroupBox;
        private TableLayoutPanel ExecutionTable;
        private Label WatchdogTimeoutLabel;
        private NoScrollNumericUpDown WatchdogTimeoutValue;
        private Label MaxShipmentSizeLabel;
        private NoScrollNumericUpDown MaxShipmentSizeValue;
        private Label ProfilingWindowSizeLabel;
        private NoScrollNumericUpDown ProfilingWindowSizeValue;
        private Label CostEmaAlphaLabel;
        private NoScrollNumericUpDown CostEmaAlphaValue;
        private Label CriticalPathRecomputeIntervalLabel;
        private NoScrollNumericUpDown CriticalPathRecomputeIntervalValue;
        private Label BatchSizeLabel;
        private NoScrollNumericUpDown BatchSizeValue;
        private Label CriticalPathBoostLabel;
        private NoScrollNumericUpDown CriticalPathBoostValue;
        private Label ExecutionModeLabel;
        private NoScrollComboBox ExecutionModeComboBox;
        private Label MaxDegreeOfParallelismLabel;
        private NoScrollNumericUpDown MaxDegreeOfParallelismValue;
        private Label EnableGcThrottlingLabel;
        private CheckBox EnableGcThrottlingValue;
        private GroupBox ApplicationGroupBox;
        private TableLayoutPanel ApplicationTable;
        private Label MaxRecentWorkspacesLabel;
        private NoScrollNumericUpDown MaxRecentWorkspacesValue;
        private Button BtnSave;
        private Button BtnReload;
        private Button BtnSetToDefault;
        private Panel BottomPanel;
        private Label NodeWidthLabel;
        private NoScrollNumericUpDown NodeWidthValue;
    }
}