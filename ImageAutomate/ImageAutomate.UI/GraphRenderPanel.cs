/**
 * GraphRenderPanel.cs
 * 
 * Panel-based control for rendering pipeline graph
 */

using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Drawing2D;

using ImageAutomate.Core;

namespace ImageAutomate.UI;

/// <summary>
/// Represents a hit test result on a socket within a block.
/// </summary>
/// <param name="Block">The block being hit</param>
/// <param name="Socket">The socket being hit</param>
/// <param name="IsInput">Is the socket Input</param>
/// <param name="Position">Exact mouse position of the hit</param>
public record SocketHit(IBlock Block, Socket Socket, bool IsInput, PointF Position);

/// <summary>
/// Custom panel for rendering and interacting with a pipeline graph.
/// </summary>
public class GraphRenderPanel : Panel
{
    #region Designer Properties

    [Category("Node Appearance")]
    [Description("Outline color for the selected block")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    [DefaultValue(typeof(Color), "Red")]
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
    [DefaultValue(6d)]
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

    [Category("Graph Appearance")]
    [Description("Node render scale factor")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    [DefaultValue(1.0f)]
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

    [Category("Graph Behavior")]
    [Description("Auto-snap zone width")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    [DefaultValue(20f)]
    public float AutoSnapZoneWidth { get; set; } = 20f;

    #endregion

    #region Public Properties
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Workspace? Workspace
    {
        get => _workspace;
        set
        {
            _workspace = value;
            Invalidate();
        }
    }
    private Workspace? _workspace;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public PipelineGraph? Graph => _workspace?.Graph;

    [Category("Graph Events")]
    [Description("Event fired when the selected item changes")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public event Action<object?, EventArgs>? SelectedItemChanged;
    #endregion

    #region Interaction States
    private PointF _panOffset = new(0, 0);
    private Point _lastMousePos;
    private bool _isPanning;
    private bool _isDraggingNode;
    private IBlock? _draggedNode;
    private PointF _dragStartNodePos;

    private bool _isConnecting;
    private SocketHit? _dragStartSocket;
    private PointF _currentMouseWorldPos;
    #endregion

    #region Cursors
    private readonly Cursor _panCursor = Cursors.SizeAll;
    private readonly Cursor _dragCursor = Cursors.Hand;
    private readonly Cursor _connectCursor = Cursors.Cross;
    #endregion

    #region CTOR
    public GraphRenderPanel()
    {
        DoubleBuffered = true;
        BackColor = Color.White;
        AllowDrop = true;
    }
    #endregion

    #region Drag & Drop Ghost State
    private Type? _ghostBlockType;
    private PointF _ghostPosition;
    private bool _isDragOver;
    #endregion

    #region Public API

    public void AddBlockAndConnect(IBlock sourceBlock, Socket sourceSocket, IBlock destBlock, Socket destSocket)
    {
        ArgumentNullException.ThrowIfNull(sourceBlock);
        ArgumentNullException.ThrowIfNull(destBlock);
        ArgumentNullException.ThrowIfNull(sourceSocket);
        ArgumentNullException.ThrowIfNull(destSocket);

        if (Graph == null)
            return;

        if (!Graph.Nodes.Contains(sourceBlock))
            Graph.AddBlock(sourceBlock);

        if (!Graph.Nodes.Contains(destBlock))
            Graph.AddBlock(destBlock);

        if (!sourceBlock.Outputs.Contains(sourceSocket))
            throw new ArgumentException($"Invalid socket {sourceSocket.Id} for block {sourceBlock.Name}", nameof(sourceSocket));

        if (!destBlock.Inputs.Contains(destSocket))
            throw new ArgumentException($"Invalid socket {destSocket.Id} for block {destBlock.Name}", nameof(destSocket));

        Graph.AddEdge(sourceBlock, sourceSocket, destBlock, destSocket);

        Invalidate();
    }

    public void AddSuccessor(Socket sourceSocket, IBlock destBlock, Socket destSocket)
    {
        ArgumentNullException.ThrowIfNull(destBlock);
        ArgumentNullException.ThrowIfNull(sourceSocket);
        ArgumentNullException.ThrowIfNull(destSocket);

        if (Graph?.SelectedItem is IBlock selectBlock)
            AddBlockAndConnect(selectBlock, sourceSocket, destBlock, destSocket);
    }

    public void AddPredecessor(IBlock sourceBlock, Socket sourceSocket, Socket destSocket)
    {
        ArgumentNullException.ThrowIfNull(sourceBlock);
        ArgumentNullException.ThrowIfNull(sourceSocket);
        ArgumentNullException.ThrowIfNull(destSocket);

        if (Graph?.SelectedItem is IBlock selectBlock)
            AddBlockAndConnect(sourceBlock, sourceSocket, selectBlock, destSocket);
    }

    public PointF GetViewportCenterWorld()
    {
        return ScreenToWorld(new Point(Width / 2, Height / 2));
    }

    public void DeleteBlock(IBlock block)
    {
        ArgumentNullException.ThrowIfNull(block);
        Graph?.RemoveNode(block);
        Invalidate();
    }

    public void DeleteConnection(Connection connection)
    {
        ArgumentNullException.ThrowIfNull(connection);
        Graph?.RemoveEdge(connection);
        Invalidate();
    }

    public void DeleteItem(object item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (Graph == null)
            return;

        if (item is IBlock block)
            Graph.RemoveNode(block);
        else if (item is Connection conn)
            Graph.RemoveEdge(conn);
        else
            throw new ArgumentException("Item must be either a Block or a Connection", nameof(item));

        Invalidate();
    }

    public void DeleteSelectedItem()
    {
        if (Graph?.SelectedItem == null)
            return;

        var selected = Graph.SelectedItem;

        if (selected is IBlock block)
            Graph.RemoveNode(block);
        else if (selected is Connection conn)
            Graph.RemoveEdge(conn);

        Graph.SelectedItem = null;

        SelectedItemChanged?.Invoke(this, EventArgs.Empty);

        Invalidate();
    }
    #endregion

    #region Private Methods

    private static float DistanceSq(PointF p1, PointF p2)
    {
        return (p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y);
    }

    private SocketHit? HitTestSocket(PointF worldPos)
    {
        if (Graph == null)
            return null;

        foreach (var block in Graph.Nodes.Reverse())
        {
            // Check Input Zone (Left Side)
            if (block.Inputs.Count > 0)
            {
                RectangleF inputZone = new(
                    (float)block.X - AutoSnapZoneWidth / 2,
                    (float)block.Y,
                    AutoSnapZoneWidth,
                    block.Height);

                if (inputZone.Contains(worldPos))
                {
                    return new SocketHit(block, block.Inputs[0], true, NodeRenderer.GetSocketPosition(block, true));
                }

                var pos = NodeRenderer.GetSocketPosition(block, true);
                if (DistanceSq(worldPos, pos) <= _socketRadius * _socketRadius * 4)
                {
                    return new SocketHit(block, block.Inputs[0], true, pos);
                }
            }

            // Check Output Zone (Right Side)
            if (block.Outputs.Count > 0)
            {
                RectangleF outputZone = new(
                    (float)(block.X + block.Width) - AutoSnapZoneWidth / 2,
                    (float)block.Y,
                    AutoSnapZoneWidth,
                    block.Height);

                if (outputZone.Contains(worldPos))
                {
                    return new SocketHit(block, block.Outputs[0], false, NodeRenderer.GetSocketPosition(block, false));
                }

                var pos = NodeRenderer.GetSocketPosition(block, false);
                if (DistanceSq(worldPos, pos) <= _socketRadius * _socketRadius * 4)
                {
                    return new SocketHit(block, block.Outputs[0], false, pos);
                }
            }
        }
        return null;
    }

    private Connection? HitTestEdge(PointF worldPosition)
    {
        if (Graph == null)
            return null;

        using Pen hitPen = new(Color.Black, 10);

        foreach (var edge in Graph.Edges)
        {
            using var path = NodeRenderer.GetEdgePath(edge.Source, edge.Target);
            if (path.IsOutlineVisible(worldPosition, hitPen))
            {
                return edge;
            }
        }
        return null;
    }

    private PointF ScreenToWorld(Point screenPoint)
    {
        return new PointF(
            (screenPoint.X - _panOffset.X) / _renderScale,
            (screenPoint.Y - _panOffset.Y) / _renderScale
        );
    }

    private void CenterCameraOnBlock(IBlock block)
    {
        float screenCX = Width / 2.0f;
        float screenCY = Height / 2.0f;

        float blockCX = (float)(block.X + block.Width / 2);
        float blockCY = (float)(block.Y + block.Height / 2);

        _panOffset.X = screenCX - blockCX * _renderScale;
        _panOffset.Y = screenCY - blockCY * _renderScale;

        Invalidate();
    }
    #endregion

    #region Handler Overrides
    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (Graph == null)
            return;

        PointF worldPosition = ScreenToWorld(e.Location);
        _lastMousePos = e.Location;

        if (e.Button == MouseButtons.Right)
        {
            _isPanning = true;
            Cursor = _panCursor;
        }
        else if (e.Button == MouseButtons.Left)
        {
            var socketHit = HitTestSocket(worldPosition);
            if (socketHit != null)
            {
                _isConnecting = true;
                _dragStartSocket = socketHit;
                _currentMouseWorldPos = worldPosition;
                Cursor = _connectCursor;
                Invalidate();
                base.OnMouseDown(e);
                return;
            }

            var hitNode = Workspace?.HitTestNode(worldPosition.X, worldPosition.Y);
            if (hitNode != null)
            {
                Graph.BringToTop(hitNode);
                Graph.SelectedItem = hitNode;
                SelectedItemChanged?.Invoke(this, EventArgs.Empty);

                _isDraggingNode = true;
                _draggedNode = hitNode;
                _dragStartNodePos = new PointF((float)hitNode.X, (float)hitNode.Y);
                Cursor = _dragCursor;
                Invalidate();
                base.OnMouseDown(e);
                return;
            }

            var hitEdge = HitTestEdge(worldPosition);
            if (hitEdge != null)
            {
                Graph.SelectedItem = hitEdge;
                SelectedItemChanged?.Invoke(this, EventArgs.Empty);
                Invalidate();
                base.OnMouseDown(e);
                return;
            }

            Graph.SelectedItem = null;
            SelectedItemChanged?.Invoke(this, EventArgs.Empty);
            Invalidate();
        }

        base.OnMouseDown(e);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        if (_isPanning && e.Button == MouseButtons.Right)
        {
            _isPanning = false;
            Cursor = Cursors.Default;
        }

        if (_isDraggingNode && e.Button == MouseButtons.Left)
        {
            _isDraggingNode = false;
            _draggedNode = null;
            Cursor = Cursors.Default;
        }

        if (_isConnecting && e.Button == MouseButtons.Left)
        {
            PointF worldPos = ScreenToWorld(e.Location);
            var socketHit = HitTestSocket(worldPos);

            if (socketHit != null && _dragStartSocket != null)
            {
                bool valid = true;

                if (socketHit.Block == _dragStartSocket.Block)
                    valid = false;

                if (socketHit.IsInput == _dragStartSocket.IsInput)
                    valid = false;

                if (Graph == null)
                    valid = false;

                if (valid)
                {
                    IBlock source, target;
                    Socket sourceSocket, targetSocket;

                    if (_dragStartSocket.IsInput)
                    {
                        target = _dragStartSocket.Block;
                        targetSocket = _dragStartSocket.Socket;
                        source = socketHit.Block;
                        sourceSocket = socketHit.Socket;
                    }
                    else
                    {
                        source = _dragStartSocket.Block;
                        sourceSocket = _dragStartSocket.Socket;
                        target = socketHit.Block;
                        targetSocket = socketHit.Socket;
                    }

                    bool exists = Graph!.Edges.Any(edge =>
                        edge.Source == source && edge.SourceSocket == sourceSocket &&
                        edge.Target == target && edge.TargetSocket == targetSocket);

                    if (!exists)
                    {
                        Graph.AddEdge(source, sourceSocket, target, targetSocket);
                    }
                }
            }

            _isConnecting = false;
            _dragStartSocket = null;
            Cursor = Cursors.Default;
            Invalidate();
        }

        base.OnMouseUp(e);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        float dx = e.X - _lastMousePos.X;
        float dy = e.Y - _lastMousePos.Y;

        PointF worldPosition = ScreenToWorld(e.Location);

        if (!_isPanning && !_isDraggingNode && !_isConnecting)
        {
            var hitEdge = HitTestEdge(worldPosition);
            Cursor = hitEdge != null ? Cursors.Hand : Cursors.Default;
        }

        if (_isPanning)
        {
            _panOffset.X += dx;
            _panOffset.Y += dy;
            Invalidate();
        }
        else if (_isDraggingNode && _draggedNode != null)
        {
            float worldDx = dx / _renderScale;
            float worldDy = dy / _renderScale;

            // Direct property manipulation - no ViewState needed!
            _draggedNode.X += worldDx;
            _draggedNode.Y += worldDy;

            Invalidate();
        }
        else if (_isConnecting)
        {
            _currentMouseWorldPos = worldPosition;
            Invalidate();
        }

        _lastMousePos = e.Location;

        base.OnMouseMove(e);
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        const float zoomFactor = 1.1f;
        float oldScale = _renderScale;

        if (e.Delta > 0)
            _renderScale *= zoomFactor;
        else
            _renderScale /= zoomFactor;

        _renderScale = Math.Max(0.1f, Math.Min(_renderScale, 5.0f));

        float mouseX = e.X;
        float mouseY = e.Y;

        float worldX = (mouseX - _panOffset.X) / oldScale;
        float worldY = (mouseY - _panOffset.Y) / oldScale;

        _panOffset.X = mouseX - (worldX * _renderScale);
        _panOffset.Y = mouseY - (worldY * _renderScale);

        Invalidate();

        base.OnMouseWheel(e);
    }

    protected override void OnDragEnter(DragEventArgs e)
    {
        _isDragOver = false;
        e.Effect = DragDropEffects.None;

        if (e.Data is null)
        {
            Debug.WriteLine("[DragDrop] No data present in drag event.");
            base.OnDragEnter(e);
            return;
        }

        try
        {
            var rawData = e.Data.GetData(e.Data.GetFormats()[0]);
            if (rawData is Type t && typeof(IBlock).IsAssignableFrom(t))
            { 
                e.Effect = DragDropEffects.Copy;
                _ghostBlockType = t;
                _isDragOver = true;
                Debug.WriteLine($"[DragDrop] Accepted: {t.Name}");
                return;
            }
        }
        catch { /* Ignore */ }

        base.OnDragEnter(e);
    }

    protected override void OnDragOver(DragEventArgs e)
    {
        if (_isDragOver)
        {
            var clientPoint = PointToClient(new Point(e.X, e.Y));
            _ghostPosition = ScreenToWorld(clientPoint);
            Invalidate();
        }
        base.OnDragOver(e);
    }

    protected override void OnDragLeave(EventArgs e)
    {
        _isDragOver = false;
        _ghostBlockType = null;
        Invalidate();
        base.OnDragLeave(e);
    }

    protected override void OnDragDrop(DragEventArgs e)
    {
        if (_isDragOver && _ghostBlockType != null && Graph != null)
        {
            try
            {
                var newBlock = (IBlock?)Activator.CreateInstance(_ghostBlockType);
                if (newBlock != null)
                {
                    // Direct property assignment - no ViewState needed!
                    newBlock.X = _ghostPosition.X - 100;
                    newBlock.Y = _ghostPosition.Y - 50;
                    newBlock.Width = 200;
                    newBlock.Height = 100;

                    Graph.AddBlock(newBlock);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to create block: {ex.Message}");
            }
        }

        _isDragOver = false;
        _ghostBlockType = null;
        Invalidate();

        base.OnDragDrop(e);
    }
    #endregion

    #region Render Override
    protected override void OnPaint(PaintEventArgs e)
    {
        if (Graph == null)
            return;

        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        using Matrix transform = new();
        transform.Translate(_panOffset.X, _panOffset.Y);
        transform.Scale(_renderScale, _renderScale);
        g.Transform = transform;

        // Draw Connections
        foreach (var edge in Graph.Edges)
        {
            bool isSelected = Graph.SelectedItem is Connection conn && edge == conn;
            NodeRenderer.Instance.DrawEdge(g, edge.Source, edge.Target, isSelected, _socketRadius);
        }

        // Draw Pending Connection Drag
        if (_isConnecting && _dragStartSocket != null)
        {
            NodeRenderer.Instance.DrawDragEdge(g, _dragStartSocket.Position, _currentMouseWorldPos);
        }

        // Draw Nodes
        foreach (var block in Graph.Nodes)
        {
            bool isSelected = block == Graph.SelectedItem;
            NodeRenderer.Instance.DrawNode(g, block, isSelected, _selectedBlockOutlineColor, _socketRadius);
        }

        // Draw Drag & Drop Ghost
        if (_isDragOver && _ghostBlockType != null)
        {
            float ghostWidth = 150;
            float ghostHeight = 80;

            float drawX = _ghostPosition.X - ghostWidth / 2;
            float drawY = _ghostPosition.Y - ghostHeight / 2;

            using var brush = new SolidBrush(Color.FromArgb(100, 200, 200, 255));
            using var pen = new Pen(Color.Blue, 2) { DashStyle = DashStyle.Dash };
            using var textBrush = new SolidBrush(Color.Blue);
            using var font = new Font("Segoe UI", 9, FontStyle.Bold);

            g.FillRectangle(brush, drawX, drawY, ghostWidth, ghostHeight);
            g.DrawRectangle(pen, drawX, drawY, ghostWidth, ghostHeight);
            g.DrawString($"New {_ghostBlockType.Name}", font, textBrush, drawX + 5, drawY + 5);
            g.FillEllipse(Brushes.Gray, drawX - 3, drawY + 10, 6, 6);
            g.FillEllipse(Brushes.Gray, drawX + ghostWidth - 3, drawY + 10, 6, 6);
        }

        g.ResetTransform();

        base.OnPaint(e);
    }
    #endregion
}