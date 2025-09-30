using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BuildingModule), true)]
public class BuildingModuleEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw default inspector
        DrawDefaultInspector();

        BuildingModule module = (BuildingModule)target;

        GUILayout.Space(10);
        GUILayout.Label("Module Status", EditorStyles.boldLabel);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Max Level:", GUILayout.Width(70));
        GUILayout.Label(module.maxLevel.ToString(), GUILayout.Width(30));
        GUILayout.EndHorizontal();

        if (module.showMaxLevelWarning)
        {
            EditorGUILayout.HelpBox($"This module can be upgraded up to level {module.maxLevel}. When max level is reached, it will show 'MAXED OUT!' in the UI.", MessageType.Info);
        }
    }
}