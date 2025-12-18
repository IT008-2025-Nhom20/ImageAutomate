# FR-SAT-001: Saturation Block

## Description
Modifies color intensity (saturation) using HSL-based saturation transformation.

## Configuration Parameters
### Saturation
Float factor:
- 1.0 = no change
- <1.0 = desaturated
- >1.0 = more saturated  
Valid range: 0.0–3.0

## Acceptance Criteria
- Saturation adjustment consistent with ImageSharp behaviour.
- Metadata and alpha preserved.

## Operational Behaviour
```csharp
image.Mutate(x => x.Saturate(saturationValue));
```

## Technical Notes
- Works for all ImageSharp-supported formats.
