# WorkItem

`WorkItem` is the fundamental unit of data in the ImageAutomate pipeline. It encapsulates the data being processed (images) along with metadata.

## Interfaces

### `IBasicWorkItem`
The base interface for all work items.

```csharp
public interface IBasicWorkItem
{
    Guid Id { get; }
    IImmutableDictionary<string, object> Metadata { get; }
}
```

*   **`Id`**: A unique identifier for the work item.
*   **`Metadata`**: A key-value store for additional information.

### `IWorkItem`
Represents a work item containing a single image.

```csharp
public interface IWorkItem : IBasicWorkItem, IDisposable
{
    Image Image { get; }
}
```

*   **`Image`**: The `SixLabors.ImageSharp.Image` object associated with this item.
*   **`Dispose`**: Disposes the underlying image to release resources.

### `IBatchWorkItem`
Represents a work item containing a collection of images.

```csharp
public interface IBatchWorkItem : IBasicWorkItem, IDisposable
{
    IReadOnlyList<Image> Images { get; }
}
```

*   **`Images`**: A read-only list of images.
*   **`Dispose`**: Disposes all images in the list.

## Implementations

### `WorkItem`
The standard implementation of `IWorkItem`.

```csharp
public sealed class WorkItem(Image image, IImmutableDictionary<string, object>? metadata = null) : IWorkItem
```

*   **Immutability**: The metadata is immutable.
*   **Lifecycle**: When `Dispose()` is called, the `Image` is disposed.

### `BatchWorkItem`
The standard implementation of `IBatchWorkItem`.

```csharp
public sealed class BatchWorkItem(IEnumerable<Image> images, IImmutableDictionary<string, object>? metadata = null) : IBatchWorkItem
```

*   **Lifecycle**: When `Dispose()` is called, all images in the `Images` list are disposed.
