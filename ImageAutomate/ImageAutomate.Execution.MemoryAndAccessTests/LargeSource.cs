using System.Collections.Immutable;
using ImageAutomate.Core;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace ImageAutomate.Execution.MemoryAndAccessTests.LargeSources;

public class LargeSource : MockBlock, IShipmentSource
{
    private readonly Image<Rgba32> _masterImage;
    private readonly int _totalItemsToProduce;
    private int _itemsProduced = 0;

    public int MaxShipmentSize { get; set; } = 5;

    public LargeSource(string name, int width, int height, int totalItems) : base(name)
    {
        _totalItemsToProduce = totalItems;
        _masterImage = new Image<Rgba32>(width, height);

        // Generate random noise
        var rnd = new Random(42); // Deterministic seed
        _masterImage.ProcessPixelRows(accessor =>
        {
            for (int y = 0; y < accessor.Height; y++)
            {
                var row = accessor.GetRowSpan(y);
                for (int x = 0; x < row.Length; x++)
                {
                    row[x] = rnd.Next(2) == 0 ? new Rgba32(0, 0, 0) : new Rgba32(255, 255, 255);
                }
            }
        });

        Outputs = new List<Socket> { new Socket("Out", "Output") };
    }

    protected override IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> ExecuteInternal(IDictionary<Socket, IReadOnlyList<IBasicWorkItem>> inputs)
    {
        var outputList = new List<IBasicWorkItem>();
        int count = 0;

        while (count < MaxShipmentSize && _itemsProduced < _totalItemsToProduce)
        {
            // Clone the master image for each dispatch
            var clonedImage = _masterImage.Clone();

            var metadata = ImmutableDictionary.CreateBuilder<string, object>();
            metadata.Add("Source", Name);
            metadata.Add("Index", _itemsProduced);

            outputList.Add(new WorkItem(clonedImage, metadata.ToImmutable()));

            _itemsProduced++;
            count++;
        }

        return new Dictionary<Socket, IReadOnlyList<IBasicWorkItem>>
        {
            { Outputs[0], outputList }
        };
    }
}

public class WorkItem : IWorkItem
{
    private bool _disposedValue;

    public WorkItem(Image image, IImmutableDictionary<string, object> metadata)
    {
        Id = Guid.NewGuid();
        Image = image;
        Metadata = metadata;
    }

    public Image Image { get; }

    public float SizeMP => (float)(Image.Width * Image.Height) / 1000000f;

    public Guid Id { get; }

    public IImmutableDictionary<string, object> Metadata { get; }

    public object Clone()
    {
        // Deep clone the image
        var clonedImage = Image.Clone(x => {});
        return new WorkItem(clonedImage, Metadata);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                Image.Dispose();
            }
            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
