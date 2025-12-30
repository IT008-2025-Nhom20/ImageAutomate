namespace ImageAutomate.Execution.Exceptions;

/// <summary>
/// Thrown when pipeline graph validation fails.
/// </summary>
public class PipelineValidationException : Exception
{
    public PipelineValidationException(string message) : base(message) { }
    public PipelineValidationException(string message, Exception innerException)
        : base(message, innerException) { }
}
