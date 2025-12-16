using System.ComponentModel;
using ImageAutomate.Core;

namespace Stub_Extension_B;

public class CBlock : IBlock
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public string Name => "CBlock";

    private string _title = "CBlock";
    public string Title
    {
        get => _title;
        set
        {
            if (_title != value)
            {
                _title = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Title)));
            }
        }
    }

    private string _content = "";
    public string Content
    {
        get => _content;
        set
        {
            if (_content != value)
            {
                _content = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Content)));
            }
        }
    }

    public int Width { get; set; }
    public int Height { get; set; }

    public IReadOnlyList<Socket> Inputs => new List<Socket>();

    public IReadOnlyList<Socket> Outputs => new List<Socket>();

    public IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> Execute(IDictionary<Socket, IReadOnlyList<IBasicWorkItem>> inputs)
    {
        Console.WriteLine($"Stub {Name} is being executed...");
        return new Dictionary<Socket, IReadOnlyList<IBasicWorkItem>>();
    }

    public IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> Execute(IDictionary<string, IReadOnlyList<IBasicWorkItem>> inputs)
    {
        Console.WriteLine($"Stub {Name} is being executed...");
        return new Dictionary<Socket, IReadOnlyList<IBasicWorkItem>>();
    }

    public void Dispose()
    {
        // No resources to dispose
    }
}
