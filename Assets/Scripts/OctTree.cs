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

    public OctTreeNode GetNode(Vector2 position)
    {
        var teoreticalNode = GetNode(position, maxLevel);
        return GetNodeOrParent(teoreticalNode, maxLevel);
    }

    private Vector3Int GetNode(Vector2 position, int level)
    {
        var nodesCount = Mathf.Pow(2, level);
        var rootBounds = root.bounds;
        var xpos = (int)(Mathf.InverseLerp(rootBounds.min.x, rootBounds.max.x, position.x) * nodesCount);
        var ypos = (int)(Mathf.InverseLerp(rootBounds.min.y, rootBounds.max.y, position.y) * nodesCount);
        var zpos = (int)(Mathf.InverseLerp(rootBounds.min.z, rootBounds.max.z, position.y) * nodesCount);
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
                        GetNodeNeighbours(node.NodeSED, direction, list);
                        GetNodeNeighbours(node.NodeNED, direction, list);
                        GetNodeNeighbours(node.NodeSEU, direction, list);
                        GetNodeNeighbours(node.NodeNEU, direction, list);
                        break;
                    case Direction.Right:
                        GetNodeNeighbours(node.NodeSWD, direction, list);
                        GetNodeNeighbours(node.NodeNWD, direction, list);
                        GetNodeNeighbours(node.NodeSWU, direction, list);
                        GetNodeNeighbours(node.NodeNWU, direction, list);
                        break;
                    case Direction.Up:
                        GetNodeNeighbours(node.NodeSWD, direction, list);
                        GetNodeNeighbours(node.NodeSED, direction, list);
                        GetNodeNeighbours(node.NodeNWD, direction, list);
                        GetNodeNeighbours(node.NodeNED, direction, list);
                        break;
                    case Direction.Down:
                        GetNodeNeighbours(node.NodeSWU, direction, list);
                        GetNodeNeighbours(node.NodeSEU, direction, list);
                        GetNodeNeighbours(node.NodeNWU, direction, list);
                        GetNodeNeighbours(node.NodeNEU, direction, list);
                        break;
                    case Direction.Forward:
                        GetNodeNeighbours(node.NodeSWD, direction, list);
                        GetNodeNeighbours(node.NodeSED, direction, list);
                        GetNodeNeighbours(node.NodeSWU, direction, list);
                        GetNodeNeighbours(node.NodeSEU, direction, list);
                        break;
                    case Direction.Back:
                        GetNodeNeighbours(node.NodeNWD, direction, list);
                        GetNodeNeighbours(node.NodeNED, direction, list);
                        GetNodeNeighbours(node.NodeNWU, direction, list);
                        GetNodeNeighbours(node.NodeNEU, direction, list);
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
            var m = node.bounds.min;
            var size = (node.bounds.max - node.bounds.min) / 2;

            var childCoords = new Vector3Int(node.position.x * 2, node.position.y * 2, node.position.z * 2);
            var childLevel = node.level + 1;
            node.NodeSWD = AddNode(childCoords + new Vector3Int(0, 0, 0), childLevel, m + new Vector3(0, 0, 0), size);
            node.NodeSED = AddNode(childCoords + new Vector3Int(1, 0, 0), childLevel, m + new Vector3(size.x, 0, 0), size);
            node.NodeNWD = AddNode(childCoords + new Vector3Int(0, 1, 0), childLevel, m + new Vector3(0, size.y, 0), size);
            node.NodeNED = AddNode(childCoords + new Vector3Int(1, 1, 0), childLevel, m + new Vector3(size.x, size.y, 0), size);
            node.NodeSWU = AddNode(childCoords + new Vector3Int(0, 0, 1), childLevel, m + new Vector3(0, 0, size.z), size);
            node.NodeSEU = AddNode(childCoords + new Vector3Int(1, 0, 1), childLevel, m + new Vector3(size.x, 0, size.z), size);
            node.NodeNWU = AddNode(childCoords + new Vector3Int(0, 1, 1), childLevel, m + new Vector3(0, size.y, size.z), size);
            node.NodeNEU = AddNode(childCoords + new Vector3Int(1, 1, 1), childLevel, m + new Vector3(size.x, size.y, size.z), size);
            node.isLeaf = false;
        }

        Insert(node.NodeSWD, rBounds);
        Insert(node.NodeSED, rBounds);
        Insert(node.NodeNWD, rBounds);
        Insert(node.NodeNED, rBounds);
        Insert(node.NodeSWU, rBounds);
        Insert(node.NodeSEU, rBounds);
        Insert(node.NodeNWU, rBounds);
        Insert(node.NodeNEU, rBounds);
    }

    private OctTreeNode AddNode(Vector3Int coords, int level, Vector3 min, Vector3 size)
    {
        var center = min + size/2;
        var quadTreeNode = new OctTreeNode(coords, level, new Bounds(center, size));
        nodes.Add((coords, level), quadTreeNode);
        return quadTreeNode;
    }
}