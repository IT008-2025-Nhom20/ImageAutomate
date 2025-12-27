# Resize Block

## Description
Block shall resize images to new dimensions.
Supports various resize modes to handle aspect ratio preservation, cropping, and padding.

---

## Configuration Parameters

### `ResizeMode`
Specifies how the image is resized.
- **Fixed**: Stretches the image to `Width` x `Height`. If `MaintainAspectRatio` is true, scales to fit within the box.
- **KeepAspect**: Scales the image to fit within `Width` or `Height`, preserving aspect ratio.
- **Fit**: Fits the image within the target box, preserving aspect ratio.
- **Fill**: Fills the target box, preserving aspect ratio (cropping if necessary).
- **Pad**: Fits the image within the target box and pads the remaining space with `BackgroundColor`.

### `Width`, `Height`
- The target dimensions in pixels.
- Must be positive integers.
- Can be nullable in some modes (e.g. KeepAspect can specify only one dimension).

### `MaintainAspectRatio`
- Used in **Fixed** mode to toggle between stretching and scaling.

### `Resampler`
Specifies the algorithm used for resampling.
- **NearestNeighbor**
- **Bilinear**
- **Bicubic**
- **Lanczos2**
- **Lanczos3**
- **Spline**

### `BackgroundColor`
- The color used for padding in **Pad** mode.

---

## Acceptance Criteria
- Output image dimensions match expectation for the selected mode.
- Image content is resampled using the selected algorithm.
- Aspect ratio is preserved or ignored based on configuration.

---

## UI Behaviour
- **ResizeMode** dropdown selects the mode.
- **Width, Height** inputs.
- **MaintainAspectRatio** checkbox (visible for Fixed mode).
- **Resampler** dropdown.
- **BackgroundColor** picker (visible for Pad mode).

---

## Operational Behaviour

### Execution
- Applies `Image.Mutate(x => x.Resize(options))` using `SixLabors.ImageSharp`.
