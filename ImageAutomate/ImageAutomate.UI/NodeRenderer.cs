using System.Drawing.Drawing2D;

using GeomNode = Microsoft.Msagl.Core.Layout.Node;
using GeomEdge = Microsoft.Msagl.Core.Layout.Edge;

using ImageAutomate.Core;

namespace ImageAutomate.UI;

public static class NodeRenderer
{
    // Cached GDI+ resources to avoid allocation in the render loop.
    private static readonly Color EdgeColor = Color.FromArgb(150, 150, 150);
    private static readonly Pen EdgePen = new(EdgeColor, 2);

    private static readonly SolidBrush BgBrush = new(Color.FromArgb(60, 60, 60));
    private static readonly SolidBrush HeaderBrush = new(Color.FromArgb(80, 80, 80));
    private static readonly SolidBrush TextBrush = new(Color.White);

    private static readonly Pen BorderPenNormal = new(Color.FromArgb(100, 100, 100), 2);
    private static readonly Pen BorderPenSelected = new(Color.Red, 3); // Will be updated if color changes, but here we assume static for trivial opt.

    // Note: If SelectedBlockOutlineColor is dynamic per panel, we can't cache the selected pen globally easily without a map or recreating it.
    // However, for "trivial optimization", caching the common brushes/pens is a big win.
    // We will keep creating the selected pen if color varies, or just cache a default one.
    // The previous code passed `selectionColor` as an argument.
    // We will cache common colors. For dynamic ones, we still create them or use a Dictionary cache.
    // To keep it simple and trivial, we'll cache the static ones.

    private static readonly Font LabelFont = new("Segoe UI", 10, FontStyle.Bold);
    private static readonly Font DetailFont = new("Segoe UI", 8);

    private static readonly SolidBrush SocketInputBrush = new(Color.FromArgb(100, 200, 100));
    private static readonly SolidBrush SocketOutputBrush = new(Color.FromArgb(200, 100, 100));
    private static readonly Pen SocketBorderPen = new(Color.White, 1.5f);

    public static void DrawEdge(Graphics g, GeomEdge geomEdge, double socketRadius)
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

        g.DrawBezier(EdgePen, start, cp1, cp2, end);
    }

    public static class DirectStrategy
    {
        // DirectStrategy is less optimized by definition, but we can still use the cached resources where appropriate.
        public static void DrawNode(Graphics g, GeomNode geomNode, bool isSelected, Color selectionColor, double socketRadius)
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
            var state = g.Save();

            using (var path = CreateRoundedRectPath(rect, radius))
            {
                g.FillPath(BgBrush, path);
                if (isSelected)
                {
                    using var borderPen = new Pen(selectionColor, 3);
                    g.DrawPath(borderPen, path);
                }
                else
                {
                    g.DrawPath(BorderPenNormal, path);
                }
            }

            RectangleF headerRect = new(rect.X, rect.Bottom - 25, rect.Width, 25);

            using (var headerPath = CreateRoundedRectPath(headerRect, radius, topOnly: true))
            {
                g.FillPath(HeaderBrush, headerPath);
            }

            g.Restore(state);
            state = g.Save();

            // Flip for text rendering
            using (var flipMatrix = new Matrix(1, 0, 0, -1, 0, 2 * (rect.Y + rect.Height / 2)))
            {
                g.MultiplyTransform(flipMatrix);
            }

            // Text Coordinates in Flipped Space
            g.DrawString(block.Name, LabelFont, TextBrush, new PointF(rect.X + 10, rect.Y + 5));

            float yOffset = rect.Y + 35;
            string[] lines = block.Content.Split('\n');
            foreach (var line in lines)
            {
                g.DrawString(line, DetailFont, TextBrush, new PointF(rect.X + 10, yOffset));
                yOffset += 15;
            }

            g.Restore(state);
            state = g.Save();

            // Draw Sockets
            DrawSocket(g, new PointF(rect.Left, rect.Top + rect.Height / 2), isInput: true, socketRadius);
            DrawSocket(g, new PointF(rect.Right, rect.Top + rect.Height / 2), isInput: false, socketRadius);

            g.Restore(state);
        }
    }

    public static class OptimizedStrategy
    {
        public static void DrawNode(Graphics g, GeomNode geomNode, bool isSelected, Color selectionColor, double socketRadius)
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
            g.FillPath(BgBrush, mainPath);
            g.FillPath(HeaderBrush, headerPath);

            // For border, if selected color is standard Red, use cached, otherwise create new.
            // But the panel passes `_selectedBlockOutlineColor`.
            if (isSelected)
            {
                using var selPen = new Pen(selectionColor, 3);
                g.DrawPath(selPen, mainPath);
            }
            else
            {
                g.DrawPath(BorderPenNormal, mainPath);
            }

            // Drawing - Text Layer
            var state = g.Save();
            using (var flipMatrix = new Matrix(1, 0, 0, -1, 0, 2 * (rect.Y + rect.Height / 2)))
            {
                g.MultiplyTransform(flipMatrix);
            }

            // Use cached fonts and brushes
            g.DrawString(block.Name, LabelFont, TextBrush, new PointF(rect.X + 10, rect.Y + 5));

            float yOffset = rect.Y + 35;
            string[] lines = block.Content.Split('\n');
            foreach (var line in lines)
            {
                g.DrawString(line, DetailFont, TextBrush, new PointF(rect.X + 10, yOffset));
                yOffset += 15;
            }

            g.Restore(state);

            DrawSocket(g, new PointF(rect.Left, rect.Top + rect.Height / 2), isInput: true, socketRadius);
            DrawSocket(g, new PointF(rect.Right, rect.Top + rect.Height / 2), isInput: false, socketRadius);
        }
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

    private static void DrawSocket(Graphics g, PointF center, bool isInput, double socketRadiusVal)
    {
        float socketRadius = (float)socketRadiusVal;
        RectangleF socketRect = new(
            center.X - socketRadius,
            center.Y - socketRadius,
            socketRadius * 2,
            socketRadius * 2
        );

        var socketBrush = isInput ? SocketInputBrush : SocketOutputBrush;

        g.FillEllipse(socketBrush, socketRect);
        g.DrawEllipse(SocketBorderPen, socketRect);
    }
}
