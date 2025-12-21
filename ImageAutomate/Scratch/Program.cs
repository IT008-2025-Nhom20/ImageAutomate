using System.Collections.Immutable;
using ImageAutomate.Core;
using ImageAutomate.StandardBlocks;

namespace Scratch;
public class Program
{
    public static void Main()
    {
        ImmutableDictionary<string, string> dict = ImmutableDictionary<string, string>.Empty;
        dict = dict.SetItem("key", "value");
        Console.WriteLine(dict["key"]);
    }
}