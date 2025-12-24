using System.ComponentModel;

namespace ImageAutomate.WebPExtension;

/// <summary>
/// Configuration options for WebP image encoding.
/// </summary>
[TypeConverter(typeof(ExpandableObjectConverter))]
public class WebPOptions : INotifyPropertyChanged
{
    private bool _lossless = false;
    private float _quality = 75f;
    private WebPFileFormatType _fileFormat = WebPFileFormatType.Lossy;
    private WebPEncodingMethod _method = WebPEncodingMethod.Default;
    private int _nearLossless = 100;

    public event PropertyChangedEventHandler? PropertyChanged;

    [Category("WebP")]
    [Description("Use lossless compression (ignores Quality if true)")]
    [DefaultValue(false)]
    public bool Lossless
    {
        get => _lossless;
        set
        {
            if (_lossless != value)
            {
                _lossless = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Lossless)));
            }
        }
    }

    [Category("WebP")]
    [Description("Quality factor (0.0 to 100.0). Higher values mean better quality but larger file size.")]
    [DefaultValue(75f)]
    public float Quality
    {
        get => _quality;
        set
        {
            value = Math.Clamp(value, 0f, 100f);

            if (_quality != value)
            {
                _quality = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Quality)));
            }
        }
    }

    [Category("WebP")]
    [Description("WebP file format type")]
    [DefaultValue(WebPFileFormatType.Lossy)]
    public WebPFileFormatType FileFormat
    {
        get => _fileFormat;
        set
        {
            if (_fileFormat != value)
            {
                _fileFormat = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FileFormat)));
            }
        }
    }

    [Category("WebP")]
    [Description("Encoding method (quality/speed tradeoff). Higher values are slower but produce better compression.")]
    [DefaultValue(WebPEncodingMethod.Default)]
    public WebPEncodingMethod Method
    {
        get => _method;
        set
        {
            if (_method != value)
            {
                _method = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Method)));
            }
        }
    }

    [Category("WebP")]
    [Description("Near lossless quality (0-100). 100 is lossless, lower values introduce more loss for smaller file size.")]
    [DefaultValue(100)]
    public int NearLossless
    {
        get => _nearLossless;
        set
        {
            value = Math.Clamp(value, 0, 100);

            if (_nearLossless != value)
            {
                _nearLossless = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NearLossless)));
            }
        }
    }

    public override string ToString()
    {
        if (Lossless)
            return "Lossless";
        return $"Quality: {Quality}, Method: {Method}";
    }
}

/// <summary>
/// WebP file format type
/// </summary>
public enum WebPFileFormatType
{
    Lossy = 0,
    Lossless = 1
}

/// <summary>
/// WebP encoding method (quality vs speed tradeoff)
/// </summary>
public enum WebPEncodingMethod
{
    Fastest = 0,
    Default = 4,
    BestQuality = 6
}
