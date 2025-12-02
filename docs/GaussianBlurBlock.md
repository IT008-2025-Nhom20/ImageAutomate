# FR-GBL-001: Gaussian Blur Block

## Description
Applies Gaussian Blur using ImageSharp’s convolution-based GaussianBlur processor.  
Used for smoothing, noise reduction, and softening edges.

## Configuration Parameters
### Sigma
Float representing blur intensity.  
Higher values = stronger blur.  
Recommended range: 0.5–25.0

### Radius
Optional override for kernel radius (int).  
If not provided, ImageSharp auto-computes based on Sigma.

## Acceptance Criteria
- Blur strength matches ImageSharp GaussianBlur behaviour.
- No halo artifacts or boundary distortions.
- Metadata and alpha preserved.

## Operational Behaviour
Implemented via:
```csharp
image.Mutate(x => x.GaussianBlur(sigma));
```

Or with radius:
```csharp
image.Mutate(x => x.GaussianBlur(new GaussianBlurOptions { Sigma = sigma, Radius = radius }));
```

## Technical Notes
- Gaussian blur increases processing cost with larger kernels.
- Works for all ImageSharp-supported formats.
