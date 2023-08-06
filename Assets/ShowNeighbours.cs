using System.Collections.Generic;
using UnityEngine;

public class ShowNeighbours : MonoBehaviour
{
    [SerializeField] private GenerateQuadTree tree;
    [SerializeField] private int level;
    
    private Vector3Int current;
    private IEnumerable<Vector3Int> neighbours;
    private QuadTree quadtree;

    private void Update()
    {
        quadtree = tree.Quadtree;
        current = quadtree.GetNode(transform.position, level);
        neighbours = quadtree.GetNodeNeighbours(current);
        // left = quadtree.GetNode(node + Vector3Int.left);
        // right = quadtree.GetNode(node + Vector3Int.right);
        // up = quadtree.GetNode(node + Vector3Int.up);
        // down = quadtree.GetNode(node + Vector3Int.down);
    }

    private void OnDrawGizmosSelected()
    {
        void DrawRect(Rect rect)
        {
            var size = rect.max - rect.min;
            var center = rect.min + size / 2;
            Gizmos.DrawCube(center, size);
        }
        // Gizmos.color = Color.blue;
        // DrawRect(current);
        // Gizmos.color = Color.red;
        // DrawRect(left);
        // Gizmos.color = Color.green;
        // DrawRect(right);
        // Gizmos.color = Color.cyan;
        // DrawRect(up);
        // Gizmos.color = Color.yellow;
        // DrawRect(down);
        
        Gizmos.color = Color.green;
        DrawRect(quadtree.GetBounds(current));
        
        Gizmos.color = Color.yellow;
        foreach (var neighbour in neighbours)
        {
            DrawRect(quadtree.GetBounds(neighbour));
        }
    }
}