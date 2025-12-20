using ImageAutomate.Core;
using ImageAutomate.StandardBlocks;

namespace ImageAutomate.Execution
{
    public class GraphValidator : IGraphValidator
    {
        public bool Validate(PipelineGraph graph)
        {
            return HasExactlyOneLoadBlock(graph)
                && HasAtLeastOneSaveBlock(graph)
                && AllInputSocketsConnected(graph)
                && IsGraphDAG(graph);
        }

        private Dictionary<IBlock, List<IBlock>> BuildAdjacencyList(PipelineGraph graph)
        {
            var adjacencyList = new Dictionary<IBlock, List<IBlock>>();

            foreach (var block in graph.Blocks)
                adjacencyList[block] = [];

            foreach (var connection in graph.Connections)
                adjacencyList[connection.Source].Add(connection.Target);

            return adjacencyList;
        }

        private bool IsGraphDAG(PipelineGraph graph)
        {
            var adjacencyList = BuildAdjacencyList(graph);
            var inDegree = graph.Blocks.ToDictionary(block => block, _ => 0);

            foreach (var source in adjacencyList.Keys)
            {
                foreach (IBlock target in adjacencyList[source])
                {
                    inDegree[target]++;
                }
            }

            Queue<IBlock> queue = new(inDegree
                .Where(kvp => kvp.Value == 0)
                .Select(kvp => kvp.Key)
            );

            int count = 0;

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                count++;

                foreach (var neighbor in adjacencyList[current])
                {
                    inDegree[neighbor]--;

                    if (inDegree[neighbor] == 0)
                        queue.Enqueue(neighbor);
                }
            }

            return count == graph.Blocks.Count;
        }

        private bool HasExactlyOneLoadBlock(PipelineGraph graph)
        {
            return graph.Blocks.OfType<LoadBlock>().Take(2).Count() == 1;
        }

        private bool HasAtLeastOneSaveBlock(PipelineGraph graph)
        {
            return graph.Blocks.OfType<SaveBlock>().Any();
        }

        private bool AllInputSocketsConnected(PipelineGraph graph)
        {
            var connectedTargets = graph.Connections
                .Select(c => c.TargetSocket)
                .ToHashSet();

            foreach (var block in graph.Blocks)
            {
                foreach (var inputSocket in block.Inputs)
                {
                    //* Note: When optional inputs are implemented,
                    //* add a check here for that.

                    if (!connectedTargets.Contains(inputSocket))
                        return false;
                }
            }

            return true;
        }
    }
}