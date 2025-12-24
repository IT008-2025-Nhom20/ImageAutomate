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
/// Represents size dimensions.
/// </summary>
public record Size(int Width, int Height);

/// <summary>
/// Stores view state information for the workspace UI.
/// Holds block layout data (positions, sizes) decoupled from the IBlock interface.
/// </summary>
/// <remarks>
/// This class uses object identity (reference equality) for block keys.
/// Block layout is managed independently from block logic, allowing IBlock
/// to remain free of UI concerns.
/// </remarks>
public class ViewState
{
    /// <summary>
    /// Default block size used when no size is explicitly set.
    /// </summary>
    public static readonly Size DefaultBlockSize = new(200, 100);

    private readonly Dictionary<IBlock, Position> _blockPositions = [];
    private readonly Dictionary<IBlock, Size> _blockSizes = [];
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
        set => _panX = double.IsFinite(value) ? value : 0.0;
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
    /// Gets the size of a block.
    /// </summary>
    public Size? GetBlockSize(IBlock block)
    {
        return _blockSizes.TryGetValue(block, out var size) ? size : null;
    }

    /// <summary>
    /// Sets the size of a block.
    /// </summary>
    public void SetBlockSize(IBlock block, Size size)
    {
        _blockSizes[block] = size;
    }

    /// <summary>
    /// Removes the size of a block.
    /// </summary>
    public void RemoveBlockSize(IBlock block)
    {
        _blockSizes.Remove(block);
    }

    /// <summary>
    /// Removes all layout data (position and size) for a specific block.
    /// Call this when a block is removed from the graph.
    /// </summary>
    /// <param name="block">The block to remove layout data for.</param>
    public void RemoveBlock(IBlock block)
    {
        _blockPositions.Remove(block);
        _blockSizes.Remove(block);
    }

    /// <summary>
    /// Clears all block positions.
    /// </summary>
    public void Clear()
    {
        _blockPositions.Clear();
        _blockSizes.Clear();
        _zoom = 1.0;
        _panX = 0.0;
        _panY = 0.0;
    }

    /// <summary>
    /// Removes all layout data for blocks not in the provided collection.
    /// Call this to clean up stale references after blocks are removed from the graph.
    /// </summary>
    /// <param name="validBlocks">The collection of blocks that should be retained.</param>
    public void PruneStaleBlocks(IEnumerable<IBlock> validBlocks)
    {
        var validSet = new HashSet<IBlock>(validBlocks);
        
        var stalePositionKeys = _blockPositions.Keys.Where(k => !validSet.Contains(k)).ToList();
        foreach (var key in stalePositionKeys)
            _blockPositions.Remove(key);

        var staleSizeKeys = _blockSizes.Keys.Where(k => !validSet.Contains(k)).ToList();
        foreach (var key in staleSizeKeys)
            _blockSizes.Remove(key);
    }

    /// <summary>
    /// Gets or sets the block position, with a fallback if not set.
    /// Returns the position or null if not found.
    /// </summary>
    public Position GetBlockPositionOrDefault(IBlock block, Position? defaultPosition = null)
    {
        if (_blockPositions.TryGetValue(block, out var pos))
            return pos;
        
        return defaultPosition ?? new Position(0, 0);
    }

    /// <summary>
    /// Gets the block size, with a fallback to DefaultBlockSize if not set.
    /// </summary>
    public Size GetBlockSizeOrDefault(IBlock block, Size? defaultSize = null)
    {
        if (_blockSizes.TryGetValue(block, out var size))
            return size;
        
        return defaultSize ?? DefaultBlockSize;
    }

    /// <summary>
    /// Converts to DTO for serialization (global view state only).
    /// Block-specific layout is serialized via BlockSerializer.
    /// </summary>
    internal ViewStateDto ToDto()
    {
        return new ViewStateDto
        {
            Zoom = Zoom,
            PanX = PanX,
            PanY = PanY
        };
    }

    /// <summary>
    /// Creates from DTO (global view state only).
    /// Block-specific layout is restored via Workspace.FromJson.
    /// </summary>
    internal static ViewState FromDto(ViewStateDto dto)
    {
        return new ViewState
        {
            Zoom = dto.Zoom,
            PanX = dto.PanX,
            PanY = dto.PanY
        };
    }
}
