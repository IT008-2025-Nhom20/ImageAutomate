namespace ImageAutomate.Execution.Exceptions;

/// <summary>
/// Thrown when pipeline execution is cancelled by the user.
/// </summary>
public class PipelineCancelledException : Exception
{
    public PipelineCancelledException(string message) : base(message) { }
    public PipelineCancelledException(string message, Exception innerException)
        : base(message, innerException) { }
}
