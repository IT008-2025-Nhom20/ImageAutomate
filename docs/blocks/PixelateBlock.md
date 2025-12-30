# Pixelate Block

## Description
The Pixelate Block applies a pixelation effect by downsampling and upscaling blocks of pixels. This is used for anonymization, stylization, or mosaic effects.

## Configuration Parameters

### `Size`
*   **Type**: `int`
*   **Description**: Pixel block size. Higher values produce larger pixel blocks.
*   **Range**: 1–100

## Operational Behavior

### Execution
The block applies `Image.Mutate(x => x.Pixelate(Size))`.
The operation creates uniform pixel blocks with crisp edges, preserving the alpha channel.