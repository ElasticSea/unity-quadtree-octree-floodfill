using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(FloodFillQuadTree))]
    public class FloodFillQuadTreeEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var showNeighbours = (FloodFillQuadTree)target;

            if (GUILayout.Button("Next"))
            {
                showNeighbours.Next();
            }
        }
    }
}