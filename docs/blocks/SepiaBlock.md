# Sepia Block

## Description
The Sepia Block applies a sepia tone effect to the image, giving it a warm, brownish-gray color associated with early photography.

## Configuration Parameters

### `Amount`
*   **Type**: `float`
*   **Description**: The intensity of the sepia effect. 0.0 is original, 1.0 is full sepia.
*   **Range**: 0.0 to 1.0

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
The block applies `Image.Mutate(x => x.Sepia(Amount, region))`.
