# Pixelate Block

## Description
Applies pixelation effect by downsampling and upscaling blocks of pixels.
Useful for anonymization, stylization, or mosaic effects.

## Configuration Parameters

### `Size`
*   **Type**: `int`
*   **Description**: Pixel block size. Higher values produce larger pixel blocks.
*   **Range**: 1–100

## Acceptance Criteria
- Pixelation grid must be uniform.
- Block edges must be crisp.
- Operation must match ImageSharp pixelation results.

## Operational Behaviour
```csharp
image.Mutate(x => x.Pixelate(Size));
```

## Technical Notes
- Pixelation is fast and memory efficient.
- Alpha channel preserved.
