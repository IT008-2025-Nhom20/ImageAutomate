import os
import random
from PIL import Image
import numpy as np

def generate_images():
    configs = [
        ("480p", 640, 480),
        ("720p", 1280, 720),
        ("1080p", 1920, 1080),
        ("2160p", 3840, 2160)
    ]

    base_dir = "Testcases"

    if not os.path.exists(base_dir):
        os.makedirs(base_dir)

    for name, width, height in configs:
        dir_path = os.path.join(base_dir, name)
        if not os.path.exists(dir_path):
            os.makedirs(dir_path)

        print(f"Generating 100 images for {name} ({width}x{height})...")

        for i in range(100):
            # Generate random noise
            # Create a numpy array of random bytes
            data = np.random.randint(0, 255, (height, width, 3), dtype=np.uint8)
            img = Image.fromarray(data, 'RGB')

            filename = f"image_{i:03d}.png"
            img.save(os.path.join(dir_path, filename))

    print("Done.")

if __name__ == "__main__":
    try:
        import numpy
        import PIL
    except ImportError:
        print("Please install numpy and pillow: pip install numpy pillow")
        exit(1)

    generate_images()
