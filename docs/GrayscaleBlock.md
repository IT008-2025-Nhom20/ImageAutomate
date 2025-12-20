# FR-GRY-001: Grayscale Block

## Description
Converts image to grayscale using ImageSharp’s luminance-based grayscale algorithm.

## Configuration Parameters
None required.

## Acceptance Criteria
- Output contains no chroma information.
- Luminance conversion consistent with ImageSharp defaults.
- Metadata preserved.

## Operational Behaviour
```csharp
image.Mutate(x => x.Grayscale());
```

## Technical Notes
- Alpha channel preserved.
- ICC profile preserved.
