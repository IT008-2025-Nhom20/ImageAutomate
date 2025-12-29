# JSON Schema for Workspace Files

The workspace file format uses JSON with a defined schema for IntelliSense support.

## Using the Schema

### In Visual Studio Code

Add a `$schema` property at the top of your workspace JSON file:

```json
{
    "$schema": "https://raw.githubusercontent.com/IT007-2025-Nhom20/ImageAutomate/project-restructure/docs/workspace-schema.json",
    "version": "1.0",
    "name": "My Workspace",
    ...
}
```

Or configure VS Code's `settings.json` to automatically associate `.imageautomate` files:

```json
{
    "json.schemas": [
        {
            "fileMatch": ["*.imageautomate"],
            "url": "./docs/workspace-schema.json"
        }
    ]
}
```

### In Visual Studio

The schema will be automatically detected if the `$schema` property is present in the JSON file.

## Schema Location

The schema is available at:
- **Local**: `docs/workspace-schema.json`
- **Remote**: `https://raw.githubusercontent.com/IT007-2025-Nhom20/ImageAutomate/project-restructure/docs/workspace-schema.json`

## Why JSON instead of XML?

JSON was chosen for the following reasons:

1. **Compact and Human-Readable**: JSON is more concise than XML, making files easier to read and edit manually
2. **Native C# Support**: System.Text.Json provides excellent performance and is included in .NET
3. **Better Tooling**: Modern editors (VS Code, Visual Studio) have superior JSON support with IntelliSense, validation, and formatting
4. **Web-Friendly**: JSON is the standard for web APIs and modern applications
5. **Type Safety**: JSON Schema provides strong validation and IntelliSense support
6. **Smaller File Size**: JSON files are typically 30-50% smaller than equivalent XML

## File Extension Recommendation

We recommend using `.imageautomate` or `.json` as the file extension for workspace files:

```csharp
workspace.SaveToFile("my-project.imageautomate");
// or
workspace.SaveToFile("my-project.json");
```

The `.imageautomate` extension makes it clear these are workspace files, while `.json` maintains compatibility with standard JSON tools.
