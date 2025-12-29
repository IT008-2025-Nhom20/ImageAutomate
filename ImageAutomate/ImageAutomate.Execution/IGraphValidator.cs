using ImageAutomate.Core;

namespace ImageAutomate.Execution
{
    public interface IGraphValidator
    {
        /// <summary>
        /// Validates the pipeline graph synchronously.
        /// </summary>
        /// <remarks>
        /// This method is obsolete and will be removed in a future version.
        /// Use <see cref="ValidateAsync"/> instead to avoid blocking the calling thread.
        /// </remarks>
        [Obsolete("Use ValidateAsync instead to avoid blocking the calling thread.")]
        bool Validate(PipelineGraph graph);

        /// <summary>
        /// Validates the pipeline graph asynchronously.
        /// </summary>
        /// <param name="graph">The pipeline graph to validate.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if the graph is valid, false otherwise.</returns>
        Task<bool> ValidateAsync(PipelineGraph graph, CancellationToken cancellationToken = default);
    }
}