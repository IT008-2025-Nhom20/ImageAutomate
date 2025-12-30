# Dither Block

## Description
The Dither Block applies dithering to the image. Dithering is a technique used to create the illusion of color depth in images with a limited color palette.

## Configuration Parameters

### `Mode`
*   **Type**: `DitherMode`
*   **Description**: The dithering algorithm to use.
*   **Options**:
    *   `FloydSteinberg`: Standard error diffusion.
    *   `Atkinson`: "Bill Atkinson" dithering (Apple Macintosh).
    *   `Bayer4x4`: Ordered dithering 4x4.
    *   `Bayer8x8`: Ordered dithering 8x8.

### `DitherScale`
*   **Type**: `float`
*   **Description**: Scales the dithering matrix. Larger values (>1.0) create larger, blockier dither patterns.
*   **Min**: 0.1

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
The block retrieves the specific `IDither` implementation based on the `Mode` and applies `Image.Mutate(x => x.Dither(ditherer, DitherScale, region))`.
