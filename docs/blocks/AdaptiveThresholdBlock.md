# Adaptive Threshold Block

## Description
The Adaptive Threshold Block applies adaptive thresholding to an image, allowing for local thresholding calculations based on the neighborhood of pixels. This is useful for images with varying illumination.

## Configuration Parameters

### `ThresholdLimit`
*   **Type**: `float`
*   **Description**: The threshold limit (0.0 to 1.0). Determines the sensitivity of the local thresholding calculation.
*   **Range**: 0.0 to 1.0

### `UpperColor`
*   **Type**: `System.Drawing.Color`
*   **Description**: The color assigned to pixels above the local threshold.
*   **Default**: White

### `LowerColor`
*   **Type**: `System.Drawing.Color`
*   **Description**: The color assigned to pixels below the local threshold.
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
The block applies `Image.Mutate(x => x.AdaptiveThreshold(...))` using the configured colors and limit within the specified region.
It converts the System.Drawing.Color inputs to ImageSharp Colors before processing.
