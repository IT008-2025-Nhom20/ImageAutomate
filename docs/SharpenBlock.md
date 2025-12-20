# FR-SHP-001: Sharpen Block

## Description
Applies ImageSharp’s Sharpen processor to enhance edges and fine details.

## Configuration Parameters
### Amount
Sharpen intensity factor.  
Range:
- 0.0 = no sharpening
- 1.0 = normal sharpening
- >1.0 = stronger sharpening

## Acceptance Criteria
- Edge detail increased without introducing excessive noise.
- Operation matches ImageSharp’s Sharpen behaviour.
- Metadata preserved.

## Operational Behaviour
```csharp
image.Mutate(x => x.Sharpen(amount));
```

## Technical Notes
- Sharpening may amplify noise on high ISO images.
