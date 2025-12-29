using System.ComponentModel;

namespace ImageAutomate.UI;

/// <summary>
/// Animated sidebar panel that expands/collapses based on hover or manual control.
/// </summary>
[DefaultProperty(nameof(ExpansionExtraWidth))]
[DefaultEvent(nameof(MouseEnter))]
public class SidebarControl : Panel
{
    #region Fields

    private readonly System.Windows.Forms.Timer _hoverCheckTimer;
    private int _baseWidth;
    private bool _isExpanded;
    private bool _autoExpand = true;
    private bool _overlayMode = true;
    private int _expansionExtraWidth = 150;

    #endregion

    #region Properties

    /// <summary>
    /// Pixels to add to base width when expanded.
    /// </summary>
    [Category("Sidebar")]
    [Description("Pixels to add to base width when expanded.")]
    [DefaultValue(150)]
    public int ExpansionExtraWidth
    {
        get => _expansionExtraWidth;
        set
        {
            if (_expansionExtraWidth != value)
            {
                _expansionExtraWidth = value;
                // Update width if currently expanded
                if (_isExpanded)
                    Width = _baseWidth + _expansionExtraWidth;
            }
        }
    }

    /// <summary>
    /// If true, sidebar overlays content (BringToFront).
    /// </summary>
    [Category("Sidebar")]
    [Description("If true, sidebar overlays content (BringToFront).")]
    [DefaultValue(true)]
    public bool OverlayMode
    {
        get => _overlayMode;
        set => _overlayMode = value;
    }

    /// <summary>
    /// If true, mouse hover triggers expansion/collapse. If false, manual control only.
    /// </summary>
    [Category("Sidebar")]
    [Description("If true, mouse hover triggers expansion/collapse. If false, manual control only.")]
    [DefaultValue(true)]
    public bool AutoExpand
    {
        get => _autoExpand;
        set => _autoExpand = value;
    }

    /// <summary>
    /// Current expansion state.
    /// When AutoExpand is true, the setter is ignored (hover controls state).
    /// When AutoExpand is false, the setter directly controls expansion.
    /// </summary>
    [Category("Sidebar")]
    [Description("Current expansion state.")]
    [DefaultValue(false)]
    public bool IsExpanded
    {
        get => _isExpanded;
        set
        {
            if (AutoExpand)
            {
                // AutoExpand mode: value is ignored, hover controls state
                return;
            }

            // Manual mode: direct control
            if (_isExpanded == value)
                return;

            _isExpanded = value;
            Width = value ? (_baseWidth + ExpansionExtraWidth) : _baseWidth;
            PropagateState(value);
        }
    }

    [Browsable(false)]
    public int BaseWidth => _baseWidth;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the SidebarControl class.
    /// </summary>
    public SidebarControl()
    {
        DoubleBuffered = true;

        _hoverCheckTimer = new System.Windows.Forms.Timer { Interval = 15 };
        _hoverCheckTimer.Tick += OnHoverCheckTimerTick;
    }

    #endregion

    #region Event Handlers

    protected override void OnHandleCreated(EventArgs e)
    {
        _baseWidth = Width;
        base.OnHandleCreated(e);
    }

    /// <summary>
    /// Handles mouse enter events on the Sidebar panel itself.
    /// </summary>
    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        AttemptExpand();
    }

    /// <summary>
    /// Handles mouse enter events on child controls to simulate bubbling.
    /// </summary>
    private void OnChildMouseEnter(object? sender, EventArgs e)
    {
        AttemptExpand();
    }

    /// <summary>
    /// Recursively wires MouseEnter events when controls are added.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "System event override")]
    protected override void OnControlAdded(ControlEventArgs e)
    {
        base.OnControlAdded(e);
        if (e.Control != null)
            WireChildEvents(e.Control);
    }

    /// <summary>
    /// Unwires events when controls are removed to prevent leaks.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "System event override")]
    protected override void OnControlRemoved(ControlEventArgs e)
    {
        base.OnControlRemoved(e);
        if (e.Control != null)
            UnwireChildEvents(e.Control);
    }

    /// <summary>
    /// Handles the hover check timer tick to detect when mouse leaves the expanded zone.
    /// </summary>
    private void OnHoverCheckTimerTick(object? sender, EventArgs e)
    {
        // Check if cursor is within the expanded geometric zone
        // PointToClient works correctly even if cursor is over a child control
        bool isHovering = ClientRectangle.Contains(PointToClient(Cursor.Position));

        if (!isHovering && _isExpanded)
        {
            // Mouse left the expanded zone - snap back to base width
            Width = _baseWidth;
            _isExpanded = false;
            _hoverCheckTimer.Stop();
            PropagateState(false);
        }
    }

    #endregion

    #region Private Methods

    private void WireChildEvents(Control control)
    {
        // Prevent duplicate subscription if called multiple times
        control.MouseEnter -= OnChildMouseEnter;
        control.MouseEnter += OnChildMouseEnter;

        // Recursively wire nested controls (e.g., Labels inside a Button UserControl)
        if (control.HasChildren)
        {
            foreach (Control child in control.Controls)
            {
                WireChildEvents(child);
            }
            // Ensure dynamically added inner controls are also wired
            control.ControlAdded += (s, args) => { if (args.Control != null) WireChildEvents(args.Control); };
            control.ControlRemoved += (s, args) => { if (args.Control != null) UnwireChildEvents(args.Control); };
        }
    }

    private void UnwireChildEvents(Control control)
    {
        control.MouseEnter -= OnChildMouseEnter;

        if (control.HasChildren)
        {
            foreach (Control child in control.Controls)
            {
                UnwireChildEvents(child);
            }
        }
    }

    /// <summary>
    /// Shared logic to trigger expansion.
    /// </summary>
    private void AttemptExpand()
    {
        if (AutoExpand && !_isExpanded)
        {
            // Instant expand
            Width = _baseWidth + ExpansionExtraWidth;
            _isExpanded = true;
            _hoverCheckTimer.Start();
            StartAnimation();
            PropagateState(true);
        }
    }

    /// <summary>
    /// Propagates the expansion state to all child ISidebarItem controls.
    /// </summary>
    private void PropagateState(bool isExpanded)
    {
        foreach (Control control in Controls)
        {
            if (control is ISidebarItem item)
                item.SetSidebarState(isExpanded);
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Captures the base width and optionally brings the control to front.
    /// Call this after setting Width property to establish the collapsed state.
    /// </summary>
    public void StartAnimation()
    {
        if (_overlayMode)
            BringToFront();
    }

    #endregion

    #region Dispose

    /// <summary>
    /// Disposes the control and its timer.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _hoverCheckTimer?.Dispose();
        }
        base.Dispose(disposing);
    }

    #endregion
}