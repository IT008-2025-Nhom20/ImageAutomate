using SixLabors.ImageSharp;

namespace ImageAutomate.Core;

/// <summary>
/// Represents a unit of work that encapsulates an image and associated metadata for processing.
/// </summary>
/// <remarks>
/// Each work item is assigned a unique identifier upon creation. The associated image is disposed when
/// the work item is disposed. The Metadata dictionary can be used to store additional information relevant to the work
/// item.
/// </remarks>
/// <param name="image">The image to associate with this work item. Cannot be null.</param>
public sealed class WorkItem(Image image) : IWorkItem
{
    private Dictionary<string, object>? _metadata;
    
    public Guid Id { get; } = Guid.NewGuid();
    public Image Image { get; } = image;
    public IDictionary<string, object> Metadata => _metadata ??= [];

    public void Dispose()
    {
        Image.Dispose();
    }
}

/// <summary>
/// Represents a unit of work that encapsulates a list of image and associated metadata for processing.
/// </summary>
/// <remarks>
/// Each work item is assigned a unique identifier upon creation. The associated images are disposed when
/// the work item is disposed. The Metadata dictionary can be used to store additional information relevant to the work
/// item.
/// </remarks>
/// <param name="images">The list of images to associate with this work item. Cannot be null.</param>
public sealed class BatchWorkItem(IEnumerable<Image> images) : IBatchWorkItem
{
    private Dictionary<string, object>? _metadata;
    
    public Guid Id { get; } = Guid.NewGuid();
    public IReadOnlyList<Image> Images { get; } = [.. images];
    public IDictionary<string, object> Metadata => _metadata ??= [];

    public void Dispose()
    {
        foreach (var image in Images)
        {
            image?.Dispose();
        }
    }
}