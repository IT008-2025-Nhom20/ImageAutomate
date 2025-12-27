# Gaussian Blur Block

## Description
Applies Gaussian Blur using ImageSharp's convolution-based GaussianBlur processor.
Used for smoothing, noise reduction, and softening edges.

## Configuration Parameters

### `Sigma`
*   **Type**: `float`
*   **Description**: Blur intensity. Higher values = stronger blur.
*   **Recommended Range**: 0.5–25.0

### `Radius`
*   **Type**: `int?` (nullable int)
*   **Description**: Optional override for Gaussian kernel radius. If null, ImageSharp auto-computes based on Sigma.

## Acceptance Criteria
- Blur strength matches ImageSharp GaussianBlur behaviour.
- No halo artifacts or boundary distortions.
- Metadata and alpha preserved.

## Operational Behaviour
Implemented via:
```csharp
image.Mutate(x => x.GaussianBlur(Sigma, Radius));
```

## Technical Notes
- Gaussian blur increases processing cost with larger kernels.
- Works for all ImageSharp-supported formats.
