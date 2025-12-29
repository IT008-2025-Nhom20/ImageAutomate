using ImageAutomate.Core;
using ImageAutomate.Execution.MemoryAndAccessTests.LargeSources;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ImageAutomate.Execution.MemoryAndAccessTests;

public abstract class PerformanceTestBase
{
    private readonly StubGraphValidator _validator;
    protected readonly GraphExecutor _executor;
    protected readonly PipelineGraph _graph;

    protected PerformanceTestBase()
    {
        _validator = new StubGraphValidator();
        _executor = new GraphExecutor(_validator);
        _graph = new PipelineGraph();
    }

    protected abstract LargeSource CreateSource(string name);
    protected abstract int ExpectedTotalItems { get; }
    protected abstract int BatchSize { get; }

    [Fact]
    public async Task Topology_Chain()
    {
        // Source -> Passthrough -> Sink
        var source = CreateSource("Source");
        source.MaxShipmentSize = BatchSize;
        var pass = new PassthroughBlock("Pass");
        var sink = new MockSink("Sink");

        _graph.AddBlock(source);
        _graph.AddBlock(pass);
        _graph.AddBlock(sink);

        _graph.AddEdge(source, source.Outputs[0], pass, pass.Inputs[0]);
        _graph.AddEdge(pass, pass.Outputs[0], sink, sink.Inputs[0]);

        var config = new ExecutorConfiguration { MaxShipmentSize = BatchSize };
        await _executor.ExecuteAsync(_graph, config, CancellationToken.None);

        Assert.Equal(ExpectedTotalItems, sink.ReceivedItems.Count);
        VerifyItems(sink.ReceivedItems);
    }

    [Fact]
    public async Task Topology_FanOut()
    {
        // Source -> Splitter(2) -> [PassA, PassB] -> Merger(2) -> Sink
        var source = CreateSource("Source");
        source.MaxShipmentSize = BatchSize;
        var split = new MultiOutputBlock("Split", 2);
        var passA = new PassthroughBlock("A");
        var passB = new PassthroughBlock("B");
        var merge = new MultiInputBlock("Merge", 2);
        var sink = new MockSink("Sink");

        _graph.AddBlock(source);
        _graph.AddBlock(split);
        _graph.AddBlock(passA);
        _graph.AddBlock(passB);
        _graph.AddBlock(merge);
        _graph.AddBlock(sink);

        _graph.AddEdge(source, source.Outputs[0], split, split.Inputs[0]);

        _graph.AddEdge(split, split.Outputs[0], passA, passA.Inputs[0]);
        _graph.AddEdge(passA, passA.Outputs[0], merge, merge.Inputs[0]);

        _graph.AddEdge(split, split.Outputs[1], passB, passB.Inputs[0]);
        _graph.AddEdge(passB, passB.Outputs[0], merge, merge.Inputs[1]);

        _graph.AddEdge(merge, merge.Outputs[0], sink, sink.Inputs[0]);

        var config = new ExecutorConfiguration { MaxShipmentSize = BatchSize };
        await _executor.ExecuteAsync(_graph, config, CancellationToken.None);

        // Expected: Source produces N. Split broadcasts N to A and N to B.
        // A produces N. B produces N.
        // Merge combines N+N = 2N.
        Assert.Equal(ExpectedTotalItems * 2, sink.ReceivedItems.Count);
        VerifyItems(sink.ReceivedItems);
    }

    [Fact]
    public async Task MultiCycle_Execution()
    {
        // Force multiple cycles by using a smaller batch size than total items if possible.
        // But here we adhere to the configured BatchSize.
        // This test logic is same as Chain but explicitly named to indicate intent.

        var source = CreateSource("Source");
        source.MaxShipmentSize = BatchSize;
        var sink = new MockSink("Sink");

        _graph.AddBlock(source);
        _graph.AddBlock(sink);
        _graph.AddEdge(source, source.Outputs[0], sink, sink.Inputs[0]);

        var config = new ExecutorConfiguration { MaxShipmentSize = BatchSize };
        await _executor.ExecuteAsync(_graph, config, CancellationToken.None);

        Assert.Equal(ExpectedTotalItems, sink.ReceivedItems.Count);
        VerifyItems(sink.ReceivedItems);
    }

    [Fact]
    public async Task AccessVerification_DisposalCheck()
    {
        // Explicitly test that we can access pixels of output items.
        // This targets the potential bug where input disposal affects output if not cloned properly.
        var source = CreateSource("Source");
        source.MaxShipmentSize = BatchSize;
        var pass = new PassthroughBlock("Pass"); // Passthrough might return input instance if implemented lazily?
        // Our mock Passthrough DOES clone. But let's assume a block might not, or we want to test Engine behavior.
        // In the Engine, if a block returns an item, it goes to Warehouse.
        // The Engine then disposes the Input.
        // If the block passed the Input directly to Output (referentially equal), then Warehouse holds a disposed item.

        var sink = new MockSink("Sink");

        _graph.AddBlock(source);
        _graph.AddBlock(pass);
        _graph.AddBlock(sink);

        _graph.AddEdge(source, source.Outputs[0], pass, pass.Inputs[0]);
        _graph.AddEdge(pass, pass.Outputs[0], sink, sink.Inputs[0]);

        var config = new ExecutorConfiguration { MaxShipmentSize = BatchSize };
        await _executor.ExecuteAsync(_graph, config, CancellationToken.None);

        Assert.NotEmpty(sink.ReceivedItems);
        foreach (var item in sink.ReceivedItems)
        {
            var workItem = item as IWorkItem;
            Assert.NotNull(workItem);
            // Accessing Image property should be fine.
            // Accessing pixel data ensures underlying buffer is valid.
            var img = workItem.Image;

            // This should throw ObjectDisposedException if the underlying image was disposed
            try
            {
                if (img is Image<Rgba32> rgba)
                {
                    rgba.ProcessPixelRows(accessor =>
                    {
                        var row = accessor.GetRowSpan(0);
                        var p = row[0]; // Touch pixel
                    });
                }
                else
                {
                    // Fallback for generic image
                    // Image abstract class does not support [x,y] indexing directly without pixel type
                    // But we can check width/height or metadata
                    _ = img.Width;
                }
            }
            catch (ObjectDisposedException)
            {
                Assert.Fail("Received item was disposed!");
            }
        }
    }

    protected void VerifyItems(IEnumerable<IBasicWorkItem> items)
    {
        foreach (var item in items)
        {
            if (item is IWorkItem wi)
            {
                // Simple touch to verify not disposed
                _ = wi.Image.Width;
            }
        }
    }
}

public class StubGraphValidator : IGraphValidator
{
    public bool Validate(PipelineGraph graph)
    {
        // Always return true to allow testing with Mock blocks
        return true;
    }

    public Task<bool> ValidateAsync(PipelineGraph graph, CancellationToken cancellationToken = default)
    {
        // Always return true to allow testing with Mock blocks
        return Task.FromResult(true);
    }
}
