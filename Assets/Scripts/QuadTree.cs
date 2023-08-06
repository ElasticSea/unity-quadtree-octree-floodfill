using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class QuadTree
{
    private readonly int maxLevel;
    private readonly QuadTreeNode root;
    private readonly Dictionary<(Vector2Int coords, int level), QuadTreeNode> nodes;

    public QuadTree(Rect rect, int maxLevel)
    {
        this.maxLevel = maxLevel;
        this.nodes = new Dictionary<(Vector2Int coords, int level), QuadTreeNode>();
        this.root = AddNode(Vector2Int.zero, 0, rect.min, rect.size);
    }

    public void Insert(Rect bounds)
    {
        Insert(root, bounds);
    }

    public IEnumerable<QuadTreeNode> GetNodes()
    {
        return nodes.Values.ToArray();
    }

    public QuadTreeNode GetNode(Vector2 position)
    {
        var teoreticalNode = GetNode(position, maxLevel);
        return GetNodeOrParent(teoreticalNode, maxLevel);
    }

    private Vector2Int GetNode(Vector2 position, int level)
    {
        var nodesCount = Mathf.Pow(2, level);
        var rootBounds = root.bounds;
        var xpos = (int)(Mathf.InverseLerp(rootBounds.xMin, rootBounds.xMax, position.x) * nodesCount);
        var ypos = (int)(Mathf.InverseLerp(rootBounds.yMin, rootBounds.yMax, position.y) * nodesCount);
        return new Vector2Int(xpos, ypos);
    }

    private enum Direction
    {
        Left,
        Right,
        Up,
        Down
    }

    public IEnumerable<QuadTreeNode> GetNodeNeighbours(QuadTreeNode node)
    {
        return GetNodeNeighbours(node.position, node.level);
    }

    private IEnumerable<QuadTreeNode> GetNodeNeighbours(Vector2Int coords, int level)
    {
        if (IsValid(coords, level) == false)
        {
            return Enumerable.Empty<QuadTreeNode>();
        }

        var all = new List<QuadTreeNode>();
        all.AddRange(GetNodeNeighbours(coords + Vector2Int.right, level, Direction.Right));
        all.AddRange(GetNodeNeighbours(coords + Vector2Int.left, level, Direction.Left));
        all.AddRange(GetNodeNeighbours(coords + Vector2Int.up, level, Direction.Up));
        all.AddRange(GetNodeNeighbours(coords + Vector2Int.down, level, Direction.Down));
        return all;
    }

    private IEnumerable<QuadTreeNode> GetNodeNeighbours(Vector2Int coords, int level, Direction direction)
    {
        if (IsOutOfBounds(coords, level))
        {
            return Enumerable.Empty<QuadTreeNode>();
        }

        if (IsValid(coords, level) == false)
        {
            return new[] { GetNodeOrParent(coords, level) };
        }

        var list = new List<QuadTreeNode>();
        GetNodeNeighbours(nodes[(coords, level)], direction, list);
        return list;
    }

    private QuadTreeNode GetNodeOrParent(Vector2Int coords, int level)
    {
        QuadTreeNode node = null;
        var coordsLevel = (coords, level);
        while (nodes.TryGetValue(coordsLevel, out node) == false && coordsLevel.level >= 0)
        {
            coordsLevel = GetParent(coordsLevel.coords, coordsLevel.level);
        }

        return node;
    }

    private (Vector2Int coords, int level) GetParent(Vector2Int coords, int level)
    {
        return (new Vector2Int(coords.x / 2, coords.y / 2), level - 1);
    }

    private bool IsOutOfBounds(Vector2Int coords, int level)
    {
        var nodesCount = Mathf.Pow(2, level);
        return level < 0 || level > maxLevel ||
               coords.x < 0 || coords.x >= nodesCount ||
               coords.y < 0 || coords.y >= nodesCount;
    }

    private void GetNodeNeighbours(QuadTreeNode node, Direction direction, List<QuadTreeNode> list)
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
                        GetNodeNeighbours(node.NodeSE, direction, list);
                        GetNodeNeighbours(node.NodeNE, direction, list);
                        break;
                    case Direction.Right:
                        GetNodeNeighbours(node.NodeSW, direction, list);
                        GetNodeNeighbours(node.NodeNW, direction, list);
                        break;
                    case Direction.Up:
                        GetNodeNeighbours(node.NodeSW, direction, list);
                        GetNodeNeighbours(node.NodeSE, direction, list);
                        break;
                    case Direction.Down:
                        GetNodeNeighbours(node.NodeNW, direction, list);
                        GetNodeNeighbours(node.NodeNE, direction, list);
                        break;
                }
            }
        }
    }

    public bool IsValid(Vector2Int coords, int level)
    {
        return nodes.ContainsKey((coords, level));
    }

    public void Insert(QuadTreeNode node, Rect rBounds)
    {
        if (rBounds.Overlaps(node.bounds) == false)
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

            var childCoords = new Vector2Int(node.position.x * 2, node.position.y * 2);
            var childLevel = node.level + 1;
            node.NodeSW = AddNode(childCoords + new Vector2Int(0, 0), childLevel, m + new Vector2(0, 0), size);
            node.NodeSE = AddNode(childCoords + new Vector2Int(1, 0), childLevel, m + new Vector2(size.x, 0), size);
            node.NodeNW = AddNode(childCoords + new Vector2Int(0, 1), childLevel, m + new Vector2(0, size.y), size);
            node.NodeNE = AddNode(childCoords + new Vector2Int(1, 1), childLevel, m + new Vector2(size.x, size.y), size);
            node.isLeaf = false;
        }

        Insert(node.NodeSW, rBounds);
        Insert(node.NodeSE, rBounds);
        Insert(node.NodeNW, rBounds);
        Insert(node.NodeNE, rBounds);
    }

    private QuadTreeNode AddNode(Vector2Int coords, int level, Vector2 min, Vector2 size)
    {
        var quadTreeNode = new QuadTreeNode(coords, level, new Rect(min, size));
        nodes.Add((coords, level), quadTreeNode);
        return quadTreeNode;
    }
}