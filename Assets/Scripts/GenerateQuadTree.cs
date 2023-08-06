using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class GenerateQuadTree : MonoBehaviour
{
    [SerializeField] private int maxLevel = 3;
    
    private QuadTree quadtree;

    [SerializeField] private Transform[] occupants;

    public QuadTree Quadtree => quadtree;

    private void Update()
    {
        quadtree = new QuadTree(new Rect(0, 0, 1, 1), maxLevel);
        foreach (var occupant in FindObjectsOfType<Collider2D>())
        {
            var worldBounds = occupant.bounds;
            var rect = new Rect(worldBounds.min.x, worldBounds.min.y, worldBounds.size.x, worldBounds.size.y);
            quadtree.Insert(rect);
        }
    }
    
    private void OnDrawGizmos()
    {
        void DrawOctree((Vector3Int pos, Rect bounds, bool occupied, bool isLeaf) octree)
        {
            if (octree.isLeaf )
            {
                if (octree.occupied )
                {
                    Gizmos.color = Color.black;
                }
                else if (octree.occupied == false )
                {
                    Gizmos.color = Color.red;
                }
                else if (octree.isLeaf == false)
                {
                    Gizmos.color = Color.yellow;
                }
                else
                {
                    return;
                }
            }
            else
            {
                Gizmos.color = Color.magenta;
            }

            var size = octree.bounds.max - octree.bounds.min;
            var center = octree.bounds.min + size / 2;
            Gizmos.DrawWireCube(center, size);
        }

        if (quadtree != null)
        {
            foreach (var node in quadtree.GetNodes())
            {
                DrawOctree(node);
            }
        }
    }
}