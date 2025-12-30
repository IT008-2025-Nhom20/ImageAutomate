using ImageAutomate.Core;

namespace ImageAutomate.Execution
{
    public class GraphValidator : IGraphValidator
    {
        public bool Validate(PipelineGraph graph)
        {
            return HasExactlyOneShipmentSource(graph)
                && HasAtLeastOneShipmentSink(graph)
                && AllInputSocketsConnected(graph)
                && IsGraphDAG(graph);
        }

        private Dictionary<IBlock, List<IBlock>> BuildAdjacencyList(PipelineGraph graph)
        {
            var adjacencyList = new Dictionary<IBlock, List<IBlock>>();

            foreach (var block in graph.Nodes)
                adjacencyList[block] = [];

            foreach (var connection in graph.Edges)
                adjacencyList[connection.Source].Add(connection.Target);

            return adjacencyList;
        }

        private bool IsGraphDAG(PipelineGraph graph)
        {
            var adjacencyList = BuildAdjacencyList(graph);
            var inDegree = graph.Nodes.ToDictionary(block => block, _ => 0);

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

            return count == graph.Nodes.Count;
        }

        private bool HasExactlyOneShipmentSource(PipelineGraph graph)
        {
            return graph.Nodes.OfType<IShipmentSource>().Take(2).Count() == 1;
        }

        private bool HasAtLeastOneShipmentSink(PipelineGraph graph)
        {
            return graph.Nodes.OfType<IShipmentSink>().Any();
        }

        private bool AllInputSocketsConnected(PipelineGraph graph)
        {
            var connectedTargets = graph.Edges
                .Select(c => c.TargetSocket)
                .ToHashSet();

            foreach (var block in graph.Nodes)
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
