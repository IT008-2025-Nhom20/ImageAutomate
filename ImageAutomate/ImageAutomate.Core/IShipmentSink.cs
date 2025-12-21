namespace ImageAutomate.Core;

/// <summary>
/// Marker interface for blocks that consume work items and act as pipeline endpoints.
/// </summary>
/// <remarks>
/// A block implementing this interface must also implement IBlock.
/// </remarks>
public interface IShipmentSink
{
}
