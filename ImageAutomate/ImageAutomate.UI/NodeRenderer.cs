using System.Drawing.Drawing2D;

using ImageAutomate.Core;

using GeomEdge = Microsoft.Msagl.Core.Layout.Edge;
using GeomNode = Microsoft.Msagl.Core.Layout.Node;

namespace ImageAutomate.UI;

public sealed class NodeRenderer: IDisposable
{
    public static readonly NodeRenderer Instance = new();

    #region Fields
    private Pen _edgePen;
    private SolidBrush _bgBrush;
    private SolidBrush _headerBrush;
    private SolidBrush _textBrush;
    private Pen _borderPenNormal;
    private Font _labelFont;
    private Font _detailFont;
    private SolidBrush _socketInputBrush;
    private SolidBrush _socketOutputBrush;
    private Pen _socketBorderPen;
    private bool _isDisposed;
    #endregion

    private NodeRenderer()
    {
        var edgeColor = Color.FromArgb(150, 150, 150);
        _edgePen = new Pen(edgeColor, 2);

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

    public void DrawEdge(Graphics g, GeomEdge geomEdge, double socketRadius)
    {
        var sourceNode = geomEdge.Source;
        var targetNode = geomEdge.Target;

        if (sourceNode?.BoundingBox == null || targetNode?.BoundingBox == null)
            return;

        // Calculate socket positions based on node boundaries
        // Input is on the Left, Output is on the Right

        // Source (Output) -> Right side
        PointF start = new(
            (float)sourceNode.BoundingBox.Right,
            (float)sourceNode.Center.Y
        );

        // Target (Input) -> Left side
        PointF end = new(
            (float)targetNode.BoundingBox.Left,
            (float)targetNode.Center.Y
        );

        // Use a bezier curve for nicer connections
        float controlPointOffset = Math.Max(Math.Abs(end.X - start.X) / 2, 50);

        PointF cp1 = new(start.X + controlPointOffset, start.Y);
        PointF cp2 = new(end.X - controlPointOffset, end.Y);

        g.DrawBezier(_edgePen, start, cp1, cp2, end);
    }

    public void DrawNodeDirect(Graphics g, GeomNode geomNode, bool isSelected, Color selectionColor, double socketRadius)
    {
        if (geomNode.UserData is not IBlock block)
            return;

        var bounds = geomNode.BoundingBox;
        RectangleF rect = new(
            (float)bounds.Left,
            (float)bounds.Bottom,
            (float)bounds.Width,
            (float)bounds.Height
        );

        float radius = 8;
        var state = g.Save();

        using (var path = CreateRoundedRectPath(rect, radius))
        {
            g.FillPath(_bgBrush, path);
            if (isSelected)
            {
                using var borderPen = new Pen(selectionColor, 3);
                g.DrawPath(borderPen, path);
            }
            else
            {
                g.DrawPath(_borderPenNormal, path);
            }
        }

        RectangleF headerRect = new(rect.X, rect.Bottom - 25, rect.Width, 25);

        using (var headerPath = CreateRoundedRectPath(headerRect, radius, topOnly: true))
        {
            g.FillPath(_headerBrush, headerPath);
        }

        g.Restore(state);
        state = g.Save();

        // Flip for text rendering
        using (var flipMatrix = new Matrix(1, 0, 0, -1, 0, 2 * (rect.Y + rect.Height / 2)))
        {
            g.MultiplyTransform(flipMatrix);
        }

        // Text Coordinates in Flipped Space
        g.DrawString(block.Title, _labelFont, _textBrush, new PointF(rect.X + 10, rect.Y + 5));

        float yOffset = rect.Y + 35;
        string[] lines = block.Content.Split('\n');
        foreach (var line in lines)
        {
            g.DrawString(line, _detailFont, _textBrush, new PointF(rect.X + 10, yOffset));
            yOffset += 15;
        }

        g.Restore(state);
        state = g.Save();

        // Draw Sockets
        DrawSocket(g, new PointF(rect.Left, rect.Top + rect.Height / 2), isInput: true, socketRadius);
        DrawSocket(g, new PointF(rect.Right, rect.Top + rect.Height / 2), isInput: false, socketRadius);

        g.Restore(state);
    }

    public void DrawNodeOptimized(Graphics g, GeomNode geomNode, bool isSelected, Color selectionColor, double socketRadius)
    {
        if (geomNode.UserData is not IBlock block) return;

        var bounds = geomNode.BoundingBox;
        RectangleF rect = new(
            (float)bounds.Left,
            (float)bounds.Bottom,
            (float)bounds.Width,
            (float)bounds.Height
        );
        float radius = 8;

        // Create the combined path for the background
        using var mainPath = CreateRoundedRectPath(rect, radius);

        // Header at Visual Top (MSAGL Top)
        RectangleF headerRect = new(rect.X, rect.Bottom - 25, rect.Width, 25);
        using var headerPath = CreateRoundedRectPath(headerRect, radius, topOnly: true);

        // Drawing - Shape Layer
        // Use cached brushes
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
        var state = g.Save();
        using (var flipMatrix = new Matrix(1, 0, 0, -1, 0, 2 * (rect.Y + rect.Height / 2)))
        {
            g.MultiplyTransform(flipMatrix);
        }

        // Use cached fonts and brushes
        g.DrawString(block.Title, _labelFont, _textBrush, new PointF(rect.X + 10, rect.Y + 5));

        float yOffset = rect.Y + 35;
        string[] lines = block.Content.Split('\n');
        foreach (var line in lines)
        {
            g.DrawString(line, _detailFont, _textBrush, new PointF(rect.X + 10, yOffset));
            yOffset += 15;
        }

        g.Restore(state);

        DrawSocket(g, new PointF(rect.Left, rect.Top + rect.Height / 2), isInput: true, socketRadius);
        DrawSocket(g, new PointF(rect.Right, rect.Top + rect.Height / 2), isInput: false, socketRadius);
    }

    // Shared Helper Methods
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
