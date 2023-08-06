using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(FloodFillOctTree))]
    public class FloodFillOctTreeEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var showNeighbours = (FloodFillOctTree)target;

            if (GUILayout.Button("Next"))
            {
                showNeighbours.Next();
            }
        }
    }
}