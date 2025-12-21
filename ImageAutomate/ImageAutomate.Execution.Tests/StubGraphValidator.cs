using ImageAutomate.Core;
using ImageAutomate.Execution;

namespace ImageAutomate.Execution.Tests;

public class StubGraphValidator : IGraphValidator
{
    public bool Validate(PipelineGraph graph)
    {
        // Always return true to allow testing with Mock blocks
        return true;
    }
}
