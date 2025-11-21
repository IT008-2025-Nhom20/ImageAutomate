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

        // Since we are outside the Node drawing, we need to adjust for the visual socket radius
        // The socket circle is drawn centered on the boundary line.
        // So the line should technically go to the center of the socket?
        // Or stop at the edge of the socket?
        // Let's go to the center of the socket.

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

            RectangleF headerRect = new(rect.X, rect.Y, rect.Width, 25);
            using (var headerBrush = new SolidBrush(Color.FromArgb(80, 80, 80)))
            using (var headerPath = CreateRoundedRectPath(headerRect, radius, topOnly: true))
            {
                g.FillPath(headerBrush, headerPath);
            }

            g.Restore(state);
            state = g.Save();

            // Flip for text rendering (Coordinate system is Y-Up, Text needs Y-Down)
            using (var flipMatrix = new Matrix(1, 0, 0, -1, 0, 2 * (rect.Y + rect.Height / 2)))
            {
                g.MultiplyTransform(flipMatrix);
            }

            using (var textBrush = new SolidBrush(Color.White))
            using (var labelFont = new Font("Segoe UI", 10, FontStyle.Bold))
            using (var detailFont = new Font("Segoe UI", 8))
            {
                g.DrawString(block.Name, labelFont, textBrush,
                    new PointF(rect.X + 10, rect.Y + 5));

                float yOffset = rect.Y + 35;

                // Handle multi-line summary
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
        // In a real production scenario, we might cache these paths in the Node/Block UserData
        // or a dictionary to avoid rebuilding them every frame if the layout hasn't changed.
        // For this PoC, "Optimized" means we build the geometry in one pass using GraphicsPath
        // to minimize GDI+ calls for the shape layers.

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

            // Create the header path
            RectangleF headerRect = new(rect.X, rect.Y, rect.Width, 25);
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
                g.DrawString(block.Name, labelFont, textBrush,
                    new PointF(rect.X + 10, rect.Y + 5));

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

            // Drawing - Socket Layer
            // Input Socket
            DrawSocket(g, new PointF(rect.Left, rect.Top + rect.Height / 2), isInput: true, socketRadius);
            // Output Socket
            DrawSocket(g, new PointF(rect.Right, rect.Top + rect.Height / 2), isInput: false, socketRadius);
        }
    }

    // Shared Helper Methods
    private static GraphicsPath CreateRoundedRectPath(RectangleF rect, float radius, bool topOnly = false)
    {
        GraphicsPath path = new();
        float diameter = radius * 2;

        // In MSAGL coords (Y-up), Top is Y+Height, Bottom is Y.
        // But RectangleF is usually (X,Y,W,H) where Y is the "top-left" in GDI+.
        // However, we are drawing in a transformed context where Y is flipped up.
        // So rect.Y is actually the BOTTOM coordinate visually if we think in Cartesian,
        // but GDI+ primitives draw assuming Y goes down.
        // Wait, we applied Scale(1, -1).
        // So drawing a rect at (10, 10, 100, 100):
        // Point (10, 10) -> transformed (10, -10).
        // Point (10, 110) -> transformed (10, -110).
        // Visual result: The rect grows "down" in screen space (because -110 is lower than -10).

        // MSAGL BoundingBox: BottomLeft (X, Y), Width, Height.
        // Our `rect` constructed in DrawNode: (Left, Bottom, Width, Height).
        // So rect.Y is the Bottom Y.

        // When we draw Arc at (rect.X, rect.Y), we are drawing at Bottom-Left corner.
        // The path construction logic needs to match the visual expectation.

        // Standard GDI+ Rounded Rect construction assumes Top-Left origin and CW winding usually.
        // If we are in a flipped Y context:
        // "Bottom" in code (rect.Y) is visually Bottom.
        // "Top" in code (rect.Y + Height) is visually Top.

        // Path construction:
        // We want the header at the "Top" (rect.Y + Height).
        // But `topOnly` logic uses `rect.Y` as the top?
        // In the original code:
        // headerRect = new(rect.X, rect.Y, rect.Width, 25);
        // topOnly=true path adds lines relative to rect.Y and rect.Bottom.

        // Let's check the original implementation carefully.
        /*
           Original:
           path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90); // Top-Left (if Y is Top)
           path.AddArc(rect.Right - diameter, rect.Y, ...); // Top-Right
        */
        // In the flipped context, we need to be careful.
        // If we use the standard path logic, but the coordinate system is flipped...
        // A standard rect (0,0, 100,100) drawn in Scale(1,-1) appears as a square from 0,0 to 100,-100.
        // Visual Top is 0. Visual Bottom is -100.

        // MSAGL Node Bounds: Center is (0,0). Width 200, Height 100.
        // Left = -100, Bottom = -50.
        // Rect = (-100, -50, 200, 100).
        // rect.Y = -50. rect.Bottom (Y+H) = 50.

        // If we draw this rect in Scale(1, -1):
        // (-100, -50) -> (-100, 50) (Screen Y) -> Visually Lower on screen (positive Y is down)
        // (-100, 50) -> (-100, -50) (Screen Y) -> Visually Higher on screen

        // So `rect.Y` (-50) is the VISUAL BOTTOM (Screen Y 50).
        // `rect.Y + Height` (50) is the VISUAL TOP (Screen Y -50).

        // The Original `CreateRoundedRectPath` used `rect.Y` for the "Top" Arcs (180, 90 start angles).
        // That implies `rect.Y` was treated as Top.
        // BUT in the original OnPaint:
        // `rect` was (bounds.Left, bounds.Bottom, Width, Height).
        // So `rect.Y` is Bottom.
        // This suggests the original code might have been drawing the header at the bottom?
        // Let's check the `flipMatrix` usage.
        // `flipMatrix` was applied for text drawing at `rect.Y + rect.Height / 2` (Center).

        // If the original code drew the header at `rect.Y`, and `rect.Y` is Bottom...
        // Then the header was at the bottom?
        // Wait, `CreateRoundedRectPath`:
        // `path.AddArc(rect.X, rect.Y, ... 180, 90)` -> Arc from 180 deg (Left) to 270 deg (Top).
        // In GDI+, +Y is Down.
        // 270 degrees is Up (-Y).
        // So this arc is the Top-Left corner.
        // So `rect.Y` is the Top coordinate for GDI+.

        // Contradiction:
        // In MSAGL, Bottom is lower value.
        // In GDI+ (untransformed), Top is lower value.
        // If `rect.Y` = -50 (Bottom in MSAGL).
        // And we treat it as Top in GDI+ path...
        // Then the path is drawn from Y=-50 downwards to Y=50.
        // Transformed by Scale(1,-1):
        // Y=-50 becomes Y=50.
        // Y=50 becomes Y=-50.
        // So the shape is flipped vertically.
        // The "Top" of the shape (GDI+ sense, Y=-50) ends up at Screen Y=50 (Visual Bottom).
        // The "Bottom" of the shape ends up at Screen Y=-50 (Visual Top).

        // So the Header (drawn at `rect.Y`) ended up at the Visual Bottom?
        // If the user says "Graph works", maybe I shouldn't break it.
        // But `ConvertBlockPoC` screenshot (if I had one) would show Header at Top.
        // Let's assume we want Header at Visual Top.

        // Visual Top corresponds to MSAGL Y_max (rect.Y + Height).
        // Transformed, Y_max becomes Screen Y_min (Top).

        // To get Header at Visual Top:
        // We need to draw it at MSAGL Y_max.
        // In the Scale(1,-1) world, we should draw at Y coordinates that are positive/high.

        // Let's reconstruct the path correctly for MSAGL coords (Up = +Y).
        // We want the rounded rect to match the MSAGL bounds.
        // Bounds: Left, Bottom, Right, Top.

        float left = rect.X;
        float bottom = rect.Y;
        float right = rect.Right;
        float top = rect.Bottom; // RectangleF.Bottom is Y + Height

        // Note: RectangleF.Bottom property is strictly Y + Height.

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
            // Reset and do top only (Header)
            // Header should be at the Top of the node.
            // MSAGL Top is `top`.
            // Header Height is 25.
            // So Header Rect is from Top-25 to Top.

            path.Reset();
            float headerBottom = top - 25;

            // Top Right
            path.AddArc(right - diameter, top - diameter, diameter, diameter, 0, 90);
            // Top Left
            path.AddArc(left, top - diameter, diameter, diameter, 90, 90);

            // Line down to header bottom
            path.AddLine(left, top - diameter + radius, left, headerBottom);
            // Line across
            path.AddLine(left, headerBottom, right, headerBottom);
            // Line up
            path.AddLine(right, headerBottom, right, top - diameter + radius);

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
