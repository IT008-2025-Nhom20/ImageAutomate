using ImageAutomate.Core;
using ImageAutomate.StandardBlocks;
using System.Runtime.CompilerServices;

namespace Scratch;
public class Program
{
    public static void Main()
    {
        LoadBlock loadBlock = new LoadBlock();
        loadBlock.SourcePath = "C:\\Users\\Admin\\Pictures\\Test";
        
        foreach (var item in loadBlock.Execute(new Dictionary<Socket, IReadOnlyList<IBasicWorkItem>>()))
        {
            if (item.Value.Count > 0)
            {
                Console.WriteLine(item.Key.Id);
                foreach (var data in item.Value)
                {
                    Console.WriteLine($"{data.Metadata}\n");
                }    
            }
        }    
    }
}