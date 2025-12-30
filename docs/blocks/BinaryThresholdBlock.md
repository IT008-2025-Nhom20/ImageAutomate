# Binary Threshold Block

## Description
The Binary Threshold Block splits the image pixels into two colors based on a threshold value. Pixels meeting the condition become the Upper Color, while others become the Lower Color.

## Configuration Parameters

### `Threshold`
*   **Type**: `float`
*   **Description**: The threshold value (0.0 to 1.0). Pixels with a calculated value higher than this get the UpperColor.
*   **Range**: 0.0 to 1.0

### `Mode`
*   **Type**: `BinaryThresholdOption`
*   **Description**: The metric used to compare against the threshold.
*   **Options**:
    *   `Luminance`: Uses pixel luminance.
    *   `MaxChroma`: Uses the maximum chromaticity.
    *   `Saturation`: Uses pixel saturation.

### `UpperColor`
*   **Type**: `System.Drawing.Color`
*   **Description**: The color assigned when the value is >= Threshold.
*   **Default**: White

### `LowerColor`
*   **Type**: `System.Drawing.Color`
*   **Description**: The color assigned when the value is < Threshold.
*   **Default**: Black

### Region Configuration

#### `IsRelative`
*   **Type**: `bool`
*   **Description**: If true, region values are percentages (0.0-1.0). If false, values are pixels.

#### `RectX`
*   **Type**: `float`
*   **Description**: X coordinate of the top-left corner of the processing region.

#### `RectY`
*   **Type**: `float`
*   **Description**: Y coordinate of the top-left corner of the processing region.

#### `RectWidth`
*   **Type**: `float`
*   **Description**: Width of the processing region.

#### `RectHeight`
*   **Type**: `float`
*   **Description**: Height of the processing region.

## Operational Behavior

### Execution
The block applies `Image.Mutate(x => x.BinaryThreshold(...))` using the specified mode, threshold, and colors within the defined region.
