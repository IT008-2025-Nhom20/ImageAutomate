/**
 * Workspace.cs
 *
 * Serializable container for a complete workspace including graph and view state.
 */

using System.Text.Json;

using ImageAutomate.Core.Serialization;

namespace ImageAutomate.Core;

/// <summary>
/// Represents a complete workspace that can be saved and loaded from disk.
/// </summary>
public class Workspace
{
    private static readonly JsonSerializerOptions _serializerOptions = new()
    {
        IncludeFields = true,
        WriteIndented = true,
    };

    private PipelineGraph _graph = null!;

    /// <summary>
    /// Gets or sets the workspace name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets the file path where this workspace was saved to or loaded from.
    /// Null if the workspace has never been saved.
    /// </summary>
    public string? FilePath { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this workspace has been saved to disk.
    /// </summary>
    public bool IsSaved => !string.IsNullOrEmpty(FilePath) && File.Exists(FilePath);

    /// <summary>
    /// Gets or sets the pipeline graph.
    /// </summary>
    public PipelineGraph Graph
    {
        get => _graph;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            _graph = value;
        }
    }

    /// <summary>
    /// Gets or sets the zoom level.
    /// </summary>
    public double Zoom { get; set; } = 1.0;

    /// <summary>
    /// Gets or sets the horizontal pan offset.
    /// </summary>
    public double PanX { get; set; }

    /// <summary>
    /// Gets or sets the vertical pan offset.
    /// </summary>
    public double PanY { get; set; }

    /// <summary>
    /// Gets or sets custom metadata for the workspace.
    /// </summary>
    public Dictionary<string, object?> Metadata { get; }

    public Workspace(PipelineGraph graph)
    {
        Name = "Untitled Workspace";
        Graph = graph ?? throw new ArgumentNullException(nameof(graph));
        Metadata = [];
    }

    public Workspace(PipelineGraph graph, string name)
    {
        Graph = graph ?? throw new ArgumentNullException(nameof(graph));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Metadata = [];
    }

    public Workspace(PipelineGraph graph, string name, Dictionary<string, object?> metadata)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Graph = graph ?? throw new ArgumentNullException(nameof(graph));
        Metadata = metadata;
    }

    /// <summary>
    /// Finds the top-most block at the given world coordinates.
    /// </summary>
    public IBlock? HitTestNode(double x, double y)
    {
        if (Graph == null)
            return null;

        // Iterate in reverse order (Top to Bottom)
        for (int i = Graph.Nodes.Count - 1; i >= 0; i--)
        {
            var block = Graph.Nodes[i];

            if (x >= block.X && x <= block.X + block.Width &&
                y >= block.Y && y <= block.Y + block.Height)
            {
                return block;
            }
        }
        return null;
    }

    /// <summary>
    /// Serializes the workspace to JSON string.
    /// </summary>
    public string ToJson()
    {
        return ToJson(SystemConfiguration.WorkspaceSchemaUrl);
    }

    /// <summary>
    /// Serializes the workspace to JSON string.
    /// </summary>
    /// <param name="schemaUrl">
    /// The schema URL to include in the $schema field. 
    /// Pass null to omit the schema reference.
    /// </param>
    public string ToJson(Uri? schemaUrl)
    {
        var dto = new WorkspaceDto(
            schema: schemaUrl,
            version: "1.0",
            name: Name,
            graph: Graph.ToDto(),
            zoom: Zoom,
            panX: PanX,
            panY: PanY,
            metadata: Metadata
        );

        return JsonSerializer.Serialize(dto, _serializerOptions);
    }

    /// <summary>
    /// Deserializes a workspace from JSON string.
    /// </summary>
    public static Workspace FromJson(string json)
    {
        var dto = JsonSerializer.Deserialize<WorkspaceDto>(json, _serializerOptions)
            ?? throw new InvalidOperationException("Failed to deserialize workspace from JSON.");

        var graph = dto.Graph != null
            ? PipelineGraph.FromDto(dto.Graph)
            : new PipelineGraph();

        var workspace = new Workspace(
            graph, dto.Name ?? "Untitled Workspace", dto.Metadata)
        {
            Zoom = dto.Zoom,
            PanX = dto.PanX,
            PanY = dto.PanY
        };

        return workspace;
    }

    /// <summary>
    /// Saves the workspace to a file.
    /// </summary>
    /// <param name="filePath">The file path to save to.</param>
    public void SaveToFile(string filePath)
    {
        var json = ToJson();
        File.WriteAllText(filePath, json);
        FilePath = filePath;
    }

    /// <summary>
    /// Loads a workspace from a file.
    /// </summary>
    public static Workspace LoadFromFile(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Workspace file not found: {filePath}", filePath);

        var json = File.ReadAllText(filePath);
        var workspace = FromJson(json);
        workspace.FilePath = filePath;
        return workspace;
    }
}