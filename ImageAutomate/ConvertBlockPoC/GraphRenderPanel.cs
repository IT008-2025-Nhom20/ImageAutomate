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

    [Category("Graph Layout")]
    [Description("Vertical spacing between nodes in the same layer")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public double NodeSpacing
    {
        get => _nodeSpacing;
        set
        {
            _nodeSpacing = value;
            Invalidate();
        }
    }
    private double _nodeSpacing = 30;

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

    [Category("Graph Behavior")]
    [Description("Allows the graph to be panned completely off-screen")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    [DefaultValue(false)]
    public bool AllowOutOfScreenPan { get; set; } = false;

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

    public GraphRenderPanel()
    {
        DoubleBuffered = true;
        BackColor = Color.White;

        Resize += (_, _) => Invalidate();
        MouseDown += OnMouseDownPan;
        MouseMove += OnMouseMovePan;
        MouseUp += OnMouseUpPan;
        MouseWheel += OnMouseWheelPan;
    }

    public void Initialize(ConvertBlock block)
    {
        if (_graph == null) _graph = new PipelineGraph();
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

    #region Private method

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

        _panOffset.X += dx;
        _panOffset.Y += dy;

        ClampPanToBounds();

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
        float mouseX = e.X - Width / 2.0f;
        float mouseY = e.Y - Height / 2.0f;

        float worldX = (mouseX - _panOffset.X) / oldScale;
        float worldY = (mouseY - _panOffset.Y) / -oldScale;

        _panOffset.X = mouseX - (worldX * _renderScale);
        _panOffset.Y = mouseY - (worldY * -_renderScale);

        ClampPanToBounds();

        Invalidate();
    }

    private void ClampPanToBounds()
    {
        if (AllowOutOfScreenPan || _graph == null) return;

        var bounds = _graph.GeomGraph.BoundingBox;

        float cx = Width / 2.0f + _panOffset.X;
        float cy = Height / 2.0f + _panOffset.Y;

        float wx1 = (float)bounds.Left;
        float wx2 = (float)bounds.Right;
        float wy1 = (float)bounds.Bottom;
        float wy2 = (float)bounds.Top;

        float sx1 = wx1 * _renderScale + cx;
        float sx2 = wx2 * _renderScale + cx;
        float sy1 = wy1 * -_renderScale + cy;
        float sy2 = wy2 * -_renderScale + cy;

        float graphScreenLeft = Math.Min(sx1, sx2);
        float graphScreenRight = Math.Max(sx1, sx2);
        float graphScreenTop = Math.Min(sy1, sy2);
        float graphScreenBottom = Math.Max(sy1, sy2);

        float margin = 30;

        if (graphScreenRight < margin)
        {
            float shift = margin - graphScreenRight;
            _panOffset.X += shift;
        }
        else if (graphScreenLeft > Width - margin)
        {
            float shift = (Width - margin) - graphScreenLeft;
            _panOffset.X += shift;
        }

        if (graphScreenBottom < margin)
        {
            float shift = margin - graphScreenBottom;
            _panOffset.Y += shift;
        }
        else if (graphScreenTop > Height - margin)
        {
            float shift = (Height - margin) - graphScreenTop;
            _panOffset.Y += shift;
        }
    }

    private void ComputeLayoutAndRender()
    {
        if (_graph == null) return;

        var graph = _graph.GeomGraph;

        var settings = new SugiyamaLayoutSettings
        {
            Transformation = PlaneTransformation.Rotation(Math.PI / 2),
            LayerSeparation = _columnSpacing,
            NodeSeparation = _nodeSpacing,
            EdgeRoutingSettings = { EdgeRoutingMode = Microsoft.Msagl.Core.Routing.EdgeRoutingMode.None },
            RandomSeedForOrdering = 0
        };

        var layout = new LayeredLayout(graph, settings);
        layout.Run();

        graph.UpdateBoundingBox();
        CenterCameraOnGraph();

        Invalidate();
    }

    private void CenterCameraOnGraph()
    {
        if (_graph == null) return;
        var bounds = _graph.GeomGraph.BoundingBox;

        float wx = (float)bounds.Center.X;
        float wy = (float)bounds.Center.Y;

        _panOffset.X = -wx * _renderScale;
        _panOffset.Y = wy * _renderScale;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        if (_graph == null)
            return;
        var graph = _graph.GeomGraph;
        if (graph.Nodes.Count == 0)
            return;

        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        using Matrix transform = new();
        transform.Translate(Width / 2f + _panOffset.X, Height / 2f + _panOffset.Y);
        transform.Scale(_renderScale, -_renderScale);
        g.Transform = transform;

        foreach (var geomEdge in graph.Edges)
            NodeRenderer.DrawEdge(g, geomEdge, _socketRadius);

        foreach (var geomNode in graph.Nodes)
        {
            bool isSelected = geomNode == _graph.CenterNode;
            NodeRenderer.OptimizedStrategy.DrawNode(g, geomNode, isSelected, _selectedBlockOutlineColor, _socketRadius);
        }

        g.ResetTransform();
    }
    #endregion
}