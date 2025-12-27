# Contrast Block

## Description
Modifies global contrast of the image based on ImageSharp's contrast transformation.

## Configuration Parameters

### `Contrast`
*   **Type**: `float`
*   **Description**: Contrast factor (0.0–3.0). 1.0 = no change, <1.0 = lower contrast, >1.0 = higher contrast.
*   **Range**: 0.0 to 3.0

## Acceptance Criteria
- Output contrast correctly matches ImageSharp's implementation.
- Metadata and alpha preserved.

## Operational Behaviour
```csharp
image.Mutate(x => x.Contrast(Contrast));
```

## Technical Notes
- Contrast operation is linear and fast.
