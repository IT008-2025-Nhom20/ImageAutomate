# FR-LOD-001: Load Image Block

## Description  
The Load Block is responsible for loading images from directories
It acts as the entry point of the processing pipeline and ensures images are decoded correctly using ImageSharp.

---

## Configuration Parameters

### SourcePath
File system path to the input directory.  
Required when loading from disk.

### AutoOrient
Boolean.  
If true, applies EXIF orientation correction automatically.

---

## Acceptance Criteria
- Successfully loads any ImageSharp-supported format:
  Bmp, Gif, Jpeg, Pbm, Png, Tiff, Tga, WebP, Qoi.
- Throws a clear error message if the file is missing or unreadable.
- When AutoOrient = true, orientation must match EXIF orientation tag.

---

## Operational Behaviour

### Directory Loading
```csharp
var files = Directory.GetFiles(SourcePath);
```

---

## Technical Notes
- Animated GIFs: only first frame loaded (ImageSharp default).
- ICC profiles preserved.
- Non-image content must raise validation errors.
