# Median Blur Block

## Description
The Median Blur Block applies a median filter to the image. This is a non-linear filter effective at removing "salt and pepper" noise while preserving edges better than standard blurs.

## Configuration Parameters

### `Radius`
*   **Type**: `int`
*   **Description**: The radius of the median blur operation. Higher values reduce noise but may remove fine details.
*   **Min**: 1

### `PreserveAlpha`
*   **Type**: `bool`
*   **Description**: If true, the alpha channel will not be blurred, preserving the original transparency edges.

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
The block applies `Image.Mutate(x => x.MedianBlur(Radius, PreserveAlpha, region))`.
