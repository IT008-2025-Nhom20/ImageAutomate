using ImageAutomate.Core;

namespace ImageAutomate.Execution
{
    public class GraphExecutor : IGraphExecutor
    {
        IGraphValidator Validator { get; init; }

        public GraphExecutor(IGraphValidator validator)
        {
            Validator = validator;
        }

        public void Execute(PipelineGraph graph)
        {
            // 1. Validation
            if (!Validator.Validate(graph))
                throw new InvalidOperationException("Error: Graph validation failed.");

            // 2. Initialisation
            Dictionary<IBlock, int> inDegree = graph.Blocks.ToDictionary(block => block, _ => 0);

            foreach (var connection in graph.Connections)
                inDegree[connection.Target]++;
        }
    }
}