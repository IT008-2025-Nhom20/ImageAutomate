namespace ImageAutomate.UI;

/// <summary>
/// Developer configuration options for the graph editor UI.
/// </summary>
public static class EditorConfiguration
{
    #region Zoom Configuration

    /// <summary>
    /// Gets or sets the factor by which zoom level changes on each mouse wheel step.
    /// </summary>
    public static float ZoomFactor { get; set; } = 1.1f;

    /// <summary>
    /// Gets or sets the minimum zoom scale allowed.
    /// </summary>
    public static float MinZoom { get; set; } = 0.1f;

    /// <summary>
    /// Gets or sets the maximum zoom scale allowed.
    /// </summary>
    public static float MaxZoom { get; set; } = 5.0f;

    #endregion

    #region Drag & Drop Ghost Settings

    /// <summary>
    /// Gets or sets the X offset from the ghost position when placing a new block.
    /// </summary>
    public static int NewBlockXOffset { get; set; } = 100;

    /// <summary>
    /// Gets or sets the Y offset from the ghost position when placing a new block.
    /// </summary>
    public static int NewBlockYOffset { get; set; } = 50;

    /// <summary>
    /// Gets or sets the width of the drag & drop ghost rectangle.
    /// </summary>
    public static int GhostWidth { get; set; } = 150;

    /// <summary>
    /// Gets or sets the height of the drag & drop ghost rectangle.
    /// </summary>
    public static int GhostHeight { get; set; } = 80;

    #endregion

    #region New Block Defaults

    /// <summary>
    /// Gets or sets the width applied to newly created blocks via drag & drop.
    /// </summary>
    public static int NewBlockWidth { get; set; } = 200;

    /// <summary>
    /// Gets or sets the height applied to newly created blocks via drag & drop.
    /// </summary>
    public static int NewBlockHeight { get; set; } = 100;

    #endregion
}
