# Background Color Block

## Description
The Background Color Block sets the background color of an image. This is particularly useful for images with transparency, where the transparent areas will be filled with the specified color.

## Configuration Parameters

### `Color`
*   **Type**: `System.Drawing.Color`
*   **Description**: The color to apply to the background (transparent areas).
*   **Default**: White

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
The block applies `Image.Mutate(x => x.BackgroundColor(Color, region))`.
It effectively composites the image over the specified background color within the defined region.
