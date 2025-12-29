# Standard Blocks Audit Report

## 1. Automation Suitability

### CropBlock
**Status: Poor**

The current `CropBlock` implementation relies heavily on absolute pixel coordinates (`CropX`, `CropY`, `CropWidth`, `CropHeight`). This approach is brittle for automated batch processing where input images may vary in dimensions.

*   **Issues**:
    *   **Absolute Coordinates**: Requires knowing the exact size of input images beforehand.
    *   **No Relative/Percentage Support**: Cannot express "Crop 10% from all edges" or "Crop the center 50%".
    *   **Center Mode**: Still requires absolute `Width`/`Height`.
    *   **Safety**: Throws exceptions if the crop region exceeds image bounds, rather than clamping or handling gracefully (though this might be intended for strict pipelines, it reduces robustness).

**Recommendation**:
*   Introduce a "Relative" or "Percentage" mode.
*   Allow specifying crop size as a ratio of the source image size (e.g., `Width = 0.8` for 80%).

### ResizeBlock
**Status: Moderate**

The `ResizeBlock` is better suited for automation due to modes like `Fit`, `Fill`, and `Pad`, which adapt to the target box. However, it still lacks relative scaling capabilities.

*   **Issues**:
    *   **Absolute Dimensions Only**: `TargetWidth` and `TargetHeight` are pixels. Cannot express "Resize to 50% of original size" (Thumbnail generation).
    *   **Mixed Logic**: `PreserveAspectRatio` is only effective in `Fixed` mode logic inside the block, while `ResizeMode.Max` (used for `KeepAspect`, `Fit`) handles it natively.

**Recommendation**:
*   Add a `ScaleFactor` property for percentage-based resizing (e.g., `0.5x`).

## 2. Missing Mutations

The following "Simple Mutations" (scoped to a single operation) available in ImageSharp 3.1 are currently **missing** from `StandardBlocks`:

*   **Transforms**:
    *   `Rotate` (by degrees)
    *   `RotateFlip` (combined)
    *   `Skew`
    *   `AutoOrient` (Crucial for processing photos with EXIF orientation tags)
    *   `EntropyCrop` (Content-aware cropping)
    *   `ProjectiveTransform` / `AffineTransform` (Advanced, maybe out of scope for "Standard")
    *   `Swizzle`

*   **Filters & Effects**:
    *   `BlackWhite`
    *   `Invert`
    *   `Sepia`
    *   `Polaroid`
    *   `Kodachrome`
    *   `Lomograph`
    *   `OilPaint`
    *   `Glow`
    *   `BoxBlur`
    *   `BokehBlur`
    *   `MedianBlur` (Noise reduction)
    *   `Opacity` (Alpha adjustment)

*   **Binarization**:
    *   `BinaryThreshold`
    *   `AdaptiveThreshold`
    *   `Dither`

*   **Color Operations**:
    *   `Lightness`
    *   `BackgroundColor` (Setting background for transparent images)
    *   `HistogramEqualization` / `AdaptiveHistogramEqualization`
    *   `ColorBlindness` (Simulation)

## 3. Missing Options in Existing Blocks

### ResizeBlock
*   **AnchorPosition**: Hardcoded to `Center`.
    *   *Impact*: `Pad` and `Crop` modes always center the image. Users cannot anchor to `TopLeft`, `Bottom`, etc.
*   **Compand**: `ResizeOptions.Compand` is not exposed.
    *   *Impact*: Affects color accuracy during resizing (gamma correction).
*   **PremultiplyAlpha**: Hardcoded to `true`.

### CropBlock
*   **Rectangle Mode**: No method to center the rectangle itself (e.g. "Center a 100x100 rect at offset X,Y").
*   **Smart Cropping**: Missing `EntropyCrop` (though this is a separate mutation, it's conceptually a "Crop Mode").

### GaussianBlurBlock
*   **BorderWrapping**: `GaussianSharpen` and `GaussianBlur` usually support a `BorderWrappingMode` (e.g., `Repeat`, `Clamp`, `Reflect`), though it might be an advanced option in the processor, the extension method `GaussianBlur(sigma)` uses default.

### VignetteBlock
*   **Geometry**: The current implementation `Vignette(options, color)` uses `BlendPercentage` but does not expose `RadiusX` / `RadiusY` or the rectangle inputs that ImageSharp might support in other overloads (if any). *Correction*: ImageSharp `Vignette` is generally radial.

### FlipBlock
*   **RotateFlip**: The `Flip` block only handles `FlipMode`. ImageSharp has `RotateFlip` which is optimized. The `Flip` block is fine as is, but a `Rotate` block is needed.

### General
*   **Rectangle Overloads**: Many mutations (`Pixelate`, `Blur`, etc.) support applying the effect to a specific `Rectangle` region of the image. Current blocks always apply to the full image.

## 4. Conclusion

The `StandardBlocks` library covers the most common basic operations but misses a significant number of creative filters and utility mutations (especially `AutoOrient` and relative sizing). For a robust batch processing system, **Relative/Percentage** coordinates for Geometry blocks (`Crop`, `Resize`) are the highest priority usability gap.
