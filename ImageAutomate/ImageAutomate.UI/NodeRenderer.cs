using System.Drawing.Drawing2D;

using ImageAutomate.Core;

using GeomEdge = Microsoft.Msagl.Core.Layout.Edge;
using GeomNode = Microsoft.Msagl.Core.Layout.Node;

namespace ImageAutomate.UI;

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

    private NodeRenderer(Workspace? workspace = null)
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

    public static PointF GetSocketPosition(Position blockPosition, Core.Size blockSize, bool isInput)
    {
        if (isInput)
        {
            return new PointF((float)blockPosition.X, (float)(blockPosition.Y + blockSize.Height / 2));
        }
        else
        {
            return new PointF((float)(blockPosition.X + blockSize.Width), (float)(blockPosition.Y + blockSize.Height / 2));
        }
    }

    public static GraphicsPath GetEdgePath(Position sourcePos, Core.Size sourceSize, Position targetPos, Core.Size targetSize)
    {
        PointF start = GetSocketPosition(sourcePos, sourceSize, isInput: false);
        PointF end = GetSocketPosition(targetPos, targetSize, isInput: true);
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

    public void DrawEdge(Graphics g, Position sourcePosition, Core.Size sourceSize, Position targetPosition, Core.Size targetSize, bool isSelected, double socketRadius)
    {
        using var path = GetEdgePath(sourcePosition, sourceSize, targetPosition, targetSize);
        g.DrawPath(isSelected ? _selectedEdgePen : _edgePen, path);
    }

    public void DrawDragEdge(Graphics g, PointF start, PointF end)
    {
        DrawBezier(g, _dragEdgePen, start, end);
    }

    private static void DrawBezier(Graphics g, Pen pen, PointF start, PointF end)
    {
        float controlPointOffset = Math.Max(Math.Abs(end.X - start.X) / 2, 50);

        PointF cp1 = new(start.X + controlPointOffset, start.Y);
        PointF cp2 = new(end.X - controlPointOffset, end.Y);

        g.DrawBezier(pen, start, cp1, cp2, end);
    }

    public void DrawNode(Graphics g, IBlock block, Position blockPosition, Core.Size blockSize, bool isSelected, Color selectionColor, double socketRadius)
    {
        RectangleF rect = new(
            (float)blockPosition.X,
            (float)blockPosition.Y,
            blockSize.Width,
            blockSize.Height
        );

        float radius = 8;

        // Cache path for background
        using var mainPath = CreateRoundedRectPath(rect, radius);

        // Header
        RectangleF headerRect = new(rect.X, rect.Y, rect.Width, 25);
        using var headerPath = CreateRoundedRectPath(headerRect, radius, topOnly: true);

        // Drawing - Shape Layer
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

        // Drawing - Text Layer
        g.DrawString(block.Title, _labelFont, _textBrush, new PointF(rect.X + 10, rect.Y + 5));

        float yOffset = rect.Y + 35;
        string[] lines = block.Content.Split('\n');
        foreach (var line in lines)
        {
            g.DrawString(line, _detailFont, _textBrush, new PointF(rect.X + 10, yOffset));
            yOffset += 15;
        }

        // Draw Sockets
        // Input (Left)
        if (block.Inputs.Count > 0)
        {
            DrawSocket(g, GetSocketPosition(blockPosition, blockSize, true), isInput: true, socketRadius);
        }
        // Output (Right)
        if (block.Outputs.Count > 0)
        {
            DrawSocket(g, GetSocketPosition(blockPosition, blockSize, false), isInput: false, socketRadius);
        }
    }

    #region Static Helper Methods

    private static GraphicsPath CreateRoundedRectPath(RectangleF rect, float radius, bool topOnly = false)
    {
        GraphicsPath path = new();
        float diameter = radius * 2;

        // MSAGL coords (Y-up)
        float left = rect.X;
        float bottom = rect.Y;
        float right = rect.Right;
        float top = rect.Bottom; // RectangleF.Bottom is Y + Height

        path.StartFigure();

        if (topOnly)
        {
            // Top Right (Rounded)
            path.AddArc(right - diameter, top - diameter, diameter, diameter, 0, 90);
            // Top Left (Rounded)
            path.AddArc(left, top - diameter, diameter, diameter, 90, 90);

            // Line down to header bottom
            path.AddLine(left, top - diameter + radius, left, bottom);
            // Line across bottom
            path.AddLine(left, bottom, right, bottom);
            // Line up
            path.AddLine(right, bottom, right, top - diameter + radius);
        }
        else
        {
            // Top Right Corner
            path.AddArc(right - diameter, top - diameter, diameter, diameter, 0, 90);
            // Top Left Corner
            path.AddArc(left, top - diameter, diameter, diameter, 90, 90);
            // Bottom Left Corner
            path.AddArc(left, bottom, diameter, diameter, 180, 90);
            // Bottom Right Corner
            path.AddArc(right - diameter, bottom, diameter, diameter, 270, 90);
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

    #endregion

    public void Dispose()
    {
        if (_isDisposed) return;

        _edgePen?.Dispose();
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
