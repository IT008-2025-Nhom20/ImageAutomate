# Brightness Block

## Description
Adjusts image brightness by scaling pixel luminance uniformly using ImageSharp's brightness processor.

## Configuration Parameters

### `Brightness`
*   **Type**: `float`
*   **Description**: Brightness factor. 1.0 = no change, <1.0 = darker, >1.0 = brighter.
*   **Range**: 0.0 to 3.0

## Acceptance Criteria
- Brightness applied uniformly across all pixels.
- No color clipping or channel overflow.
- Metadata and alpha preserved.

## Operational Behaviour
ImageSharp implementation:
```csharp
image.Mutate(x => x.Brightness(Brightness));
```

## Technical Notes
- Works on all ImageSharp-supported formats.
- ICC profile preserved.
