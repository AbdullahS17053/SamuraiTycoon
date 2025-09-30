using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BuildingConfig))]
public class BuildingConfigEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw default inspector
        DrawDefaultInspector();

        BuildingConfig config = (BuildingConfig)target;

        GUILayout.Space(10);
        GUILayout.Label("Module Management", EditorStyles.boldLabel);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Test Income Module"))
        {
            // You can create a default module here or reference existing ones
            Debug.Log("Add your module assignment logic here");
        }

        if (GUILayout.Button("Clear All Modules"))
        {
            config.modules.Clear();
            EditorUtility.SetDirty(config);
            Debug.Log("Cleared all modules from " + config.DisplayName);
        }
        GUILayout.EndHorizontal();

        // Show current modules
        GUILayout.Space(5);
        GUILayout.Label($"Current Modules: {config.modules.Count}/{config.maxModuleSlots}");
        foreach (var module in config.modules)
        {
            if (module != null)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label($" - {module.moduleName}", GUILayout.Width(150));
                if (GUILayout.Button("Remove", GUILayout.Width(60)))
                {
                    config.modules.Remove(module);
                    EditorUtility.SetDirty(config);
                    break;
                }
                GUILayout.EndHorizontal();
            }
        }
    }
}