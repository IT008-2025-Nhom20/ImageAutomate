# Glow Block

## Description
The Glow Block adds a glowing effect to the image, simulating light bleeding into darker areas.

## Configuration Parameters

### `GlowColor`
*   **Type**: `SixLabors.ImageSharp.Color`
*   **Description**: The color of the glow effect.
*   **Default**: Gold

### `Radius`
*   **Type**: `float`
*   **Description**: The radius of the glow effect (spread size). Larger values result in a wider, softer glow.
*   **Min**: 0.0

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
The block applies `Image.Mutate(x => x.Glow(GlowColor, Radius, region))`.
