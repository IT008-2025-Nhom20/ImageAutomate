/**
 * PipelineGraph.cs
 *
 * Graph datastructure for managing Block nodes and their connections.
 */

using Microsoft.Msagl.Core.Geometry.Curves;
using System.Collections.Immutable;
using GeomEdge = Microsoft.Msagl.Core.Layout.Edge;
using GeomGraph = Microsoft.Msagl.Core.Layout.GeometryGraph;
using GeomNode = Microsoft.Msagl.Core.Layout.Node;
using MsaglPoint = Microsoft.Msagl.Core.Geometry.Point;

namespace ConvertBlockPoC;

public class PipelineGraph()
{
    private readonly GeomGraph _geomGraph = new();
    private readonly Dictionary<IBlock, GeomNode> _blockToNode = [];

    public event Action<IBlock>? OnNodeRemoved;

    public GeomNode? CenterNode { get; set; }
    public GeomGraph GeomGraph => _geomGraph;

    public IReadOnlyCollection<GeomNode> Nodes => [.. _geomGraph.Nodes];
    public IReadOnlyCollection<GeomEdge> Edges => [.. _geomGraph.Edges];

    public int NodeCount => _geomGraph.Nodes.Count;
    public int EdgeCount => _geomGraph.Edges.Count;

    public GeomNode AddNode(IBlock block)
    {
        if (_blockToNode.TryGetValue(block, out var existingNode))
            return existingNode;

        var node = CreateNode(block);
        _geomGraph.Nodes.Add(node);
        _blockToNode[block] = node;
        return node;
    }

    public void AddEdge(GeomNode from, GeomNode to)
    {
        var edge = new GeomEdge(from, to);
        _geomGraph.Edges.Add(edge);
    }

    public void RemoveNode(GeomNode node)
    {
        var edgesToRemove = _geomGraph.Edges
            .Where(e => e.Source == node || e.Target == node)
            .ToList();

        foreach (var edge in edgesToRemove)
            _geomGraph.Edges.Remove(edge);

        _geomGraph.Nodes.Remove(node);

        if (node.UserData is IBlock block)
        {
            _blockToNode.Remove(block);
            OnNodeRemoved?.Invoke(block);
        }
    }

    public GeomNode? GetNode(IBlock block) =>
        _blockToNode.GetValueOrDefault(block);

    public GeomNode? GetNodeAt(int index)
    {
        if (index < 0 || index >= _geomGraph.Nodes.Count)
            return null;
        return _geomGraph.Nodes.ElementAt(index);
    }

    public GeomNode? GetRandomNode()
    {
        if (_geomGraph.Nodes.Count == 0)
            return null;

        int index = Random.Shared.Next(_geomGraph.Nodes.Count);
        return _geomGraph.Nodes.ElementAt(index);
    }

    public IEnumerable<GeomNode> EnumerateNodes() => _geomGraph.Nodes;

    public IEnumerable<(GeomNode node, IBlock block)> EnumerateNodesWithBlocks()
    {
        foreach (var node in _geomGraph.Nodes)
        {
            if (node.UserData is IBlock block)
                yield return (node, block);
        }
    }

    public void Clear()
    {
        _geomGraph.Edges.Clear();
        _geomGraph.Nodes.Clear();
        _blockToNode.Clear();
        CenterNode = null;
    }

    private GeomNode CreateNode(IBlock block)
    {
        // Create a rectangle with the block's specific dimensions
        return new GeomNode(
            CurveFactory.CreateRectangle(block.Width, block.Height, new MsaglPoint(0, 0))
        )
        {
            UserData = block
        };
    }
}