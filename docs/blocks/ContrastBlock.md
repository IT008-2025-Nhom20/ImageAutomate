# Contrast Block

## Description
The Contrast Block modifies the global contrast of the image based on ImageSharp's contrast transformation.

## Configuration Parameters

### `Contrast`
*   **Type**: `float`
*   **Description**: Contrast factor. 1.0 = no change, <1.0 = lower contrast, >1.0 = higher contrast.
*   **Range**: 0.0 to 3.0

## Operational Behavior

### Execution
The block applies `Image.Mutate(x => x.Contrast(Contrast))` using the configured contrast factor.
The operation preserves metadata and the alpha channel.