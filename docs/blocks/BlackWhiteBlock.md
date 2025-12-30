# Black White Block

## Description
The Black White Block converts an image to black and white (monochrome). This is a specialized desaturation effect that mimics black and white photography.

## Configuration Parameters
This block has no specific configuration parameters other than standard layout.

## Operational Behavior

### Execution
The block applies `Image.Mutate(x => x.BlackWhite())`.
It operates on the entire image.
