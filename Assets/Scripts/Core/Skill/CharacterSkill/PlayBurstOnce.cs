using UnityEngine;

[ExecuteInEditMode]
public class PlayBurstOnce : MonoBehaviour
{
    private Renderer _renderer;

    // Cache Renderer để tránh gọi GetComponent liên tục
    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
    }

    void OnEnable()
    {
        TriggerBurst();
    }

    [ContextMenu("Trigger Burst")]
    public void TriggerBurst()
    {
        if (_renderer == null) _renderer = GetComponent<Renderer>();
        if (_renderer == null) return;

        // CHÌA KHÓA Ở ĐÂY:
        // Nếu game đang chạy (Play), dùng .material để tạo bản sao riêng cho object
        // Nếu đang ở Editor, dùng .sharedMaterial để chỉnh sửa trực tiếp vào file gốc, tránh leak.
        Material mat = Application.isPlaying ? _renderer.material : _renderer.sharedMaterial;

        if (mat != null)
        {
            float currentTime = Application.isPlaying ? Time.time : (float)Time.realtimeSinceStartup;
            mat.SetFloat("_StartTime", currentTime);

#if UNITY_EDITOR
            // Ép Editor vẽ lại Scene ngay lập tức để thấy hiệu ứng nổ
            if (!Application.isPlaying)
            {
                UnityEditor.SceneView.RepaintAll();
            }
#endif
        }
    }

    // Trigger khi click vào object trong Scene View (yêu cầu có Collider)
    void OnMouseDown()
    {
        TriggerBurst();
    }
}