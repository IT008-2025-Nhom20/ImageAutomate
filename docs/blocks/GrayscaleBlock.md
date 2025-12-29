# Grayscale Block

## Description
Converts image to grayscale using ImageSharp's luminance-based grayscale algorithm.

## Configuration Parameters

### `GrayscaleOption`
*   **Type**: `GrayscaleOptions` (enum)
*   **Description**: The grayscale conversion mode to use.
*   **Values**:
    *   `Bt601`: BT.601 luminance coefficients (standard definition)
    *   `Bt709`: BT.709 luminance coefficients (high definition)

## Acceptance Criteria
- Output contains no chroma information.
- Luminance conversion consistent with ImageSharp implementation.
- Metadata preserved.

## Operational Behaviour
```csharp
image.Mutate(x => x.Grayscale(GrayscaleOption));
```

## Technical Notes
- Alpha channel preserved.
- ICC profile preserved.
