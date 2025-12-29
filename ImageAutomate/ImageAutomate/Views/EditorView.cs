using ImageAutomate.Core;
using ImageAutomate.Dialogs;
using ImageAutomate.Execution;
using ImageAutomate.Services;
using ImageAutomate.StandardBlocks;
using ImageAutomate.UI;


namespace ImageAutomate.Views;

public partial class EditorView : UserControl
{
    private PipelineGraph graph = new();
    private WorkspaceService workspaceService = WorkspaceService.Instance;

    public event EventHandler? CloseRequested;

    public EditorView()
    {
        InitializeComponent();

        this.SetStyle(ControlStyles.Selectable, true);

        var workspace = new Workspace(graph);
        GraphPanel.Workspace = workspace;

        Toolbox.DisplayMember = "Name";
        Toolbox.Items.AddRange(new Type[]
        {
            typeof(LoadBlock),
            typeof(SaveBlock),
            typeof(ConvertBlock),
            typeof(CropBlock),
            typeof(ResizeBlock),
            typeof(FlipBlock),
            typeof(GrayscaleBlock),
            typeof(BrightnessBlock),
            typeof(ContrastBlock),
            typeof(HueBlock),
            typeof(SaturationBlock),
            typeof(GaussianBlurBlock),
            typeof(SharpenBlock),
            typeof(PixelateBlock),
            typeof(VignetteBlock)
        });
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == Keys.Delete)
        {
            GraphPanel.DeleteSelectedItem();
            GraphPanel.Invalidate();
            return true;
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }

    private void OnToolboxMouseDown(object sender, MouseEventArgs e)
    {
        ListBox lb = Toolbox;
        Point pt = new(e.X, e.Y);
        int index = lb.IndexFromPoint(pt);

        if (index >= 0)
        {
            lb.DoDragDrop(lb.Items[index], DragDropEffects.Copy);
        }
    }

