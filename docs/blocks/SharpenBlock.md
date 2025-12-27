# Sharpen Block

## Description
Applies ImageSharp's Sharpen processor to enhance edges and fine details.

## Configuration Parameters

### `Amount`
*   **Type**: `float`
*   **Description**: Sharpen intensity. 0.0 = no sharpening, 1.0 = normal sharpening, >1.0 = stronger.
*   **Range**: 0.0–3.0

## Acceptance Criteria
- Edge detail increased without introducing excessive noise.
- Operation matches ImageSharp's Sharpen behaviour.
- Metadata preserved.

## Operational Behaviour
```csharp
if (Amount > 0.0f)
    image.Mutate(x => x.GaussianSharpen(Amount));
```

## Technical Notes
- Sharpening may amplify noise on high ISO images.
