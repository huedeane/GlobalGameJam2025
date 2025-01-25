using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ProceduralMapGenerator)), CanEditMultipleObjects]
public class ProceduralMapGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default inspector UI first (optional but common) 
        base.OnInspectorGUI();

        // Grab reference to the currently selected ProceduralMapGenerator
        ProceduralMapGenerator mapGen = (ProceduralMapGenerator)target;
        
        // Add a button to the inspector
        if (GUILayout.Button("Generate"))
        {
            mapGen.GenerateMap();
        }
    }
}