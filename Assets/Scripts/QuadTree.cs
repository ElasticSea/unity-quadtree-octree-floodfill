using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class QuadTree
{
    private readonly int maxLevel;
    private readonly QuadTreeNode root;
    private readonly Dictionary<Vector3Int,QuadTreeNode> nodes;

    public QuadTree(Rect rect, int maxLevel)
    {
        this.maxLevel = maxLevel;
        this.nodes = new Dictionary<Vector3Int, QuadTreeNode>();
        this.root = new QuadTreeNode(this, Vector3Int.zero, rect.min, rect.size);
    }

    public void Insert(Rect bounds)
    {
        root.Insert(bounds);
    }

    public IEnumerable<(Vector3Int pos, Rect bounds, bool occupied, bool isLeaf)> GetNodes()
    {
        return root.GetNodes();
    }

    public Vector3Int GetNode(Vector2 position)
    {
        var teoreticalNode = GetNode(position, maxLevel);
        return GetNodeOrParent(teoreticalNode);
    }

    public Vector3Int GetNode(Vector2 position, int level)
    {
        level = Mathf.Clamp(level, 0, maxLevel);
        var nodesCount = Mathf.Pow(2, level);
        var rootBounds = root.bounds;
        var xpos = (int)(Mathf.InverseLerp(rootBounds.xMin, rootBounds.xMax, position.x) * nodesCount);
        var ypos = (int)(Mathf.InverseLerp(rootBounds.yMin, rootBounds.yMax, position.y) * nodesCount);
        return new Vector3Int(xpos, ypos, level);
    }

    public bool IsOccupied(Vector3Int coords)
    {
        if (nodes.TryGetValue(coords, out var node))
        {
            return node.occupied;
        }

        return default;
    }

    public Rect GetBounds(Vector3Int coords)
    {
        if (nodes.TryGetValue(coords, out var node))
        {
            return node.bounds;
        }

        return default;
    }
    
    private enum Direction
    {
        Left, Right, Up, Down
    }

    public IEnumerable<Vector3Int> GetNodeNeighbours(Vector3Int coords)
    {
        if (IsValid(coords) == false)
        {
            return Enumerable.Empty<Vector3Int>();
        }

        var all = new List<Vector3Int>();
        all.AddRange(GetNodeNeighbours(coords + Vector3Int.right, Direction.Right));
        all.AddRange(GetNodeNeighbours(coords + Vector3Int.left, Direction.Left));
        all.AddRange(GetNodeNeighbours(coords + Vector3Int.up, Direction.Up));
        all.AddRange(GetNodeNeighbours(coords + Vector3Int.down, Direction.Down));
        return all;
    }
    
    
    private IEnumerable<Vector3Int> GetNodeNeighbours(Vector3Int coords, Direction direction)
    {
        if (IsOutOfBounds(coords))
        {
            return Enumerable.Empty<Vector3Int>();
        }

        if (IsValid(coords) == false)
        {
            return new[] { GetNodeOrParent(coords) };
        }

        var list = new List<Vector3Int>();
        GetNodeNeighbours(nodes[coords], direction, list);
        return list;
    }

    private Vector3Int GetNodeOrParent(Vector3Int coords)
    {
        while (nodes.ContainsKey(coords) == false && coords.z >= 0)
        {
            coords = GetParent(coords);
        }

        return coords;
    }

    private Vector3Int GetParent(Vector3Int coords)
    {
        return new Vector3Int(coords.x / 2, coords.y / 2, coords.z - 1);
    }

    private bool IsOutOfBounds(Vector3Int coords)
    {
        var nodesCount = Mathf.Pow(2, coords.z);
        return coords.z < 0 || coords.z > maxLevel ||
               coords.x < 0 || coords.x >= nodesCount ||
               coords.y < 0 || coords.y >= nodesCount;
    }

    private void GetNodeNeighbours(QuadTreeNode node, Direction direction, List<Vector3Int> list)
    {
        if (node.isLeaf)
        {
            list.Add(node.coords);
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

    public bool IsValid(Vector3Int coords)
    {
        return nodes.ContainsKey(coords);
    }
    

    // public IEnumerable<Rect> GetNeighbours(int x, int y, int level)
    // {
    //     var position = (x, y, level);
    //     while (nodes.ContainsKey(position) == false)
    //     {
    //         position = GetParent(position.x, position.y, position.level);
    //     }
    //
    //     var foundNode = position;
    // }

    private class QuadTreeNode
    {
        public QuadTreeNode NodeSW;
        public QuadTreeNode NodeSE;
        public QuadTreeNode NodeNW;
        public QuadTreeNode NodeNE;
        public readonly QuadTree tree;
        public readonly Vector3Int coords;
        public readonly Rect bounds;
        public readonly bool isMaxLevel;
        public bool isLeaf;
        public bool occupied;

        public QuadTreeNode(QuadTree tree, Vector3Int coords, Vector2 min, Vector2 size)
        {
            this.tree = tree;
            this.coords = coords;
            this.isLeaf = true;
            this.isMaxLevel = coords.z == tree.maxLevel;
            this.bounds = new Rect(min, size);
            this.tree.nodes.Add(coords, this);
        }

        public void Insert(Rect rBounds)
        {
            if (rBounds.Overlaps(bounds) == false)
            {
                return;
            }

            if (isMaxLevel)
            {
                occupied = true;
                return;
            }

            if (isLeaf)
            {
                var m = bounds.min;
                var size = (bounds.max - bounds.min) / 2;

                var childCoords = new Vector3Int(coords.x * 2, coords.y * 2, coords.z + 1);
                NodeSW = new QuadTreeNode(tree, childCoords + new Vector3Int(0, 0), m + new Vector2(0, 0), size);
                NodeSE = new QuadTreeNode(tree, childCoords + new Vector3Int(1, 0), m + new Vector2(size.x, 0), size);
                NodeNW = new QuadTreeNode(tree, childCoords + new Vector3Int(0, 1), m + new Vector2(0, size.y), size);
                NodeNE = new QuadTreeNode(tree, childCoords + new Vector3Int(1, 1), m + new Vector2(size.x, size.y), size);
                isLeaf = false;
            }

            NodeSW.Insert(rBounds);
            NodeSE.Insert(rBounds);
            NodeNW.Insert(rBounds);
            NodeNE.Insert(rBounds);
        }

        public IEnumerable<(Vector3Int pos, Rect bounds, bool occupied, bool isLeaf)> GetNodes()
        {
            yield return (coords, bounds, occupied, isLeaf);
            if (NodeSW != null) foreach(var node in NodeSW.GetNodes()) yield return node;
            if (NodeSE != null) foreach(var node in NodeSE.GetNodes()) yield return node;
            if (NodeNW != null) foreach(var node in NodeNW.GetNodes()) yield return node;
            if (NodeNE != null) foreach(var node in NodeNE.GetNodes()) yield return node;
        }
    }
}