using ImageAutomate.Core;
using ImageAutomate.StandardBlocks;

namespace ImageAutomate.Executor;

public class Execution
{
    public List<IBlock> GetExecutionOrder(IEnumerable<IBlock> blocks, IEnumerable<Connection> connections)
    {
        var result = new List<IBlock>();

        var inDegreeBlocks = new Dictionary<IBlock, int>(); // int: number of inputs, int = 0 => complete visiting
        var adjacentBlocks = new Dictionary<IBlock, List<IBlock>>();

        foreach (var block in blocks)
        {
            inDegreeBlocks[block] = 0;
            adjacentBlocks[block] = new List<IBlock>();
        }

        int loadConnection = 0;
        foreach (var connection in connections)
        {
            if (connection.Source is LoadBlock)
                loadConnection++;
            inDegreeBlocks[connection.Target]++;
            adjacentBlocks[connection.Source].Add(connection.Target);
        }
        
        // Is LoadBlock connected?
        if (loadConnection == 0)
            throw new ArgumentException("Missing Connection");

        var queue = new Queue<IBlock>();
        foreach(var kvp in inDegreeBlocks)
        {
            if (kvp.Value == 0)
                queue.Enqueue(kvp.Key);            
        }
        
        // Is LoadBlock connecting?
        foreach(var block in queue)
        {
            if (block is not LoadBlock)
                throw new ArgumentException("Invalid Source Count");
        }

        while (queue.Count > 0)
        {
            var currentBlock = queue.Dequeue();
            result.Add(currentBlock);

            foreach(var neighbor in adjacentBlocks[currentBlock])
            {
                inDegreeBlocks[neighbor]--;
                if (inDegreeBlocks[neighbor] == 0)
                {
                    queue.Enqueue(neighbor);
                }
            }
        }

        // Is graph DAG?
        if (result.Count != blocks.Count())
        {
            throw new ArgumentException("Cycle detected");
        }
        return result;
    }

    public async Task ExecutingGraph(List<IBlock> excutionorder, List<Connection> connections)
    {
        var buffer = new Dictionary<Socket, IReadOnlyList<IBasicWorkItem>>();
        foreach(IBlock block in excutionorder)
        {
            var blockInputs = new Dictionary<Socket, IReadOnlyList<IBasicWorkItem>>();

            foreach (Socket inputSocket in block.Inputs)
            {
                //find targetsocket
                var connection = connections.FirstOrDefault(x => x.Target == block && x.TargetSocket == inputSocket);
                
                if (connection != null && buffer.TryGetValue(connection.SourceSocket, out var workItem))
                {
                    blockInputs[inputSocket] = workItem;
                }
                else
                {
                    blockInputs[inputSocket] = new List<IBasicWorkItem>();
                }    
            }

            var outputs = await Task.Run(() => block.Execute(blockInputs));

            foreach(var kvp in outputs)
            {
                buffer[kvp.Key] = kvp.Value;
            }    
        }    
    }
}
