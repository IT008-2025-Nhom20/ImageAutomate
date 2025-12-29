/**
 * Workspace.cs
 *
 * Serializable container for a complete workspace including graph and view state.
 */

using System.Diagnostics;
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
    public string Name { get; set; } = "Untitled Workspace";

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
    public double PanX { get; set; } = 0.0;

    /// <summary>
    /// Gets or sets the vertical pan offset.
    /// </summary>
    public double PanY { get; set; } = 0.0;

    /// <summary>
    /// Gets or sets custom metadata for the workspace.
    /// </summary>
    public Dictionary<string, object?> Metadata { get; set; } = [];

    public Workspace(PipelineGraph graph)
    {
        Graph = graph ?? throw new ArgumentNullException(nameof(graph));
    }

    public Workspace(PipelineGraph graph, string name)
    {
        Graph = graph ?? throw new ArgumentNullException(nameof(graph));
        Name = name ?? throw new ArgumentNullException(nameof(name));
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
    public string ToJson(string? schemaUrl)
    {
        var dto = new WorkspaceDto
        {
            Version = "1.0",
            Name = Name,
            Metadata = Metadata,
            Zoom = Zoom,
            PanX = PanX,
            PanY = PanY
        };

        if (!string.IsNullOrEmpty(schemaUrl))
        {
            dto.Schema = schemaUrl;
        }

        if (Graph != null)
        {
            dto.Graph = Graph.ToDto();
        }

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

        var workspace = new Workspace(graph, dto.Name ?? "Untitled Workspace")
        {
            Metadata = dto.Metadata ?? [],
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
        //Debug.WriteLine($"Saving workspace '{Name}' to file: {filePath}");
        var json = ToJson();
        File.WriteAllText(filePath, json);
    }

    /// <summary>
    /// Loads a workspace from a file.
    /// </summary>
    public static Workspace LoadFromFile(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Workspace file not found: {filePath}", filePath);

        var json = File.ReadAllText(filePath);
        return FromJson(json);
    }
}