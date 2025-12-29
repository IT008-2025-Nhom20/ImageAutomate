# Hue Block

## Description
Rotates the hue channel in HSL color space using ImageSharp's Hue processor.

## Configuration Parameters

### `HueShift`
*   **Type**: `float`
*   **Description**: Hue shift in degrees. Range: -180 to +180. 0 = no change.
*   **Range**: -180.0 to +180.0

## Acceptance Criteria
- Uniform hue rotation across entire image.
- Luminance and saturation preserved.
- Metadata preserved.

## Operational Behaviour
```csharp
image.Mutate(x => x.Hue(HueShift));
```

## Technical Notes
- Suitable for color shifting and palette adjustments.
