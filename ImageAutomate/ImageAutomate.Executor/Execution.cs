using ImageAutomate.Core;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ImageAutomate.Executor;

public class ExecutionState
{
    private readonly ConcurrentDictionary<IBlock, int> _latches = new();
    private readonly ConcurrentDictionary<Socket, List<IBasicWorkItem>> _buffers = new();

    public void Initialize(IEnumerable<IBlock> blocks, IEnumerable<Connection> connections)
    {
        _latches.Clear();
        _buffers.Clear();

        foreach (var block in blocks)
        {
            _latches[block] = 0;
        }

        foreach (var connection in connections)
        {
            // nếu chưa tôn tài value = 1, đã tồn tại value++
            _latches.AddOrUpdate(connection.Target, 1, (key, value) => value + 1);
        }
    }
    public int DecrementLatch(IBlock block)
    {
        return _latches.AddOrUpdate(block, 0, (key, value) => value - 1);
    }
    public int GetLatch(IBlock block)
    {
        return _latches.TryGetValue(block, out var val) ? val : 0;
    }
    public void AddToBuffer(Socket socket, IBasicWorkItem item)
    {
        _buffers.AddOrUpdate(socket, k => new List<IBasicWorkItem> { item }, (k, list) =>
        {
            lock (list)
            {
                list.Add(item);
                return list;
            }
        });
    }

    public IReadOnlyList<IBasicWorkItem> GetBuffer(Socket socket)
    {
        if (_buffers.TryGetValue(socket, out var list))
        {
            lock (list)
            {
                return list.ToList();
            }
        }
        return new List<IBasicWorkItem>();
    }

    public IEnumerable<List<IBasicWorkItem>> GetAllBuffers()
    {
        return _buffers.Values;
    }
}

public class Execution_updated
{
    private readonly ExecutionState _state = new ExecutionState();
    private readonly ConcurrentDictionary<Socket, IReadOnlyList<IBasicWorkItem>> _finalResult = new ConcurrentDictionary<Socket, IReadOnlyList<IBasicWorkItem>>();

