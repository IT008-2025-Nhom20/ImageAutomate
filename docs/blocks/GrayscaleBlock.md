# Grayscale Block

## Description
The Grayscale Block converts the image to grayscale using ImageSharp's luminance-based grayscale algorithm.

## Configuration Parameters

### `GrayscaleOption`
*   **Type**: `GrayscaleOptions` (enum)
*   **Description**: The grayscale conversion mode to use.
*   **Values**:
    *   `Bt601`: BT.601 luminance coefficients (standard definition).
    *   `Bt709`: BT.709 luminance coefficients (high definition).

## Operational Behavior

### Execution
The block applies `Image.Mutate(x => x.Grayscale(GrayscaleOption))`.
The alpha channel and ICC profile are preserved.