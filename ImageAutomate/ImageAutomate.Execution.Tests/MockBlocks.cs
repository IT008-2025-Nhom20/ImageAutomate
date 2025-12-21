using System.ComponentModel;
using System.Diagnostics;
using ImageAutomate.Core;

namespace ImageAutomate.Execution.Tests;

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
        // Default no-op behavior:
        // If 1 input and 1 output, pass through.
        // Otherwise return empty.

        var outputs = new Dictionary<Socket, IReadOnlyList<IBasicWorkItem>>();

        if (Inputs.Count == 1 && Outputs.Count == 1)
        {
            var inputList = inputs.TryGetValue(Inputs[0], out var l) ? l : new List<IBasicWorkItem>();
            var outputList = new List<IBasicWorkItem>();
            foreach (var item in inputList)
            {
                if (item is ICloneable cloneable)
                {
                    // Clone because the engine disposes inputs
                    outputList.Add((IBasicWorkItem)cloneable.Clone());
                }
            }
            outputs[Outputs[0]] = outputList;
        }

        Debug.WriteLine($"MockBlock '{Name}' executed. Inputs: {inputs.Count}, Outputs: {outputs.Count}");

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

public class MockSource : MockBlock, IShipmentSource
{
    private int _itemsProduced = 0;
    public int TotalItemsToProduce { get; set; } = 10;
    public int MaxShipmentSize { get; set; } = 5;

    public MockSource(string name, int totalItems = 10) : base(name)
    {
        Outputs = new List<Socket> { new Socket("Out", "Output") };
        TotalItemsToProduce = totalItems;
    }

    protected override IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> ExecuteInternal(IDictionary<Socket, IReadOnlyList<IBasicWorkItem>> inputs)
    {
        var outputList = new List<IBasicWorkItem>();
        int count = 0;

        while (count < MaxShipmentSize && _itemsProduced < TotalItemsToProduce)
        {
            outputList.Add(new MockWorkItem(Name));
            _itemsProduced++;
            count++;
        }

        Debug.WriteLine($"MockSource '{Name}' produced {count} items. Total produced: {_itemsProduced}/{TotalItemsToProduce}");

        return new Dictionary<Socket, IReadOnlyList<IBasicWorkItem>>
        {
            { Outputs[0], outputList }
        };
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

public class WillFailBlock : MockBlock
{
    public WillFailBlock(string name) : base(name)
    {
        Inputs = new List<Socket> { new Socket("In", "Input") };
        Outputs = new List<Socket> { new Socket("Out", "Output") };
    }

    protected override IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> ExecuteInternal(IDictionary<Socket, IReadOnlyList<IBasicWorkItem>> inputs)
    {
        throw new Exception($"Block {Name} failed intentionally.");
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

public class MultiIOBlock : MockBlock
{
    public MultiIOBlock(string name, int inputCount = 2, int outputCount = 2) : base(name)
    {
        var inputs = new List<Socket>();
        for (int i = 0; i < inputCount; i++)
        {
            inputs.Add(new Socket($"In{i}", $"Input {i}"));
        }
        Inputs = inputs;

        var outputs = new List<Socket>();
        for (int i = 0; i < outputCount; i++)
        {
            outputs.Add(new Socket($"Out{i}", $"Output {i}"));
        }
        Outputs = outputs;
    }

    protected override IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> ExecuteInternal(IDictionary<Socket, IReadOnlyList<IBasicWorkItem>> inputs)
    {
        // Simple strategy: Interleave inputs to all outputs
        var allInputs = new List<IBasicWorkItem>();
         foreach (var socket in Inputs)
        {
            if (inputs.TryGetValue(socket, out var items))
            {
                allInputs.AddRange(items);
            }
        }

        var result = new Dictionary<Socket, IReadOnlyList<IBasicWorkItem>>();
        foreach (var socket in Outputs)
        {
            var outputList = new List<IBasicWorkItem>();
            foreach (var item in allInputs)
            {
                 if (item is ICloneable cloneable)
                {
                    outputList.Add((IBasicWorkItem)cloneable.Clone());
                }
            }
            result[socket] = outputList;
        }

        return result;
    }
}

public class SpinlockSource : MockBlock, IShipmentSource
{
    public int MaxShipmentSize { get; set; } = 5;

    public SpinlockSource(string name) : base(name)
    {
        Outputs = new List<Socket> { new Socket("Out", "Output") };
    }

    protected override IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> ExecuteInternal(IDictionary<Socket, IReadOnlyList<IBasicWorkItem>> inputs)
    {
        // Spin indefinitely until cancellation is requested
        // This simulates a long-running operation that can only be stopped by cancellation
        while (true)
        {
            // Check if we should break (in tests, cancellation will throw)
            Thread.Sleep(10);
        }
    }
}
