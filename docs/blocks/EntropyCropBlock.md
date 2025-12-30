# Entropy Crop Block

## Description
The Entropy Crop Block crops the image to preserve the areas with the highest entropy (detail). It is useful for automatically removing low-detail borders or background.

## Configuration Parameters

### `Threshold`
*   **Type**: `float`
*   **Description**: The entropy threshold (0.0 to 1.0). Controls how aggressively low-detail areas are removed. Higher values crop more tightly around high-detail areas.
*   **Range**: 0.0 to 1.0

## Operational Behavior

### Execution
The block applies `Image.Mutate(x => x.EntropyCrop(Threshold))`.
This changes the dimensions of the image.
