using ImageAutomate.Core;
using System.Collections.ObjectModel;
using System.ComponentModel;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace ImageAutomate.StandardBlocks;

public class LoadBlock : IBlock
{
    #region Fields
    private readonly Socket _outputSocket = new("Load.out", "Image.out");
    private readonly IReadOnlyList<Socket> _inputs;
    private readonly IReadOnlyList<Socket> _outputs;

    private string _sourcePath = string.Empty;
    private bool _autoOrient = false;

    private bool _alwaysEncode = false;
    private bool disposedValue = false;

    private int _width = 200;
    private int _height = 100;

    private string _title = "Load";
    private string _content = "Load image";
    #endregion

    #region InotifyPropertyChanged
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion

    #region Constructor
    public LoadBlock()
    {
        _inputs = Array.Empty<Socket>();
        _outputs = new[] { _outputSocket};
    }
    #endregion

    #region Basic Properties
    public string Name => "Load";

    public string Title 
    { 
        get => _title;
    }
    public string Content 
    { 
        get => $"Path: {SourcePath}\nRe-encode: {AlwaysEncode}";
    }

    [Category("Layout")]
    [Description("Width of the black node")]
    public int Width 
    { 
        get => _width;
        set
        {
            if (_width != value)
            {
                _width = value;
                OnPropertyChanged(nameof(Width));
            }
        }
    }

    [Category("Layout")]
    [Description("Width of the black node")]
    public int Height 
    { 
        get => _height;
        set 
        {
            if (_height != value)
            {
                _height = value;
                OnPropertyChanged(nameof(Height));
            }    
        }
    }
    #endregion

    #region Configuration Propertise
    [Category("Configuration")]
    [Description("File system path to the input image ")]
    public string SourcePath
    {
        get => _sourcePath;
        set
        {
            if (_sourcePath != value)
            {
                _sourcePath = value;
                OnPropertyChanged(nameof(SourcePath));
            }    
        }
    }

    [Category("Confiuration")]
    [Description("If true, applies EXIF orientation correction automatically")]
    public bool AutoOrient
    {
        get => _autoOrient;
        set
        {
            if (_autoOrient != value)
            {
                _autoOrient = value;
                OnPropertyChanged(nameof(AutoOrient));
            }
        }
    }

    [Category("Configuration")]
    [Description("Force re-encoding even when format matches")]
    public bool AlwaysEncode
    {
        get => _alwaysEncode;
        set
        {
            if (_alwaysEncode != value)
            {
                _alwaysEncode = value;
                OnPropertyChanged(nameof(AlwaysEncode));
            }
        }
    }
    #endregion

    #region Socket
    public IReadOnlyList<Socket> Inputs => _inputs;

    public IReadOnlyList<Socket> Outputs => _outputs;
    #endregion

    #region Execute
    public IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> Execute(IDictionary<Socket, IReadOnlyList<IBasicWorkItem>> inputs)
    {
        var items = LoadWorkItems();

        var list = new List<IBasicWorkItem>(items);

        var readOnly = new ReadOnlyCollection<IBasicWorkItem>(list);

        return new Dictionary<Socket, IReadOnlyList<IBasicWorkItem>>
            {
                { _outputSocket, readOnly }
            };
    }

    public IReadOnlyDictionary<string, IReadOnlyList<IBasicWorkItem>> Execute(IDictionary<string, IReadOnlyList<IBasicWorkItem>> inputs)
    {
        var items = LoadWorkItems();

        var list = new List<IBasicWorkItem>(items);
   
        var readOnly = new ReadOnlyCollection<IBasicWorkItem>(list);

        return new Dictionary<string, IReadOnlyList<IBasicWorkItem>>
            {
                { _outputSocket.Id, readOnly }
            };
    }

    private IEnumerable<IBasicWorkItem?> LoadWorkItems()
    {
        if (string.IsNullOrWhiteSpace(SourcePath))
            throw new InvalidOperationException("LoadBlock: SourcePath is required when LoadFromUrl = false.");
        return LoadFromFolderInternal();
    }

    public IEnumerable<IBasicWorkItem?> LoadFromFolderInternal()
    {
        if (string.IsNullOrWhiteSpace(SourcePath)) throw new InvalidOperationException("LoadBlock: SourcePath is required");
        if (!Directory.Exists(SourcePath)) throw new DirectoryNotFoundException("LoadBlock: SourPath of directory is not found");

        var type = "*.jpg; *.png";
        var patterns = type.Split(';');

        var files = new List<string>();

        foreach ( var pattern in patterns)
        {
            var found = Directory.GetFiles(SourcePath, pattern);
            files.AddRange(found);
        }
        files.Sort(StringComparer.OrdinalIgnoreCase);

        foreach (var file in files)
        {
            // Reuse hàm cũ
            IBasicWorkItem? wi = null;

            wi = LoadFromFileInternal(file);

            if (wi != null)
            {
                // Bổ sung metadata cho batch
                wi.Metadata["BatchFolder"] = SourcePath;
                wi.Metadata["FileName"] = Path.GetFileName(file);
                wi.Metadata["FullPath"] = file;
            }
            yield return wi!;
        }
    }
    public IBasicWorkItem LoadFromFileInternal(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"LoadBlock: File not found at path '{path}'.", path);

        try
        {
            var fs = File.OpenRead(path);
            var image = Image.Load<Rgba32>(fs);

            var format = image.Metadata.DecodedImageFormat;
            if (format is null)
                throw new InvalidOperationException($"LoadBlock: Unsupported or unknown image format for file '{path}'.");
            
            if (AutoOrient)
            {
                image.Mutate(x => x.AutoOrient());
            }

            using var ms = new MemoryStream();
            image.Save(ms, format);
            var bytes = ms.ToArray();

            return new WorkItem(image);
        }
        catch (Exception ex) when (ex is IOException
                                   || ex is UnauthorizedAccessException
                                   || ex is SixLabors.ImageSharp.UnknownImageFormatException
                                   || ex is SixLabors.ImageSharp.InvalidImageContentException)
        {
            throw new InvalidOperationException(
                $"LoadBlock: Failed to load image from file '{path}': {ex.Message}", ex);
        }
    }

    #endregion

    #region IDisposable
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~LoadBlock()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    #endregion

}
