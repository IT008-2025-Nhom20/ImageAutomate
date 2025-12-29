using System.Diagnostics;
using ImageAutomate.Core;
using ImageAutomate.Execution;
using ImageAutomate.Execution.Exceptions;
using Xunit.v3;

[assembly: CaptureConsole]
[assembly: CaptureTrace]

namespace ImageAutomate.Execution.Tests;

public class GraphExecutorTests
{
    private readonly GraphExecutor _executor;
    private readonly PipelineGraph _graph;
    private readonly IGraphValidator _validator;

    public GraphExecutorTests()
    {
        // Use StubGraphValidator to bypass strict type checks (LoadBlock/SaveBlock)
        // so we can test execution logic with Mocks.
        _validator = new StubGraphValidator();
        _executor = new GraphExecutor(_validator);
        _graph = new PipelineGraph();
    }

    [Fact]
    public void DependencySatisfaction_NormalChain_A_B_C()
    {
        // A -> B -> C
        var source = new MockSource("Source", 10);
        var blockB = new PassthroughBlock("BlockB");
        var sink = new MockSink("Sink");

        _graph.AddBlock(source);
        _graph.AddBlock(blockB);
        _graph.AddBlock(sink);

        _graph.AddEdge(source, source.Outputs[0], blockB, blockB.Inputs[0]);
        _graph.AddEdge(blockB, blockB.Outputs[0], sink, sink.Inputs[0]);

        _executor.Execute(_graph);

        Assert.Equal(10, sink.ReceivedItems.Count);
    }

    [Fact]
    public void DependencySatisfaction_CrossLayerLinks()
    {
        // Load -> A; Load -> B; Load -> C
        // B -> C; A -> C
        // C -> Save

        var load = new MockSource("Load", 5);
        var blockA = new PassthroughBlock("A");
        var blockB = new PassthroughBlock("B");
        var blockC = new MultiInputBlock("C", 3); // Inputs: 0 from Load, 1 from B, 2 from A
        var save = new MockSink("Save");

        _graph.AddBlock(load);
        _graph.AddBlock(blockA);
        _graph.AddBlock(blockB);
        _graph.AddBlock(blockC);
        _graph.AddBlock(save);

        // Load -> A
        _graph.AddEdge(load, load.Outputs[0], blockA, blockA.Inputs[0]);
        // Load -> B
        _graph.AddEdge(load, load.Outputs[0], blockB, blockB.Inputs[0]);
        // Load -> C (Input 0)
        _graph.AddEdge(load, load.Outputs[0], blockC, blockC.Inputs[0]);

        // B -> C (Input 1)
        _graph.AddEdge(blockB, blockB.Outputs[0], blockC, blockC.Inputs[1]);

        // A -> C (Input 2)
        _graph.AddEdge(blockA, blockA.Outputs[0], blockC, blockC.Inputs[2]);

        // C -> Save
        _graph.AddEdge(blockC, blockC.Outputs[0], save, save.Inputs[0]);

        _executor.Execute(_graph);

        // Analysis:
        // Load produces 5 items.
        // A receives 5, produces 5.
        // B receives 5, produces 5.
        // C receives 5 (from Load) + 5 (from B) + 5 (from A) = 15 inputs total.
        // C is MultiInputBlock, it merges all inputs to output.
        // Save should receive 15 items.

        Assert.Equal(15, save.ReceivedItems.Count);
    }

