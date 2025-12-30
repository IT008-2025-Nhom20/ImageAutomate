# Histogram Equalization Block

## Description
The Histogram Equalization Block adjusts the contrast of an image using its histogram. It can operate globally or adaptively (CLAHE) to enhance local contrast.

## Configuration Parameters

### `Method`
*   **Type**: `HistogramEqualizationMethod`
*   **Description**: The equalization algorithm.
*   **Options**:
    *   `Global`: Adjusts contrast based on the global histogram.
    *   `Adaptive`: Uses Contrast Limited Adaptive Histogram Equalization (CLAHE) on local tiles.

### `ClipHistogram`
*   **Type**: `bool`
*   **Description**: If true, limits the contrast amplification to avoid amplifying noise (Used in Adaptive mode).

### `ClipLimit`
*   **Type**: `int`
*   **Description**: The contrast limit value for clipping. Higher values allow more contrast but potentially more noise.

### `LuminanceLevels`
*   **Type**: `int`
*   **Description**: The number of luminance levels (bins). Standard is 256 for 8-bit images.

### `NumberOfTiles`
*   **Type**: `int`
*   **Description**: The number of tiles (grid size) for Adaptive mode (e.g., 8 means 8x8 grid).

### `SyncChannels`
*   **Type**: `bool`
*   **Description**: Whether to synchronize equalization across color channels to prevent color shifts.

## Operational Behavior

### Execution
The block creates a `HistogramEqualizationOptions` object and applies `Image.Mutate(x => x.HistogramEqualization(options))`.
