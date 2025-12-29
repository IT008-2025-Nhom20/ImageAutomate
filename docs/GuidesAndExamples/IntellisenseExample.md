# IntelliSense Support Examples

When you open a workspace JSON file with the `$schema` property in VS Code or Visual Studio, you get:

## 1. Auto-Completion

Type `"` inside the root object and you'll see suggestions for:

- `$schema`
- `version`
- `name`
- `graph`
- `zoom`
- `panX`
- `panY`
- `metadata`

## 2. Property Validation

If you type an invalid property name, the editor will underline it in red and show an error message.

## 3. Value Suggestions

For `blockType` properties, you'll get a dropdown list with all valid block types:
- LoadBlock
- SaveBlock
- BrightnessBlock
- ContrastBlock
- ResizeBlock
- ConvertBlock
- etc.

## 4. Hover Documentation

Hover over any property name to see its description:
- **zoom**: "Zoom level of the canvas (minimum: 0.1, maximum: 10.0, default: 1.0)"
- **sourceBlockIndex**: "Index of the source block in the blocks array (minimum: 0)"

## 5. Type Checking

The editor will show errors if you provide wrong types:

- ❌ `"zoom": "1.5"` (string instead of number)
- ✅ `"zoom": 1.5` (correct)

## Example File with Full IntelliSense Support

```json
{
    "$schema": "https://raw.githubusercontent.com/IT007-2025-Nhom20/ImageAutomate/project-restructure/docs/workspace-schema.json",
    "version": "1.0",
    "name": "My Workspace",
    "graph": {
        "blocks": [
            {
                "blockType": "LoadBlock",  // <-- Shows dropdown with all block types
                "layout": {                // <-- Layout embedded in each block
                    "x": 100,
                    "y": 100,
                    "width": 200,          // <-- Must be number >= 50
                    "height": 100          // <-- Must be number >= 50
                }
            }
        ]
    },
    "zoom": 1.25,                      // <-- Must be between 0.1 and 10.0
    "panX": 0.0,
    "panY": 0.0
}
```

All of this happens automatically when the `$schema` property is present.