    [Fact]
    public void DependencySatisfaction_Backlinks()
    {
        // Note: The graph executor assumes a DAG (Directed Acyclic Graph).
        // Backlinks usually imply cycles, which should be rejected by the validator.
        // Or if "backlink" refers to layout, topologically it's still a DAG.
        // The prompt says: Load -> A; Load -> B -> C; C -> A -> Save
        // Graph edges:
        // Load -> A
        // Load -> B
        // B -> C
        // C -> A
        // A -> Save

        // Is this a cycle?
        // Load -> B -> C -> A
        // Load -> A
        // No cycle. A just happens to be downstream of C, but also downstream of Load directly.
        // It's a "join" or "convergence" at A.

        var load = new MockSource("Load", 5);
        var blockB = new PassthroughBlock("B");
        var blockC = new PassthroughBlock("C");
        var blockA = new MultiInputBlock("A", 2); // Input 0 from Load, Input 1 from C
        var save = new MockSink("Save");

        _graph.AddBlock(load);
        _graph.AddBlock(blockB);
        _graph.AddBlock(blockC);
        _graph.AddBlock(blockA);
        _graph.AddBlock(save);

        // Load -> A (Input 0)
        _graph.AddEdge(load, load.Outputs[0], blockA, blockA.Inputs[0]);

        // Load -> B
        _graph.AddEdge(load, load.Outputs[0], blockB, blockB.Inputs[0]);

        // B -> C
        _graph.AddEdge(blockB, blockB.Outputs[0], blockC, blockC.Inputs[0]);

        // C -> A (Input 1)
        _graph.AddEdge(blockC, blockC.Outputs[0], blockA, blockA.Inputs[1]);

        // A -> Save
        _graph.AddEdge(blockA, blockA.Outputs[0], save, save.Inputs[0]);

        _executor.Execute(_graph);

        // Load -> 5 items
        // Branch 1: Load -> A(In0) : 5 items
        // Branch 2: Load -> B -> C -> A(In1) : 5 items
        // A receives 5 + 5 = 10 items.
        // Save receives 10 items.

        Assert.Equal(10, save.ReceivedItems.Count);
    }

    [Fact]
    public void FailedBlock_AtSource()
    {
        // Source fails immediately
        var sourceMock = new WillFailBlock("BadSource");
        // Note: WillFailBlock is a MockBlock, not IShipmentSource.
        // To test "Source" failure properly in executor which treats IShipmentSource differently,
        // we might need a FailingSource.
        // But let's stick to simple block failure first.
        // If the graph is Source(Fail) -> Sink, validation fails if Source has inputs.
        // Wait, WillFailBlock has inputs.
        // Let's make a wrapper or a new class for failing source if needed.
        // Or just use WillFailBlock as a middle node.

        // Let's create a graph: GoodSource -> FailBlock -> Sink

        var source = new MockSource("GoodSource", 5);
        var failBlock = new WillFailBlock("FailBlock");
        var sink = new MockSink("Sink");

        _graph.AddBlock(source);
        _graph.AddBlock(failBlock);
        _graph.AddBlock(sink);

        _graph.AddEdge(source, source.Outputs[0], failBlock, failBlock.Inputs[0]);
        _graph.AddEdge(failBlock, failBlock.Outputs[0], sink, sink.Inputs[0]);

        // Expectation: Execution throws AggregateException containing the failure
        var ex = Assert.Throws<AggregateException>(() => _executor.Execute(_graph));

        Assert.Contains(ex.InnerExceptions, e => e.Message.Contains("FailBlock"));
        Assert.Empty(sink.ReceivedItems);
    }

    [Fact]
    public void FailedBlock_AtSink()
    {
        // Source -> Sink(Fail)
        // We can't make MockSink fail easily as it's a specific class.
        // We can use WillFailBlock as the sink (it's just a block with no outputs connected, effectively a sink).

        var source = new MockSource("Source", 5);
        var failSink = new WillFailBlock("FailSink");

        _graph.AddBlock(source);
        _graph.AddBlock(failSink);

        _graph.AddEdge(source, source.Outputs[0], failSink, failSink.Inputs[0]);

        var ex = Assert.Throws<AggregateException>(() => _executor.Execute(_graph));
        Assert.Contains(ex.InnerExceptions, e => e.Message.Contains("FailSink"));
    }

