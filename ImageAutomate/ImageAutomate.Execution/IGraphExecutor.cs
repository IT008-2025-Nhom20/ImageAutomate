using ImageAutomate.Core;

namespace ImageAutomate.Execution
{
    interface IGraphExecutor
    {
        void Execute(PipelineGraph graph);
    }
}