using System.ComponentModel;
using System.Diagnostics;
using ImageAutomate.Core;
using SixLabors.ImageSharp;

namespace ImageAutomate.Execution.MemoryAndAccessTests;

/// <summary>
/// Base class for mock blocks.
/// </summary>
public abstract class MockBlock : IBlock
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public MockBlock(string name)
    {
        Name = name;
        Title = name;
        Content = string.Empty;
        Id = Guid.NewGuid();
    }

    public Guid Id { get; }

    public string Name { get; }

    public string Title { get; }

    public string Content { get; }

    public int Width { get; set; } = 100;

    public int Height { get; set; } = 100;

    public virtual IReadOnlyList<Socket> Inputs { get; protected set; } = new List<Socket>();

    public virtual IReadOnlyList<Socket> Outputs { get; protected set; } = new List<Socket>();

    public virtual IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> Execute(IDictionary<Socket, IReadOnlyList<IBasicWorkItem>> inputs)
    {
        return ExecuteInternal(inputs);
    }

    public virtual IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> Execute(IDictionary<string, IReadOnlyList<IBasicWorkItem>> inputs)
    {
        // Map string IDs to Sockets
        var mappedInputs = new Dictionary<Socket, IReadOnlyList<IBasicWorkItem>>();
        foreach (var kvp in inputs)
        {
            var socket = Inputs.FirstOrDefault(s => s.Id == kvp.Key);
            if (socket != null)
            {
                mappedInputs[socket] = kvp.Value;
            }
        }
        return ExecuteInternal(mappedInputs);
    }

    protected virtual IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> ExecuteInternal(IDictionary<Socket, IReadOnlyList<IBasicWorkItem>> inputs)
    {
        var outputs = new Dictionary<Socket, IReadOnlyList<IBasicWorkItem>>();

        if (Inputs.Count == 1 && Outputs.Count == 1)
        {
            var inputList = inputs.TryGetValue(Inputs[0], out var l) ? l : new List<IBasicWorkItem>();
            var outputList = new List<IBasicWorkItem>();
            foreach (var item in inputList)
            {
                if (item is ICloneable cloneable)
                {
                    outputList.Add((IBasicWorkItem)cloneable.Clone());
                }
            }
            outputs[Outputs[0]] = outputList;
        }

        return outputs;
    }

    public void Dispose()
    {
        // No-op
    }

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class PassthroughBlock : MockBlock
{
    public PassthroughBlock(string name) : base(name)
    {
        Inputs = new List<Socket> { new Socket("In", "Input") };
        Outputs = new List<Socket> { new Socket("Out", "Output") };
    }
}

public class MockSink : MockBlock, IShipmentSink
{
    public List<IBasicWorkItem> ReceivedItems { get; } = new List<IBasicWorkItem>();

    public MockSink(string name) : base(name)
    {
        Inputs = new List<Socket> { new Socket("In", "Input") };
    }

    protected override IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> ExecuteInternal(IDictionary<Socket, IReadOnlyList<IBasicWorkItem>> inputs)
    {
        if (inputs.TryGetValue(Inputs[0], out var items))
        {
            foreach (var item in items)
            {
                if (item is ICloneable cloneable)
                {
                    ReceivedItems.Add((IBasicWorkItem)cloneable.Clone());
                }
            }
        }
        return new Dictionary<Socket, IReadOnlyList<IBasicWorkItem>>();
    }
}

public class MultiInputBlock : MockBlock
{
    public MultiInputBlock(string name, int inputCount = 2) : base(name)
    {
        var inputs = new List<Socket>();
        for (int i = 0; i < inputCount; i++)
        {
            inputs.Add(new Socket($"In{i}", $"Input {i}"));
        }
        Inputs = inputs;
        Outputs = new List<Socket> { new Socket("Out", "Output") };
    }

    protected override IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> ExecuteInternal(IDictionary<Socket, IReadOnlyList<IBasicWorkItem>> inputs)
    {
        var outputList = new List<IBasicWorkItem>();

        foreach (var socket in Inputs)
        {
            if (inputs.TryGetValue(socket, out var items))
            {
                foreach (var item in items)
                {
                     if (item is ICloneable cloneable)
                    {
                        outputList.Add((IBasicWorkItem)cloneable.Clone());
                    }
                }
            }
        }

        return new Dictionary<Socket, IReadOnlyList<IBasicWorkItem>>
        {
            { Outputs[0], outputList }
        };
    }
}

public class MultiOutputBlock : MockBlock
{
    public MultiOutputBlock(string name, int outputCount = 2) : base(name)
    {
        Inputs = new List<Socket> { new Socket("In", "Input") };
        var outputs = new List<Socket>();
        for (int i = 0; i < outputCount; i++)
        {
            outputs.Add(new Socket($"Out{i}", $"Output {i}"));
        }
        Outputs = outputs;
    }

    protected override IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> ExecuteInternal(IDictionary<Socket, IReadOnlyList<IBasicWorkItem>> inputs)
    {
        var result = new Dictionary<Socket, IReadOnlyList<IBasicWorkItem>>();

        if (inputs.TryGetValue(Inputs[0], out var items))
        {
            foreach (var socket in Outputs)
            {
                var outputList = new List<IBasicWorkItem>();
                foreach (var item in items)
                {
                    if (item is ICloneable cloneable)
                    {
                        outputList.Add((IBasicWorkItem)cloneable.Clone());
                    }
                }
                result[socket] = outputList;
            }
        }
        else
        {
             foreach (var socket in Outputs)
            {
                result[socket] = new List<IBasicWorkItem>();
            }
        }

        return result;
    }
}
