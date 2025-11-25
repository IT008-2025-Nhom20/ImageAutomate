using System.ComponentModel;

namespace ConvertBlockPoC;

public class MainForm : Form
{
    private PipelineGraph _graph = new();
    private GraphRenderPanel _graphPanel;
    private PropertyGrid _propertyGrid;

    void OnBlockChanged(object? sender, EventArgs e)
    {
        _graphPanel.Invalidate();
    }

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
            Dock = DockStyle.Fill,
            Graph = _graph
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
        // Create Blocks with specified Width and Height as per PoC requirements
        var source = new ConvertBlock {
            TargetFormat = ImageFormat.Jpeg,
            JpegOptions = { Quality = 90 },
            Width = 200,
            Height = 120
        };
        source.PropertyChanged += OnBlockChanged;
        var process = new ConvertBlock {
            TargetFormat = ImageFormat.Png,
            AlwaysReEncode = true,
            Width = 200,
            Height = 120
        };
        var output = new ConvertBlock {
            TargetFormat = ImageFormat.WebP,
            Width = 200,
            Height = 120
        };

        // Initialize Panel
        _graphPanel.Initialize(source);

        // Setup Graph
        _graphPanel.AddBlockAndConnect(process, source);
        _graphPanel.AddBlockAndConnect(output, process);
        var selected = (_graphPanel.Graph?.CenterNode) ?? throw new Exception();
        _propertyGrid.SelectedObject = selected.UserData;

        // Add some branching to test layout
        var branch = new ConvertBlock {
            TargetFormat = ImageFormat.Bmp,
            Width = 200,
            Height = 120
        };
        _graphPanel.AddBlockAndConnect(branch, source);
    }
}
