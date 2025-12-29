using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace ImageAutomate.UI
{
    /// <summary>
    /// Button that displays the selected color and opens a ColorDialog when clicked.
    /// </summary>
    [ToolboxItem(true)]
    [ToolboxBitmap(typeof(Button))]
    public class ColorDialogButton : Button
    {
        private Color _selectedColor = Color.Red;

        /// <summary>
        /// Gets or sets the currently selected color.
        /// </summary>
        [Category("Appearance")]
        [Description("The color selected by this button.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [DefaultValue(typeof(Color), "Red")]
        public Color SelectedColor
        {
            get => _selectedColor;
            set
            {
                if (_selectedColor != value)
                {
                    _selectedColor = value;
                    Invalidate();
                    SelectedColorChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Occurs when the SelectedColor property changes.
        /// </summary>
        [Category("Action")]
        [Description("Occurs when the SelectedColor property changes.")]
        public event EventHandler? SelectedColorChanged;

        /// <summary>
        /// Initializes a new instance of ColorDialogButton.
        /// </summary>
        public ColorDialogButton()
        {
            Click += OnButtonClick;
        }

        /// <summary>
        /// Handles button click to open color dialog.
        /// </summary>
        private void OnButtonClick(object? sender, EventArgs e)
        {
            using var colorDialog = new ColorDialog
            {
                Color = _selectedColor,
                AllowFullOpen = true,
                FullOpen = true,
                ShowHelp = false
            };

            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                SelectedColor = colorDialog.Color;
            }
        }

        /// <summary>
        /// Paints the button with the selected color.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "<Pending>")]
        protected override void OnPaint(PaintEventArgs pevent)
        {
            base.OnPaint(pevent);

            // Draw color preview
            var g = pevent.Graphics;
            var bounds = ClientRectangle;

            // Inset the color rectangle slightly
            var colorRect = new Rectangle(
                bounds.X + 4,
                bounds.Y + 4,
                bounds.Width - 8,
                bounds.Height - 8
            );

            // Draw color background
            using var brush = new SolidBrush(_selectedColor);
            g.FillRectangle(brush, colorRect);

            // Draw border
            using var pen = new Pen(ForeColor, 1);
            g.DrawRectangle(pen, colorRect);

            // Optional: Draw a checkered pattern background for transparency indication
            if (_selectedColor.A < 255)
            {
                DrawTransparencyPattern(g, colorRect);
            }
        }

        /// <summary>
        /// Draws a checkered pattern to indicate transparency.
        /// </summary>
        private void DrawTransparencyPattern(Graphics g, Rectangle rect)
        {
            const int checkerSize = 4;
            var light = Color.White;
            var dark = Color.LightGray;

            for (int y = rect.Top; y < rect.Bottom; y += checkerSize)
            {
                for (int x = rect.Left; x < rect.Right; x += checkerSize)
                {
                    bool isLight = ((x / checkerSize) + (y / checkerSize)) % 2 == 0;
                    using var brush = new SolidBrush(isLight ? light : dark);
                    g.FillRectangle(brush, x, y, checkerSize, checkerSize);
                }
            }

            // Draw semi-transparent color over checkerboard
            using var colorBrush = new SolidBrush(_selectedColor);
            g.FillRectangle(colorBrush, rect);
        }
    }
}
