# FR-FLP-001: Flip Image Block

## Description
Block shall flip images horizontally or vertically using ImageSharp. Supports mirroring across X or Y axes.

## Configuration Parameters

### FlipMode
Supported values:
- Horizontal
- Vertical

## Acceptance Criteria
- Image must flip according to selected FlipMode.
- Final output must be pixel-identical to ImageSharp flip behavior.
- Metadata (EXIF, ICC, DPI) preserved.

## UI Behaviour
- FlipMode selectable via dropdown.
- No additional parameters required.

## Operational Behaviour
- Implemented via:
  - image.Mutate(x => x.Flip(FlipMode.Horizontal))
  - image.Mutate(x => x.Flip(FlipMode.Vertical))
- Alpha channel preserved.
- Output format unchanged.

## Technical Notes
- Flipping is O(n) and memory-efficient.
- Works on all ImageSharp-supported formats.
