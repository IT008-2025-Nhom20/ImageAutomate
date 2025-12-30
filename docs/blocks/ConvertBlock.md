# Convert Block

## Description
The Convert Block prepares images for format conversion by setting target format metadata. The actual encoding is performed by `SaveBlock` using these metadata instructions. It supports configurable encoder options for each target format.

## Configuration Parameters

### `TargetFormat`
Specifies the desired output format. Supported values include:
- Bmp
- Gif
- Jpeg
- Png
- Tiff
- Tga
- WebP
- Qoi
- Pbm

### Encoding Options
Format-specific encoder configurations are exposed based on the `TargetFormat` selection:

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

## Operational Behavior

### Metadata Updates
The block sets the following metadata on each `WorkItem`:
- `"Format"`: Target format name (e.g., "Jpeg", "Png")
- `"EncodingOptions"`: The format-specific options object

### Image Handling
The Convert Block modifies the `WorkItem` metadata. The actual format conversion and encoding are performed by the downstream `SaveBlock`.

### Transparency
Transparency handling depends on the target format and is applied at save time:
- **Non-alpha formats** (e.g., JPEG, BMP without transparency): Alpha channel is flattened by the encoder.
- **Alpha-capable formats** (e.g., PNG, TGA, TIFF, WebP, QOI): Alpha channel is preserved.

### Notes
- This block does **not** perform the actual format conversion; it only sets metadata instructions.
- ICC profiles and other image metadata are preserved through the pipeline.
- For animated GIFs, only the first frame is processed (as the `WorkItem` contains a single frame).