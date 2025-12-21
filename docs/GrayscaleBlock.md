# FR-GRY-001: Grayscale Block

## Description
Converts image to grayscale using ImageSharp’s luminance-based grayscale algorithm.

## Configuration Parameters
### GrayscaleOption
Specifies the grayscale conversion mode. Supported values:
- Bt601 (default)
- Bt709

## Acceptance Criteria
- Output contains no chroma information.
- Luminance conversion consistent with ImageSharp defaults.
- Metadata preserved.

## Operational Behaviour
```csharp
image.Mutate(x => x.Grayscale(grayscaleMode));
```

## Technical Notes
- Alpha channel preserved.
- ICC profile preserved.
