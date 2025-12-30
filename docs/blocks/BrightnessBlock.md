# Brightness Block

## Description
The Brightness Block adjusts image brightness by scaling pixel luminance uniformly using ImageSharp's brightness processor.

## Configuration Parameters

### `Brightness`
*   **Type**: `float`
*   **Description**: Brightness factor. 1.0 = no change, <1.0 = darker, >1.0 = brighter.
*   **Range**: 0.0 to 3.0

## Operational Behavior

### Execution
The block applies `Image.Mutate(x => x.Brightness(Brightness))` using the configured brightness factor.
The operation preserves metadata and the alpha channel.