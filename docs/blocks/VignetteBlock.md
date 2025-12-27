# Vignette Block

## Description
Applies a vignette effect by darkening the edges while keeping the center bright.

## Configuration Parameters

### `Color`
*   **Type**: `Color` (SixLabors.ImageSharp.PixelOperations)
*   **Description**: Vignette color used at the edges. Default is black.

### `Strength`
*   **Type**: `float`
*   **Description**: Vignette strength (0.0–1.0). 0 = no effect, 1 = full effect.
*   **Range**: 0.0 to 1.0

## Acceptance Criteria
- Vignette gradient must be smooth.
- Center remains unaffected except for very high strength.
- Metadata preserved.

## Operational Behaviour
```csharp
image.Mutate(x => x.Vignette(Color, Strength));
```

## Technical Notes
- Works for stylization, portrait emphasis, or framing.
