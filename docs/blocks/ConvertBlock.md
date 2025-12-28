# Convert Block

## Description
Prepares images for format conversion by setting target format metadata.
The actual encoding is performed by `SaveBlock` using the metadata instructions.
Supports configurable encoder options for each target format.

---

## Configuration Parameters

### TargetFormat
Specifies desired output format.
Selection is provided via a dropdown list. Supported values:
- Bmp
- Gif
- Jpeg
- Png
- Tiff
- Tga
- WebP
- Qoi
- Pbm

### JpegEncodingOptions / PngEncodingOptions / BmpEncodingOptions / GifEncodingOptions / TiffEncodingOptions / TgaEncodingOptions / WebPEncodingOptions / QoiEncodingOptions / PbmEncodingOptions
Format-specific encoder configuration, exposed based on `TargetFormat`:

| Format | Options |
|--------|---------|
| JPEG | Quality, ColorType, Interleaved |
| PNG | CompressionLevel, BitDepth, ColorType, InterlaceMode, ChunkFilter, TransparentColorMode |
| WebP | Quality, FileFormat (Lossy/Lossless), NearLossless, NearLosslessQuality, SkipMetadata, TransparentColorMode, Quantizer |
| TIFF | Compression, BitsPerPixel, PhotometricInterpretation, HorizontalPredictor, CompressionLevel |
| BMP | BitsPerPixel, SupportTransparency, QuantizerOptions |
| GIF | ColorTableMode, QuantizerOptions |
| TGA | Compression, BitsPerPixel |
| QOI | ColorSpace, Channels |
| PBM | ColorType, ComponentType, Encoding |

---

## Acceptance Criteria
- Sets `Format` and `EncodingOptions` metadata on output WorkItems.
- Encoder parameters stored correctly for downstream `SaveBlock`.
- Image data passed through.

---

## UI Behaviour
- TargetFormat dropdown displays supported formats.
- Only the relevant format options panel is shown based on TargetFormat selection.

---

## Operational Behaviour

### Metadata Updates
The block sets the following metadata on each WorkItem:
- `"Format"`: Target format name (e.g., "Jpeg", "Png")
- `"EncodingOptions"`: The format-specific options object

### Image Handling
ConvertBlock modifies the `WorkItem` metadata.
The actual format conversion/encoding is performed by `SaveBlock` using the metadata.

### Transparency
Transparency handling depends on target format and is applied at save time:
- Non-alpha formats (JPEG, BMP without transparency): Alpha flattened by encoder
- Alpha-capable formats (PNG, TGA, TIFF, WebP, QOI): Alpha preserved

---

## Technical Notes
- This block does **not** perform actual format conversion - it only sets metadata instructions.
- `SaveBlock` reads the `Format` and `EncodingOptions` metadata to perform the actual encoding.
- ICC profiles and other image metadata are preserved through the pipeline.
- Animated GIFs: only first frame processed (Image is single frame in WorkItem).
- Large batch operations may require memory warnings and sequential processing.
