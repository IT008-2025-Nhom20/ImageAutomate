# LoadBlock

`LoadBlock` is a source block that loads images from a specified directory on the file system. It acts as the entry point for image data into the pipeline.

## Description
This block scans a local directory for supported image files and emits them as `IWorkItem` instances. It supports batching (shipments) to manage memory usage when processing large directories.

## Configuration Parameters

### `SourcePath`
*   **Type**: `string`
*   **Description**: The full file system path to the directory containing input images.
*   **Editor**: A folder selection dialog is provided for ease of use.
*   **Required**: Yes

### `AutoOrient`
*   **Type**: `bool`
*   **Description**: If true, automatically rotates the image based on EXIF orientation metadata.
*   **Default**: `false`

## Interface Implementation
LoadBlock implements the `IShipmentSource` marker interface for batch-producing blocks.

## Properties

### `MaxShipmentSize`
*   **Type**: `int`
*   **Description**: The maximum number of images to load and emit in a single execution cycle.
*   **Default**: `64`
*   **Visibility**: Hidden in property grid.

### `MaxCount`
*   **Type**: `int`
*   **Description**: The maximum number of images to load (optional).

## Behavior

*   **Initialization**: On the first execution, it scans the `SourcePath` and caches the list of valid image files.
*   **Execution**: Loads a batch of images (up to `MaxShipmentSize`) and emits them.
*   **Metadata**: Adds the following metadata to each `WorkItem`:
    *   `BatchFolder`: The source directory path.
    *   `FileName`: The file name (including extension).
    *   `FullPath`: The full path to the file.
    *   `Format`: The detected image format name.
    *   `ShipmentOffset`: The index offset of the current batch.
    *   `ShipmentIndex`: The index of the item within the batch.

## Error Handling
*   Skips files that are not valid images or cannot be loaded.
*   Throws `DirectoryNotFoundException` if `SourcePath` does not exist.
