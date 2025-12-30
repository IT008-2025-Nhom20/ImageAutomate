# Oil Paint Block

## Description
The Oil Paint Block applies an oil painting effect to the image. It works by replacing each pixel with the most frequent color in a neighboring window.

## Configuration Parameters

### `Level`
*   **Type**: `int`
*   **Description**: The number of intensity levels. Higher values result in more color detail, while lower values flatten colors more.
*   **Min**: 1

### `BrushSize`
*   **Type**: `int`
*   **Description**: The size of the brush (neighbor window). Larger brushes create a more abstract, painting-like effect.
*   **Min**: 1

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
The block applies `Image.Mutate(x => x.OilPaint(Level, BrushSize, region))`.
This is a computationally intensive effect.