    public void ValidateGraph(IEnumerable<IBlock> blocks, IEnumerable<Connection> connections)
    {
        foreach (var conn in connections)
        {
            // không tồn tại connection trong các blocks đầu vào
            if (!blocks.Contains(conn.Source) || !blocks.Contains(conn.Target))
                throw new ArgumentException($"Connection reference unknown block: {conn.Source.Title} --> {conn.Target.Title}");
        }

        if (HasCycle(blocks, connections))
        {
            throw new ArgumentException("Cycle dected in pipeline Graph.");
        }
    }
    private bool HasCycle(IEnumerable<IBlock> blocks, IEnumerable<Connection> connections)
    {
        var adj = new Dictionary<IBlock, List<IBlock>>();
        var inDegree = new Dictionary<IBlock, int>();

        foreach (var block in blocks)
        {
            adj[block] = new List<IBlock>();
            inDegree[block] = 0;
        }

        foreach (var conn in connections)
        {
            adj[conn.Source].Add(conn.Target);
            inDegree[conn.Target]++;
        }

        int visitedCount = 0;
        var queue = new Queue<IBlock>();

        foreach (var x in inDegree)
        {
            if (x.Value == 0)
            {
                queue.Enqueue(x.Key);
            }
        }

        while (queue.Count > 0)
        {
            var curBlock = queue.Dequeue();
            visitedCount++;

            foreach (var block in adj[curBlock])
            {
                inDegree[block]--;
                if (inDegree[block] == 0)
                {
                    queue.Enqueue(block);
                }
            }
        }
        return blocks.Count() != visitedCount;
    }
    public async Task<IDictionary<Socket, IReadOnlyList<IBasicWorkItem>>> ExecutatingGraph(List<IBlock> blocks, List<Connection> connections)
    {
        ValidateGraph(blocks, connections);

        _state.Initialize(blocks, connections);
        _finalResult.Clear();

        var scheduler = new ConcurrentQueue<IBlock>();
        foreach (var block in blocks)
        {
            if (_state.GetLatch(block) == 0)
            {
                scheduler.Enqueue(block);
            }
        }

        var semaphore = new SemaphoreSlim(Environment.ProcessorCount);
        var activateTasks = new List<Task>();


        try
        {
            while (!scheduler.IsEmpty || activateTasks.Count > 0)
            {
                if (scheduler.TryDequeue(out var block))
                {
                    var task = ProcessBlockAsync(block, connections, semaphore, scheduler);
                    activateTasks.Add(task);
                }
                else
                {
                    if (activateTasks.Count > 0)
                    {
                        var completedTask = await Task.WhenAny(activateTasks);
                        activateTasks.Remove(completedTask);

                        await completedTask;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
        catch (Exception)
        {
            if (activateTasks.Count > 0)
            {
                try
                {
                    await Task.WhenAll(activateTasks);
                }
                catch
                {
                }
            }

            // Cleanup on error
            DisposeAllBuffers();
            throw;
        }

        return _finalResult;
    }

    private async Task ProcessBlockAsync(IBlock block, List<Connection> connections, SemaphoreSlim semaphore, ConcurrentQueue<IBlock> scheduler)
    {
        await semaphore.WaitAsync();
        try
        {
            //1. Prepare Inputs
            var blockInputs = new Dictionary<Socket, IReadOnlyList<IBasicWorkItem>>();
            foreach (var inputSocket in block.Inputs)
            {
                blockInputs[inputSocket] = _state.GetBuffer(inputSocket);
            }

            //2. Execute Block
            IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> outputs;
            try
            {
                outputs = await Task.Run(() => block.Execute(blockInputs));
            }
            catch
            {
                DisposeInputs(blockInputs, new HashSet<IBasicWorkItem>());
                throw;
            }

            var allOutputItems = new HashSet<IBasicWorkItem>();
            if (outputs != null)
            {
                foreach (var list in outputs.Values)
                {
                    foreach (var item in list)
                    {
                        allOutputItems.Add(item);
                    }
                }
            }
            DisposeInputs(blockInputs, allOutputItems);

            try
            {
                foreach (var outputKvp in outputs)
                {
                    var sourceSocket = outputKvp.Key;
                    var items = outputKvp.Value;

                    var outgoingConnections = connections
                        .Where(c => c.Source == block && c.SourceSocket == sourceSocket)
                        .ToList();

                    if (outgoingConnections.Count == 0)
                    {
                        _finalResult[sourceSocket] = items;
                        continue;
                    }

                    for (int i = 0; i < outgoingConnections.Count; i++)
                    {
                        var connection = outgoingConnections[i];
                        var targetBlock = connection.Target;
                        var targetSocket = connection.TargetSocket;

                        bool isLast = (i == outgoingConnections.Count - 1);

                        var itemsForTarget = new List<IBasicWorkItem>();

                        foreach (var item in items)
                        {
                            if (isLast)
                            {
                                itemsForTarget.Add(item);
                            }
                            else
                            {
                                // Clone
                                itemsForTarget.Add(CloneWorkItem(item));
                            }
                        }

                        foreach (var item in itemsForTarget)
                        {
                            _state.AddToBuffer(targetSocket, item);
                        }

                        // Decrement Latch
                        int newLatch = _state.DecrementLatch(targetBlock);
                        if (newLatch == 0)
                        {
                            scheduler.Enqueue(targetBlock);
                        }
                    }
                }
            }
            catch
            {
                foreach (var list in outputs.Values)
                {
                    foreach (var item in list)
                    {
                        if (item is IDisposable d) d.Dispose();
                    }
                }
                throw;
            }
        }
        finally
        {
            semaphore.Release();
        }
    }
    private void DisposeAllBuffers()
    {
        foreach (var list in _state.GetAllBuffers())
        {
            lock (list)
            {
                foreach (var item in list)
                {
                    if (item is IDisposable d) d.Dispose();
                }
            }
        }
    }
    private void DisposeInputs(Dictionary<Socket, IReadOnlyList<IBasicWorkItem>> inputs, HashSet<IBasicWorkItem> exemptedItem)
    {
        foreach (var list in inputs.Values)
        {
            foreach (var item in list)
            {
                if (!exemptedItem.Contains(item) && item is IDisposable d)
                {
                    d.Dispose();
                }
            }
        }
    }

    private IBasicWorkItem CloneWorkItem(IBasicWorkItem item)
    {
        if (item is IWorkItem workItem)
        {
            var cloneImage = workItem.Image.Clone(x => { });
            return new WorkItem(cloneImage, workItem.Metadata);
        }
        throw new NotSupportedException($"Work item type {item.GetType().Name} does not support cloning/is unknown.");
    }
}
