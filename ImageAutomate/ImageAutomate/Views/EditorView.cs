using System.Diagnostics;

using ImageAutomate.Core;
using ImageAutomate.Execution;
using ImageAutomate.StandardBlocks;
using ImageAutomate.UI;


namespace ImageAutomate.Views;

public partial class EditorView : UserControl
{
    private PipelineGraph graph = new();
    public EditorView()
    {
        InitializeComponent();

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
        Debug.WriteLine("Execute was called");

        GraphExecutor executor = new(new GraphValidator());

        try
        {
            // Disable the execute button/menu during execution
            if (sender is ToolStripMenuItem menuItem)
            {
                menuItem.Enabled = false;
            }

            // Use ExecuteAsync with default configuration and cancellation token
            await executor.ExecuteAsync(graph, new ExecutorConfiguration(), CancellationToken.None);

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

        using SaveFileDialog dialog = new()
        {
            Filter = "ImageAutomate Workspace (*.imageautomate)|*.imageautomate|JSON File (*.json)|*.json|All Files (*.*)|*.*",
            DefaultExt = "imageautomate",
            Title = "Save Workspace"
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            try
            {
                GraphPanel.Workspace.SaveToFile(dialog.FileName);
                MessageBox.Show("Workspace saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save workspace: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void OnCloseMenuItemClick(object sender, EventArgs e)
    {
        var result = MessageBox.Show(
            "Close the current workspace? Any unsaved changes will be lost.",
            "Close Workspace",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (result == DialogResult.Yes)
        {
            // Clear the graph and reset the workspace
            graph = new PipelineGraph();
            var workspace = new Workspace(graph);
            GraphPanel.Workspace = workspace;
            GraphPanel.Invalidate();
            BlockPropertyGrid.SelectedObject = null;
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
}
