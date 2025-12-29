# Crop Block

## Description
Block shall crop images to a specified region.
Supports multiple cropping modes including explicit rectangle, centered cropping, and anchor-based cropping.

---

## Configuration Parameters

### `CropMode`
Specifies how the crop region is determined.
- **Rectangle**: Uses explicit `X`, `Y` coordinates and `Width`, `Height`.
- **Center**: Centers the crop region of size `Width` x `Height` within the image.
- **Anchor**: Aligns the crop region of size `Width` x `Height` relative to an `AnchorPosition`.

### `X`, `Y`
- Only used in **Rectangle** mode.
- Top-left coordinates of the crop rectangle.
- Must be positive integers.

### `Width`, `Height`
- The dimensions of the resulting cropped image.
- Must be positive integers.

### `AnchorPosition`
- Only used in **Anchor** mode.
- Positions the crop rectangle relative to the source image.
- Options: `TopLeft`, `Top`, `TopRight`, `Left`, `Center`, `Right`, `BottomLeft`, `Bottom`, `BottomRight`.

---

## Acceptance Criteria
- Output image has dimensions `Width` x `Height`.
- Crop region is correctly positioned according to `CropMode` and parameters.
- Throws error if crop region exceeds source image bounds.

---

## UI Behaviour
- **CropMode** dropdown selects the mode.
- **X, Y** visible only when `CropMode` is `Rectangle`.
- **AnchorPosition** visible only when `CropMode` is `Anchor`.
- **Width, Height** always visible.

---

## Operational Behaviour

### Bounds Checking
- **Rectangle Mode**: Throws if `X + Width > SourceWidth` or `Y + Height > SourceHeight`.
- **Center/Anchor Mode**: Throws if `Width > SourceWidth` or `Height > SourceHeight`.

### Execution
- Applies `Image.Mutate(x => x.Crop(rectangle))` using the calculated rectangle.
