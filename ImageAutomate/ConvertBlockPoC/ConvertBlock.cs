using System.ComponentModel;

namespace ConvertBlockPoC;

public enum ImageFormat
{
    Bmp,
    Gif,
    Jpeg,
    Pbm,
    Png,
    Tiff,
    Tga,
    WebP,
    Qoi
}

public enum PngCompressionLevel
{
    NoCompression = 0,
    BestSpeed = 1,
    Level2 = 2,
    Level3 = 3,
    Level4 = 4,
    Level5 = 5,
    DefaultCompression = 6,
    Level7 = 7,
    Level8 = 8,
    BestCompression = 9
}

public class ConvertBlock : IBlock
{
    private ImageFormat _targetFormat = ImageFormat.Png;
    private bool _alwaysReEncode = false;
    private JpegEncodingOptions _jpegOptions = new();
    private PngEncodingOptions _pngOptions = new();
    private double _width = 200;
    private double _height = 100;

    public event PropertyChangedEventHandler? PropertyChanged;

    void Options_OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is JpegEncodingOptions)
            OnPropertyChanged(nameof(JpegOptions));
        else if (sender is PngEncodingOptions)
            OnPropertyChanged(nameof(PngOptions));
    }

    public ConvertBlock()
    {
        _jpegOptions.PropertyChanged += Options_OnPropertyChanged;
        _pngOptions.PropertyChanged += Options_OnPropertyChanged;
    }

    [Browsable(false)]
    public string Name => "Convert";

    [Browsable(false)]
    public string ConfigurationSummary
    {
        get
        {
            var opts = TargetFormat switch
            {
                ImageFormat.Jpeg => $"Quality: {JpegOptions.Quality}",
                ImageFormat.Png => $"Compression: {PngOptions.CompressionLevel}",
                _ => "Options: Default"
            };
            return $"Format: {TargetFormat}\nRe-encode: {AlwaysReEncode}\n{opts}";
        }
    }

    [Category("Layout")]
    [Description("Width of the block node")]
    public double Width
    {
        get => _width;
        set
        {
            if (Math.Abs(_width - value) > double.Epsilon)
            {
                _width = value;
                OnPropertyChanged(nameof(Width));
            }
        }
    }

    [Category("Layout")]
    [Description("Height of the block node")]
    public double Height
    {
        get => _height;
        set
        {
            if (Math.Abs(_height - value) > double.Epsilon)
            {
                _height = value;
                OnPropertyChanged(nameof(Height));
            }
        }
    }

    [Category("Configuration")]
    [Description("Target image format for conversion")]
    public ImageFormat TargetFormat
    {
        get => _targetFormat;
        set
        {
            if (_targetFormat != value)
            {
                _targetFormat = value;
                OnPropertyChanged(nameof(TargetFormat));
                OnPropertyChanged(nameof(JpegOptions));
                OnPropertyChanged(nameof(PngOptions));
                OnPropertyChanged(nameof(ConfigurationSummary));
            }
        }
    }

    [Category("Configuration")]
    [Description("Force re-encoding even when format matches")]
    public bool AlwaysReEncode
    {
        get => _alwaysReEncode;
        set
        {
            if (_alwaysReEncode != value)
            {
                _alwaysReEncode = value;
                OnPropertyChanged(nameof(AlwaysReEncode));
                OnPropertyChanged(nameof(ConfigurationSummary));
            }
        }
    }

    [Category("Encoding Options")]
    [Description("JPEG encoding parameters")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public JpegEncodingOptions JpegOptions
    {
        get => _jpegOptions;
        set
        {
            // unsubscribe from old options to avoid memory leaks
            if (_jpegOptions != null)
                _jpegOptions.PropertyChanged -= Options_OnPropertyChanged;
            _jpegOptions = value;
            // resubscribe after assignment
            if (_jpegOptions != null)
                _jpegOptions.PropertyChanged += Options_OnPropertyChanged;
            OnPropertyChanged(nameof(JpegOptions));
        }
    }

    [Category("Encoding Options")]
    [Description("PNG encoding parameters")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public PngEncodingOptions PngOptions
    {
        get => _pngOptions;
        set
        {
            if (_pngOptions != null)
                _pngOptions.PropertyChanged -= Options_OnPropertyChanged;
            _pngOptions = value;
            if (_pngOptions != null)
                _pngOptions.PropertyChanged += Options_OnPropertyChanged;
            OnPropertyChanged(nameof(PngOptions));
        }
    }

    [Browsable(false)]
    public bool ShouldSerializeJpegOptions()
    {
        return TargetFormat == ImageFormat.Jpeg;
    }

    [Browsable(false)]
    public bool ShouldSerializePngOptions()
    {
        return TargetFormat == ImageFormat.Png;
    }

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

[TypeConverter(typeof(ExpandableObjectConverter))]
public class JpegEncodingOptions : INotifyPropertyChanged
{
    private int _quality = 75;

    public event PropertyChangedEventHandler? PropertyChanged;

    [Category("JPEG")]
    [Description("JPEG quality (1-100, higher = better quality, larger file)")]
    [DefaultValue(75)]
    public int Quality
    {
        get => _quality;
        set
        {
            value = Math.Clamp(value, 1, 100);

            if (_quality != value)
            {
                _quality = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Quality)));
            }
        }
    }

    public override string ToString()
    {
        return $"Quality: {Quality}";
    }
}

[TypeConverter(typeof(ExpandableObjectConverter))]
public class PngEncodingOptions : INotifyPropertyChanged
{
    private PngCompressionLevel _compressionLevel = PngCompressionLevel.DefaultCompression;

    public event PropertyChangedEventHandler? PropertyChanged;

    [Category("PNG")]
    [Description("PNG compression level")]
    [DefaultValue(PngCompressionLevel.DefaultCompression)]
    public PngCompressionLevel CompressionLevel
    {
        get => _compressionLevel;
        set
        {
            if (_compressionLevel != value)
            {
                _compressionLevel = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CompressionLevel)));
            }
        }
    }

    public override string ToString()
    {
        return $"Compression: {CompressionLevel}";
    }
}