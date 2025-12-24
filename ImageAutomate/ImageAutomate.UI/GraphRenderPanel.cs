/**
 * GraphRenderPanel.cs
 * 
 * Panel-based control for rendering pipeline graph
 */

using System.ComponentModel;
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
/// Custom panel for rendering and interacting with a pipeline graph. Supports node visualization, layout,
/// selection, zooming, and panning operations.
/// </summary>
/// <remarks>GraphRenderPanel enables visualization and manipulation of directed graphs composed of blocks and
/// connections. It supports interactive features such as selecting nodes, centering the view, zooming, and panning,
/// with configurable appearance and layout options. The panel automatically updates its rendering when relevant
/// properties or the underlying graph change. Thread safety is not guaranteed; all interactions should occur on the UI
/// thread.</remarks>
public class GraphRenderPanel : Panel
{
    #region Exposed Properties

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

    private PointF _panOffset = new(0, 0);
    private Point _lastMousePos;

    #region Interaction States
    private bool _isPanning;
    private bool _isDraggingNode;
    private IBlock? _draggedNode;
    private PointF _dragStartNodePos;
    private Point _mouseDownLocation;

    private bool _isConnecting;
    private SocketHit? _dragStartSocket;
    private PointF _currentMouseWorldPos;
    #endregion

    #region Cursors
    private Cursor _panCursor = Cursors.SizeAll;
    private Cursor _dragCursor = Cursors.Hand;
    private Cursor _connectCursor = Cursors.Cross;
    #endregion

    public GraphRenderPanel()
    {
        DoubleBuffered = true;
        BackColor = Color.White;
    }

    #region Public API

    /// <summary>
    /// Adds the specified blocks to the graph if not already present and connects the
    /// source block's output socket to the destination block's input socket.
    /// </summary>
    /// <remarks>If either block is not already part of the graph, it will be added automatically before the
    /// connection is made.</remarks>
    /// <param name="sourceBlock">The block that provides the output socket to be connected. Cannot be null.</param>
    /// <param name="sourceSocket">The output socket on the source block to connect. Must be an Output of sourceBlock.</param>
    /// <param name="destBlock">The block that receives the input socket connection. Cannot be null.</param>
    /// <param name="destSocket">The input socket on the destination block to connect. Must be an Input of destBlock.</param>
    /// <exception cref="Exception">Thrown if the specified source socket is not present in the source block's Outputs collection, or if the
    /// destination socket is not present in the destination block's Inputs collection.</exception>
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

        // TODO: Implement proper exception type for mismatching block and socket.
        // Note: Temporary solution using ArgumentException.
        if (!sourceBlock.Outputs.Contains(sourceSocket))
            throw new ArgumentException($"Invalid socket {sourceSocket.Id} for block {sourceBlock.Name}", nameof(sourceSocket));

        if (!destBlock.Inputs.Contains(destSocket))
            throw new ArgumentException($"Invalid socket {destSocket.Id} for block {destBlock.Name}", nameof(destSocket));

        Graph.AddEdge(sourceBlock, sourceSocket, destBlock, destSocket);

