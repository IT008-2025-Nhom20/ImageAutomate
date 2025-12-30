# Save Block

`SaveBlock` is a sink block that saves processed images to a specified directory on the file system. It serves as an endpoint for the pipeline.

## Description
This block takes incoming image work items and writes them to disk. It handles directory creation, file naming, and format selection based on file extensions or metadata.

## Configuration Parameters

### `OutputPath`
*   **Type**: `string`
*   **Description**: The directory path where images will be saved.
*   **Required**: Yes

### `Overwrite`
*   **Type**: `bool`
*   **Description**: If true, overwrites existing files with the same name. If false, throws an error if the file exists.
*   **Default**: `false`

### `CreateDirectory`
*   **Type**: `bool`
*   **Description**: If true, automatically creates the `OutputPath` directory if it does not exist.
*   **Default**: `true`

## Properties

### `SkipMetadata`
*   **Type**: `bool`
*   **Description**: If true, skips writing metadata to the output file.
*   **Default**: `false`

## Operational Behavior

### File Naming
The block uses the `FileName` metadata from the input `WorkItem`.

### Format Selection
The output format is determined based on the `Format` and `EncodingOptions` metadata (typically set by `ConvertBlock`), or falls back to the file extension.

### Execution
Each incoming image is saved to the disk. As a Sink block, it does not emit any output work items.

### Error Handling
*   Throws `InvalidOperationException` if `OutputPath` is not set or `FileName` metadata is missing.
*   Throws `IOException` if the file exists and `Overwrite` is false.