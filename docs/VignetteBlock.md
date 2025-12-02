# FR-VIG-001: Vignette Block

## Description
Applies a vignette effect by darkening or coloring the edges while keeping the center bright.

## Configuration Parameters
### Color
Optional color for vignette falloff.  
Default: Black.

### Strength
Float controlling intensity.  
Range: 0.0–1.0

## Acceptance Criteria
- Vignette gradient must be smooth.
- Center remains unaffected except for very high Strength.
- Metadata preserved.

## Operational Behaviour
```csharp
image.Mutate(x => x.Vignette(color));
```
Or with default:
```csharp
image.Mutate(x => x.Vignette());
```

## Technical Notes
- Works for stylization, portrait emphasis, or framing.
