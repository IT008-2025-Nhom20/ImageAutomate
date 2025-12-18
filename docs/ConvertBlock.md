# FR-CVT-001: Convert Image to Selected Format Block

## Description
Block shall convert images between supported formats using ImageSharp.  
Supports re-encoding, metadata preservation, alpha handling, and configurable encoder options.

---

## Configuration Parameters

### TargetFormat
Specifies desired output format. Supported values:
- Bmp
- Gif
- Jpeg
- Pbm
- Png
- Tiff
- Tga
- WebP
- Qoi

### PreserveMetadata
Boolean.  
- true = retain EXIF, ICC profile, and general metadata  
- false = output contains only pixel data

### AlwaysReEncode
Boolean.  
Controls behavior when source format == TargetFormat:
- false = passthrough unless encoder options change  
- true = always re-encode

### EncodingOptions
Format-specific encoder configuration:
- JPEG: Quality  
- PNG: CompressionLevel  
- WEBP: Quality, Lossless  
- TIFF: Compression
- BMP: BitsPerPixel
- GIF: UseDithering, ColorPaletteSize
- TGA: Compress
- QOI: IncludeAlpha
Displayed only for relevant TargetFormat.

---

## Acceptance Criteria
- Produces a valid output file in TargetFormat.
- Encoder parameters applied correctly.
- Metadata preserved only when PreserveMetadata = true.
- Passthrough occurs only when source and target formats match and AlwaysReEncode = false.
- Transparency preserved for formats with alpha support; flattened for non-alpha formats.

---

## UI Behaviour
- TargetFormat dropdown displays: BMP, GIF, JPEG, PNG, TGA, TIFF, WEBP.
- EncodingOptions shows only parameters relevant to selected TargetFormat.
- JPEG  exposes Quality list.
- PNG exposes CompressionLevel list.
- WEBP exposes  Quality list and Lossless true/false.
- BMP exposes BitsPerPixel value.
- GIF exposes UseDithering true/false and ColorPaletteSize value.
- TIFF exposes Compression list.
- TGA exposes Compress true/false.
- QOI exposes IncludeAlpha true/false
---

## Operational Behaviour

### Format Detection
Input format auto-detected using:
```csharp
var info = Image.Identify(stream);
```

### Conversion Rules
- Re-encode when:
  - TargetFormat differs from source format
  - EncodingOptions provided
  - AlwaysReEncode = true
- Passthrough allowed only when:
  - Formats match
  - No EncodingOptions set
  - AlwaysReEncode = false

### Metadata
When PreserveMetadata = true:
```csharp
output.Metadata = input.Metadata.DeepClone();
```

### Transparency
- For non-alpha formats (JPEG, BMP), transparency is flattened to white.
- Alpha preserved for PNG, TGA, TIFF, WebP, QOI.

### DPI & Resolution
DPI and resolution preserved from original image unless changed elsewhere.

---

## Technical Notes
- ImageSharp supports all listed formats.
- ICC profiles preserved unless explicitly removed.
- Animated GIFs: only first frame processed.
- Large batch operations may require memory warnings and sequential processing.

---

## Known Limitations
- Lossy formats (JPEG, WEBP-lossy) degrade quality on re-encode.
- Some metadata fields not supported by certain formats.
- No animation support for GIF output (ImageSharp writes only static images).

