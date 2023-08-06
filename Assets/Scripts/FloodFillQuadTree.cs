using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class FloodFillQuadTree : MonoBehaviour
{
    [SerializeField] private GenerateQuadTree tree;
    
    private HashSet<QuadTreeNode> current;
    private HashSet<QuadTreeNode> done;

    private void OnDrawGizmosSelected()
    {
        void DrawRect(Rect rect, Color color, bool filled)
        {
            Gizmos.color = color;
            var size = rect.max - rect.min;
            var center = rect.min + size / 2;

            if (filled)
            {
                Gizmos.DrawCube(center, size);
            }
            else
            {
                Gizmos.DrawWireCube(center, size);
            }
        }
        
        if (done != null)
        {
            foreach (var node in done)
            {
                DrawRect(node.bounds, Color.green, true);
            }
        }

        if (current != null)
        {
            foreach (var node in current)
            {
                DrawRect(node.bounds, Color.yellow, true);
            }
        }

        DrawRect(tree.Quadtree.GetNode(transform.position).bounds, Color.magenta, false);
    }

    public void Next()
    {
        if (current == null || current.Any() == false)
        {
            current = new HashSet<QuadTreeNode> { tree.Quadtree.GetNode(transform.position) };
        }
        else
        {
            if (done == null)
            {
                done = new HashSet<QuadTreeNode>();
            }
            done.AddRange(current);

            current = new HashSet<QuadTreeNode>();
            foreach (var d in done)
            {
                var neighbours = tree.Quadtree.GetNodeNeighbours(d);
                foreach (var n in neighbours)
                {
                    if (n.occupied == false)
                    {
                        if (done.Contains(n) == false)
                        {
                            current.Add(n);
                        }
                    }
                }
            }
        }
    }
}