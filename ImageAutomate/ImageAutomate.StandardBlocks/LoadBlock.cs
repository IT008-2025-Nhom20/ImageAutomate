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

    private readonly IReadOnlyList<Socket> _inputs = [];
    private readonly IReadOnlyList<Socket> _outputs = [new("Load.out", "Image.out")];

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

    #region Basic Properties

    public string Name => "Load";

    public string Title 
    { 
        get => _title;
    }
    public string Content 
    { 
        get => $"Path: {SourcePath}\nAuto Orient: {AutoOrient}";
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
                { _outputs[0], readOnly }
            };
    }

    public IReadOnlyDictionary<Socket, IReadOnlyList<IBasicWorkItem>> Execute(IDictionary<string, IReadOnlyList<IBasicWorkItem>> inputs)
    {
        var items = LoadWorkItems();

        var list = new List<IBasicWorkItem>(items);

        var readOnly = new ReadOnlyCollection<IBasicWorkItem>(list);

        return new Dictionary<Socket, IReadOnlyList<IBasicWorkItem>>
            {
                { _outputs[0], readOnly }
            };
    }
    
    #endregion

    private IEnumerable<IBasicWorkItem> LoadWorkItems()
    {
        if (string.IsNullOrWhiteSpace(SourcePath))
            throw new InvalidOperationException("LoadBlock: SourcePath is required when LoadFromUrl = false.");
        
        return LoadImageFromDirectory();
    }

    private IEnumerable<IBasicWorkItem> LoadImageFromDirectory()
    {
        if (string.IsNullOrWhiteSpace(SourcePath))
            throw new InvalidOperationException("LoadBlock: SourcePath is required");
        if (!Directory.Exists(SourcePath))
            throw new DirectoryNotFoundException("LoadBlock: directory not found");

        var type = "*.jpg; *.png";
        var patterns = type.Split(';');

        var files = new List<string>();

        foreach (var pattern in patterns)
            files.AddRange(Directory.GetFiles(SourcePath, pattern));

        files.Sort(StringComparer.OrdinalIgnoreCase);

        foreach (var file in files)
        {
            WorkItem wi = LoadImageFile(file);
            wi.Metadata["BatchFolder"] = SourcePath;
            wi.Metadata["FileName"] = Path.GetFileName(file);
            wi.Metadata["FullPath"] = file;
            yield return wi!;
        }
    }

    private WorkItem LoadImageFile(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"LoadBlock: File not found at path '{path}'.", path);

        try
        {
            var image = Image.Load<Rgba32>(path);

            var format = image.Metadata.DecodedImageFormat
                ?? throw new InvalidOperationException($"LoadBlock: Unsupported or unknown image format for file '{path}'.");
            
            if (AutoOrient)
                image.Mutate(x => x.AutoOrient());

            return new WorkItem(image);
        }
        catch (Exception ex) when (ex is IOException
                                   || ex is UnauthorizedAccessException
                                   || ex is UnknownImageFormatException
                                   || ex is InvalidImageContentException)
        {
            throw new InvalidOperationException(
                $"LoadBlock: Failed to load image from file '{path}': {ex.Message}", ex);
        }
    }

    #region IDisposable

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion
}
