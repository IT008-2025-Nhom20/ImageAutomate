# Box Blur Block

## Description
The Box Blur Block applies a box blur filter to the image. A box blur averages the pixels within a square neighborhood, resulting in a distinct, blocky blur effect compared to Gaussian blur.

## Configuration Parameters

### `Radius`
*   **Type**: `int`
*   **Description**: The radius of the blur. Higher values create a stronger, blockier blur effect.
*   **Min**: 0

### `BorderWrapModeX`
*   **Type**: `BorderWrappingMode`
*   **Description**: Controls how pixels are extrapolated at the left/right image borders.

### `BorderWrapModeY`
*   **Type**: `BorderWrappingMode`
*   **Description**: Controls how pixels are extrapolated at the top/bottom image borders.

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
The block applies `Image.Mutate(x => x.BoxBlur(Radius, region, BorderWrapModeX, BorderWrapModeY))` to the image.
Using `Wrap` modes allows for seamless tileable blurring.
