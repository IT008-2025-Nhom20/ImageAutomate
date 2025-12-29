# SaveBlock

`SaveBlock` is a sink block that saves processed images to a specified directory on the file system. It acts as an endpoint for the pipeline.

## Description
This block takes incoming image work items and writes them to disk. It handles directory creation, file naming, and format selection based on file extensions.

## Configuration Parameters

### `OutputPath`
*   **Type**: `string`
*   **Description**: The directory path where images will be saved.
*   **Editor**: A folder selection dialog is provided for ease of use.
*   **Required**: Yes

### `Overwrite`
*   **Type**: `bool`
*   **Description**: If true, overwrites existing files with the same name. If false, throws an error if the file exists.
*   **Default**: `false`

### `CreateDirectory`
*   **Type**: `bool`
*   **Description**: If true, automatically creates the `OutputPath` directory if it does not exist.
*   **Default**: `true`

## Interface Implementation
SaveBlock implements the `IShipmentSink` marker interface for terminal blocks.

## Properties

### `SkipMetadata`
*   **Type**: `bool`
*   **Description**: If true, skips writing metadata to the output file.
*   **Default**: `false`

## Behavior

*   **File Naming**: Uses the `FileName` metadata from the input `WorkItem`.
*   **Format Selection**: Determines output format based on the `Format` and `EncodingOptions` metadata (from `ConvertBlock`), or falls back to file extension.
*   **Execution**: Saves each incoming image to the disk. Does not emit any output work items (Sink).

## Error Handling
*   Throws `InvalidOperationException` if `OutputPath` is not set or `FileName` metadata is missing.
*   Throws `IOException` if the file exists and `Overwrite` is false.
