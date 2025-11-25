using Microsoft.Msagl.Core.Layout;
using System.Drawing.Drawing2D;
using GeomNode = Microsoft.Msagl.Core.Layout.Node;
using GeomEdge = Microsoft.Msagl.Core.Layout.Edge;

namespace ConvertBlockPoC;

public static class NodeRenderer
{
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

        using var edgePen = new Pen(Color.FromArgb(150, 150, 150), 2);
        g.DrawBezier(edgePen, start, cp1, cp2, end);
    }

    public static class DirectStrategy
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
            var state = g.Save();

            using (var bgBrush = new SolidBrush(Color.FromArgb(60, 60, 60)))
            using (var borderPen = new Pen(
                isSelected ? selectionColor : Color.FromArgb(100, 100, 100),
                isSelected ? 3 : 2))
            using (var path = CreateRoundedRectPath(rect, radius))
            {
                g.FillPath(bgBrush, path);
                g.DrawPath(borderPen, path);
            }

            // Header at the Top (Visually) -> MSAGL Top (rect.Bottom)
            // MSAGL Y-Up: Rect goes from Y to Y+H.
            // Visual Top is Y+H.
            // So HeaderRect should be from Y+H-25 to Y+H.
            RectangleF headerRect = new(rect.X, rect.Bottom - 25, rect.Width, 25);

            using (var headerBrush = new SolidBrush(Color.FromArgb(80, 80, 80)))
            using (var headerPath = CreateRoundedRectPath(headerRect, radius, topOnly: true))
            {
                g.FillPath(headerBrush, headerPath);
            }

            g.Restore(state);
            state = g.Save();

            // Flip for text rendering
            using (var flipMatrix = new Matrix(1, 0, 0, -1, 0, 2 * (rect.Y + rect.Height / 2)))
            {
                g.MultiplyTransform(flipMatrix);
            }

            using (var textBrush = new SolidBrush(Color.White))
            using (var labelFont = new Font("Segoe UI", 10, FontStyle.Bold))
            using (var detailFont = new Font("Segoe UI", 8))
            {
                // Text Coordinates in Flipped Space:
                // Visual Top (Screen -Y) maps to MSAGL Top (Y_max).
                // Local Flipped Transform: Y -> -Y + 2C.
                // Input Y_max -> -Y_max + 2C = Y_min.
                // So drawing at rect.Y (Y_min) puts text at Visual Top.

                // Label (Header Text)
                g.DrawString(block.Name, labelFont, textBrush,
                    new PointF(rect.X + 10, rect.Y + 5));

                // Content Text (Below Header)
                // Header is 25px.
                // Start content at 35px down from Visual Top.
                float yOffset = rect.Y + 35;

                string[] lines = block.ConfigurationSummary.Split('\n');
                foreach (var line in lines)
                {
                    g.DrawString(line, detailFont, textBrush,
                        new PointF(rect.X + 10, yOffset));
                    yOffset += 15;
                }
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
            using (var bgBrush = new SolidBrush(Color.FromArgb(60, 60, 60)))
            using (var headerBrush = new SolidBrush(Color.FromArgb(80, 80, 80)))
            using (var borderPen = new Pen(
                isSelected ? selectionColor : Color.FromArgb(100, 100, 100),
                isSelected ? 3 : 2))
            {
                g.FillPath(bgBrush, mainPath);
                g.FillPath(headerBrush, headerPath);
                g.DrawPath(borderPen, mainPath);
            }

            // Drawing - Text Layer
            var state = g.Save();
            using (var flipMatrix = new Matrix(1, 0, 0, -1, 0, 2 * (rect.Y + rect.Height / 2)))
            {
                g.MultiplyTransform(flipMatrix);
            }

            using (var textBrush = new SolidBrush(Color.White))
            using (var labelFont = new Font("Segoe UI", 10, FontStyle.Bold))
            using (var detailFont = new Font("Segoe UI", 8))
            {
                // Label in Header
                g.DrawString(block.Name, labelFont, textBrush,
                    new PointF(rect.X + 10, rect.Y + 5));

                // Content below Header
                float yOffset = rect.Y + 35;
                string[] lines = block.ConfigurationSummary.Split('\n');
                foreach (var line in lines)
                {
                    g.DrawString(line, detailFont, textBrush,
                        new PointF(rect.X + 10, yOffset));
                    yOffset += 15;
                }
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

        // Top Right Corner
        path.AddArc(right - diameter, top - diameter, diameter, diameter, 0, 90);
        // Top Left Corner
        path.AddArc(left, top - diameter, diameter, diameter, 90, 90);

        // Bottom Left Corner
        path.AddArc(left, bottom, diameter, diameter, 180, 90);
        // Bottom Right Corner
        path.AddArc(right - diameter, bottom, diameter, diameter, 270, 90);

        path.CloseFigure();

        if (topOnly)
        {
            // Header logic:
            // The input `rect` for header is defined as the Header Strip itself (Height 25).
            // So `rect.Y` (bottom of header) is MSAGL Top - 25.
            // `rect.Bottom` (top of header) is MSAGL Top.

            // We want the Top corners to be rounded (matching the node).
            // The Bottom corners to be square (connecting to the body).

            path.Reset();

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

            path.CloseFigure();
        }

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

        var socketColor = isInput
            ? Color.FromArgb(100, 200, 100)
            : Color.FromArgb(200, 100, 100);

        using var socketBrush = new SolidBrush(socketColor);
        using var socketBorder = new Pen(Color.White, 1.5f);
        g.FillEllipse(socketBrush, socketRect);
        g.DrawEllipse(socketBorder, socketRect);
    }
}
