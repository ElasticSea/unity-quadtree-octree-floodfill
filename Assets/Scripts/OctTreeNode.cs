using System;
using UnityEngine;

public class OctTreeNode
{
    public OctTreeNode NodeWDS;
    public OctTreeNode NodeWDN;
    public OctTreeNode NodeWUS;
    public OctTreeNode NodeWUN;
    public OctTreeNode NodeEDS;
    public OctTreeNode NodeEDN;
    public OctTreeNode NodeEUS;
    public OctTreeNode NodeEUN;
    
    public readonly Vector3Int position;
    public readonly int level;
    public readonly Bounds bounds;
    public bool isLeaf = true;
    public bool occupied;

    public OctTreeNode(Vector3Int position, int level, Bounds bounds)
    {
        this.position = position;
        this.level = level;
        this.bounds = bounds;
    }

    protected bool Equals(OctTreeNode other)
    {
        return position.Equals(other.position) && level == other.level;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((OctTreeNode)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(position, level);
    }

    public static bool operator ==(OctTreeNode left, OctTreeNode right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(OctTreeNode left, OctTreeNode right)
    {
        return !Equals(left, right);
    }
}