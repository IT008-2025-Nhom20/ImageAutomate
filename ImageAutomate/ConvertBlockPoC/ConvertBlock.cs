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
    private JpegEncodingOptions _jpegOptions = new JpegEncodingOptions();
    private PngEncodingOptions _pngOptions = new PngEncodingOptions();

    public event PropertyChangedEventHandler? PropertyChanged;

    public ConvertBlock()
    {
        _jpegOptions.PropertyChanged += (s, e) => OnPropertyChanged(nameof(JpegOptions));
        _pngOptions.PropertyChanged += (s, e) => OnPropertyChanged(nameof(PngOptions));
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
            _jpegOptions = value;
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
            _pngOptions = value;
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
            if (value < 1) value = 1;
            if (value > 100) value = 100;

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