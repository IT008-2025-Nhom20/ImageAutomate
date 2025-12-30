namespace ImageAutomate.Execution.Exceptions;

/// <summary>
/// Thrown when the pipeline detects no progress within the configured timeout.
/// Indicates a potential deadlock or stalled execution.
/// </summary>
public class PipelineDeadlockException : Exception
{
    public PipelineDeadlockException(string message) : base(message) { }
    public PipelineDeadlockException(string message, Exception innerException)
        : base(message, innerException) { }
}
