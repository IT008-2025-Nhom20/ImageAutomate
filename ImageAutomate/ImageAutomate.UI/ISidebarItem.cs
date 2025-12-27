namespace ImageAutomate.UI;

/// <summary>
/// Contract for controls that can respond to sidebar state changes.
/// </summary>
public interface ISidebarItem
{
    /// <summary>Text displayed when sidebar is expanded.</summary>
    string? ExpandedText { get; set; }

    /// <summary>Text displayed when sidebar is collapsed.</summary>
    string? CollapsedText { get; set; }

    /// <summary>Image displayed when sidebar is expanded.</summary>
    Image? ExpandedImage { get; set; }

    /// <summary>Image displayed when sidebar is collapsed.</summary>
    Image? CollapsedImage { get; set; }

    /// <summary>
    /// Called by SidebarControl to notify the item of state changes.
    /// Implementers should update Text/Image and call Invalidate().
    /// </summary>
    void SetSidebarState(bool isExpanded);
}
