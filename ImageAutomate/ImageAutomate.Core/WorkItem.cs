using System.Collections.Immutable;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace ImageAutomate.Core;

public sealed class WorkItem : IWorkItem
{
    public Guid Id { get; } = Guid.NewGuid();
    public Image Image { get; }
    public float SizeMP { get; }
    private IImmutableDictionary<string, object>? _metadata;
    public IImmutableDictionary<string, object> Metadata =>
        _metadata ??= ImmutableDictionary<string, object>.Empty;

    /// <summary>
    /// Represents a unit of work that encapsulates an image and associated metadata for processing.
    /// </summary>
    /// <remarks>
    /// Each work item is assigned a unique identifier upon creation. The associated image is disposed when
    /// the work item is disposed. The Metadata dictionary can be used to store additional information relevant to the work
    /// item.
    /// </remarks>
    /// <exception cref=""ArgumentNullException">Thrown when the provided image is null.</exception>
    /// <param name="image">The image to associate with this work item. Cannot be null.</param>
    /// <param name="metadata">Optional metadata dictionary associated with this work item.</param>
    public WorkItem(Image image, IImmutableDictionary<string, object>? metadata = null)
    {
        Image = image ?? throw new ArgumentNullException(nameof(image));
        _metadata = metadata;
        SizeMP = Image.Width * Image.Height / 1_000_000f;
    }

    /// <summary>
    /// Internal constructor to clone with pre-calculated size.
    /// </summary>
    /// <exception cref=""ArgumentNullException">Thrown when the provided image is null.</exception>
    /// <param name="image">The image to associate with this work item. Cannot be null.</param>
    /// <param name="metadata">Optional metadata dictionary associated with this work item.</param>
    /// <param name="sizeMP">Pre-calculated size in megapixels.</param>
    WorkItem(Image image, float sizeMP, IImmutableDictionary<string, object>? metadata = null)
    {
        Image = image ?? throw new ArgumentNullException(nameof(image));
        _metadata = metadata;
        SizeMP = sizeMP;
    }

    public void Dispose()
    {
        Image.Dispose();
    }

    /// <summary>
    /// Creates a deep clone of this work item, including a cloned image.
    /// </summary>
    /// <returns>A deep-cloned instance of this WorkItem</returns>
    public object Clone()
        => new WorkItem(Image.Clone(x => { }), SizeMP, Metadata);
}

/// <summary>
/// Represents a unit of work that encapsulates a list of image and associated metadata for processing.
/// </summary>
/// <remarks>
/// Each work item is assigned a unique identifier upon creation. The associated images are disposed when
/// the work item is disposed. The Metadata dictionary can be used to store additional information relevant to the work
/// item.
/// </remarks>
/// <exception cref="ArgumentNullException">Thrown when the provided list of images is null.</exception>
/// <param name="images">The list of images to associate with this work item. Cannot be null.</param>
public sealed class BatchWorkItem(IEnumerable<Image> images, IImmutableDictionary<string, object>? metadata = null) : IBatchWorkItem
{
    public Guid Id { get; } = Guid.NewGuid();
    private IImmutableDictionary<string, object>? _metadata = metadata;
    public IReadOnlyList<Image> Images { get; } = [.. images ?? throw new ArgumentNullException(nameof(images))];
    public float TotalSizeMP => Images.Sum(img => img.Width * img.Height / 1_000_000f);
    public IImmutableDictionary<string, object> Metadata =>
        _metadata ??= ImmutableDictionary<string, object>.Empty;
    
    public void Dispose()
    {
        foreach (var image in Images)
        {
            image?.Dispose();
        }
    }

    /// <summary>
    /// Creates a deep clone of this batch work item, including cloned images.
    /// </summary>
    /// <returns>A deep-cloned instance of this BatchWorkItem</returns>
    public object Clone()
    {
        var clonedImages = Images.Select(img => img.Clone(x => { })).ToList();
        return new BatchWorkItem(clonedImages, Metadata);
    }
}