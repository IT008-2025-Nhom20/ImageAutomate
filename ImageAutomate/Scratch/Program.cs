using System.Collections.Immutable;
using ImageAutomate.Core;
using ImageAutomate.StandardBlocks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;


namespace Scratch;
public class Program
{
    public static void Main()
    {
        Image image = Image.Load("C:\\Users\\Admin\\Pictures\\Saved Pictures\\Dương xỉ mini.jpg");
        image.Mutate(x => x.BackgroundColor(new GraphicsOptions { BlendPercentage = 0.3f }, Color.Black));
        image.Save("C:\\Users\\Admin\\Pictures\\Screenshots\\image.png", new PngEncoder());
    }
}