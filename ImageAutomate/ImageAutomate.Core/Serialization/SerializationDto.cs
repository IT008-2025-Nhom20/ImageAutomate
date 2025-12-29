/**
 * SerializationDto.cs
 *
 * Data Transfer Objects for serializing PipelineGraph and related types.
 */

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace ImageAutomate.Core.Serialization;

/// <summary>
/// DTO for serializing Socket data.
/// </summary>
public class SocketDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    public SocketDto() { }

    public SocketDto(Socket socket)
    {
        ArgumentNullException.ThrowIfNull(socket);
        Id = socket.Id;
        Name = socket.Name;
    }

    public Socket ToSocket()
    {
        return new Socket(Id, Name);
    }
}

/// <summary>
/// DTO for serializing IBlock data.
/// Layout properties (X, Y, Width, Height) are serialized as regular properties.
/// </summary>
[SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "DTO requires setters for JSON deserialization")]
public class BlockDto
{
    [JsonPropertyName("blockType")]
    public string BlockType { get; set; } = string.Empty;

    [JsonPropertyName("assemblyQualifiedName")]
    public string AssemblyQualifiedName { get; set; } = string.Empty;

    [JsonPropertyName("properties")]
    public Dictionary<string, object?> Properties { get; set; } = [];

    [JsonPropertyName("inputs")]
    public IReadOnlyList<SocketDto> Inputs { get; set; } = [];

    [JsonPropertyName("outputs")]
    public IReadOnlyList<SocketDto> Outputs { get; set; } = [];

    public BlockDto()
    {
    }

    public BlockDto(string blockType, string assemblyQualifiedName, Dictionary<string, object?> properties, IReadOnlyList<SocketDto> inputs, IReadOnlyList<SocketDto> outputs)
    {
        BlockType = blockType;
        AssemblyQualifiedName = assemblyQualifiedName;
        Properties = properties;
        Inputs = inputs;
        Outputs = outputs;
    }
}

/// <summary>
/// DTO for serializing Connection data.
/// </summary>
public class ConnectionDto
{
    [JsonPropertyName("sourceBlockIndex")]
    public int SourceBlockIndex { get; set; }

    [JsonPropertyName("sourceSocketId")]
    public string SourceSocketId { get; set; } = string.Empty;

    [JsonPropertyName("targetBlockIndex")]
    public int TargetBlockIndex { get; set; }

    [JsonPropertyName("targetSocketId")]
    public string TargetSocketId { get; set; } = string.Empty;
}

/// <summary>
/// DTO for serializing PipelineGraph data.
/// </summary>
public class PipelineGraphDto
{
    [JsonPropertyName("blocks")]
    public IReadOnlyList<BlockDto> Blocks { get; set; }

    [JsonPropertyName("connections")]
    public IReadOnlyList<ConnectionDto> Connections { get; set; }

    [JsonPropertyName("centerBlockIndex")]
    public int? CenterBlockIndex { get; set; }

    public PipelineGraphDto()
        : this([], [], null)
    {
    }

    public PipelineGraphDto(IReadOnlyList<BlockDto> blocks, IReadOnlyList<ConnectionDto> connections, int? centerBlockIndex)
    {
        Blocks = blocks;
        Connections = connections;
        CenterBlockIndex = centerBlockIndex;
    }
}

/// <summary>
/// DTO for serializing a complete Workspace.
/// </summary>
[SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "DTO requires setters for JSON deserialization")]
public class WorkspaceDto
{
    [JsonPropertyName("$schema")]
    public Uri? Schema { get; set; }

    [JsonPropertyName("version")]
    public string Version { get; set; } = "1.0";

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("graph")]
    public PipelineGraphDto? Graph { get; set; }

    [JsonPropertyName("zoom")]
    public double Zoom { get; set; } = 1.0;

    [JsonPropertyName("panX")]
    public double PanX { get; set; } = 0.0;

    [JsonPropertyName("panY")]
    public double PanY { get; set; } = 0.0;

    [JsonPropertyName("metadata")]
    public Dictionary<string, object?> Metadata { get; set; } = [];

    public WorkspaceDto()
    {
    }

    public WorkspaceDto(Uri? schema, string version, string name, PipelineGraphDto? graph, double zoom, double panX, double panY, Dictionary<string, object?> metadata)
    {
        Schema = schema;
        Version = version;
        Name = name;
        Graph = graph;
        Zoom = zoom;
        PanX = panX;
        PanY = panY;
        Metadata = metadata;
    }

    public WorkspaceDto(string version, string name, PipelineGraphDto? graph, double zoom, double panX, double panY, Dictionary<string, object?> metadata)
        : this(null, version, name, graph, zoom, panX, panY, metadata)
    {
    }
}