    [Fact]
    public void FailedBlock_AtChokePoint()
    {
        // Source1 -> A \
        //               -> Choke(Fail) -> B -> Sink
        // Source2 -> C /

        var source1 = new MockSource("S1", 5);
        var source2 = new MockSource("S2", 5);
        var blockA = new PassthroughBlock("A");
        var blockC = new PassthroughBlock("C");
        var choke = new WillFailBlock("Choke");
        var blockB = new PassthroughBlock("B");
        var sink = new MockSink("Sink");

        // Choke needs 2 inputs? WillFailBlock has 1 input.
        // We can connect both A and C to Choke's single input (merge).

        _graph.AddBlock(source1);
        _graph.AddBlock(source2);
        _graph.AddBlock(blockA);
        _graph.AddBlock(blockC);
        _graph.AddBlock(choke);
        _graph.AddBlock(blockB);
        _graph.AddBlock(sink);

        _graph.AddEdge(source1, source1.Outputs[0], blockA, blockA.Inputs[0]);
        _graph.AddEdge(source2, source2.Outputs[0], blockC, blockC.Inputs[0]);

        // Merge A and C into Choke
        _graph.AddEdge(blockA, blockA.Outputs[0], choke, choke.Inputs[0]);
        _graph.AddEdge(blockC, blockC.Outputs[0], choke, choke.Inputs[0]);

        _graph.AddEdge(choke, choke.Outputs[0], blockB, blockB.Inputs[0]);
        _graph.AddEdge(blockB, blockB.Outputs[0], sink, sink.Inputs[0]);

        var ex = Assert.Throws<AggregateException>(() => _executor.Execute(_graph));
        Assert.Contains(ex.InnerExceptions, e => e.Message.Contains("Choke"));
        Assert.Empty(sink.ReceivedItems);
    }

    [Fact]
    public void BlockedBlock_DownstreamOfFailure()
    {
        // Source -> Fail -> Blocked -> Sink
        // The 'Blocked' block should strictly be 'Blocked' state, not run.
        // We can verify it didn't run by ensuring no items reached sink and no side effects (if we had specific checks).
        // Since we check exceptions, we know Fail failed.

        var source = new MockSource("Source", 5);
        var fail = new WillFailBlock("Fail");
        var downstream = new PassthroughBlock("Downstream");
        var sink = new MockSink("Sink");

        _graph.AddBlock(source);
        _graph.AddBlock(fail);
        _graph.AddBlock(downstream);
        _graph.AddBlock(sink);

        _graph.AddEdge(source, source.Outputs[0], fail, fail.Inputs[0]);
        _graph.AddEdge(fail, fail.Outputs[0], downstream, downstream.Inputs[0]);
        _graph.AddEdge(downstream, downstream.Outputs[0], sink, sink.Inputs[0]);

        var ex = Assert.Throws<AggregateException>(() => _executor.Execute(_graph));
        // Verify downstream never ran?
        // We can't easily inspect internal state here without exposing it, but the outcome implies it.
        Assert.Empty(sink.ReceivedItems);
    }

    [Fact]
    public void ExhaustedSource_NoBackupLink()
    {
        // Source1 -> Merge -> Sink
        // Source1 creates 5 items.
        // Merge should process 5 items.
        // When Source1 exhausts, Merge sees input exhausted, completes.

        var source1 = new MockSource("S1", 5);
        var merge = new PassthroughBlock("Merge");
        var sink = new MockSink("Sink");

        _graph.AddBlock(source1);
        _graph.AddBlock(merge);
        _graph.AddBlock(sink);

        _graph.AddEdge(source1, source1.Outputs[0], merge, merge.Inputs[0]);
        _graph.AddEdge(merge, merge.Outputs[0], sink, sink.Inputs[0]);

        _executor.Execute(_graph);
        Assert.Equal(5, sink.ReceivedItems.Count);
    }

    [Fact]
    public void ExhaustedSource_WithBackupLink()
    {
        // Source1 (5 items) -> Merge -> Sink
        // Source2 (10 items) -> Merge
        // Both connect to Merge.Input[0].
        // When Source1 exhausts, Source2 is still running.
        // Merge should process 5 + 10 = 15 items total.

        var source1 = new MockSource("S1", 5);
        var source2 = new MockSource("S2", 10);
        var merge = new PassthroughBlock("Merge");
        var sink = new MockSink("Sink");

        _graph.AddBlock(source1);
        _graph.AddBlock(source2);
        _graph.AddBlock(merge);
        _graph.AddBlock(sink);

        _graph.AddEdge(source1, source1.Outputs[0], merge, merge.Inputs[0]);
        _graph.AddEdge(source2, source2.Outputs[0], merge, merge.Inputs[0]);
        _graph.AddEdge(merge, merge.Outputs[0], sink, sink.Inputs[0]);

        _executor.Execute(_graph);
        Assert.Equal(15, sink.ReceivedItems.Count);
    }

