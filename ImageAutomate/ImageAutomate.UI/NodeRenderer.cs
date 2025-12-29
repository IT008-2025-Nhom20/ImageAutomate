using System.Drawing.Drawing2D;

using ImageAutomate.Core;

namespace ImageAutomate.UI;

/// <summary>
/// Handles the low-level GDI+ rendering of nodes and edges in the graph panel.
/// </summary>
public sealed class NodeRenderer : IDisposable
{
    public static readonly NodeRenderer Instance = new();

    #region Fields
    private Pen _edgePen;
    private Pen _selectedEdgePen;
    private Pen _dragEdgePen;
    private Pen _borderPenNormal;
    private Pen _socketBorderPen;
    private SolidBrush _bgBrush;
    private SolidBrush _headerBrush;
    private SolidBrush _textBrush;
    private SolidBrush _socketInputBrush;
    private SolidBrush _socketOutputBrush;
    private Font _labelFont;
    private Font _detailFont;
    private bool _isDisposed;
    #endregion

    private NodeRenderer()
    {
        var edgeColor = Color.FromArgb(150, 150, 150);
        _edgePen = new Pen(edgeColor, 2);
        _selectedEdgePen = new Pen(Color.Red, 3);
        _dragEdgePen = new Pen(Color.Orange, 2) { DashStyle = DashStyle.Dash };

        _bgBrush = new SolidBrush(Color.FromArgb(60, 60, 60));
        _headerBrush = new SolidBrush(Color.FromArgb(80, 80, 80));
        _textBrush = new SolidBrush(Color.White);

        _borderPenNormal = new Pen(Color.FromArgb(100, 100, 100), 2);

        _labelFont = new Font("Segoe UI", 10, FontStyle.Bold);
        _detailFont = new Font("Segoe UI", 8);

        _socketInputBrush = new SolidBrush(Color.FromArgb(100, 200, 100));
        _socketOutputBrush = new SolidBrush(Color.FromArgb(200, 100, 100));
        _socketBorderPen = new Pen(Color.White, 1.5f);
    }

    /// <summary>
    /// Calculates the screen position of a socket based on the block's layout properties.
    /// </summary>
    public static PointF GetSocketPosition(IBlock block, bool isInput)
    {
        ArgumentNullException.ThrowIfNull(block);
        if (isInput)
        {
            return new PointF((float)block.X, (float)(block.Y + block.Height / 2));
        }
        else
        {
            return new PointF((float)(block.X + block.Width), (float)(block.Y + block.Height / 2));
        }
    }

    /// <summary>
    /// Generates a Bezier curve path representing an edge between two blocks.
    /// </summary>
    public static GraphicsPath GetEdgePath(IBlock source, IBlock target)
    {
        PointF start = GetSocketPosition(source, isInput: false);
        PointF end = GetSocketPosition(target, isInput: true);
        return CreateBezierPath(start, end);
    }

    private static GraphicsPath CreateBezierPath(PointF start, PointF end)
    {
        GraphicsPath path = new();
        float controlPointOffset = Math.Max(Math.Abs(end.X - start.X) / 2, 50);

        PointF cp1 = new(start.X + controlPointOffset, start.Y);
        PointF cp2 = new(end.X - controlPointOffset, end.Y);

        path.AddBezier(start, cp1, cp2, end);
        return path;
    }

    /// <summary>
    /// Draws a connection edge on the graphics surface.
    /// </summary>
    internal void DrawEdge(Graphics g, IBlock source, IBlock target, bool isSelected, double socketRadius)
    {
        using var path = GetEdgePath(source, target);
        g.DrawPath(isSelected ? _selectedEdgePen : _edgePen, path);
    }

    /// <summary>
    /// Draws a temporary edge being dragged by the user.
    /// </summary>
    internal void DrawDragEdge(Graphics g, PointF start, PointF end)
    {
        float controlPointOffset = Math.Max(Math.Abs(end.X - start.X) / 2, 50);
        PointF cp1 = new(start.X + controlPointOffset, start.Y);
        PointF cp2 = new(end.X - controlPointOffset, end.Y);
        g.DrawBezier(_dragEdgePen, start, cp1, cp2, end);
    }

