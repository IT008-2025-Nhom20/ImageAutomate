namespace ImageAutomate.Execution.Exceptions;

/// <summary>
/// Thrown when a block execution fails during pipeline execution.
/// </summary>
public class PipelineExecutionException : Exception
{
    public PipelineExecutionException(string message) : base(message) { }
    public PipelineExecutionException(string message, Exception innerException)
        : base(message, innerException) { }
}
