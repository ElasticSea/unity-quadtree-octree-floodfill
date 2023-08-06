using UnityEngine;
using UnityEngine.Serialization;

public class GenerateQuadTree : MonoBehaviour
{
    [SerializeField] private int maxLevel = 3;
    
    private QuadTree quadtree;

    [SerializeField] private Transform[] occupants;

    private void Update()
    {
        quadtree = new QuadTree(new Rect(0, 0, 1, 1), maxLevel);
        foreach (var occupant in occupants)
        {
            if (occupant)
            {
                quadtree.Insert(new Rect(occupant.position.x, occupant.position.y, 0, 0));
            }
        }
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        
        void DrawOctree((Vector2Int pos, Rect bounds, bool occupied, bool isLeaf) octree)
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