/**
 * ViewState.cs
 *
 * Stores visual/UI state information for a workspace.
 */

using ImageAutomate.Core.Serialization;

namespace ImageAutomate.Core;

/// <summary>
/// Represents a 2D position.
/// </summary>
public record Position(double X, double Y);

/// <summary>
/// Stores view state information for the workspace UI.
/// </summary>
public class ViewState
{
    private readonly Dictionary<IBlock, Position> _blockPositions = new();
    private double _zoom = 1.0;
    private double _panX = 0.0;
    private double _panY = 0.0;

    /// <summary>
    /// Gets or sets the zoom level.
    /// </summary>
    public double Zoom
    {
        get => _zoom;
        set => _zoom = Math.Clamp(value, 0.1, 10.0);
    }

    /// <summary>
    /// Gets or sets the horizontal pan offset.
    /// </summary>
    public double PanX
    {
        get => _panX;
        set => _panX = value;
    }

    /// <summary>
    /// Gets or sets the vertical pan offset.
    /// </summary>
    public double PanY
    {
        get => _panY;
        set => _panY = value;
    }

    /// <summary>
    /// Gets or sets the position of a block.
    /// </summary>
    public Position? GetBlockPosition(IBlock block)
    {
        return _blockPositions.TryGetValue(block, out var pos) ? pos : null;
    }

    /// <summary>
    /// Sets the position of a block.
    /// </summary>
    public void SetBlockPosition(IBlock block, Position position)
    {
        _blockPositions[block] = position;
    }

    /// <summary>
    /// Removes the position of a block.
    /// </summary>
    public void RemoveBlockPosition(IBlock block)
    {
        _blockPositions.Remove(block);
    }

    /// <summary>
    /// Clears all block positions.
    /// </summary>
    public void Clear()
    {
        _blockPositions.Clear();
        _zoom = 1.0;
        _panX = 0.0;
        _panY = 0.0;
    }

    /// <summary>
    /// Converts to DTO for serialization.
    /// </summary>
    internal ViewStateDto ToDto(IReadOnlyList<IBlock> blocks)
    {
        var dto = new ViewStateDto
        {
            Zoom = Zoom,
            PanX = PanX,
            PanY = PanY
        };

        foreach (var kvp in _blockPositions)
        {
            // Find block index manually
            var blockIndex = -1;
            for (int i = 0; i < blocks.Count; i++)
            {
                if (blocks[i] == kvp.Key)
                {
                    blockIndex = i;
                    break;
                }
            }

            if (blockIndex >= 0)
            {
                dto.BlockPositions[blockIndex] = new PositionDto
                {
                    X = kvp.Value.X,
                    Y = kvp.Value.Y
                };
            }
        }

        return dto;
    }

    /// <summary>
    /// Creates from DTO.
    /// </summary>
    internal static ViewState FromDto(ViewStateDto dto, IReadOnlyList<IBlock> blocks)
    {
        var viewState = new ViewState
        {
            Zoom = dto.Zoom,
            PanX = dto.PanX,
            PanY = dto.PanY
        };

        foreach (var kvp in dto.BlockPositions)
        {
            if (kvp.Key >= 0 && kvp.Key < blocks.Count)
            {
                var block = blocks[kvp.Key];
                viewState.SetBlockPosition(block, new Position(kvp.Value.X, kvp.Value.Y));
            }
        }

        return viewState;
    }
}
