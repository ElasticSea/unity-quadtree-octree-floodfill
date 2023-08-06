using UnityEngine;

public class GenerateOctTree : MonoBehaviour
{
    [SerializeField] private int maxLevel = 3;
    private OctTree tree;
    public OctTree Tree => tree;

    private void Update()
    {
        tree = new OctTree(new Bounds(Vector3.one * 0.5f, Vector3.one), maxLevel);
        foreach (var occupant in GetComponentsInChildren<Collider>())
        {
            tree.Insert( occupant.bounds);
        }
    }
    
    private void OnDrawGizmos()
    {
        void DrawOctree(OctTreeNode octree)
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

        if (tree != null)
        {
            foreach (var node in tree.GetNodes())
            {
                DrawOctree(node);
            }
        }
    }
}