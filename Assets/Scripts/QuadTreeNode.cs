using System;
using UnityEngine;

public class QuadTreeNode
{
    public QuadTreeNode NodeSW;
    public QuadTreeNode NodeSE;
    public QuadTreeNode NodeNW;
    public QuadTreeNode NodeNE;
    
    public readonly Vector2Int position;
    public readonly int level;
    public readonly Rect bounds;
    public bool isLeaf = true;
    public bool occupied;

    public QuadTreeNode(Vector2Int position, int level, Rect bounds)
    {
        this.position = position;
        this.level = level;
        this.bounds = bounds;
    }

    protected bool Equals(QuadTreeNode other)
    {
        return position.Equals(other.position) && level == other.level;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((QuadTreeNode)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(position, level);
    }

    public static bool operator ==(QuadTreeNode left, QuadTreeNode right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(QuadTreeNode left, QuadTreeNode right)
    {
        return !Equals(left, right);
    }
}