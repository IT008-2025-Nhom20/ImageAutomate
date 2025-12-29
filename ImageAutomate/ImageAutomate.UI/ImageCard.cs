using System.ComponentModel;
using System.Globalization;

namespace ImageAutomate.UI;

[DefaultEvent("Click")]
public class ImageCard : Panel
{
    // Controls
#pragma warning disable CA2213 // Disposable fields should be disposed. Reason: WinForm manages these
    private PictureBox _pictureBox;
    private Panel _bottomPanel;
    private Label _lblTitle;
    private Label _lblDate;
#pragma warning restore CA2213 // Disposable fields should be disposed

    // Backing Fields
    private Color _defaultBackColor;
    private Color _hoverColor = Color.LightGray;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public ImageCard()
    {
        InitializeComponent();
        SetupEventPropagation();

        // Default Styling
        this.Size = new Size(200, 250);
        this.BorderStyle = BorderStyle.FixedSingle;
        this.Cursor = Cursors.Hand;
        _defaultBackColor = this.BackColor;
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    #region Properties

    [Category("Card Data")]
    [Description("The image displayed in the main area.")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public Image? CardImage
    {
        get => _pictureBox.Image;
        set => _pictureBox.Image = value;
    }

    [Category("Card Data")]
    [Description("The title text displayed on the bottom left.")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public string Title
    {
        get => _lblTitle.Text;
        set => _lblTitle.Text = value;
    }

    [Category("Card Data")]
    [Description("The datetime displayed on the bottom right.")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public DateTime LastModified
    {
        get => DateTime.Parse(_lblDate.Text, CultureInfo.InvariantCulture); // Simplified parsing
        set => _lblDate.Text = value.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
    }

    [Category("Appearance")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public Color HoverColor
    {
        get => _hoverColor;
        set => _hoverColor = value;
    }

    #endregion

    #region Initialization & Layout

    private void InitializeComponent()
    {
        // 1. Bottom Panel (Container for Text)
        _bottomPanel = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 30,
            BackColor = Color.WhiteSmoke,
            Padding = new Padding(2)
        };

        // 2. Date Label (Right aligned)
        _lblDate = new Label
        {
            Dock = DockStyle.Right,
            AutoSize = true, // Allow width to fit text
            TextAlign = ContentAlignment.MiddleRight,
            Font = new Font("Segoe UI", 8F, FontStyle.Regular),
            ForeColor = Color.Gray,
            Text = DateTime.Now.ToString(CultureInfo.CurrentCulture)
        };

        // 3. Title Label (Left aligned, fills remaining)
        _lblTitle = new Label
        {
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            Text = "Title",
            AutoEllipsis = true // Handle long text
        };

        // 4. PictureBox (Fills remaining top space)
        _pictureBox = new PictureBox
        {
            Dock = DockStyle.Fill,
            SizeMode = PictureBoxSizeMode.Zoom, // Or CenterImage
            BackColor = Color.Transparent,
            ErrorImage = null, // Prevent red X if null
            Image = null
        };

        // Hierarchy Assembly
        _bottomPanel.Controls.Add(_lblTitle); // Add Title first to fill
        _bottomPanel.Controls.Add(_lblDate);  // Add Date docked right

        this.Controls.Add(_pictureBox);       // Fill
        this.Controls.Add(_bottomPanel);      // Bottom
    }

    #endregion

    #region Event Propagation (Simulate Button Behavior)

    private void SetupEventPropagation()
    {
        // Iterate only children to prevent self-subscription recursion.
        foreach (Control child in this.Controls)
        {
            HookEvents(child);
        }
    }

    private void HookEvents(Control ctrl)
    {
        // These handlers forward child events to the parent's logic.
        // If 'ctrl' were 'this', it would create a stack overflow loop.
        ctrl.Click += ChildOnClick;
        ctrl.MouseEnter += ChildMouseEnter;
        ctrl.MouseLeave += ChildMouseLeave;

        // Recurse for nested controls (e.g., labels inside bottom panel)
        foreach (Control child in ctrl.Controls)
        {
            HookEvents(child);
        }
    }

    private void ChildOnClick(object? sender, EventArgs e)
    {
        this.InvokeOnClick(this, e);
    }

    private void ChildMouseEnter(object? sender, EventArgs e)
    {
        this.OnMouseEnter(e);
    }

    private void ChildMouseLeave(object? sender, EventArgs e)
    {
        this.OnMouseLeave(e);
    }

    // Hover Effects
    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        // Check if mouse is actually within bounds to prevent flicker
        if (this.ClientRectangle.Contains(this.PointToClient(Cursor.Position)))
        {
            this.BackColor = _hoverColor;
            _bottomPanel.BackColor = ControlPaint.Light(_hoverColor);
        }
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        // Verify mouse actually left
        if (!this.ClientRectangle.Contains(this.PointToClient(Cursor.Position)))
        {
            this.BackColor = _defaultBackColor;
            _bottomPanel.BackColor = Color.WhiteSmoke;
        }
    }

    #endregion
}