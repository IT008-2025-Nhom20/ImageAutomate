using ImageAutomate.Core;

namespace ImageAutomate.Execution
{
    public interface IGraphValidator
    {
        bool Validate(PipelineGraph graph);
    }
}