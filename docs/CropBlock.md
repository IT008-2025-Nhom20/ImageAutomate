# FR-CRP-001: Crop Image Block

## Description
Block shall crop images to a specified rectangle or region using ImageSharp. Supports absolute pixel cropping, relative percentage-based cropping, and center/anchor-based cropping.

## Configuration Parameters

### CropMode
Defines how the crop region is selected:
- Rectangle
- Center
- Anchor

### X
Left coordinate of crop origin (pixels). Used in Rectangle mode.

### Y
Top coordinate of crop origin (pixels). Used in Rectangle mode.

### Width
Crop width in pixels. Required for Rectangle mode.

### Height
Crop height in pixels. Required for Rectangle mode.

### AnchorPosition
Used when CropMode = Anchor. Supported values:
- TopLeft
- Top
- TopRight
- Left
- Center
- Right
- BottomLeft
- Bottom
- BottomRight

## Acceptance Criteria
- Cropping respects selected mode.
- Output dimensions reflect exact cropped region.
- Coordinates validated to stay inside original image bounds.
- Metadata preserved unless removed by downstream blocks.

## UI Behaviour
- CropMode selectable via dropdown.
- X, Y, Width, Height fields enabled only when CropMode = Rectangle.
- AnchorPosition dropdown visible only when CropMode = Anchor.

## Operational Behaviour
- Implemented via Image.Mutate(x => x.Crop(new Rectangle(...))).
- Center mode computes crop region based on image center.
- Anchor mode computes region based on anchor position and crop size.
- Alpha, ICC profile, DPI preserved.

## Technical Notes
- GIF animations: Only first frame is cropped.
- Cropping outside bounds raises error and must be prevalidated.
