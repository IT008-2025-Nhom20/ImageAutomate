# Load Block

`LoadBlock` is a source block that loads images from a specified directory on the file system. It serves as the pipeline's entry point for image data.

## Description
This block scans a local directory for supported image files and emits them as `IWorkItem` instances. It supports batching (shipments) to manage memory usage when processing large directories.

## Configuration Parameters

### `SourcePath`
*   **Type**: `string`
*   **Description**: The full file system path to the directory containing input images.
*   **Required**: Yes

### `AutoOrient`
*   **Type**: `bool`
*   **Description**: If true, automatically rotates the image based on EXIF orientation metadata.
*   **Default**: `false`

## Properties

### `MaxShipmentSize`
*   **Type**: `int`
*   **Description**: The maximum number of images to load and emit in a single execution cycle.
*   **Default**: `64`

### `MaxCount`
*   **Type**: `int`
*   **Description**: The maximum number of images to load (optional).

## Operational Behavior

### Initialization
On the first execution, the block scans the `SourcePath` and caches the list of valid image files.

### Execution
It loads a batch of images (up to `MaxShipmentSize`) and emits them.

### Metadata
The block adds the following metadata to each `WorkItem`:
*   `BatchFolder`: The source directory path.
*   `FileName`: The file name (including extension).
*   `FullPath`: The full path to the file.
*   `Format`: The detected image format name.
*   `ShipmentOffset`: The index offset of the current batch.
*   `ShipmentIndex`: The index of the item within the batch.

### Error Handling
*   Files that are not valid images or cannot be loaded are skipped.
*   A `DirectoryNotFoundException` is thrown if `SourcePath` does not exist.