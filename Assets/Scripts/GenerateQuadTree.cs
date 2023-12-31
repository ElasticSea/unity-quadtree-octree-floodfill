using UnityEngine;

public class GenerateQuadTree : MonoBehaviour
{
    [SerializeField] private int maxLevel = 3;
    private QuadTree quadtree;
    public QuadTree Quadtree => quadtree;

    private void Update()
    {
        quadtree = new QuadTree(new Rect(0, 0, 1, 1), maxLevel);
        foreach (var occupant in GetComponentsInChildren<Collider2D>())
        {
            var worldBounds = occupant.bounds;
            var rect = new Rect(worldBounds.min.x, worldBounds.min.y, worldBounds.size.x, worldBounds.size.y);
            quadtree.Insert(rect);
        }
    }
    
    private void OnDrawGizmos()
    {
        void DrawOctree(QuadTreeNode octree)
        {
            if (octree.isLeaf )
            {
                if (octree.occupied == false )
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
            else if(octree.occupied == false)
            {
                Gizmos.color = Color.magenta;
            }
            else
            {
                return;
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