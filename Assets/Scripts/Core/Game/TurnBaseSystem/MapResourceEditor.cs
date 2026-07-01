#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(MapResource))]
[CanEditMultipleObjects]
public class MapResourceEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MapResource resource = (MapResource)target;

        EditorGUI.BeginChangeCheck();
        
        // Draw default inspector
        DrawDefaultInspector();

        if (GUILayout.Button("Generate Unique ID"))
        {
            GenerateUniqueID(resource);
        }

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(resource);
        }
    }

    private void GenerateUniqueID(MapResource resource)
    {
        resource.UniqueID = System.Guid.NewGuid().ToString();
        EditorUtility.SetDirty(resource);
        if (!Application.isPlaying)
        {
            EditorSceneManager.MarkSceneDirty(resource.gameObject.scene);
        }
        Debug.Log($"[MapResource] Generated new ID for {resource.gameObject.name}: {resource.UniqueID}");
    }
}
#endif
