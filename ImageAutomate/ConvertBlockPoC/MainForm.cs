using System.ComponentModel;

namespace ConvertBlockPoC;

public class MainForm : Form
{
    private GraphRenderPanel _graphPanel;
    private PropertyGrid _propertyGrid;

    public MainForm()
    {
        Text = "Pipeline Graph PoC";
        Size = new Size(1200, 800);

        var splitContainer = new SplitContainer
        {
            Dock = DockStyle.Fill,
            SplitterDistance = 900
        };

        _graphPanel = new GraphRenderPanel
        {
            Dock = DockStyle.Fill
        };

        _propertyGrid = new PropertyGrid
        {
            Dock = DockStyle.Fill
        };

        splitContainer.Panel1.Controls.Add(_graphPanel);
        splitContainer.Panel2.Controls.Add(_propertyGrid);
        Controls.Add(splitContainer);

        InitializeGraph();
    }

    private void InitializeGraph()
    {
        // Create Blocks
        var source = new ConvertBlock { TargetFormat = ImageFormat.Jpeg, JpegOptions = { Quality = 90 } };
        var process = new ConvertBlock { TargetFormat = ImageFormat.Png, AlwaysReEncode = true };
        var output = new ConvertBlock { TargetFormat = ImageFormat.WebP };

        // Initialize Panel
        _propertyGrid.SelectedObject = _graphPanel;

        // Setup Graph
        _graphPanel.Initialize(source);
        _graphPanel.AddBlockAndConnect(process, source);
        _graphPanel.AddBlockAndConnect(output, process);

        // Add some branching to test layout
        var branch = new ConvertBlock { TargetFormat = ImageFormat.Bmp };
        _graphPanel.AddBlockAndConnect(branch, source);
    }
}
