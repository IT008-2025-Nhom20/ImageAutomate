# Bokeh Blur Block

## Description
The Bokeh Blur Block applies a bokeh effect to the image, simulating the aesthetic quality of the blur produced in the out-of-focus parts of an image produced by a lens.

## Configuration Parameters

### `Radius`
*   **Type**: `int`
*   **Description**: The radius of the blur kernel. Larger values create a stronger blur.
*   **Min**: 1

### `Components`
*   **Type**: `int`
*   **Description**: The number of components (aperture blades) used to simulate the lens shape. For example, 6 creates a hexagonal bokeh shape.
*   **Min**: 1

### `Gamma`
*   **Type**: `float`
*   **Description**: The gamma strength, affecting the highlight intensity in the blurred areas.
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
The block applies `Image.Mutate(x => x.BokehBlur(Radius, Components, Gamma, region))` to the specified region.
The effect is computationally more expensive than standard blurs but produces artistically pleasing results.
