# Rotate Flip Block

## Description
The Rotate Flip Block performs orthogonal rotation (90-degree steps) and axis flipping operations. This is generally faster and loss-less compared to arbitrary rotation.

## Configuration Parameters

### `RotateMode`
*   **Type**: `RotateMode`
*   **Description**: Rotation step applied before flipping.
*   **Options**:
    *   `None`
    *   `Rotate90`: 90 degrees clockwise.
    *   `Rotate180`: 180 degrees.
    *   `Rotate270`: 270 degrees clockwise.

### `FlipMode`
*   **Type**: `FlipMode`
*   **Description**: Flip direction applied after rotation.
*   **Options**:
    *   `None`
    *   `Horizontal`: Flip horizontally.
    *   `Vertical`: Flip vertically.

## Operational Behavior

### Execution
The block applies `Image.Mutate(x => x.RotateFlip(RotateMode, FlipMode))`.
