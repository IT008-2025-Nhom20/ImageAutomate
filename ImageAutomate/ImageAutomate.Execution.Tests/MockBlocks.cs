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

    public string Title { get; set; }

    public string Content { get; }

    public int Width { get; set; } = 100;

    public int Height { get; set; } = 100;

    public virtual IReadOnlyList<Socket> Inputs { get; protected set; } = new List<Socket>();

    public virtual IReadOnlyList<Socket> Outputs { get; protected set; } = new List<Socket>();
    public double X { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public double Y { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public virtual IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> Execute(IDictionary<Socket, IReadOnlyList<IBasicWorkItem>> inputs)
    {
        return Execute(inputs, CancellationToken.None);
    }

    public virtual IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> Execute(IDictionary<Socket, IReadOnlyList<IBasicWorkItem>> inputs, CancellationToken cancellationToken)
    {
        return ExecuteInternal(inputs, cancellationToken);
    }

    public virtual IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> Execute(IDictionary<string, IReadOnlyList<IBasicWorkItem>> inputs)
    {
        return Execute(inputs, CancellationToken.None);
    }

    public virtual IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> Execute(IDictionary<string, IReadOnlyList<IBasicWorkItem>> inputs, CancellationToken cancellationToken)
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
        return ExecuteInternal(mappedInputs, cancellationToken);
    }

    protected virtual IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> ExecuteInternal(IDictionary<Socket, IReadOnlyList<IBasicWorkItem>> inputs, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

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
    public IReadOnlyList<string>? ShipmentData { get; set; }

    public MockSource(string name, int totalItems = 10) : base(name)
    {
        Outputs = new List<Socket> { new Socket("Out", "Output") };
        TotalItemsToProduce = totalItems;
    }

    protected override IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> ExecuteInternal(IDictionary<Socket, IReadOnlyList<IBasicWorkItem>> inputs, CancellationToken cancellationToken)
    {
        var outputList = new List<IBasicWorkItem>();
        int count = 0;

        while (count < MaxShipmentSize && _itemsProduced < TotalItemsToProduce)
        {
            cancellationToken.ThrowIfCancellationRequested();
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

    public IReadOnlyList<string> GetShipmentTargets()
    {
        // Return dummy targets matching TotalItemsToProduce
        // ExecutionContext uses count to track exhaustion
        return Enumerable.Range(0, TotalItemsToProduce)
            .Select(i => $"mock-item-{i}")
            .ToList();
    }
}

public class MockSink : MockBlock, IShipmentSink
{
    public List<IBasicWorkItem> ReceivedItems { get; } = new List<IBasicWorkItem>();

    public MockSink(string name) : base(name)
    {
        Inputs = new List<Socket> { new Socket("In", "Input") };
    }

    protected override IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> ExecuteInternal(IDictionary<Socket, IReadOnlyList<IBasicWorkItem>> inputs, CancellationToken cancellationToken)
    {
        if (inputs.TryGetValue(Inputs[0], out var items))
        {
            foreach (var item in items)
            {
                cancellationToken.ThrowIfCancellationRequested();
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

    protected override IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> ExecuteInternal(IDictionary<Socket, IReadOnlyList<IBasicWorkItem>> inputs, CancellationToken cancellationToken)
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

    protected override IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> ExecuteInternal(IDictionary<Socket, IReadOnlyList<IBasicWorkItem>> inputs, CancellationToken cancellationToken)
    {
        var outputList = new List<IBasicWorkItem>();

        foreach (var socket in Inputs)
        {
            if (inputs.TryGetValue(socket, out var items))
            {
                foreach (var item in items)
                {
                     cancellationToken.ThrowIfCancellationRequested();
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

    protected override IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> ExecuteInternal(IDictionary<Socket, IReadOnlyList<IBasicWorkItem>> inputs, CancellationToken cancellationToken)
    {
        var result = new Dictionary<Socket, IReadOnlyList<IBasicWorkItem>>();

        if (inputs.TryGetValue(Inputs[0], out var items))
        {
            foreach (var socket in Outputs)
            {
                var outputList = new List<IBasicWorkItem>();
                foreach (var item in items)
                {
                    cancellationToken.ThrowIfCancellationRequested();
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

    protected override IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> ExecuteInternal(IDictionary<Socket, IReadOnlyList<IBasicWorkItem>> inputs, CancellationToken cancellationToken)
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
                 cancellationToken.ThrowIfCancellationRequested();
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
    public IReadOnlyList<string>? ShipmentData { get; set; }
    public CancellationToken CancellationToken { get; set; }

    public SpinlockSource(string name) : base(name)
    {
        Outputs = new List<Socket> { new Socket("Out", "Output") };
    }

    protected override IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> ExecuteInternal(IDictionary<Socket, IReadOnlyList<IBasicWorkItem>> inputs, CancellationToken cancellationToken)
    {
        // Spin indefinitely until cancellation is requested
        // This simulates a long-running operation that can only be stopped by cancellation
        // Also check the passed cancellationToken
        while (!CancellationToken.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            Thread.Sleep(10);
        }
        
        // Return empty result when cancelled
        return new Dictionary<Socket, IReadOnlyList<IBasicWorkItem>>();
    }

    public IReadOnlyList<string> GetShipmentTargets()
    {
        // Return dummy targets matching MaxShipmentSize
        // This source spins indefinitely until cancelled
        return Enumerable.Range(0, MaxShipmentSize)
            .Select(i => $"spinlock-item-{i}")
            .ToList();
    }
}

public class SingleItemSource : MockBlock, IShipmentSource
{
    public int MaxShipmentSize { get; set; } = 10;
    public IReadOnlyList<string>? ShipmentData { get; set; }
    private bool _produced = false;

    public SingleItemSource(string name) : base(name) 
    {
        Outputs = new List<Socket> { new Socket("Out", "Out") };
    }

    protected override IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> ExecuteInternal(IDictionary<Socket, IReadOnlyList<IBasicWorkItem>> inputs, CancellationToken cancellationToken)
    {
        if (_produced) return new Dictionary<Socket, IReadOnlyList<IBasicWorkItem>> { { Outputs[0], new List<IBasicWorkItem>() } };
        
        _produced = true;
        return new Dictionary<Socket, IReadOnlyList<IBasicWorkItem>> 
        { 
            { Outputs[0], new List<IBasicWorkItem> { new MutableWorkItem("Original") } } 
        };
    }

    public IReadOnlyList<string> GetShipmentTargets()
    {
        // Return exactly 1 dummy target for single item source
        return new List<string> { "single-item" };
    }
}

public class ModifierBlock : MockBlock
{
    private readonly string _newValue;
    public ModifierBlock(string name, string newValue) : base(name) 
    {
            _newValue = newValue;
            Inputs = new List<Socket> { new Socket("In", "In") };
            Outputs = new List<Socket> { new Socket("Out", "Out") };
    }

    protected override IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> ExecuteInternal(IDictionary<Socket, IReadOnlyList<IBasicWorkItem>> inputs, CancellationToken cancellationToken)
    {
        var list = new List<IBasicWorkItem>();
        foreach(var item in inputs[Inputs[0]])
        {
            cancellationToken.ThrowIfCancellationRequested();
            var mutable = (MutableWorkItem)item; // No clone here, we modify "in place" (which should be a clone from Warehouse)
            mutable.Value = _newValue;
            list.Add(mutable);
        }
        return new Dictionary<Socket, IReadOnlyList<IBasicWorkItem>> { { Outputs[0], list } };
    }
}

public class InspectorBlock : MockBlock
{
    public List<IBasicWorkItem> InspectedItems { get; } = new List<IBasicWorkItem>();
    public InspectorBlock(string name) : base(name) 
    {
            Inputs = new List<Socket> { new Socket("In", "In") };
    }

    protected override IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> ExecuteInternal(IDictionary<Socket, IReadOnlyList<IBasicWorkItem>> inputs, CancellationToken cancellationToken)
    {
        if(inputs.TryGetValue(Inputs[0], out var list))
        {
            // Clone again to save state for assertion
            foreach(var item in list)
            {
                cancellationToken.ThrowIfCancellationRequested();
                InspectedItems.Add((IBasicWorkItem)((ICloneable)item).Clone());
            }
        }
        return new Dictionary<Socket, IReadOnlyList<IBasicWorkItem>>();
    }
}

public class SwitchBlock : MockBlock
{
    private readonly bool _out0;
    private readonly bool _out1;

    public SwitchBlock(string name, bool sendToOut0, bool sendToOut1) : base(name)
    {
        _out0 = sendToOut0;
        _out1 = sendToOut1;
        Inputs = new List<Socket> { new Socket("In", "In") };
        Outputs = new List<Socket> { new Socket("Out0", "Out0"), new Socket("Out1", "Out1") };
    }

    protected override IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> ExecuteInternal(IDictionary<Socket, IReadOnlyList<IBasicWorkItem>> inputs, CancellationToken cancellationToken)
    {
        var incoming = inputs[Inputs[0]];
        var res = new Dictionary<Socket, IReadOnlyList<IBasicWorkItem>>();
        
        // Output 0
        if (_out0) res[Outputs[0]] = CloneAll(incoming);
        // Intentionally OMIT key for Output 1 if false, or send empty? 
        // Engine spec says "ExportOutputs" takes dictionary. 
        // If we omit the key, the Warehouse for that socket is not created/updated?
        // Let's send EMPTY list to simulate "Filtered but Active"
        else res[Outputs[1]] = new List<IBasicWorkItem>(); 

        if (_out1 && !_out0) res[Outputs[1]] = CloneAll(incoming); // Simplified logic
        
        return res;
    }

    private List<IBasicWorkItem> CloneAll(IEnumerable<IBasicWorkItem> items)
    {
            return items.Select(x => (IBasicWorkItem)((ICloneable)x).Clone()).ToList();
    }
}

public class CallbackBlock : MockBlock
{
    private readonly Func<IBasicWorkItem, IBasicWorkItem> _action;

    public CallbackBlock(string name, Func<IBasicWorkItem, IBasicWorkItem> action) : base(name)
    {
        _action = action;
        Inputs = new List<Socket> { new Socket("In", "In") };
        Outputs = new List<Socket> { new Socket("Out", "Out") };
    }

    protected override IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> ExecuteInternal(IDictionary<Socket, IReadOnlyList<IBasicWorkItem>> inputs, CancellationToken cancellationToken)
    {
        var outList = new List<IBasicWorkItem>();
        foreach(var item in inputs[Inputs[0]])
        {
            cancellationToken.ThrowIfCancellationRequested();
            outList.Add(_action(item));
        }
        return new Dictionary<Socket, IReadOnlyList<IBasicWorkItem>> { { Outputs[0], outList } };
    }
}