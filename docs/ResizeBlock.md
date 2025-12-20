# FR-RSZ-001: Resize Image Block

## Description
Block shall resize images to configurable dimensions using ImageSharp resampling operations. Supports absolute pixel sizing, proportional scaling, aspect-ratio constraints, and various resampling kernels.

## Configuration Parameters

### ResizeMode
Defines how the output size is computed. Supported values:
- Fixed
- KeepAspect
- Fit
- Fill
- Pad

### Width
Target pixel width. Interpreted based on ResizeMode. Optional when ResizeMode = KeepAspect.

### Height
Target pixel height. Interpreted based on ResizeMode. Optional when ResizeMode = KeepAspect.

### PreserveAspectRatio
Boolean indicating whether aspect ratio should be maintained. Only applicable to Fixed mode.

### Resampler
Resampling kernel used for resizing. Supported values:
- NearestNeighbor
- Bilinear
- Bicubic
- Lanczos2
- Lanczos3
- Spline

### BackgroundColor (Pad mode)
Color used when padding is required to maintain aspect ratio.

## Acceptance Criteria
- Block shall resize the input image according to ResizeMode and dimensions.
- Output image must retain valid pixel format.
- Width and Height validate positive integers.
- Aspect ratio preserved when required.
- Padding applied only in Pad mode.
- Alpha channel preserved for transparent formats.

## UI Behaviour
- ResizeMode selectable via dropdown.
- Width/Height fields enabled based on mode.
- BackgroundColor visible only when ResizeMode = Pad.
- Resampler dropdown always visible.

## Operational Behaviour

### Format Support
- Accepts Bmp, Gif, Jpeg, Pbm, Png, Tiff, Tga, WebP, Qoi.
- Source format auto-detected via Image.Identify().

### Configuration Behaviour
- Metadata preserved.
- DPI preserved.
- Padding applied only for Pad mode.

### Resize Behaviour
- Fixed: direct Width x Height.
- KeepAspect: scale proportionally.
- Fit: scale to fit bounding box.
- Fill: crop overflow.
- Pad: pad empty areas.

## Technical Notes

### Known Limitations
- Only first frame of animated GIFs processed.
- Large resize ratios may introduce ringing depending on resampler.

### OOM Behaviour
- Warn when batch size exceeds 80% memory.
- Process large images individually.

### Library Notes
- Resampling uses ResizeOptions.
- ICC profiles preserved.
- Orientation respected unless stripped.
