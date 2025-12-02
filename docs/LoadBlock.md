# FR-LOD-001: Load Image Block

## Description
The Load Block is responsible for loading images from disk, memory streams, or URLs.  
It acts as the entry point of the processing pipeline and ensures images are decoded correctly using ImageSharp.

---

## Configuration Parameters

### SourcePath
File system path to the input image.  
Required when loading from disk.

### LoadFromUrl
Boolean.  
- false = load from local file path  
- true = load from remote URL

### Url
String URL to load the image from.  
Visible only when LoadFromUrl = true.

### AutoOrient
Boolean.  
If true, applies EXIF orientation correction automatically.

---

## Acceptance Criteria
- Successfully loads any ImageSharp-supported format:
  Bmp, Gif, Jpeg, Pbm, Png, Tiff, Tga, WebP, Qoi.
- Throws a clear error message if the file is missing or unreadable.
- When AutoOrient = true, orientation must match EXIF orientation tag.
- URL loading must validate:
  - URL accessibility
  - Content type (must be an image)
  - Network failures

---

## Operational Behaviour

### Disk Loading
```csharp
using var image = Image.Load(path);
```

### URL Loading
```csharp
using var http = new HttpClient();
var stream = await http.GetStreamAsync(url);
using var image = Image.Load(stream);
```

### EXIF Auto Orientation
```csharp
image.Mutate(x => x.AutoOrient());
```

---

## Technical Notes
- Animated GIFs: only first frame loaded (ImageSharp default).
- ICC profiles preserved.
- Non-image content must raise validation errors.
