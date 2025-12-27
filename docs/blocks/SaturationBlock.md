# Saturation Block

## Description
Modifies color intensity (saturation) using HSL-based saturation transformation.

## Configuration Parameters

### `Saturation`
*   **Type**: `float`
*   **Description**: Saturation factor (0.0–3.0). 1.0 = no change, <1.0 = desaturate, >1.0 = more saturated.
*   **Range**: 0.0 to 3.0

## Acceptance Criteria
- Saturation adjustment consistent with ImageSharp behaviour.
- Metadata and alpha preserved.

## Operational Behaviour
```csharp
image.Mutate(x => x.Saturate(Saturation));
```

## Technical Notes
- Works for all ImageSharp-supported formats.
