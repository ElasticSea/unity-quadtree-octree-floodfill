using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FloodFillQuadTree : MonoBehaviour
{
    [SerializeField] private GenerateQuadTree tree;
    
    private IEnumerable<Vector3Int> current;
    private IEnumerable<Vector3Int> done;

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
                DrawRect(tree.Quadtree.GetBounds(node), Color.green, true);
            }
        }

        if (current != null)
        {
            foreach (var node in current)
            {
                DrawRect(tree.Quadtree.GetBounds(node), Color.yellow, true);
            }
        }

        DrawRect(tree.Quadtree.GetBounds(tree.Quadtree.GetNode(transform.position)), Color.magenta, false);
    }

    public void Next()
    {
        if (current == null || current.Any() == false)
        {
            current = new[] { tree.Quadtree.GetNode(transform.position) };
        }
        else
        {
            
            
            done = current;
            current = done.SelectMany(d => tree.Quadtree.GetNodeNeighbours(d)).Distinct().Where(
                d => tree.Quadtree.IsOccupied(d) == false).ToArray();
        }
        
        // current = quadtree.GetNode(transform.position, level);
        // neighbours = quadtree.GetNodeNeighbours(current);
        // throw new System.NotImplementedException();
    }
}