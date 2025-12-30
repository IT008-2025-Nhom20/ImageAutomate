# Sharpen Block

## Description
The Sharpen Block applies ImageSharp's Sharpen processor to enhance edges and fine details.

## Configuration Parameters

### `Amount`
*   **Type**: `float`
*   **Description**: Sharpen intensity. 0.0 = no sharpening, 1.0 = normal sharpening, >1.0 = stronger.
*   **Range**: 0.0–3.0

## Operational Behavior

### Execution
If `Amount` > 0.0, the block applies `Image.Mutate(x => x.GaussianSharpen(Amount))`.
Metadata is preserved.