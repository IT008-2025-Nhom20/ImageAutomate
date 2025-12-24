/**
 * SerializationDto.cs
 *
 * Data Transfer Objects for serializing PipelineGraph and related types.
 */

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
        Id = socket.Id;
        Name = socket.Name;
    }

    public Socket ToSocket()
    {
        return new Socket(Id, Name);
    }
}

/// <summary>
/// DTO for block layout information (embedded in each block for human readability).
/// </summary>
public class BlockLayoutDto
{
    [JsonPropertyName("x")]
    public double X { get; set; }

    [JsonPropertyName("y")]
    public double Y { get; set; }

    [JsonPropertyName("width")]
    public int Width { get; set; }

    [JsonPropertyName("height")]
    public int Height { get; set; }
}

/// <summary>
/// DTO for serializing IBlock data.
/// </summary>
public class BlockDto
{
    [JsonPropertyName("blockType")]
    public string BlockType { get; set; } = string.Empty;

    [JsonPropertyName("assemblyQualifiedName")]
    public string AssemblyQualifiedName { get; set; } = string.Empty;

    /// <summary>
    /// Layout information (position and size) embedded in the block for human-readable JSON.
    /// </summary>
    [JsonPropertyName("layout")]
    public BlockLayoutDto? Layout { get; set; }

    [JsonPropertyName("properties")]
    public Dictionary<string, object?> Properties { get; set; } = new();

    [JsonPropertyName("inputs")]
    public List<SocketDto> Inputs { get; set; } = new();

    [JsonPropertyName("outputs")]
    public List<SocketDto> Outputs { get; set; } = new();
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
    public List<BlockDto> Blocks { get; set; } = new();

    [JsonPropertyName("connections")]
    public List<ConnectionDto> Connections { get; set; } = new();

    [JsonPropertyName("centerBlockIndex")]
    public int? CenterBlockIndex { get; set; }
}

/// <summary>
/// DTO for storing view state information (global view settings only).
/// Block-specific layout is now embedded in each BlockDto.
/// </summary>
public class ViewStateDto
{
    [JsonPropertyName("zoom")]
    public double Zoom { get; set; } = 1.0;

    [JsonPropertyName("panX")]
    public double PanX { get; set; } = 0.0;

    [JsonPropertyName("panY")]
    public double PanY { get; set; } = 0.0;
}

/// <summary>
/// DTO for serializing a complete Workspace.
/// </summary>
public class WorkspaceDto
{
    [JsonPropertyName("$schema")]
    public string? Schema { get; set; }

    [JsonPropertyName("version")]
    public string Version { get; set; } = "1.0";

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("graph")]
    public PipelineGraphDto? Graph { get; set; }

    [JsonPropertyName("viewState")]
    public ViewStateDto? ViewState { get; set; }

    [JsonPropertyName("metadata")]
    public Dictionary<string, object?> Metadata { get; set; } = new();
}
