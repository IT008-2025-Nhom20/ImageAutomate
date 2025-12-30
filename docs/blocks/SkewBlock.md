# Skew Block

## Description
The Skew Block skews (shears) the image along the X and/or Y axes by specified angles.

## Configuration Parameters

### `DegreesX`
*   **Type**: `float`
*   **Description**: Skew angle along the X-axis in degrees.

### `DegreesY`
*   **Type**: `float`
*   **Description**: Skew angle along the Y-axis in degrees.

## Operational Behavior

### Execution
The block applies `Image.Mutate(x => x.Skew(DegreesX, DegreesY))`.
This operation distorts the image geometry.
