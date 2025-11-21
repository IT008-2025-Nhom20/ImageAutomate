/**
 * GraphRenderPanel.cs
 * 
 * Panel-based control for rendering pipeline graph
 */

using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Layout.Layered;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using GeomEdge = Microsoft.Msagl.Core.Layout.Edge;
using GeomNode = Microsoft.Msagl.Core.Layout.Node;
using MsaglPoint = Microsoft.Msagl.Core.Geometry.Point;

namespace ConvertBlockPoC;

public class GraphRenderPanel : Panel
{
    #region Exposed Properties

    [Category("Node Appearance")]
    [Description("Width of each node in the graph")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public double NodeWidth
    {
        get => _nodeWidth;
        set
        {
            if (Math.Abs(_nodeWidth - value) > double.Epsilon)
            {
                _nodeWidth = value;
                if (_graph != null)
                    _graph.NodeWidth = value;
                Invalidate();
            }
        }
    }
    private double _nodeWidth = 200;

    [Category("Node Appearance")]
    [Description("Height of each node in the graph")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public double NodeHeight
    {
        get => _nodeHeight;
        set
        {
            if (Math.Abs(_nodeHeight - value) > double.Epsilon)
            {
                _nodeHeight = value;
                if (_graph != null)
                    _graph.NodeHeight = value;
                Invalidate();
            }
        }
    }
    private double _nodeHeight = 100;

    [Category("Node Appearance")]
    [Description("Outline color for the selected block")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public Color SelectedBlockOutlineColor
    {
        get => _selectedBlockOutlineColor;
        set
        {
            _selectedBlockOutlineColor = value;
            Invalidate();
        }
    }
    private Color _selectedBlockOutlineColor = Color.Red;

    [Category("Node Appearance")]
    [Description("Connection socket size")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public double SocketRadius
    {
        get => _socketRadius;
        set
        {
            _socketRadius = value;
            Invalidate();
        }
    }
    private double _socketRadius = 6;

    [Category("Graph Layout")]
    [Description("Spacing between columns (layers) in the graph")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public double ColumnSpacing
    {
        get => _columnSpacing;
        set
        {
            _columnSpacing = value;
            Invalidate();
        }
    }
    private double _columnSpacing = 250;

    [Category("Graph Appearance")]
    [Description("Node render scale factor")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public float RenderScale
    {
        get => _renderScale;
        set
        {
            _renderScale = value;
            Invalidate();
        }
    }
    private float _renderScale = 1.0f;

    #endregion

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public PipelineGraph? Graph
    {
        get => _graph;
        set
        {
            _graph = value;
            if (_graph != null)
            {
                _graph.NodeWidth = _nodeWidth;
                _graph.NodeHeight = _nodeHeight;
                ComputeLayoutAndRender();
            }
            else
            {
                Invalidate();
            }
        }
    }

    private PipelineGraph? _graph;
    private PointF _panOffset = new(0, 0);
    private Point _lastMousePos;
    private bool _isPanning;

    // Deprecated but kept for PoC compatibility
    public PipelineGraph PGraph => _graph ?? throw new InvalidOperationException("Graph not initialized");

    public GraphRenderPanel()
    {
        DoubleBuffered = true;
        BackColor = Color.White;

        // For PoC compatibility, we can initialize a default graph
        // But standard usage should be setting the Graph property
        _graph = new PipelineGraph(_nodeWidth, _nodeHeight);

        Resize += (_, _) => Invalidate();
        MouseDown += OnMouseDownPan;
        MouseMove += OnMouseMovePan;
        MouseUp += OnMouseUpPan;
        MouseWheel += OnMouseWheelPan;
    }

    public void Initialize(ConvertBlock block)
    {
        if (_graph == null) _graph = new PipelineGraph(_nodeWidth, _nodeHeight);
        _graph.CenterNode = _graph.AddNode(block);
        ComputeLayoutAndRender();
    }

    public void SetCenterBlock(ConvertBlock block)
    {
        if (_graph == null) return;
        var node = _graph.GetNode(block) ?? _graph.AddNode(block);
        _graph.CenterNode = node;
        ComputeLayoutAndRender();
    }

    public void AddBlockAndConnect(ConvertBlock block, ConvertBlock? connectTo = null)
    {
        if (_graph == null) return;
        var newNode = _graph.AddNode(block);

        if (connectTo != null)
        {
            var targetNode = _graph.GetNode(connectTo);
            if (targetNode != null)
                _graph.AddEdge(targetNode, newNode);
        }

        ComputeLayoutAndRender();
    }

    public void AddSuccessor(ConvertBlock block)
    {
        if (_graph == null || _graph.CenterNode == null) return;
        var newNode = _graph.AddNode(block);
        _graph.AddEdge(_graph.CenterNode, newNode);
        ComputeLayoutAndRender();
    }

    public void AddPredecessor(ConvertBlock block)
    {
        if (_graph == null || _graph.CenterNode == null) return;
        var newNode = _graph.AddNode(block);
        _graph.AddEdge(newNode, _graph.CenterNode);
        ComputeLayoutAndRender();
    }

    private void OnMouseDownPan(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            _isPanning = true;
            _lastMousePos = e.Location;
            Cursor = Cursors.Hand;
        }
    }

    private void OnMouseUpPan(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            _isPanning = false;
            Cursor = Cursors.Default;
        }
    }

    private void OnMouseMovePan(object? sender, MouseEventArgs e)
    {
        if (!_isPanning) return;

        // Calculate delta in screen pixels
        float dx = e.X - _lastMousePos.X;
        float dy = e.Y - _lastMousePos.Y;

        // Adjust pan offset (Camera move)
        // We're moving the camera, so moving mouse right means camera moves left relative to world,
        // or rather, the world moves right.
        _panOffset.X += dx;
        _panOffset.Y += dy;

        _lastMousePos = e.Location;
        Invalidate();
    }

    private void OnMouseWheelPan(object? sender, MouseEventArgs e)
    {
        // Zoom logic
        const float zoomFactor = 1.1f;
        float oldScale = _renderScale;

        if (e.Delta > 0)
            _renderScale *= zoomFactor;
        else
            _renderScale /= zoomFactor;

        // Clamp scale
        _renderScale = Math.Max(0.1f, Math.Min(_renderScale, 5.0f));

        // Zoom towards mouse pointer
        // World = (Screen - Pan) / Scale
        // NewPan = Screen - (World * NewScale)
        // => NewPan = Screen - ((Screen - OldPan) / OldScale * NewScale)

        // Current mouse position relative to the panel center (which is our origin for translation)
        float mouseX = e.X - Width / 2.0f;
        float mouseY = e.Y - Height / 2.0f;

        // The point in the world that is currently under the mouse
        float worldX = (mouseX - _panOffset.X) / oldScale;
        float worldY = (mouseY - _panOffset.Y) / -oldScale; // -oldScale because Y is flipped

        // We want this world point to remain under the mouse after scaling
        // NewScreenPos = World * NewScale + NewPan
        // mouseX = worldX * NewScale + NewPanX
        // NewPanX = mouseX - worldX * NewScale

        _panOffset.X = mouseX - (worldX * _renderScale);
        // For Y, remember the flip:
        // mouseY = worldY * -NewScale + NewPanY
        // NewPanY = mouseY - (worldY * -NewScale)
        _panOffset.Y = mouseY - (worldY * -_renderScale);

        Invalidate();
    }

    private void ClampPanToBounds(ref float dx, ref float dy, float margin = 20)
    {
        // With the new camera system, we can implement more sophisticated clamping later.
        // For now, allowing free pan.
    }

    private void ComputeLayoutAndRender()
    {
        if (_graph == null) return;

        var graph = _graph.GeomGraph;

        var settings = new SugiyamaLayoutSettings
        {
            Transformation = PlaneTransformation.Rotation(Math.PI / 2),
            LayerSeparation = _columnSpacing,
            NodeSeparation = 30,
            EdgeRoutingSettings = { EdgeRoutingMode = Microsoft.Msagl.Core.Routing.EdgeRoutingMode.Spline },
            RandomSeedForOrdering = 0
        };

        var layout = new LayeredLayout(graph, settings);
        layout.Run();

        graph.UpdateBoundingBox();
        // No longer centering the graph at origin (modifying world coords).
        // We handle centering via the camera transform.

        // Auto-center camera on first layout
        CenterCameraOnGraph();

        Invalidate();
    }

    private void CenterCameraOnGraph()
    {
        if (_graph == null) return;
        var bounds = _graph.GeomGraph.BoundingBox;

        // We want to position the camera so the center of the graph is at (0,0) screen relative offset
        // World Center
        float wx = (float)bounds.Center.X;
        float wy = (float)bounds.Center.Y;

        // Screen = World * Scale + Pan (conceptually, ignoring the center offset of Width/2, Height/2)
        // We want Screen to be (0,0) relative to panel center
        // 0 = wx * Scale + PanX => PanX = -wx * Scale
        // 0 = wy * -Scale + PanY => PanY = wy * Scale

        _panOffset.X = -wx * _renderScale;
        _panOffset.Y = wy * _renderScale; // Positive because Y is flipped (World Y up, Screen Y down)
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        if (_graph == null) return;
        var graph = _graph.GeomGraph;
        if (graph.Nodes.Count == 0) return;

        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        // Apply Camera Transform
        // 1. Translate to center of panel (so (0,0) is at center)
        // 2. Translate by Pan Offset
        // 3. Scale (and flip Y)

        using Matrix transform = new();
        transform.Translate(Width / 2f + _panOffset.X, Height / 2f + _panOffset.Y);
        transform.Scale(_renderScale, -_renderScale);
        g.Transform = transform;

        // Use NodeRenderer strategies
        // We can switch between Direct (legacy-ish) and Optimized (GraphicsPath) strategies
        bool useOptimized = true; // Toggle this to switch strategies

        // Draw edges first
        foreach (var geomEdge in graph.Edges)
        {
            NodeRenderer.DrawEdge(g, geomEdge, _socketRadius);
        }

        // Draw nodes on top
        foreach (var geomNode in graph.Nodes)
        {
            bool isSelected = geomNode == _graph.CenterNode;
            if (useOptimized)
            {
                NodeRenderer.OptimizedStrategy.DrawNode(g, geomNode, isSelected, _selectedBlockOutlineColor, _socketRadius);
            }
            else
            {
                NodeRenderer.DirectStrategy.DrawNode(g, geomNode, isSelected, _selectedBlockOutlineColor, _socketRadius);
            }
        }

        g.ResetTransform();
    }
}