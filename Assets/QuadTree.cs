using System.Collections.Generic;
using UnityEngine;

public class QuadTree
{
    private readonly int maxLevel;
    private readonly QuadTreeNode root;

    public QuadTree(Rect rect, int maxLevel)
    {
        this.maxLevel = maxLevel;
        this.root = new QuadTreeNode(Vector2Int.zero, rect.min, rect.size, this.maxLevel, 0);
    }

    public void Insert(Rect bounds)
    {
        root.Insert(bounds);
    }

    public IEnumerable<(Vector2Int pos, Rect bounds, bool occupied, bool isLeaf)> GetNodes()
    {
        return root.GetNodes();
    }

    private class QuadTreeNode
    {
        private QuadTreeNode NodeSW;
        private QuadTreeNode NodeSE;
        private QuadTreeNode NodeNW;
        private QuadTreeNode NodeNE;
        
        private readonly Vector2Int position;
        private readonly Rect bounds;
        private readonly int level;
        private readonly int maxLevel;
        private readonly bool isLeaf;

        private bool initiliazed;
        private bool occupied;

        public QuadTreeNode(Vector2Int position, Vector2 min, Vector2 size, int maxLevel, int level)
        {
            this.position = position;
            this.bounds = new Rect(min, size);
            this.level = level;
            this.maxLevel = maxLevel;
            this.isLeaf = level == maxLevel;
        }

        public void Insert(Rect rBounds)
        {
            if (rBounds.Overlaps(bounds) == false)
            {
                return;
            }

            if (isLeaf)
            {
                this.occupied = true;
                return;
            }

            if (initiliazed == false)
            {
                var m = bounds.min;
                var size = (bounds.max - bounds.min) / 2;

                NodeSW = new QuadTreeNode(position * 2 + new Vector2Int(0, 0), m + new Vector2(0, 0), size, maxLevel, level + 1);
                NodeSE = new QuadTreeNode(position * 2 + new Vector2Int(1, 0), m + new Vector2(size.x, 0), size, maxLevel, level + 1);
                NodeNW = new QuadTreeNode(position * 2 + new Vector2Int(0, 1), m + new Vector2(0, size.y), size, maxLevel, level + 1);
                NodeNE = new QuadTreeNode(position * 2 + new Vector2Int(1, 1), m + new Vector2(size.x, size.y), size, maxLevel, level + 1);
                initiliazed = true;
            }

            NodeSW.Insert(rBounds);
            NodeSE.Insert(rBounds);
            NodeNW.Insert(rBounds);
            NodeNE.Insert(rBounds);
        }

        public IEnumerable<(Vector2Int pos, Rect bounds, bool occupied, bool isLeaf)> GetNodes()
        {
            yield return (position, bounds, occupied, isLeaf);
            if (NodeSW != null) foreach(var node in NodeSW.GetNodes()) yield return node;
            if (NodeSE != null) foreach(var node in NodeSE.GetNodes()) yield return node;
            if (NodeNW != null) foreach(var node in NodeNW.GetNodes()) yield return node;
            if (NodeNE != null) foreach(var node in NodeNE.GetNodes()) yield return node;
        }
    }
}