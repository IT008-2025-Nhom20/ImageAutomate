using System.ComponentModel;

namespace WebPExtension;

/// <summary>
/// Configuration options for WebP image encoding in ImageAutomate.
/// These options are used to configure the WebP encoder when converting images.
/// </summary>
[TypeConverter(typeof(ExpandableObjectConverter))]
public class WebPOptions : INotifyPropertyChanged
{
    private bool _lossless = false;
    private float _quality = 75f;
    private WebPEncodingMethod _method = WebPEncodingMethod.Default;
    private int _nearLossless = 100;
    private bool _useAlphaCompression = true;

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Use lossless compression. When true, ignores Quality setting.
    /// </summary>
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
                OnPropertyChanged(nameof(Lossless));
                OnPropertyChanged(nameof(FileFormat));
            }
        }
    }

    /// <summary>
    /// Quality factor (0.0 to 100.0). Higher values mean better quality but larger file size.
    /// Only applies in lossy mode.
    /// </summary>
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
                OnPropertyChanged(nameof(Quality));
            }
        }
    }

    /// <summary>
    /// WebP file format type (derived from Lossless setting).
    /// </summary>
    [Category("WebP")]
    [Description("WebP file format type (derived from Lossless setting)")]
    [DefaultValue(WebPFileFormatType.Lossy)]
    public WebPFileFormatType FileFormat
    {
        get => _lossless ? WebPFileFormatType.Lossless : WebPFileFormatType.Lossy;
        set
        {
            bool newLossless = value == WebPFileFormatType.Lossless;
            if (_lossless != newLossless)
            {
                _lossless = newLossless;
                OnPropertyChanged(nameof(FileFormat));
                OnPropertyChanged(nameof(Lossless));
            }
        }
    }

    /// <summary>
    /// Encoding method (quality/speed tradeoff). Higher values are slower but produce better compression.
    /// </summary>
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
                OnPropertyChanged(nameof(Method));
            }
        }
    }

    /// <summary>
    /// Near lossless quality (0-100). 100 is lossless, lower values introduce more loss for smaller file size.
    /// Only applies when Lossless is true.
    /// </summary>
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
                OnPropertyChanged(nameof(NearLossless));
            }
        }
    }

    /// <summary>
    /// Use alpha compression for images with transparency.
    /// </summary>
    [Category("WebP")]
    [Description("Use alpha compression for images with transparency")]
    [DefaultValue(true)]
    public bool UseAlphaCompression
    {
        get => _useAlphaCompression;
        set
        {
            if (_useAlphaCompression != value)
            {
                _useAlphaCompression = value;
                OnPropertyChanged(nameof(UseAlphaCompression));
            }
        }
    }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public override string ToString()
    {
        if (Lossless)
            return $"Lossless (NearLossless: {NearLossless})";
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
