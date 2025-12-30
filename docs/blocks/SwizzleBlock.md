# Swizzle Block

## Description
The Swizzle Block rearranges the color channels of the image based on the selected mode.

## Configuration Parameters

### `Mode`
*   **Type**: `SwizzleMode`
*   **Description**: Selects the channel permutation mode.
*   **Options**:
    *   `RGB`: No change.
    *   `BGR`: Swap Red and Blue (common in some video formats).
    *   `RBG`: Swap Green and Blue.
    *   `GRB`: Swap Red and Green.
    *   `GBR`: Cycle Right.
    *   `BRG`: Cycle Left.

## Operational Behavior

### Execution
The block constructs a `ColorMatrix` corresponding to the selected mode and applies `Image.Mutate(x => x.Filter(matrix))`.