        Invalidate();
    }

    /// <summary>
    /// Connects the specified source socket of the Selected block to a receiving block and socket within the graph.
    /// </summary>
    /// <remarks>If the graph or the Selected block is not initialized, the method returns immediately.</remarks>
    /// <param name="sourceSocket">The socket in the Selected block from which the connection originates.</param>
    /// <param name="destBlock">The block that will be connected to the source socket.</param>
    /// <param name="destSocket">The socket in the destination block to which the connection is made.</param>
    public void AddSuccessor(Socket sourceSocket, IBlock destBlock, Socket destSocket)
    {
        ArgumentNullException.ThrowIfNull(destBlock);
        ArgumentNullException.ThrowIfNull(sourceSocket);
        ArgumentNullException.ThrowIfNull(destSocket);

        if (Graph?.SelectedItem is null)
            return;

        if (Graph.SelectedItem is IBlock selectBlock)
            AddBlockAndConnect(selectBlock, sourceSocket, destBlock, destSocket);
    }

    /// <summary>
    /// Connects the specified source block to the Selected block of the graph
    /// </summary>
    /// <remarks>If the graph or its center block is null, the method returns immediately.</remarks>
    /// <param name="sourceBlock">The block to be added as a predecessor and connected to the center block.</param>
    /// <param name="sourceSocket">The socket on the source block used for the connection.</param>
    /// <param name="destSocket">The socket on the center block that will be connected to the source block.</param>
    public void AddPredecessor(IBlock sourceBlock, Socket sourceSocket, Socket destSocket)
    {
        ArgumentNullException.ThrowIfNull(sourceBlock);
        ArgumentNullException.ThrowIfNull(sourceSocket);
        ArgumentNullException.ThrowIfNull(destSocket);

        if (Graph?.SelectedItem is null)
            return;

        if (Graph.SelectedItem is IBlock selectBlock)
            AddBlockAndConnect(sourceBlock, sourceSocket, selectBlock, destSocket);
    }

    /// <summary>
    /// Gets the world coordinates of the center point of the current viewport.
    /// </summary>
    /// <returns>The world coordinates corresponding to the center of the viewport.</returns>
    public PointF GetViewportCenterWorld()
    {
        return ScreenToWorld(new Point(Width / 2, Height / 2));
    }

    /// <summary>
    /// Deletes the specified block from the graph.
    /// </summary>
    /// <param name="block">Block to remove</param>
    public void DeleteBlock(IBlock block)
    {
        ArgumentNullException.ThrowIfNull(block);

        if (Graph?.SelectedItem == null)
            return;

        Graph.RemoveNode(block);

        Invalidate();
    }

    /// <summary>
    /// Deletes the specified connection from the graph.
    /// </summary>
    /// <param name="connection">Connection to remove</param>
    public void DeleteConnection(Core.Connection connection)
    {
        ArgumentNullException.ThrowIfNull(connection);
        if (Graph == null)
            return;

        Graph.RemoveEdge(connection);

        Invalidate();
    }

    /// <summary>
    /// Deletes the specified item from the graph. Item must be either a Block or a Connection.
    /// </summary>
    /// <param name="item">item to remove</param>
    public void DeleteItem(object item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (Graph == null)
            return;

        if (item is IBlock block)
            Graph.RemoveNode(block);
        else if (item is Core.Connection conn)
            Graph.RemoveEdge(conn);
        else
            throw new ArgumentException("Item must be either a Block or a Connection", nameof(item));

        Invalidate();
    }

    /// <summary>
    /// Deletes the currently selected item from the graph.
    /// </summary>
    public void DeleteSelectedItem()
    {
        if (Graph?.SelectedItem == null)
            return;

        var selected = Graph.SelectedItem;

        if (selected is IBlock block)
            Graph.RemoveNode(block);
        else if (selected is Core.Connection conn)
            Graph.RemoveEdge(conn);

        Invalidate();
    }

    #endregion


    #region Implicit Event Handlers

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

        var viewState = Workspace?.ViewState;
        if (viewState == null)
            return null;

        // Reverse order to match top-down visual
        foreach (var block in Graph.Nodes.Reverse())
        {
            Position blockPos = viewState.GetBlockPositionOrDefault(block);
            Core.Size blockSize = viewState.GetBlockSizeOrDefault(block);

            // Check Input Zone (Left Side)
            if (block.Inputs.Count > 0)
            {

                RectangleF inputZone = new(
                    (float)blockPos.X - AutoSnapZoneWidth / 2,
                    (float)blockPos.Y,
                    AutoSnapZoneWidth,
                    blockSize.Height);

                if (inputZone.Contains(worldPos))
                {
                    // Return first input (Assuming single input for PoC)
                    // If multiple, would find closest Y
                    return new SocketHit(block, block.Inputs[0], true, NodeRenderer.GetSocketPosition(blockPos, blockSize, true));
                }

                // Also check strict circle for precision if outside zone (e.g. just slightly off)
                var pos = NodeRenderer.GetSocketPosition(blockPos, blockSize, true);
                if (DistanceSq(worldPos, pos) <= _socketRadius * _socketRadius * 4)
                {
                    return new SocketHit(block, block.Inputs[0], true, pos);
                }
            }

            // Check Output Zone (Right Side)
            if (block.Outputs.Count > 0)
            {
                RectangleF outputZone = new(
                    (float)(blockPos.X + blockSize.Width) - AutoSnapZoneWidth / 2,
                    (float)blockPos.Y,
                    AutoSnapZoneWidth,
                    blockSize.Height);

                if (outputZone.Contains(worldPos))
                {
                    return new SocketHit(block, block.Outputs[0], false, NodeRenderer.GetSocketPosition(blockPos, blockSize, false));
                }

                var pos = NodeRenderer.GetSocketPosition(blockPos, blockSize, false);
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

        var viewState = Workspace?.ViewState;
        if (viewState == null)
            return null;

        // Check edges. Order might matter if they overlap, but usually doesn't.
        using Pen hitPen = new Pen(Color.Black, 10); // Wide pen for hit testing

        foreach (var edge in Graph.Edges)
        {
            var sourcePos = viewState.GetBlockPositionOrDefault(edge.Source);
            var sourceSize = viewState.GetBlockSizeOrDefault(edge.Source);
            var targetPos = viewState.GetBlockPositionOrDefault(edge.Target);
            var targetSize = viewState.GetBlockSizeOrDefault(edge.Target);
            using var path = NodeRenderer.GetEdgePath(sourcePos, sourceSize, targetPos, targetSize);
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

    private Matrix GetWorldToScreenMatrix()
    {
        Matrix matrix = new();

        // Translate to center + pan offset
        matrix.Translate(Width / 2f + _panOffset.X, Height / 2f + _panOffset.Y);

        // Scale (MSAGL using flipped y)
        matrix.Scale(_renderScale, -_renderScale);

        return matrix;
    }

    private void CenterCameraOnBlock(IBlock block)
    {
        var viewState = Workspace?.ViewState;
        if (viewState == null)
            return;

        var blockPos = viewState.GetBlockPositionOrDefault(block);
        var blockSize = viewState.GetBlockSizeOrDefault(block);
        float screenCX = Width / 2.0f;
        float screenCY = Height / 2.0f;

        float blockCX = (float)(blockPos.X + blockSize.Width / 2);
        float blockCY = (float)(blockPos.Y + blockSize.Height / 2);

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
        _mouseDownLocation = e.Location;

        if (e.Button == MouseButtons.Right)
        {
            _isPanning = true;
            Cursor = _panCursor;
        }
        else if (e.Button == MouseButtons.Left)
        {
            // 1. Check Socket Hit (Connecting)
            var socketHit = HitTestSocket(worldPosition);
            if (socketHit != null)
            {
                _isConnecting = true;
                _dragStartSocket = socketHit;
                _currentMouseWorldPos = worldPosition;
                Cursor = _connectCursor;
                Invalidate();
                return;
            }

            // 2. Check Node Hit (Selection / Dragging)
            var hitNode = Workspace?.HitTestNode(worldPosition.X, worldPosition.Y);
            if (hitNode != null && Workspace?.ViewState != null)
            {
                Graph.BringToTop(hitNode);
                Graph.SelectedItem = hitNode;
                var blockPos = Workspace.ViewState.GetBlockPositionOrDefault(hitNode);

                _isDraggingNode = true;
                _draggedNode = hitNode;
                _dragStartNodePos = new PointF((float)blockPos.X, (float)blockPos.Y);
                Cursor = _dragCursor;
                Invalidate();
                return;
            }

            // 3. Check Edge Hit (Selection)
            var hitEdge = HitTestEdge(worldPosition);
            if (hitEdge != null)
            {
                Graph.SelectedItem = hitEdge;
                Invalidate();
                return;
            }

            // 4. Hit Background (Deselect)
            Graph.SelectedItem = null;
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
            // End Connection Drag
            PointF worldPos = ScreenToWorld(e.Location);
            var socketHit = HitTestSocket(worldPos);

            if (socketHit != null && _dragStartSocket != null)
            {
                // Validate Connection
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

                    // Prevent duplicate edges
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

        // Hover Feedback Logic
        // If not dragging, check if over edge to change cursor
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

            Workspace?
                .ViewState
                .SetBlockPosition(_draggedNode, new Position(
                    _dragStartNodePos.X + worldDx,
                    _dragStartNodePos.Y + worldDy));

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
        // TODO: Move this out as configurable property
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

    protected override void OnPaint(PaintEventArgs e)
    {
        if (Graph == null)
            return;

        var viewState = Workspace?.ViewState;
        if (viewState == null)
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
            var sourcePos = viewState.GetBlockPositionOrDefault(edge.Source);
            var sourceSize = viewState.GetBlockSizeOrDefault(edge.Source);
            var targetPos = viewState.GetBlockPositionOrDefault(edge.Target);
            var targetSize = viewState.GetBlockSizeOrDefault(edge.Target);
            bool isSelected = Graph.SelectedItem is Connection conn
                && edge == conn;
            NodeRenderer.Instance.DrawEdge(g, sourcePos, sourceSize, targetPos, targetSize, isSelected, _socketRadius);
        }

        // Draw Pending Connection Drag
        if (_isConnecting && _dragStartSocket != null)
        {
            NodeRenderer.Instance.DrawDragEdge(g, _dragStartSocket.Position, _currentMouseWorldPos);
        }

        // Draw Nodes
        foreach (var block in Graph.Nodes)
        {
            var blockPos = viewState.GetBlockPositionOrDefault(block);
            var blockSize = viewState.GetBlockSizeOrDefault(block);
            bool isSelected = block == Graph.SelectedItem;
            NodeRenderer.Instance.DrawNode(g, block, blockPos, blockSize, isSelected, _selectedBlockOutlineColor, _socketRadius);
        }

        g.ResetTransform();

        base.OnPaint(e);
    }
    #endregion
}