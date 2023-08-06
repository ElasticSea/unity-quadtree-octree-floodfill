using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OctTree
{
    private readonly int maxLevel;
    private readonly OctTreeNode root;
    private readonly Dictionary<(Vector3Int coords, int level), OctTreeNode> nodes;

    public OctTree(Bounds bounds, int maxLevel)
    {
        this.maxLevel = maxLevel;
        this.nodes = new Dictionary<(Vector3Int coords, int level), OctTreeNode>();
        this.root = AddNode(Vector3Int.zero, 0, bounds.min, bounds.size);
    }

    public void Insert(Bounds bounds)
    {
        Insert(root, bounds);
    }

    public IEnumerable<OctTreeNode> GetNodes()
    {
        return nodes.Values.ToArray();
    }

    public OctTreeNode GetNode(Vector3 position)
    {
        var teoreticalNode = GetNode(position, maxLevel);
        return GetNodeOrParent(teoreticalNode, maxLevel);
    }

    private Vector3Int GetNode(Vector3 position, int level)
    {
        var nodesCount = Mathf.Pow(2, level);
        var rootBounds = root.bounds;
        var xpos = (int)(Mathf.InverseLerp(rootBounds.min.x, rootBounds.max.x, position.x) * nodesCount);
        var ypos = (int)(Mathf.InverseLerp(rootBounds.min.y, rootBounds.max.y, position.y) * nodesCount);
        var zpos = (int)(Mathf.InverseLerp(rootBounds.min.z, rootBounds.max.z, position.z) * nodesCount);
        return new Vector3Int(xpos, ypos, zpos);
    }

    private enum Direction
    {
        Left,
        Right,
        Up,
        Down,
        Forward,
        Back
    }

    public IEnumerable<OctTreeNode> GetNodeNeighbours(OctTreeNode node)
    {
        return GetNodeNeighbours(node.position, node.level);
    }

    private IEnumerable<OctTreeNode> GetNodeNeighbours(Vector3Int coords, int level)
    {
        if (IsValid(coords, level) == false)
        {
            return Enumerable.Empty<OctTreeNode>();
        }

        var all = new List<OctTreeNode>();
        all.AddRange(GetNodeNeighbours(coords + Vector3Int.right, level, Direction.Right));
        all.AddRange(GetNodeNeighbours(coords + Vector3Int.left, level, Direction.Left));
        all.AddRange(GetNodeNeighbours(coords + Vector3Int.up, level, Direction.Up));
        all.AddRange(GetNodeNeighbours(coords + Vector3Int.down, level, Direction.Down));
        all.AddRange(GetNodeNeighbours(coords + Vector3Int.forward, level, Direction.Forward));
        all.AddRange(GetNodeNeighbours(coords + Vector3Int.back, level, Direction.Back));
        return all;
    }

    private IEnumerable<OctTreeNode> GetNodeNeighbours(Vector3Int coords, int level, Direction direction)
    {
        if (IsOutOfBounds(coords, level))
        {
            return Enumerable.Empty<OctTreeNode>();
        }

        if (IsValid(coords, level) == false)
        {
            return new[] { GetNodeOrParent(coords, level) };
        }

        var list = new List<OctTreeNode>();
        GetNodeNeighbours(nodes[(coords, level)], direction, list);
        return list;
    }

    private OctTreeNode GetNodeOrParent(Vector3Int coords, int level)
    {
        OctTreeNode node = null;
        var coordsLevel = (coords, level);
        while (nodes.TryGetValue(coordsLevel, out node) == false && coordsLevel.level >= 0)
        {
            coordsLevel = GetParent(coordsLevel.coords, coordsLevel.level);
        }

        return node;
    }

    private (Vector3Int coords, int level) GetParent(Vector3Int coords, int level)
    {
        return (new Vector3Int(coords.x / 2, coords.y / 2, coords.z / 2), level - 1);
    }

    private bool IsOutOfBounds(Vector3Int coords, int level)
    {
        var nodesCount = Mathf.Pow(2, level);
        return level < 0 || level > maxLevel ||
               coords.x < 0 || coords.x >= nodesCount ||
               coords.y < 0 || coords.y >= nodesCount ||
               coords.z < 0 || coords.z >= nodesCount;
    }

    private void GetNodeNeighbours(OctTreeNode node, Direction direction, List<OctTreeNode> list)
    {
        if (node.isLeaf)
        {
            list.Add(node);
        }
        else
        {
            if (node.isLeaf == false)
            {
                switch (direction)
                {
                    case Direction.Left:
                        GetNodeNeighbours(node.NodeEDS, direction, list);
                        GetNodeNeighbours(node.NodeEDN, direction, list);
                        GetNodeNeighbours(node.NodeEUS, direction, list);
                        GetNodeNeighbours(node.NodeEUN, direction, list);
                        break;
                    case Direction.Right:
                        GetNodeNeighbours(node.NodeWDS, direction, list);
                        GetNodeNeighbours(node.NodeWDN, direction, list);
                        GetNodeNeighbours(node.NodeWUS, direction, list);
                        GetNodeNeighbours(node.NodeWUN, direction, list);
                        break;
                    case Direction.Up:
                        GetNodeNeighbours(node.NodeWDS, direction, list);
                        GetNodeNeighbours(node.NodeWDN, direction, list);
                        GetNodeNeighbours(node.NodeEDS, direction, list);
                        GetNodeNeighbours(node.NodeEDN, direction, list);
                        break;
                    case Direction.Down:
                        GetNodeNeighbours(node.NodeWUS, direction, list);
                        GetNodeNeighbours(node.NodeWUN, direction, list);
                        GetNodeNeighbours(node.NodeEUS, direction, list);
                        GetNodeNeighbours(node.NodeEUN, direction, list);
                        break;
                    case Direction.Forward:
                        GetNodeNeighbours(node.NodeWDS, direction, list);
                        GetNodeNeighbours(node.NodeWUS, direction, list);
                        GetNodeNeighbours(node.NodeEDS, direction, list);
                        GetNodeNeighbours(node.NodeEUS, direction, list);
                        break;
                    case Direction.Back:
                        GetNodeNeighbours(node.NodeWDN, direction, list);
                        GetNodeNeighbours(node.NodeWUN, direction, list);
                        GetNodeNeighbours(node.NodeEDN, direction, list);
                        GetNodeNeighbours(node.NodeEUN, direction, list);
                        break;
                }
            }
        }
    }

    public bool IsValid(Vector3Int coords, int level)
    {
        return nodes.ContainsKey((coords, level));
    }

    public void Insert(OctTreeNode node, Bounds rBounds)
    {
        if (rBounds.Intersects(node.bounds) == false)
        {
            return;
        }

        if (node.level == maxLevel)
        {
            node.occupied = true;
            return;
        }

        if (node.isLeaf)
        {
            node.NodeWDS = AddChildNode(node, new Vector3Int(0, 0, 0));
            node.NodeWDN = AddChildNode(node, new Vector3Int(0, 0, 1));
            node.NodeWUS = AddChildNode(node, new Vector3Int(0, 1, 0));
            node.NodeWUN = AddChildNode(node, new Vector3Int(0, 1, 1));
            node.NodeEDS = AddChildNode(node, new Vector3Int(1, 0, 0));
            node.NodeEDN = AddChildNode(node, new Vector3Int(1, 0, 1));
            node.NodeEUS = AddChildNode(node, new Vector3Int(1, 1, 0));
            node.NodeEUN = AddChildNode(node, new Vector3Int(1, 1, 1));
            node.isLeaf = false;
        }

        Insert(node.NodeWDS, rBounds);
        Insert(node.NodeWDN, rBounds);
        Insert(node.NodeWUS, rBounds);
        Insert(node.NodeWUN, rBounds);
        Insert(node.NodeEDS, rBounds);
        Insert(node.NodeEDN, rBounds);
        Insert(node.NodeEUS, rBounds);
        Insert(node.NodeEUN, rBounds);
    }

    private OctTreeNode AddChildNode(OctTreeNode node, Vector3Int offset)
    {
        var position = node.position;
        var parentMin = node.bounds.min;
        var baseCoords = new Vector3Int(position.x * 2, position.y * 2, position.z * 2);
        var childSize = (node.bounds.max - node.bounds.min) / 2;
        var childMin = parentMin + new Vector3(offset.x * childSize.x, offset.y * childSize.y, offset.z * childSize.z);
        return AddNode(baseCoords + offset, node.level + 1, childMin, childSize);
    }

    private OctTreeNode AddNode(Vector3Int coords, int level, Vector3 min, Vector3 size)
    {
        var center = min + size/2;
        var quadTreeNode = new OctTreeNode(coords, level, new Bounds(center, size));
        nodes.Add((coords, level), quadTreeNode);
        return quadTreeNode;
    }
}