    private async void OnExecuteMenuItemClick(object sender, EventArgs e)
    {
        GraphExecutor executor = new(new GraphValidator());

        try
        {
            // Disable the execute button/menu during execution
            if (sender is ToolStripMenuItem menuItem)
            {
                menuItem.Enabled = false;
            }

            // Use Task.Run to ensure the entire ExecuteAsync call runs on ThreadPool thread
            // This prevents UI freeze during validation (synchronous preamble) and file scanning
            await Task.Run(async () =>
            {
                await executor.ExecuteAsync(graph, new ExecutorConfiguration(), CancellationToken.None);
            }, CancellationToken.None);

            MessageBox.Show("Pipeline execution completed successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"An error occurred during execution: {ex.Message}", "Execution Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            // Re-enable the execute button/menu
            if (sender is ToolStripMenuItem menuItem)
            {
                menuItem.Enabled = true;
            }
        }
    }

    private void OnGraphSelectedItemChange(object? sender, EventArgs e)
    {
        if (sender is GraphRenderPanel panel)
        {
            BlockPropertyGrid.SelectedObject = panel.Graph?.SelectedItem;
        }
    }

    private void OnNewMenuItemClick(object sender, EventArgs e)
    {
        var result = MessageBox.Show(
            "Create a new workspace? Any unsaved changes will be lost.",
            "New Workspace",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (result == DialogResult.Yes)
        {
            graph = new PipelineGraph();
            var workspace = new Workspace(graph);
            GraphPanel.Workspace = workspace;
            GraphPanel.Invalidate();
        }
    }

    private void OnOpenMenuItemClick(object sender, EventArgs e)
    {
        using OpenFileDialog dialog = new()
        {
            Filter = "ImageAutomate Workspace (*.imageautomate;*.json)|*.imageautomate;*.json|All Files (*.*)|*.*",
            Title = "Open Workspace"
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            try
            {
                var workspace = Workspace.LoadFromFile(dialog.FileName);
                graph = workspace.Graph;
                GraphPanel.Workspace = workspace;
                GraphPanel.Invalidate();

                MessageBox.Show("Workspace loaded successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load workspace: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void OnSaveMenuItemClick(object sender, EventArgs e)
    {
        if (GraphPanel.Workspace == null)
        {
            MessageBox.Show("No workspace to save.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        // If workspace was previously saved, save to same location
        if (GraphPanel.Workspace.IsSaved)
        {
            SaveWorkspaceToFile(GraphPanel.Workspace.FilePath!);
        }
        else
        {
            // Show save dialog for new workspaces
            ShowSaveDialog();
        }
    }

    private void OnSaveAsMenuItemClick(object sender, EventArgs e)
    {
        if (GraphPanel.Workspace == null)
        {
            MessageBox.Show("No workspace to save.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        // Always show the save dialog
        ShowSaveDialog();
    }

    private void ShowSaveDialog()
    {
        var workspace = GraphPanel.Workspace!;

        using var dialog = new WorkspaceSaveDialog(
            workspace.Name,
            workspace.FilePath,
            null, // No image path stored in workspace currently
            false // Not edit mode
        );

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            try
            {
                workspace.Name = dialog.WorkspaceName;
                workspace.SaveToFile(dialog.FilePath);
                workspaceService.AddOrUpdateWorkspace(dialog.FilePath, workspace.Name, null, dialog.ImagePath);
                MessageBox.Show("Workspace saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save workspace: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void SaveWorkspaceToFile(string filePath)
    {
        try
        {
            GraphPanel.Workspace!.SaveToFile(filePath);
            workspaceService.AddOrUpdateWorkspace(filePath, GraphPanel.Workspace.Name);
            MessageBox.Show("Workspace saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to save workspace: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void OnCloseMenuItemClick(object sender, EventArgs e)
    {
        // We let the shell (Main) handle the actual closing/hiding of the view.
        // We just fire the event. 
        // Note: Ideally we should check for unsaved changes here before firing.
        // For now, we'll keep it simple as per requirements.

        var result = MessageBox.Show(
            "Close the current workspace and return to dashboard? Any unsaved changes will be lost.",
            "Close Workspace",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (result == DialogResult.Yes)
        {
            // Clear the graph to ensure next open is fresh? 
            // Or we can rely on LoadWorkspace replacing it.
            // Clearing it is safer to avoid showing old data if re-opened without a file.
            graph = new PipelineGraph();
            var workspace = new Workspace(graph);
            GraphPanel.Workspace = workspace;
            GraphPanel.Invalidate();
            BlockPropertyGrid.SelectedObject = null;

            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
    }

    private void OnAboutMenuItemClick(object sender, EventArgs e)
    {
        MessageBox.Show(
            "ImageAutomate - Visual Node-Based Image Processing\n\n" +
            "Version 1.0\n\n" +
            "A powerful image processing pipeline editor with a visual programming interface.\n\n" +
            "© 2025 ImageAutomate Team",
            "About ImageAutomate",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    private void OnHelpMenuItemClick(object sender, EventArgs e)
    {
        MessageBox.Show(
            "ImageAutomate Help\n\n" +
            "• Drag blocks from the toolbox to the canvas\n" +
            "• Connect blocks by dragging from output sockets to input sockets\n" +
            "• Select blocks to edit their properties in the property grid\n" +
            "• Press Delete to remove selected items\n" +
            "• Use Execute menu to run the pipeline\n\n" +
            "For more information, visit the documentation.",
            "Help",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    /// <summary>
    /// Loads a workspace from the specified file path.
    /// </summary>
    public void LoadWorkspace(string filePath)
    {
        try
        {
            var workspace = Workspace.LoadFromFile(filePath);
            graph = workspace.Graph;
            GraphPanel.Workspace = workspace;
            GraphPanel.Invalidate();

            // Add to recent? It is already added by the caller (WelcomeView/WorkspaceView)
            // But if opened via File->Open in Editor, we should probably add it.
            // For this specific method, it's called by Main when switching views.
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to load workspace: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// Resets the editor to a blank state.
    /// </summary>
    public void CreateNewWorkspace()
    {
        graph = new PipelineGraph();
        var workspace = new Workspace(graph);
        GraphPanel.Workspace = workspace;
        GraphPanel.Invalidate();
    }
}
