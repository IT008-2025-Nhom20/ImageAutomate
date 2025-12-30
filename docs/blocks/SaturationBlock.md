# Saturation Block

## Description
The Saturation Block modifies color intensity (saturation) using an HSL-based saturation transformation.

## Configuration Parameters

### `Saturation`
*   **Type**: `float`
*   **Description**: Saturation factor. 1.0 = no change, <1.0 = desaturate, >1.0 = more saturated.
*   **Range**: 0.0 to 3.0

## Operational Behavior

### Execution
The block applies `Image.Mutate(x => x.Saturate(Saturation))`.
Metadata and the alpha channel are preserved.