    [Fact]
    public void MultiIO_ComplexRouting()
    {
        // Source(10) -> Splitter(1 in, 2 out)
        //    Out0 -> A -> Merger(2 in, 1 out) -> Sink
        //    Out1 -> B -> Merger

        var source = new MockSource("Source", 10);
        var splitter = new MultiOutputBlock("Splitter", 2);
        var blockA = new PassthroughBlock("A");
        var blockB = new PassthroughBlock("B");
        var merger = new MultiInputBlock("Merger", 2);
        var sink = new MockSink("Sink");

        _graph.AddBlock(source);
        _graph.AddBlock(splitter);
        _graph.AddBlock(blockA);
        _graph.AddBlock(blockB);
        _graph.AddBlock(merger);
        _graph.AddBlock(sink);

        // Source -> Splitter
        _graph.AddEdge(source, source.Outputs[0], splitter, splitter.Inputs[0]);

        // Splitter Out0 -> A -> Merger In0
        _graph.AddEdge(splitter, splitter.Outputs[0], blockA, blockA.Inputs[0]);
        _graph.AddEdge(blockA, blockA.Outputs[0], merger, merger.Inputs[0]);

        // Splitter Out1 -> B -> Merger In1
        _graph.AddEdge(splitter, splitter.Outputs[1], blockB, blockB.Inputs[0]);
        _graph.AddEdge(blockB, blockB.Outputs[0], merger, merger.Inputs[1]);

        // Merger -> Sink
        _graph.AddEdge(merger, merger.Outputs[0], sink, sink.Inputs[0]);

        _executor.Execute(_graph);

        // Source: 10 items
        // Splitter: 10 inputs.
        //   MultiOutputBlock behavior: Duplicates inputs to all outputs (based on implementation logic?)
        //   Let's check MultiOutputBlock logic:
        //   "foreach (var socket in Outputs) ... result[socket] = outputList"
        //   Yes, it broadcasts.
        // So Out0 gets 10 clones, Out1 gets 10 clones.
        // A gets 10 -> Merger In0 gets 10.
        // B gets 10 -> Merger In1 gets 10.
        // Merger combines In0 and In1 -> 20 items.

        Assert.Equal(20, sink.ReceivedItems.Count);
    }

    [Fact]
    public void ChokePoint_PartialFailure_WithBackup()
    {
        // Source1 -> A(Fail) -> Merger -> Sink
        // Source2 -> B(Pass) -> Merger
        // Merger is a choke point for Source1 and Source2 into Sink.
        // A fails. Branch 1 dies.
        // B succeeds. Branch 2 lives.
        // Merger should still process items from B?
        // OR does failure propagate and kill the whole graph execution?
        // GraphExecutor throws AggregateException if ANY exception occurs.
        // So this test expects failure.

        var source1 = new MockSource("S1", 5);
        var source2 = new MockSource("S2", 5);
        var failA = new WillFailBlock("FailA");
        var passB = new PassthroughBlock("PassB");
        var merger = new PassthroughBlock("Merger"); // 1 input socket, merging 2 connections
        var sink = new MockSink("Sink");

        _graph.AddBlock(source1);
        _graph.AddBlock(source2);
        _graph.AddBlock(failA);
        _graph.AddBlock(passB);
        _graph.AddBlock(merger);
        _graph.AddBlock(sink);

        // Branch 1 (Fails)
        _graph.AddEdge(source1, source1.Outputs[0], failA, failA.Inputs[0]);
        _graph.AddEdge(failA, failA.Outputs[0], merger, merger.Inputs[0]);

        // Branch 2 (OK)
        _graph.AddEdge(source2, source2.Outputs[0], passB, passB.Inputs[0]);
        _graph.AddEdge(passB, passB.Outputs[0], merger, merger.Inputs[0]);

        _graph.AddEdge(merger, merger.Outputs[0], sink, sink.Inputs[0]);

        // Current engine behavior: Throw exception on failure.
        // But maybe we want to verify partial success?
        // The current implementation throws, so we expect throw.
        // But ideally, items from B should have reached Sink before failure or concurrent with it?
        // This is race-condition dependent unless we orchestrate timing.

        var ex = Assert.Throws<AggregateException>(() => _executor.Execute(_graph));
        Assert.Contains(ex.InnerExceptions, e => e.Message.Contains("FailA"));
    }

