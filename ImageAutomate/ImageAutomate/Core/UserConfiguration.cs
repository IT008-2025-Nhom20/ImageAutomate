using System.Text.Json;
using System.Text.Json.Serialization;

namespace ImageAutomate.Core
{
    /// <summary>
    /// Centralized storage for user-configurable options.
    /// All settings are persisted to user preferences and loaded on application startup.
    /// </summary>
    public static class UserConfiguration
    {
        private static readonly string ConfigPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ImageAutomate",
            "settings.json");

        private static JsonSerializerOptions jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };

        #region Application Settings

        /// <summary>
        /// Gets or sets the maximum number of recent workspaces to display.
        /// Default: 10.
        /// </summary>
        public static int MaxRecentWorkspaces { get; set; } = 10;

        #endregion

        #region Execution Settings

        /// <summary>
        /// Gets or sets the execution mode or custom scheduler name.
        /// Default: "simpledfs".
        /// </summary>
        public static string Mode { get; set; } = "simpledfs";

        /// <summary>
        /// Gets or sets the maximum degree of parallelism (concurrent block executions).
        /// Default: Number of logical processors.
        /// </summary>
        public static int MaxDegreeOfParallelism { get; set; } = Environment.ProcessorCount;

        /// <summary>
        /// Gets or sets the watchdog timeout for deadlock detection.
        /// If no progress occurs within this duration, a PipelineDeadlockException is thrown.
        /// Default: 30 seconds.
        /// </summary>
        public static int WatchdogTimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// Gets or sets whether GC throttling is enabled.
        /// When enabled, the engine pauses dispatches if GC frequency exceeds 10/second.
        /// Default: true.
        /// </summary>
        public static bool EnableGcThrottling { get; set; } = true;

        /// <summary>
        /// Gets or sets the maximum number of work items to process per shipment.
        /// Used for batching work from shipment sources (e.g., LoadBlock).
        /// Higher values increase memory pressure but reduce overhead.
        /// Default: 64 work items.
        /// </summary>
        public static int MaxShipmentSize { get; set; } = 64;

        /// <summary>
        /// Gets or sets the profiling window size for cost estimation (Adaptive Mode only).
        /// Default: 20 samples.
        /// </summary>
        public static int ProfilingWindowSize { get; set; } = 20;

        /// <summary>
        /// Gets or sets the exponential moving average alpha for cost profiling (Adaptive Mode only).
        /// Default: 0.2 (emphasizes recent behavior).
        /// </summary>
        public static double CostEmaAlpha { get; set; } = 0.2;

        /// <summary>
        /// Gets or sets the critical path recomputation interval in blocks (Adaptive Mode only).
        /// Default: Every 10 blocks.
        /// </summary>
        public static int CriticalPathRecomputeInterval { get; set; } = 10;

        /// <summary>
        /// Gets or sets the batch size for grouped scheduling (AdaptiveBatched mode only).
        /// Default: 5 blocks.
        /// </summary>
        public static int BatchSize { get; set; } = 5;

        /// <summary>
        /// Gets or sets the critical path boost multiplier (Adaptive Mode only).
        /// Blocks on the critical path receive priority × this multiplier.
        /// Default: 1.5.
        /// </summary>
        public static double CriticalPathBoost { get; set; } = 1.5;

        #endregion

        #region Editor Settings

        /// <summary>
        /// Gets or sets the outline color for selected blocks.
        /// Default: Red.
        /// </summary>
        public static Color SelectedBlockOutlineColor { get; set; } = Color.Red;

        /// <summary>
        /// Gets or sets the radius of connection sockets.
        /// Default: 6.
        /// </summary>
        public static int SocketRadius { get; set; } = 6;

        /// <summary>
        /// Gets or sets the render scale factor for the graph editor.
        /// Default: 1.0.
        /// </summary>
        public static float RenderScale { get; set; } = 1.0f;

        /// <summary>
        /// Gets or sets whether the graph can be panned completely off-screen.
        /// Default: false.
        /// </summary>
        public static bool AllowOutOfScreenPan { get; set; } = false;

        /// <summary>
        /// Gets or sets the auto-snap zone width for block connections.
        /// Default: 20.
        /// </summary>
        public static int AutoSnapZoneWidth { get; set; } = 20;

        #endregion

        #region Theme Settings

        /// <summary>
        /// Gets or sets the default node color.
        /// Default: RGB(60, 60, 60).
        /// </summary>
        public static Color DefaultNodeColor { get; set; } = Color.FromArgb(60, 60, 60);

        /// <summary>
        /// Gets or sets the hovered node color.
        /// Default: RGB(80, 80, 80).
        /// </summary>
        public static Color HoveredNodeColor { get; set; } = Color.FromArgb(80, 80, 80);

        /// <summary>
        /// Gets or sets the text color for nodes.
        /// Default: White.
        /// </summary>
        public static Color TextColor { get; set; } = Color.White;

        /// <summary>
        /// Gets or sets the color for disabled nodes.
        /// Default: RGB(100, 100, 100).
        /// </summary>
        public static Color DisabledNodeColor { get; set; } = Color.FromArgb(100, 100, 100);

        /// <summary>
        /// Gets or sets the success color for UI elements.
        /// Default: RGB(100, 200, 100).
        /// </summary>
        public static Color SuccessColor { get; set; } = Color.FromArgb(100, 200, 100);

        /// <summary>
        /// Gets or sets the error color for UI elements.
        /// Default: RGB(200, 100, 100).
        /// </summary>
        public static Color ErrorColor { get; set; } = Color.FromArgb(200, 100, 100);

        /// <summary>
        /// Gets or sets the selected block outline color (duplicate property for Theme category).
        /// Default: Red.
        /// </summary>
        public static ThemeColorInfo SelectedBlockOutline { get; set; } = new ThemeColorInfo(Color.Red, "Selected Block Outline");

        /// <summary>
        /// Gets or sets the hovered block outline color.
        /// Default: Orange.
        /// </summary>
        public static ThemeColorInfo HoveredBlockOutline { get; set; } = new ThemeColorInfo(Color.Orange, "Hovered Block Outline");

        /// <summary>
        /// Gets or sets the border outline color.
        /// Default: RGB(150, 150, 150).
        /// </summary>
        public static ThemeColorInfo BorderOutline { get; set; } = new ThemeColorInfo(Color.FromArgb(150, 150, 150), "Border Outline");

        /// <summary>
        /// Gets or sets the socket connection color.
        /// Default: RGB(150, 150, 150).
        /// </summary>
        public static ThemeColorInfo SocketConnectionColor { get; set; } = new ThemeColorInfo(Color.FromArgb(100, 100, 100), "Socket Connection");

        /// <summary>
        /// Gets or sets the node width size.
        /// Default: 35.
        /// </summary>
        public static int NodeWidth { get; set; } = 35;

        /// <summary>
        /// Gets or sets the node border width.
        /// Default: 8.
        /// </summary>
        public static int NodeBorderWidth { get; set; } = 8;

        /// <summary>
        /// Gets or sets the spacing between nodes.
        /// Default: 25.
        /// </summary>
        public static int NodeSpacing { get; set; } = 25;

        #endregion

        /// <summary>
        /// Resets all configuration values to their defaults.
        /// </summary>
        public static void ResetToDefaults()
        {
            // Application Settings
            MaxRecentWorkspaces = 10;

            // Execution Settings
            Mode = "simpledfs";
            MaxDegreeOfParallelism = Environment.ProcessorCount;
            WatchdogTimeoutSeconds = 30;
            EnableGcThrottling = true;
            MaxShipmentSize = 64;
            ProfilingWindowSize = 20;
            CostEmaAlpha = 0.2;
            CriticalPathRecomputeInterval = 10;
            BatchSize = 5;
            CriticalPathBoost = 1.5;

            // Editor Settings
            SelectedBlockOutlineColor = Color.Red;
            SocketRadius = 6;
            RenderScale = 1.0f;
            AllowOutOfScreenPan = false;
            AutoSnapZoneWidth = 20;

            // Theme Settings
            DefaultNodeColor = Color.FromArgb(60, 60, 60);
            HoveredNodeColor = Color.FromArgb(80, 80, 80);
            TextColor = Color.White;
            DisabledNodeColor = Color.FromArgb(100, 100, 100);
            SuccessColor = Color.FromArgb(100, 200, 100);
            ErrorColor = Color.FromArgb(200, 100, 100);
            SelectedBlockOutline = new ThemeColorInfo(Color.Red, "Selected Block Outline");
            HoveredBlockOutline = new ThemeColorInfo(Color.Orange, "Hovered Block Outline");
            BorderOutline = new ThemeColorInfo(Color.FromArgb(150, 150, 150), "Border Outline");
            SocketConnectionColor = new ThemeColorInfo(Color.FromArgb(100, 100, 100), "Socket Connection");
            NodeWidth = 35;
            NodeBorderWidth = 8;
            NodeSpacing = 25;
        }

        /// <summary>
        /// Saves all configuration values to persistent storage.
        /// </summary>
        public static void Save()
        {
            try
            {
                string? directory = Path.GetDirectoryName(ConfigPath);

                if (directory == null)
                {
                    MessageBox.Show("Failed to determine configuration directory.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var data = new ConfigurationData
                {
                    MaxRecentWorkspaces = MaxRecentWorkspaces,
                    Mode = Mode,
                    MaxDegreeOfParallelism = MaxDegreeOfParallelism,
                    WatchdogTimeoutSeconds = WatchdogTimeoutSeconds,
                    EnableGcThrottling = EnableGcThrottling,
                    MaxShipmentSize = MaxShipmentSize,
                    ProfilingWindowSize = ProfilingWindowSize,
                    CostEmaAlpha = CostEmaAlpha,
                    CriticalPathRecomputeInterval = CriticalPathRecomputeInterval,
                    BatchSize = BatchSize,
                    CriticalPathBoost = CriticalPathBoost,
                    SelectedBlockOutlineColor = ColorToHtml(SelectedBlockOutlineColor),
                    SocketRadius = SocketRadius,
                    RenderScale = RenderScale,
                    AllowOutOfScreenPan = AllowOutOfScreenPan,
                    AutoSnapZoneWidth = AutoSnapZoneWidth,
                    DefaultNodeColor = ColorToHtml(DefaultNodeColor),
                    HoveredNodeColor = ColorToHtml(HoveredNodeColor),
                    TextColor = ColorToHtml(TextColor),
                    DisabledNodeColor = ColorToHtml(DisabledNodeColor),
                    SuccessColor = ColorToHtml(SuccessColor),
                    ErrorColor = ColorToHtml(ErrorColor),
                    SelectedBlockOutlineThemeColor = ColorToHtml(SelectedBlockOutline.Color),
                    HoveredBlockOutlineThemeColor = ColorToHtml(HoveredBlockOutline.Color),
                    BorderOutlineThemeColor = ColorToHtml(BorderOutline.Color),
                    SocketConnectionThemeColor = ColorToHtml(SocketConnectionColor.Color),
                    NodeWidth = NodeWidth,
                    NodeBorderWidth = NodeBorderWidth,
                    NodeSpacing = NodeSpacing
                };

                var json = JsonSerializer.Serialize(data, jsonOptions);
                File.WriteAllText(ConfigPath, json);
            }
            catch (Exception ex)
            {
                // In a real app we might log this, but for now we suppress it to avoid crashing
                System.Diagnostics.Debug.WriteLine($"Failed to save settings: {ex.Message}");
            }
        }

        /// <summary>
        /// Loads all configuration values from persistent storage.
        /// </summary>
        public static void Load()
        {
            try
            {
                if (!File.Exists(ConfigPath)) return;

                var json = File.ReadAllText(ConfigPath);
                var data = JsonSerializer.Deserialize<ConfigurationData>(json, jsonOptions);

                if (data != null)
                {
                    MaxRecentWorkspaces = data.MaxRecentWorkspaces;
                    Mode = data.Mode;
                    MaxDegreeOfParallelism = data.MaxDegreeOfParallelism;
                    WatchdogTimeoutSeconds = data.WatchdogTimeoutSeconds;
                    EnableGcThrottling = data.EnableGcThrottling;
                    MaxShipmentSize = data.MaxShipmentSize;
                    ProfilingWindowSize = data.ProfilingWindowSize;
                    CostEmaAlpha = data.CostEmaAlpha;
                    CriticalPathRecomputeInterval = data.CriticalPathRecomputeInterval;
                    BatchSize = data.BatchSize;
                    CriticalPathBoost = data.CriticalPathBoost;

                    SelectedBlockOutlineColor = HtmlToColor(data.SelectedBlockOutlineColor);
                    SocketRadius = data.SocketRadius;
                    RenderScale = data.RenderScale;
                    AllowOutOfScreenPan = data.AllowOutOfScreenPan;
                    AutoSnapZoneWidth = data.AutoSnapZoneWidth;

                    DefaultNodeColor = HtmlToColor(data.DefaultNodeColor);
                    HoveredNodeColor = HtmlToColor(data.HoveredNodeColor);
                    TextColor = HtmlToColor(data.TextColor);
                    DisabledNodeColor = HtmlToColor(data.DisabledNodeColor);
                    SuccessColor = HtmlToColor(data.SuccessColor);
                    ErrorColor = HtmlToColor(data.ErrorColor);

                    SelectedBlockOutline.Color = HtmlToColor(data.SelectedBlockOutlineThemeColor);
                    HoveredBlockOutline.Color = HtmlToColor(data.HoveredBlockOutlineThemeColor);
                    BorderOutline.Color = HtmlToColor(data.BorderOutlineThemeColor);
                    SocketConnectionColor.Color = HtmlToColor(data.SocketConnectionThemeColor);

                    NodeWidth = data.NodeWidth;
                    NodeBorderWidth = data.NodeBorderWidth;
                    NodeSpacing = data.NodeSpacing;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load settings: {ex.Message}");
            }
        }

        private static string ColorToHtml(Color color)
        {
            return ColorTranslator.ToHtml(color);
        }

        private static Color HtmlToColor(string? html)
        {
            if (string.IsNullOrEmpty(html)) return Color.Black;
            try
            {
                return ColorTranslator.FromHtml(html);
            }
            catch
            {
                return Color.Black;
            }
        }

        private class ConfigurationData
        {
            public int MaxRecentWorkspaces { get; set; } = 10;
            public string Mode { get; set; } = "SimpleDfs";
            public int MaxDegreeOfParallelism { get; set; }
            public int WatchdogTimeoutSeconds { get; set; }
            public bool EnableGcThrottling { get; set; }
            public int MaxShipmentSize { get; set; }
            public int ProfilingWindowSize { get; set; }
            public double CostEmaAlpha { get; set; }
            public int CriticalPathRecomputeInterval { get; set; }
            public int BatchSize { get; set; }
            public double CriticalPathBoost { get; set; }
            public string? SelectedBlockOutlineColor { get; set; }
            public int SocketRadius { get; set; }
            public float RenderScale { get; set; }
            public bool AllowOutOfScreenPan { get; set; }
            public int AutoSnapZoneWidth { get; set; }
            public string? DefaultNodeColor { get; set; }
            public string? HoveredNodeColor { get; set; }
            public string? TextColor { get; set; }
            public string? DisabledNodeColor { get; set; }
            public string? SuccessColor { get; set; }
            public string? ErrorColor { get; set; }
            public string? SelectedBlockOutlineThemeColor { get; set; }
            public string? HoveredBlockOutlineThemeColor { get; set; }
            public string? BorderOutlineThemeColor { get; set; }
            public string? SocketConnectionThemeColor { get; set; }
            public int NodeWidth { get; set; }
            public int NodeBorderWidth { get; set; }
            public int NodeSpacing { get; set; }
        }
    }

    /// <summary>
    /// Represents a color with metadata for theme configuration.
    /// </summary>
    public class ThemeColorInfo
    {
        /// <summary>
        /// Gets or sets the color value.
        /// </summary>
        public Color Color { get; set; }

        /// <summary>
        /// Gets or sets the display name/description of the color.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Initializes a new instance of ThemeColorInfo.
        /// </summary>
        public ThemeColorInfo(Color color, string displayName)
        {
            Color = color;
            DisplayName = displayName;
        }
    }
}