using System.ComponentModel;

namespace ImageAutomate.UI;

/// <summary>
/// Button control that adapts its appearance based on sidebar expansion state.
/// </summary>
public class SidebarButton : Button, ISidebarItem
{
    /// <summary>Text displayed when sidebar is expanded.</summary>
    [Category("Sidebar")]
    [Description("Text displayed when sidebar is expanded.")]
    [DefaultValue(null)]
    public string? ExpandedText { get; set; }

    /// <summary>Text displayed when sidebar is collapsed.</summary>
    [Category("Sidebar")]
    [Description("Text displayed when sidebar is collapsed.")]
    [DefaultValue(null)]
    public string? CollapsedText { get; set; }

    /// <summary>Image displayed when sidebar is expanded.</summary>
    [Category("Sidebar")]
    [Description("Image displayed when sidebar is expanded.")]
    [DefaultValue(null)]
    public Image? ExpandedImage { get; set; }

    /// <summary>Image displayed when sidebar is collapsed.</summary>
    [Category("Sidebar")]
    [Description("Image displayed when sidebar is collapsed.")]
    [DefaultValue(null)]
    public Image? CollapsedImage { get; set; }

    /// <summary>
    /// Called by SidebarControl to notify the button of state changes.
    /// Updates Text/Image based on the expanded state and forces a repaint.
    /// </summary>
    public void SetSidebarState(bool isExpanded)
    {
        Text = isExpanded ? ExpandedText : CollapsedText;
        Image = isExpanded ? ExpandedImage : CollapsedImage;
        Invalidate();
    }
}
