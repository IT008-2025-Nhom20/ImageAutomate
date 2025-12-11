/**
 * GraphRenderPanel.cs
 * 
 * Panel-based control for rendering pipeline graph
 */

using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Layout.Layered;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using ImageAutomate.Core;
using GeomNode = Microsoft.Msagl.Core.Layout.Node;
using GeomGraph = Microsoft.Msagl.Core.Layout.GeometryGraph;
using GeomEdge = Microsoft.Msagl.Core.Layout.Edge;
using MsaglPoint = Microsoft.Msagl.Core.Geometry.Point;
using System.Diagnostics;

namespace ImageAutomate.UI;

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
            if (_graph != null)
                _graph.GraphChanged -= OnGraphChangedRecompute;
            _graph = value;
            if (_graph != null)
            {
                _graph.GraphChanged += OnGraphChangedRecompute;
                ComputeLayout();
                CenterCameraOnGraph();
            }
        }
    }

    private PipelineGraph? _graph;
    private GeomGraph _geomGraph = new();
    private Dictionary<IBlock, GeomNode> _blockToNodeMap = new();
    private PointF _panOffset = new(0, 0);
    private Point _lastMousePos;
    private bool _isPanning;
    private Point _mouseDownLocation;
    private const int ClickDragThreshold = 5; // pixels

    public GraphRenderPanel()
    {
        DoubleBuffered = true;
        BackColor = Color.White;

        Resize += (_, _) => Invalidate();
        MouseDown += OnMouseDownPan;
        MouseMove += OnMouseMovePan;
        MouseUp += OnMouseUpPan;
        MouseWheel += OnMouseWheelZoom;
    }

    public void AddBlockAndConnect(IBlock sourceBlock, Socket sourceSocket, IBlock destBlock, Socket destSocket)
    {
        if (_graph == null)
            return;

        _graph.AddBlock(sourceBlock);

        if (destBlock != null)
        {
            if (!_graph.Blocks.Contains(destBlock))
                _graph.AddBlock(destBlock);
            if (destSocket != null)
                _graph.Connect(sourceBlock, sourceSocket, destBlock, destSocket);
        }

        Invalidate();
    }

    public void AddSuccessor(Socket sourceSocket, IBlock destBlock, Socket destSocket)
    {
        if (_graph is null)
            return;
        if (_graph.Center is null)
            return;
        AddBlockAndConnect(_graph.Center, sourceSocket, destBlock, destSocket);
    }

    public void AddPredecessor(IBlock sourceBlock, Socket sourceSocket, Socket destSocket)
    {
        if (_graph is null)
            return;
        if (_graph.Center is null)
            return;
        AddBlockAndConnect(sourceBlock, sourceSocket, _graph.Center, destSocket);
    }

    public void CenterCameraOnGraph()
    {
        if (_graph == null)
            return;
        var bounds = _geomGraph.BoundingBox;

        var node = _geomGraph.Nodes.First(n => n.UserData == _graph.Center);
        float wx = (float)node.Center.X;
        float wy = (float)node.Center.Y;

        _panOffset.X = -wx * _renderScale;
        _panOffset.Y = wy * _renderScale;

        Invalidate();
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
        if (!_isPanning)
            return;

        // Calculate delta in screen pixels
        float dx = e.X - _lastMousePos.X;
        float dy = e.Y - _lastMousePos.Y;

        _panOffset.X += dx;
        _panOffset.Y += dy;

        ClampPanToBounds();

        _lastMousePos = e.Location;
        Invalidate();
    }

    private void OnMouseWheelZoom(object? sender, MouseEventArgs e)
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
        if (AllowOutOfScreenPan || _graph == null)
            return;

        var bounds = _geomGraph.BoundingBox;

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

    private void RebuildVisualGraph()
    {
        _geomGraph = new GeomGraph();
        _blockToNodeMap.Clear();

        if (_graph == null) return;

        foreach (var block in _graph.Blocks)
        {
            var geomNode = new GeomNode(
                CurveFactory.CreateRectangle(block.Width, block.Height, new MsaglPoint(0, 0))
            )
            {
                UserData = block
            };

            _geomGraph.Nodes.Add(geomNode);
            _blockToNodeMap[block] = geomNode;
        }

        foreach (var conn in _graph.Connections)
        {
            if (_blockToNodeMap.TryGetValue(conn.Source, out var sourceNode) &&
                _blockToNodeMap.TryGetValue(conn.Target, out var targetNode))
            {
                // TODO: model Sockets in MSAGL. Currently NodeRenderer
                // handles the socket visual positions.
                var edge = new GeomEdge(sourceNode, targetNode);
                _geomGraph.Edges.Add(edge);
            }
        }
    }

    private void ComputeLayout()
    {
        if (_graph == null)
            return;

        RebuildVisualGraph();

        var settings = new SugiyamaLayoutSettings
        {
            Transformation = PlaneTransformation.Rotation(Math.PI / 2),
            LayerSeparation = _columnSpacing,
            NodeSeparation = _nodeSpacing,
            EdgeRoutingSettings = { EdgeRoutingMode = Microsoft.Msagl.Core.Routing.EdgeRoutingMode.None },
            RandomSeedForOrdering = 0
        };

        var layout = new LayeredLayout(_geomGraph, settings);
        layout.Run();
    }

    private void OnGraphChangedRecompute(object? sender, EventArgs args)
    {
        ComputeLayout();
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        if (_graph == null)
            return;
        if (_geomGraph.Nodes.Count == 0)
            return;

        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        using Matrix transform = new();
        transform.Translate(Width / 2f + _panOffset.X, Height / 2f + _panOffset.Y);
        transform.Scale(_renderScale, -_renderScale);
        g.Transform = transform;

        foreach (var geomEdge in _geomGraph.Edges)
            NodeRenderer.DrawEdge(g, geomEdge, _socketRadius);

        foreach (var geomNode in _geomGraph.Nodes)
        {
            bool isSelected = geomNode.UserData == _graph.Center;
            NodeRenderer.OptimizedStrategy.DrawNode(g, geomNode, isSelected, _selectedBlockOutlineColor, _socketRadius);
        }

        g.ResetTransform();
    }
    #endregion
}