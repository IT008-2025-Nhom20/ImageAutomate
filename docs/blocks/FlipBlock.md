# Flip Block

## Description
Block shall flip images horizontally or vertically.

## Configuration Parameters

### `FlipMode`
*   **Type**: `FlipModeOption` (enum)
*   **Description**: Specifies the direction of the flip.
*   **Values**:
    *   `Horizontal`: Mirrors the image along the vertical axis (left becomes right).
    *   `Vertical`: Mirrors the image along the horizontal axis (top becomes bottom).

## Acceptance Criteria
- Output image is flipped according to the `FlipMode`.
- Image dimensions and format are preserved.

## Operational Behaviour

### Execution
- Applies `Image.Mutate(x => x.Flip(...))` using the appropriate flip mode.
