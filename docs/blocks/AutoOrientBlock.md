# Auto Orient Block

## Description
The Auto Orient Block automatically adjusts the orientation of an image based on its EXIF metadata. It rotates or flips the image so that it is displayed correctly according to the camera's orientation sensor data.

## Configuration Parameters
This block has no specific configuration parameters.

## Operational Behavior

### Execution
The block applies `Image.Mutate(x => x.AutoOrient())`.
This operation relies on the presence of EXIF orientation tags in the image metadata. If no such metadata exists, the image remains unchanged.
