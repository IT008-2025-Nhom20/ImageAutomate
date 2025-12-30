# Hue Block

## Description
The Hue Block rotates the hue channel in the HSL color space using ImageSharp's Hue processor.

## Configuration Parameters

### `HueShift`
*   **Type**: `float`
*   **Description**: Hue shift in degrees. Range: -180 to +180. 0 indicates no change.

## Operational Behavior

### Execution
The block applies `Image.Mutate(x => x.Hue(HueShift))`.
Luminance, saturation, and metadata are preserved.