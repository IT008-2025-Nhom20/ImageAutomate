# Zoom Strategy Proposal

## Current Implementation Analysis

The current zoom implementation in `GraphRenderPanel.cs` uses a simple geometric progression:
- **Zoom In:** `Scale = Scale * 1.1`
- **Zoom Out:** `Scale = Scale / 1.1`
- **Clamping:** `0.1f` to `5.0f`
- **Rendering:** Uses GDI+ `Matrix.Scale` transformation.

### Issues Identified

1.  **Quality Degradation:**
    - Usage of global `ScaleTransform` scales all drawing primitives, including text and borders.
    - When zooming out (< 1.0), text becomes unreadable and aliased.
    - When zooming in (> 1.0), text and vector shapes may become blurry if not drawn with high-quality settings, or simply too large to be useful.

2.  **Perceptual Rate Mismatch:**
    - While mathematically symmetric, users report "zoom out" feels faster. This is because the visible world area grows quadratically when zooming out ($Area \propto 1/Scale^2$).
    - Zooming in at high scales yields diminishing returns in terms of visible detail.

3.  **Lack of Semantic Zoom:**
    - The level of detail (LOD) remains constant regardless of zoom level. Rendering full text at 10% scale is wasteful and ugly.

## Proposed Strategy

### 1. Logarithmic Zoom Steps

Instead of multiplying/dividing the current scale, we should track a `TargetZoomLevel` (float) and map it to `RenderScale`.

$$ Scale = Base^{ZoomLevel} $$

Where $Base \approx 1.2$ and $ZoomLevel$ ranges from e.g., -10 to +10.
This provides consistent "steps" that feel uniform to the user.

### 2. Semantic Zoom (Level of Detail)

To address quality issues, we should implement Semantic Zoom. The `NodeRenderer` should check the current `RenderScale` and adjust drawing:

-   **Scale > 0.8:** Render full details (Headers, Content Text, Ports).
-   **Scale 0.4 - 0.8:** Render Headers and Ports. Hide Content Text.
-   **Scale < 0.4:** Render only colored blocks and connections. Hide all text.

This improves performance and readability (avoiding "ant trails" of tiny text).

### 3. Clamping and Damping

-   **Clamp:** Restrict scale to a useful range, e.g., `0.2f` (Overview) to `3.0f` (Detail). Going beyond 3.0x rarely adds value for vector UI unless it is for accessibility.
-   **Damping:** To fix the "too fast" feeling, we can adjust the $Base$ or use a custom curve.
    -   Example: Use smaller steps when `Scale < 1.0` and larger steps when `Scale > 1.0`?
    -   Actually, the standard logarithmic approach is usually best. The "too fast" feeling might simply be the lack of clamping or the $1.1$ factor being too aggressive for repeated scroll wheel events.

### 4. Text Rendering Quality

Instead of scaling the Graphics context (`g.ScaleTransform`) for everything:
-   Keep the global transform for positioning (`TranslateTransform`).
-   For **Text**, calculate the font size dynamically: `FontSize = BaseFontSize * Scale`.
-   However, this breaks layout if the bounding boxes are also scaled.
-   **Alternative:** Continue using `ScaleTransform` but ensure `TextRenderingHint` is set to `AntiAlias` (not `ClearTypeGridFit` which is optimized for 1:1 pixel grids) when `Scale != 1.0`.

## Implementation Plan

1.  **Refactor Zoom Logic:**
    Change `OnMouseWheel` to modify a `_targetZoomLevel` index, then calculate `_renderScale`.

    ```csharp
    const float ZoomBase = 1.15f;
    const int MinLevel = -10; // ~0.25x
    const int MaxLevel = 8;   // ~3.0x

    // On Wheel
    _currentLevel = Math.Clamp(_currentLevel + Math.Sign(e.Delta), MinLevel, MaxLevel);
    _renderScale = (float)Math.Pow(ZoomBase, _currentLevel);
    ```

2.  **Update NodeRenderer:**
    Pass `scale` to `DrawNode`.
    ```csharp
    if (scale < 0.5f) { /* Skip text */ }
    ```

3.  **Coordinate Mapping:**
    Ensure `ScreenToWorld` and `WorldToScreen` remain consistent with the new scale calculation.