    #region Shipment and Batching Tests

    [Fact]
    public async Task FinalBatch_ProcessedWhenSourceExhausts()
    {
        // CRITICAL: Tests the bug where final batch was dropped when source exhausted.
        // Source has 5 items, batchSize is 32 (larger than item count).
        // Source produces all 5 items in one batch, then exhausts.
        // All 5 items should reach the sink.

        var source = new MockSource("Source", 5);
        var blockA = new PassthroughBlock("A");
        var sink = new MockSink("Sink");

        _graph.AddBlock(source);
        _graph.AddBlock(blockA);
        _graph.AddBlock(sink);

        _graph.AddEdge(source, source.Outputs[0], blockA, blockA.Inputs[0]);
        _graph.AddEdge(blockA, blockA.Outputs[0], sink, sink.Inputs[0]);

        // Use configuration with larger batch size than source items
        var config = new ExecutorConfiguration { MaxShipmentSize = 32 };
        await _executor.ExecuteAsync(_graph, config, TestContext.Current.CancellationToken);

        // All 5 items must be processed, not dropped
        Assert.Equal(5, sink.ReceivedItems.Count);
    }

    [Fact]
    public async Task MultipleShipmentCycles_AllItemsProcessed()
    {
        // Source has 25 items, batchSize is 10.
        // Should produce 3 cycles: 10 + 10 + 5 = 25 items total.

        var source = new MockSource("Source", 25);
        var sink = new MockSink("Sink");

        _graph.AddBlock(source);
        _graph.AddBlock(sink);

        _graph.AddEdge(source, source.Outputs[0], sink, sink.Inputs[0]);

        var config = new ExecutorConfiguration { MaxShipmentSize = 10 };
        await _executor.ExecuteAsync(_graph, config, TestContext.Current.CancellationToken);

        Assert.Equal(25, sink.ReceivedItems.Count);
    }

    [Fact]
    public async Task TwoSources_DifferentExhaustionTimes()
    {
        // Source1: 5 items (exhausts first)
        // Source2: 15 items (exhausts later)
        // Both feed into Merger -> Sink
        // Total: 20 items should reach sink

        var source1 = new MockSource("S1", 5);
        var source2 = new MockSource("S2", 15);
        var merger = new PassthroughBlock("Merger");
        var sink = new MockSink("Sink");

        _graph.AddBlock(source1);
        _graph.AddBlock(source2);
        _graph.AddBlock(merger);
        _graph.AddBlock(sink);

        _graph.AddEdge(source1, source1.Outputs[0], merger, merger.Inputs[0]);
        _graph.AddEdge(source2, source2.Outputs[0], merger, merger.Inputs[0]);
        _graph.AddEdge(merger, merger.Outputs[0], sink, sink.Inputs[0]);

        var config = new ExecutorConfiguration { MaxShipmentSize = 10 };
        await _executor.ExecuteAsync(_graph, config, TestContext.Current.CancellationToken);

        // S1 produces: 5 (exhausts)
        // S2 produces: 10 + 5 = 15
        // Total: 20
        Assert.Equal(20, sink.ReceivedItems.Count);
    }

    [Fact]
    public void EmptySource_NoItems()
    {
        // Edge case: Source with 0 items should complete without error

        var source = new MockSource("EmptySource", 0);
        var sink = new MockSink("Sink");

        _graph.AddBlock(source);
        _graph.AddBlock(sink);

        _graph.AddEdge(source, source.Outputs[0], sink, sink.Inputs[0]);

        _executor.Execute(_graph);

        Assert.Empty(sink.ReceivedItems);
    }

    #endregion

    #region Graph Pattern Tests

