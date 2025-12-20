using System.Collections.Immutable;
using SixLabors.ImageSharp;

namespace ImageAutomate.Core;

/// <summary>
/// Represents a basic unit of work with a unique identifier and associated metadata.
/// </summary>
/// <remarks>
/// This interface provides a minimal contract for work items that can be tracked and extended with
/// custom metadata. Implementations may use the metadata dictionary to store additional information relevant to the
/// work item's processing or lifecycle.
/// </remarks>
public interface IBasicWorkItem : IDisposable, ICloneable
{
    Guid Id { get; }
    IImmutableDictionary<string, object> Metadata { get; }
}

/// <summary>
/// Represents a unit of work that encapsulates an image and associated metadata for processing.
/// </summary>
/// <remarks>
/// Each work item is assigned a unique identifier upon creation. The associated image is disposed when
/// the work item is disposed. The Metadata dictionary can be used to store additional information relevant to the work
/// item.
/// </remarks>
public interface IWorkItem : IBasicWorkItem
{
    Image Image { get; }
    float SizeMP { get; }
}

/// <summary>
/// Represents a unit of work that encapsulates a list of image and associated metadata for processing.
/// </summary>
/// <remarks>
/// Each work item is assigned a unique identifier upon creation. The associated images are disposed when
/// the work item is disposed. The Metadata dictionary can be used to store additional information relevant to the work
/// item.
/// </remarks>
public interface IBatchWorkItem : IBasicWorkItem
{
    IReadOnlyList<Image> Images { get; }
    float TotalSizeMP { get; }
}