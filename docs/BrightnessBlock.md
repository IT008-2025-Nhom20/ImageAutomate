# FR-BRI-001: Brightness Block

## Description
Adjusts image brightness by scaling pixel luminance uniformly using ImageSharp’s brightness processor.

## Configuration Parameters
### Brightness
Float value:
- 1.0 = no change
- <1.0 = darker
- >1.0 = brighter  
Valid range: 0.0–3.0

## Acceptance Criteria
- Brightness applied uniformly across all pixels.
- No color clipping or channel overflow.
- Metadata and alpha preserved.

## Operational Behaviour
ImageSharp implementation:
```csharp
image.Mutate(x => x.Brightness(brightnessValue));
```

## Technical Notes
- Works on all ImageSharp-supported formats.
- ICC profile preserved.
