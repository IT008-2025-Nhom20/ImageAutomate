using System.Collections.Immutable;

using ImageAutomate.Core;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace ImageAutomate.Execution.Tests;

/// <summary>
/// Mock implementation of IWorkItem.
/// </summary>
public class MockWorkItem : IWorkItem
{
    private bool _isDisposed;

    public MockWorkItem(string? sourceBlockName = null)
    {
        Id = Guid.NewGuid();
        Image = new Image<Rgba32>(1, 1);
        Metadata = ImmutableDictionary<string, object>.Empty;
        if (sourceBlockName != null)
        {
            Metadata = Metadata.Add("Source", sourceBlockName);
        }
        SizeMP = 0.000001f;
    }

    private MockWorkItem(Guid id, Image image, IImmutableDictionary<string, object> metadata)
    {
        Id = Guid.NewGuid(); // New ID for clone
        Image = image.Clone(x => { });
        Metadata = metadata;
        SizeMP = 0.000001f;
    }

    public Guid Id { get; }

    public IImmutableDictionary<string, object> Metadata { get; set; }

    public Image Image { get; }

    public float SizeMP { get; }

    public void Dispose()
    {
        if (!_isDisposed)
        {
            Image.Dispose();
            _isDisposed = true;
        }
    }

    public object Clone()
    {
        return new MockWorkItem(Id, Image, Metadata);
    }
}

public class MutableWorkItem : IBasicWorkItem, ICloneable
{
    public Guid Id { get; } = Guid.NewGuid();
    public string Value { get; set; }
    public float SizeMP => 0;

    public IImmutableDictionary<string, object> Metadata { get; set; } = ImmutableDictionary<string, object>.Empty;

    public MutableWorkItem(string value) { Value = value; }

    public object Clone() => new MutableWorkItem(this.Value); // Deep clone behavior
    public void Dispose() { }
}