    /// <summary>
    /// Draws a node (block) on the graphics surface using the block's layout properties.
    /// </summary>
    internal void DrawNode(Graphics g, IBlock block, bool isSelected, Color selectionColor, double socketRadius)
    {
        RectangleF rect = new(
            (float)block.X,
            (float)block.Y,
            block.Width,
            block.Height
        );

        float radius = 8;

        using var mainPath = CreateRoundedRectPath(rect, radius);

        RectangleF headerRect = new(rect.X, rect.Y, rect.Width, 25);
        using var headerPath = CreateRoundedRectPath(headerRect, radius, topOnly: true);

        g.FillPath(_bgBrush, mainPath);
        g.FillPath(_headerBrush, headerPath);

        if (isSelected)
        {
            using var selPen = new Pen(selectionColor, 3);
            g.DrawPath(selPen, mainPath);
        }
        else
        {
            g.DrawPath(_borderPenNormal, mainPath);
        }

        g.DrawString(block.Title, _labelFont, _textBrush, new PointF(rect.X + 10, rect.Y + 5));

        float yOffset = rect.Y + 35;
        string[] lines = block.Content.Split('\n');
        foreach (var line in lines)
        {
            g.DrawString(line, _detailFont, _textBrush, new PointF(rect.X + 10, yOffset));
            yOffset += 15;
        }

        if (block.Inputs.Count > 0)
        {
            DrawSocket(g, GetSocketPosition(block, true), isInput: true, socketRadius);
        }
        if (block.Outputs.Count > 0)
        {
            DrawSocket(g, GetSocketPosition(block, false), isInput: false, socketRadius);
        }
    }

    private static GraphicsPath CreateRoundedRectPath(RectangleF rect, float radius, bool topOnly = false)
    {
        GraphicsPath path = new();
        float diameter = radius * 2;

        float left = rect.X;
        float visualTop = rect.Y;
        float right = rect.Right;
        float visualBottom = rect.Bottom;

        path.StartFigure();

        if (topOnly)
        {
            // Visual Top Only (Rounded Top, Flat Bottom)
            path.AddArc(left, visualTop, diameter, diameter, 180, 90); // Top Left
            path.AddArc(right - diameter, visualTop, diameter, diameter, 270, 90); // Top Right
            path.AddLine(right, visualTop + radius, right, visualBottom); // Down Right
            path.AddLine(right, visualBottom, left, visualBottom); // Across Bottom
            path.AddLine(left, visualBottom, left, visualTop + radius); // Up Left
        }
        else
        {
            path.AddArc(right - diameter, visualBottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(left, visualBottom - diameter, diameter, diameter, 90, 90);
            path.AddArc(left, visualTop, diameter, diameter, 180, 90);
            path.AddArc(right - diameter, visualTop, diameter, diameter, 270, 90);
        }

        path.CloseFigure();
        return path;
    }

    private void DrawSocket(Graphics g, PointF center, bool isInput, double socketRadiusVal)
    {
        float socketRadius = (float)socketRadiusVal;
        RectangleF socketRect = new(
            center.X - socketRadius,
            center.Y - socketRadius,
            socketRadius * 2,
            socketRadius * 2
        );

        var socketBrush = isInput ? _socketInputBrush : _socketOutputBrush;

        g.FillEllipse(socketBrush, socketRect);
        g.DrawEllipse(_socketBorderPen, socketRect);
    }

    public void Dispose()
    {
        if (_isDisposed) return;

        _edgePen?.Dispose();
        _selectedEdgePen?.Dispose();
        _dragEdgePen?.Dispose();
        _bgBrush?.Dispose();
        _headerBrush?.Dispose();
        _textBrush?.Dispose();
        _borderPenNormal?.Dispose();
        _labelFont?.Dispose();
        _detailFont?.Dispose();
        _socketInputBrush?.Dispose();
        _socketOutputBrush?.Dispose();
        _socketBorderPen?.Dispose();

        _isDisposed = true;
        GC.SuppressFinalize(this);
    }
}