# FR-CON-001: Contrast Block

## Description
Modifies global contrast of the image based on ImageSharp's contrast transformation.

## Configuration Parameters
### Contrast
Float value:
- 1.0 = no change
- <1.0 = reduced contrast
- >1.0 = increased contrast  
Valid range: 0.0–3.0

## Acceptance Criteria
- Output contrast correctly matches ImageSharp’s implementation.
- Metadata and alpha preserved.

## Operational Behaviour
```csharp
image.Mutate(x => x.Contrast(contrastValue));
```

## Technical Notes
- Contrast operation is linear and fast.
