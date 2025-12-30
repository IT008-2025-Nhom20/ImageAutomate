# Vignette Block

## Description
The Vignette Block applies a vignette effect by darkening the edges of the image while keeping the center bright.

## Configuration Parameters

### `Color`
*   **Type**: `Color` (SixLabors.ImageSharp.PixelOperations)
*   **Description**: Vignette color used at the edges. Default is black.

### `Strength`
*   **Type**: `float`
*   **Description**: Vignette strength. 0 = no effect, 1 = full effect.
*   **Range**: 0.0 to 1.0

## Operational Behavior

### Execution
The block applies `Image.Mutate(x => x.Vignette(Color, Strength))`.
Metadata is preserved.