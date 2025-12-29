namespace ImageAutomate.Dialogs
{
    /// <summary>
    /// Dialog for saving/editing workspace metadata (name and thumbnail image).
    /// </summary>
    public partial class WorkspaceSaveDialog : Form
    {
        private bool _isEditMode;

        /// <summary>
        /// Gets the workspace name entered by the user.
        /// </summary>
        public string WorkspaceName => textBoxName.Text.Trim();

        /// <summary>
        /// Gets the file path where the workspace will be saved.
        /// </summary>
        public string FilePath => textBoxFilePath.Text.Trim();

        /// <summary>
        /// Gets the optional image path selected by the user.
        /// </summary>
        public string? ImagePath => string.IsNullOrWhiteSpace(textBoxImagePath.Text) ? null : textBoxImagePath.Text.Trim();

        /// <summary>
        /// Initializes a new instance of the WorkspaceSaveDialog for saving a new workspace.
        /// </summary>
        /// <param name="defaultName">Default workspace name.</param>
        public WorkspaceSaveDialog(string defaultName) : this(defaultName, null, null, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the WorkspaceSaveDialog.
        /// </summary>
        /// <param name="defaultName">Default workspace name.</param>
        /// <param name="defaultFilePath">Default file path (null for new workspaces).</param>
        /// <param name="defaultImagePath">Default image path for thumbnail.</param>
        /// <param name="isEditMode">True if editing an existing workspace entry.</param>
        public WorkspaceSaveDialog(string defaultName, string? defaultFilePath, string? defaultImagePath, bool isEditMode)
        {
            InitializeComponent();

            _isEditMode = isEditMode;

            textBoxName.Text = defaultName;
            textBoxFilePath.Text = defaultFilePath ?? string.Empty;
            textBoxImagePath.Text = defaultImagePath ?? string.Empty;

            // Configure based on mode
            if (isEditMode)
            {
                Text = "Edit Workspace";
                buttonOK.Text = "Save";

                // Hide file path row entirely in edit mode
                labelFilePath.Visible = false;
                panelFilePathSelection.Visible = false;
                tableLayoutPanel.RowStyles[1].Height = 0;
            }
            else
            {
                Text = "Save Workspace";
                buttonOK.Text = "Save";
            }

            // Wire up events
            buttonBrowseImage.Click += OnBrowseImageClick;
            buttonBrowseFile.Click += OnBrowseFileClick;
            buttonOK.Click += OnOKClick;
            textBoxImagePath.TextChanged += OnImagePathChanged;

            // Load initial preview
            UpdateImagePreview();
        }

        private void OnBrowseImageClick(object? sender, EventArgs e)
        {
            using var dialog = new OpenFileDialog
            {
                Filter = "Image Files (*.png;*.jpg;*.jpeg;*.bmp;*.gif)|*.png;*.jpg;*.jpeg;*.bmp;*.gif|All Files (*.*)|*.*",
                Title = "Select Thumbnail Image"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                textBoxImagePath.Text = dialog.FileName;
            }
        }

        private void OnBrowseFileClick(object? sender, EventArgs e)
        {
            using var dialog = new SaveFileDialog
            {
                Filter = "ImageAutomate Workspace (*.imageautomate)|*.imageautomate|JSON File (*.json)|*.json|All Files (*.*)|*.*",
                DefaultExt = "imageautomate",
                Title = "Save Workspace As",
                FileName = string.IsNullOrWhiteSpace(textBoxName.Text) ? "Untitled" : textBoxName.Text
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                textBoxFilePath.Text = dialog.FileName;

                // Auto-fill name from filename if name is empty
                if (string.IsNullOrWhiteSpace(textBoxName.Text))
                {
                    textBoxName.Text = Path.GetFileNameWithoutExtension(dialog.FileName);
                }
            }
        }

        private void OnImagePathChanged(object? sender, EventArgs e)
        {
            UpdateImagePreview();
        }

        private void UpdateImagePreview()
        {
            var path = textBoxImagePath.Text.Trim();

            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                pictureBoxPreview.Image?.Dispose();
                pictureBoxPreview.Image = null;
                return;
            }

            try
            {
                // Load image without locking the file
                using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                var image = Image.FromStream(stream);

                // Dispose previous image
                pictureBoxPreview.Image?.Dispose();
                pictureBoxPreview.Image = image;
            }
            catch
            {
                pictureBoxPreview.Image?.Dispose();
                pictureBoxPreview.Image = null;
            }
        }

        private void OnOKClick(object? sender, EventArgs e)
        {
            // Validate name
            if (string.IsNullOrWhiteSpace(textBoxName.Text))
            {
                MessageBox.Show("Please enter a workspace name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxName.Focus();
                return;
            }

            // Validate file path for new workspaces
            if (!_isEditMode && string.IsNullOrWhiteSpace(textBoxFilePath.Text))
            {
                MessageBox.Show("Please select a location to save the workspace.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                buttonBrowseFile.Focus();
                return;
            }

            // Validate image path if provided
            if (!string.IsNullOrWhiteSpace(textBoxImagePath.Text) && !File.Exists(textBoxImagePath.Text))
            {
                MessageBox.Show("The selected image file does not exist.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxImagePath.Focus();
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            // Clean up the preview image
            pictureBoxPreview.Image?.Dispose();
            pictureBoxPreview.Image = null;
        }
    }
}