    [Fact]
    public void DiamondPattern_ConvergenceAtEnd()
    {
        // Classic diamond DAG:
        //       A
        //      / \
        //     B   C
        //      \ /
        //       D
        //       |
        //     Sink

        var source = new MockSource("A", 10);
        var blockB = new PassthroughBlock("B");
        var blockC = new PassthroughBlock("C");
        var blockD = new MultiInputBlock("D", 2);
        var sink = new MockSink("Sink");

        _graph.AddBlock(source);
        _graph.AddBlock(blockB);
        _graph.AddBlock(blockC);
        _graph.AddBlock(blockD);
        _graph.AddBlock(sink);

        _graph.AddEdge(source, source.Outputs[0], blockB, blockB.Inputs[0]);
        _graph.AddEdge(source, source.Outputs[0], blockC, blockC.Inputs[0]);
        _graph.AddEdge(blockB, blockB.Outputs[0], blockD, blockD.Inputs[0]);
        _graph.AddEdge(blockC, blockC.Outputs[0], blockD, blockD.Inputs[1]);
        _graph.AddEdge(blockD, blockD.Outputs[0], sink, sink.Inputs[0]);

        _executor.Execute(_graph);

        // A produces 10 items (broadcast to B and C)
        // B gets 10, outputs 10
        // C gets 10, outputs 10
        // D gets 10 + 10 = 20 inputs, outputs 20
        Assert.Equal(20, sink.ReceivedItems.Count);
    }

    [Fact]
    public void LargeFanOut_ManyParallelBranches()
    {
        // Source -> [B1, B2, B3, B4, B5] -> Merger -> Sink
        // Tests parallel branch handling

        var source = new MockSource("Source", 10);
        var merger = new MultiInputBlock("Merger", 5);
        var sink = new MockSink("Sink");

        var branches = new List<PassthroughBlock>();
        for (int i = 0; i < 5; i++)
        {
            branches.Add(new PassthroughBlock($"B{i}"));
        }

        _graph.AddBlock(source);
        foreach (var b in branches)
            _graph.AddBlock(b);
        _graph.AddBlock(merger);
        _graph.AddBlock(sink);

        for (int i = 0; i < 5; i++)
        {
            _graph.AddEdge(source, source.Outputs[0], branches[i], branches[i].Inputs[0]);
            _graph.AddEdge(branches[i], branches[i].Outputs[0], merger, merger.Inputs[i]);
        }
        _graph.AddEdge(merger, merger.Outputs[0], sink, sink.Inputs[0]);

        _executor.Execute(_graph);

        // Source broadcasts 10 items to each of 5 branches
        // Each branch produces 10 items
        // Merger receives 5 * 10 = 50 items
        Assert.Equal(50, sink.ReceivedItems.Count);
    }

    [Fact]
    public void DeepChain_ManySequentialBlocks()
    {
        // Source -> B1 -> B2 -> B3 -> B4 -> B5 -> Sink
        // Tests deep sequential chains

        var source = new MockSource("Source", 10);
        var sink = new MockSink("Sink");

        _graph.AddBlock(source);

        IBlock previous = source;
        Socket previousOutput = source.Outputs[0];

        for (int i = 0; i < 5; i++)
        {
            var block = new PassthroughBlock($"B{i}");
            _graph.AddBlock(block);
            _graph.AddEdge(previous, previousOutput, block, block.Inputs[0]);
            previous = block;
            previousOutput = block.Outputs[0];
        }

        _graph.AddBlock(sink);
        _graph.AddEdge(previous, previousOutput, sink, sink.Inputs[0]);

        _executor.Execute(_graph);

        Assert.Equal(10, sink.ReceivedItems.Count);
    }

    #endregion

    #region Cancellation Tests

    [Fact]
    public async Task Cancellation_StopsExecution()
    {
        // Use SpinlockSource that only stops when cancelled
        var cts = new CancellationTokenSource();
        var source = new SpinlockSource("SpinlockSource") { CancellationToken = cts.Token };
        var sink = new MockSink("Sink");

        _graph.AddBlock(source);
        _graph.AddBlock(sink);

        _graph.AddEdge(source, source.Outputs[0], sink, sink.Inputs[0]);

        var config = new ExecutorConfiguration();

        // Cancel immediately to ensure cancellation is detected
        cts.CancelAfter(2000);

        await Assert.ThrowsAsync<PipelineCancelledException>(
            () => _executor.ExecuteAsync(_graph, config, cts.Token));

        // Should not have processed any items
        Assert.Empty(sink.ReceivedItems);
    }

    #endregion
}
