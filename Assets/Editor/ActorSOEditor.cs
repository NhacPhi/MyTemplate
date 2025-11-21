using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ActorSO), true)]
public class ActorSOEditor : Editor
{
    public override void OnInspectorGUI()
    {

        DrawDefaultInspector();

        ActorSO actor = (ActorSO)target;

        if (actor.Texture != null)
        {
            GUILayout.Label("Icon Preview", EditorStyles.boldLabel);
            Texture2D texture = AssetPreview.GetAssetPreview(actor.Texture);
            if (texture != null)
            {
                float size = 128f;
                Rect rect = GUILayoutUtility.GetRect(size, size);
                GUI.DrawTexture(rect, texture, ScaleMode.ScaleToFit);
            }
        }
    }
}
