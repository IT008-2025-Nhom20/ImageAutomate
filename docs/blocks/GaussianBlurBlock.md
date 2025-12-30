# Gaussian Blur Block

## Description
The Gaussian Blur Block applies a Gaussian Blur using ImageSharp's convolution-based GaussianBlur processor. This is used for smoothing, noise reduction, and softening edges.

## Configuration Parameters

### `Sigma`
*   **Type**: `float`
*   **Description**: Blur intensity. Higher values indicate a stronger blur.
*   **Recommended Range**: 0.5–25.0

### `Radius`
*   **Type**: `int?` (nullable int)
*   **Description**: Optional override for the Gaussian kernel radius. If null, ImageSharp auto-computes the radius based on Sigma.

## Operational Behavior

### Execution
The block applies `Image.Mutate(x => x.GaussianBlur(Sigma, Radius))`.
Metadata and the alpha channel are preserved.

### Notes
- Gaussian blur processing cost increases with larger kernels.
- The operation supports all ImageSharp-supported formats.