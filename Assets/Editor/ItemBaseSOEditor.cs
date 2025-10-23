using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ItemBaseSO), true)]
public class ItemBaseSOEditor : Editor
{
    public override void OnInspectorGUI()
    {

        DrawDefaultInspector();

        ItemBaseSO item  = (ItemBaseSO)target;

        if(item.Icon != null)
        {
            GUILayout.Label("Icon Preview", EditorStyles.boldLabel);
            Texture2D texture = AssetPreview.GetAssetPreview(item.Icon);
            if (texture != null)
            {
                float size = 128f;
                Rect rect = GUILayoutUtility.GetRect(size, size);
                GUI.DrawTexture(rect, texture, ScaleMode.ScaleToFit);
            }
        }
    }
}
