using ImageAutomate.Core;

namespace ImageAutomate.Execution.Tests;

public class GeminiTest
{
    private readonly GraphExecutor _executor;
    private readonly PipelineGraph _graph;
    private readonly StubGraphValidator _validator;

    public GeminiTest()
    {
        _validator = new StubGraphValidator();
        _executor = new GraphExecutor(_validator);
        _graph = new PipelineGraph();
    }

    [Fact]
    public void DataIsolation_Modifications_Are_Local()
    {
        // Scenario: Source -> (A, B). 
        // A modifies the item. B reads the item.
        // B should NOT see A's modification.

        var source = new SingleItemSource("Source");
        var modifier = new ModifierBlock("Modifier", "ModifiedByA");
        var reader = new InspectorBlock("Reader");

        _graph.AddBlock(source);
        _graph.AddBlock(modifier);
        _graph.AddBlock(reader);

        // Connect Source -> Modifier
        _graph.AddEdge(source, source.Outputs[0], modifier, modifier.Inputs[0]);
        // Connect Source -> Reader (Parallel branch)
        _graph.AddEdge(source, source.Outputs[0], reader, reader.Inputs[0]);

        _executor.Execute(_graph);

        // Verify Reader saw the ORIGINAL value, not the one modified by sibling
        Assert.Single(reader.InspectedItems);
        var item = reader.InspectedItems[0] as MutableWorkItem;
        Assert.NotNull(item);
        Assert.Equal("Original", item.Value); // If cloning failed, this would be "ModifiedByA"
    }

    [Fact]
    public void SelectiveFlow_Switch_Behavior()
    {
        // Scenario: Switch block sends data to Out0, but NOTHING to Out1.
        // Branch 1 (Active): Switch.Out0 -> Passthrough -> Merger
        // Branch 2 (Empty):  Switch.Out1 -> Passthrough -> Merger
        // Merger should receive data from B1 and empty from B2.

        var source = new MockSource("Source", 5);
        var switchBlock = new SwitchBlock("Switch", sendToOut0: true, sendToOut1: false);
        var pass0 = new PassthroughBlock("Pass0");
        var pass1 = new PassthroughBlock("Pass1");
        var merger = new MultiInputBlock("Merger", 2);
        var sink = new MockSink("Sink");

        _graph.AddBlock(source);
        _graph.AddBlock(switchBlock);
        _graph.AddBlock(pass0);
        _graph.AddBlock(pass1);
        _graph.AddBlock(merger);
        _graph.AddBlock(sink);

        // Source -> Switch
        _graph.AddEdge(source, source.Outputs[0], switchBlock, switchBlock.Inputs[0]);

        // Branch 0 (Active)
        _graph.AddEdge(switchBlock, switchBlock.Outputs[0], pass0, pass0.Inputs[0]);
        _graph.AddEdge(pass0, pass0.Outputs[0], merger, merger.Inputs[0]);

        // Branch 1 (Silent/Empty)
        _graph.AddEdge(switchBlock, switchBlock.Outputs[1], pass1, pass1.Inputs[0]);
        _graph.AddEdge(pass1, pass1.Outputs[0], merger, merger.Inputs[1]);

        // Merger -> Sink
        _graph.AddEdge(merger, merger.Outputs[0], sink, sink.Inputs[0]);

        _executor.Execute(_graph);

        // Analysis:
        // Source: 5 items.
        // Switch: Sends 5 to Out0, 0 to Out1.
        // Pass0: Processes 5.
        // Pass1: Runs but gets empty input, produces 0.
        // Merger: Gets 5 from In0, 0 from In1. Total 5.
        Assert.Equal(5, sink.ReceivedItems.Count);
    }

    [Fact]
    public void Deterministic_Partial_Failure()
    {
        // Scenario: Parallel branches. One fails, one succeeds.
        // We use latches to ensure the Success branch finishes execution 
        // BEFORE the Fail branch throws. This proves the engine doesn't 
        // discard results from successful branches when an aggregate exception occurs.

        // Source -> Split -> SlowFail
        //                 -> FastSuccess -> Sink

        var source = new MockSource("Source", 1);
        var split = new MultiOutputBlock("Split", 2);
        var sink = new MockSink("Sink");

        // Sync primitives
        using var successFinished = new ManualResetEventSlim(false);

        // Blocks
        var fastBranch = new CallbackBlock("Fast", (item) =>
        {
            // 1. Signal that we are about to finish
            successFinished.Set();

            // 2. CRITICAL FIX: Clone the item.
            // The engine will dispose 'item' (the input) immediately after this method returns.
            // If we return 'item' directly, the downstream Sink receives a disposed object.
            return (IBasicWorkItem)item.Clone();
        });

        var slowBranch = new CallbackBlock("Slow", (item) =>
        {
            // 1. Wait for Fast branch to finish completely
            bool signaled = successFinished.Wait(2000);
            if (!signaled) throw new TimeoutException("Test timed out waiting for fast branch");

            // 2. Now fail
            throw new Exception("Intentional Failure");
        });

        _graph.AddBlock(source);
        _graph.AddBlock(split);
        _graph.AddBlock(fastBranch);
        _graph.AddBlock(slowBranch);
        _graph.AddBlock(sink);

        _graph.AddEdge(source, source.Outputs[0], split, split.Inputs[0]);

        // Fast Branch
        _graph.AddEdge(split, split.Outputs[0], fastBranch, fastBranch.Inputs[0]);
        _graph.AddEdge(fastBranch, fastBranch.Outputs[0], sink, sink.Inputs[0]);

        // Slow Branch
        _graph.AddEdge(split, split.Outputs[1], slowBranch, slowBranch.Inputs[0]);

        // Execute expecting failure
        var ex = Assert.Throws<AggregateException>(() => _executor.Execute(_graph));

        // Verification
        // FIX: Use .Contains() because the executor wraps the exception 
        // e.g. "Block 'Slow' failed: Intentional Failure"
        Assert.Contains(ex.InnerExceptions, e => e.Message.Contains("Intentional Failure"));

        // We DO NOT expect ObjectDisposedException (which happens if we mess up cloning)
        Assert.DoesNotContain(ex.InnerExceptions, e => e.Message.Contains("ObjectDisposedException"));

        // Crucial: The sink MUST have received the item from the fast branch
        // despite the failure in the slow branch.
        Assert.Single(sink.ReceivedItems);
    }
}