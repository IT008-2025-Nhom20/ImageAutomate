# Rotate Block

## Description
The Rotate Block rotates the image by an arbitrary angle.

## Configuration Parameters

### `Degrees`
*   **Type**: `float`
*   **Description**: Rotation angle in degrees. Positive values rotate clockwise.

## Operational Behavior

### Execution
The block applies `Image.Mutate(x => x.Rotate(Degrees))`.
The image size may change to accommodate the rotated content.
