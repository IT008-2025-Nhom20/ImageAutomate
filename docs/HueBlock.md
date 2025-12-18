# FR-HUE-001: Hue Block

## Description
Rotates the hue channel in HSL color space using ImageSharp's Hue processor.

## Configuration Parameters
### HueShift
Degree rotation:
- Range: −180 to +180

## Acceptance Criteria
- Uniform hue rotation across entire image.
- Luminance and saturation preserved.
- Metadata preserved.

## Operational Behaviour
```csharp
image.Mutate(x => x.Hue(hueShiftDegrees));
```

## Technical Notes
- Suitable for color shifting and palette adjustments.
