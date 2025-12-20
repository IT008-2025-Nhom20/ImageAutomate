# FR-SAV-001: Save Image Block

## Description
The Save Block outputs the processed image to disk. 
Supports all ImageSharp encoders and handles directory validation and overwrite rules.

---

## Configuration Parameters

### OutputPath
File path for saving the processed image.  
Directory must exist unless CreateDirectory is true.

### Overwrite
Boolean.  
- false = prevent overwrite if file exists  
- true = overwrite existing file

### CreateDirectory
Boolean.  
If true, missing folders in the OutputPath are created automatically.

### EncoderFormat
Optional override to specify the encoder explicitly:  
Bmp, Gif, Jpeg, Png, Pbm, Tiff, Tga, WebP, Qoi.  
If omitted, format is inferred from the file extension.

### EncoderOptions
Format-specific encoder settings (same structure as ConvertBlock):
- JPEG: Quality
- PNG: CompressionLevel
- WEBP: Quality, Lossless
- TIFF: Compression
- BMP: BitsPerPixel
- PBM: ColorType
- GIF: UseDithering, ColorPaletteSize
- TGA: Compress
- QOI: IncludeAlpha
---

## Acceptance Criteria
- Output file must be written successfully.
- Encoder used must match EncoderFormat or file extension.
- Overwrite behaviour must follow Overwrite setting.
- Must create directories automatically when CreateDirectory = true.
- Metadata preserved unless removed earlier in the pipeline.

---

## Operational Behaviour

### Saving with automatic format detection
```csharp
image.Save(outputPath);
```

### Saving with explicit encoder
```csharp
image.Save(outputPath, new JpegEncoder { Quality = 90 });
```

### Directory handling
```csharp
Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
```

---

## Technical Notes
- GIF saving is static-only; animation is not supported.
- ICC profile preserved by default.
- Alpha flattened when saving to non-alpha formats (JPEG, BMP).
