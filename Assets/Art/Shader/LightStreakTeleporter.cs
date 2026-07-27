using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Script điều khiển tương tác Cưỡi Mây / Biến Lên Trời dành riêng cho Nhân Vật Chính (Protagonist):
/// 1. Protagonist đi vào vùng Collision (Trigger) -> Hiện UI gợi ý tương tác (Mặc định phím F).
/// 2. Khi người chơi nhấn phím tương tác:
///    - PHA 1 (LÊN TRỜI): Ẩn Protagonist (Disable Render/Collider/Script di chuyển) -> Chạy hiệu ứng dải sáng bay LÊN -> Active Target Object (Cân Đẩu Vân / Vùng Đích) -> Dịch chuyển Protagonist.
///    - PHA 2 (HẠ THẾ): Chạy hiệu ứng dải sáng bay XUỐNG -> Deactive Target Object -> Hiện lại Protagonist tại điểm đích.
/// </summary>
public class LightStreakTeleporter : MonoBehaviour
{
    [Header("Teleport & Target Objects")]
    [Tooltip("Object sẽ được Kích Hoạt khi biến lên đỉnh (Ví dụ: Cân Đẩu Vân / Vùng Đích / Form mới)")]
    [SerializeField] private GameObject _targetObject;

    [Tooltip("Điểm tọa độ đích dịch chuyển Protagonist (Tùy chọn)")]
    [SerializeField] private Transform _destinationPoint;

    [Header("Effect Material Settings")]
    [Tooltip("Material sử dụng Custom/LightStreakEffect (Tự động lấy nếu để trống)")]
    [SerializeField] private Material _effectMaterial;

    [Tooltip("Thời gian chờ thêm giữa các pha")]
    [SerializeField] private float _extraDelay = 0.2f;

    // Trạng thái runtime
    private Protagonist _protagonist;
    private bool _isPlayerInRange = false;
    private bool _isAtDestination = false;
    private bool _isProcessing = false;

    private void Awake()
    {
        InitMaterial();
    }

    public void ExecuteTeleport()
    {
        if (_isProcessing) return;

        if (_protagonist == null)
        {
            _protagonist = FindObjectOfType<Protagonist>();
        }

        if (!_isAtDestination)
        {
            // PHA 1: Ẩn Protagonist -> Hiệu ứng Bay LÊN -> Active Target Object
            StartCoroutine(RoutineAscend());
        }
        else
        {
            // PHA 2: Hiệu ứng Đáp XUỐNG -> Deactive Target Object -> Hiện lại Protagonist
            StartCoroutine(RoutineDescend());
        }
    }

    // ==========================================
    // 3D TRIGGER COLLISION (Detect Protagonist)
    // ==========================================
    private void OnTriggerEnter(Collider other)
    {
        Protagonist p = other.GetComponent<Protagonist>() ?? other.GetComponentInParent<Protagonist>();
        if (p != null || other.CompareTag("Player"))
        {
            _protagonist = p ?? FindObjectOfType<Protagonist>();
            _isPlayerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Protagonist p = other.GetComponent<Protagonist>() ?? other.GetComponentInParent<Protagonist>();
        if (p != null || other.CompareTag("Player"))
        {
            _isPlayerInRange = false;
        }
    }

    // ==========================================
    // 2D TRIGGER COLLISION (Hỗ trợ cả game 2D)
    // ==========================================
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Protagonist p = collision.GetComponent<Protagonist>() ?? collision.GetComponentInParent<Protagonist>();
        if (p != null || collision.CompareTag("Player"))
        {
            _protagonist = p ?? FindObjectOfType<Protagonist>();
            _isPlayerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Protagonist p = collision.GetComponent<Protagonist>() ?? collision.GetComponentInParent<Protagonist>();
        if (p != null || collision.CompareTag("Player"))
        {
            _isPlayerInRange = false;
        }
    }

    // ==========================================
    // PHA 1: PROTAGONIST BIẾN LÊN TRỜI (ASCEND)
    // ==========================================
    private IEnumerator RoutineAscend()
    {
        _isProcessing = true;

        float duration = CalculateEffectDuration();

        // 1. NGAY KHI ẤN: Ẩn Protagonist & Tắt di chuyển lập tức
        if (_protagonist != null)
        {
            SetProtagonistVisibility(_protagonist, false);
            Debug.Log("<color=yellow>[LightStreakTeleporter] 1. Ấn phím -> Ẩn Protagonist & Tắt di chuyển ngay lập tức.</color>");
        }

        // 2. Kích hoạt hiệu ứng dải sáng bay từ DƯỚI lên TRÊN (_Reverse = 0)
        Debug.Log("<color=yellow>[LightStreakTeleporter] 2. Kích hoạt hiệu ứng dải sáng bay LÊN TRỜI...</color>");
        if (_effectMaterial != null)
        {
            _effectMaterial.SetFloat("_EditorPreview", 0f);
            _effectMaterial.SetFloat("_Reverse", 0f);
            _effectMaterial.SetFloat("_StartTime", Time.time);
        }

        // 3. Chờ hiệu ứng dải sáng bay xong
        yield return new WaitForSeconds(duration + _extraDelay);

        // 4. KHI BAY XONG: Active Target Object có sẵn trên đó
        if (_targetObject != null)
        {
            _targetObject.SetActive(true);
            Debug.Log("<color=green>[LightStreakTeleporter] 3. Bay xong -> Active Target Object trên đó thành công!</color>");
        }

        _isAtDestination = true;
        _isProcessing = false;
    }

    // ==========================================
    // PHA 2: PROTAGONIST GIÁNG THẾ HẠ CÁNH (DESCEND)
    // ==========================================
    private IEnumerator RoutineDescend()
    {
        _isProcessing = true;

        float duration = CalculateEffectDuration();

        // 1. NGAY KHI ẤN: Deactive Target Object trên đó lập tức
        if (_targetObject != null)
        {
            _targetObject.SetActive(false);
            Debug.Log("<color=orange>[LightStreakTeleporter] 1. Ấn phím -> Deactive Target Object ngay lập tức.</color>");
        }

        // 2. Kích hoạt hiệu ứng dải sáng bay từ TRÊN xuống DƯỚI (_Reverse = 1)
        Debug.Log("<color=cyan>[LightStreakTeleporter] 2. Kích hoạt hiệu ứng dải sáng bay TỪ TRÊN XUỐNG...</color>");
        if (_effectMaterial != null)
        {
            _effectMaterial.SetFloat("_EditorPreview", 0f);
            _effectMaterial.SetFloat("_Reverse", 1f);
            _effectMaterial.SetFloat("_StartTime", Time.time);
        }

        // 3. Chờ hiệu ứng dải sáng bay xuống xong
        yield return new WaitForSeconds(duration + _extraDelay);

        // 4. KHI BAY XONG: Hiện lại Protagonist & Mở lại di chuyển
        if (_protagonist != null)
        {
            SetProtagonistVisibility(_protagonist, true);
            Debug.Log("<color=green>[LightStreakTeleporter] 3. Bay xong -> Hiển thị lại Protagonist & Mở lại di chuyển!</color>");
        }

        _isAtDestination = false;
        _isProcessing = false;
    }

    // ==========================================
    // HELPER METHODS FOR PROTAGONIST
    // ==========================================
    private float CalculateEffectDuration()
    {
        if (_effectMaterial == null) return 1.0f;

        float beamLength = _effectMaterial.GetFloat("_BeamLength");
        float speed = _effectMaterial.GetFloat("_AscendSpeed");
        if (speed <= 0.01f) speed = 2.5f;

        return (1.0f + beamLength) / speed;
    }

    private void SetProtagonistVisibility(Protagonist protagonist, bool isVisible)
    {
        if (protagonist == null) return;

        // 1. Bật/tắt Script Protagonist để dừng/mở lại di chuyển và Input
        protagonist.enabled = isVisible;

        // 2. Bật/tắt Renderers (SpriteRenderer / MeshRenderer hình ảnh nhân vật)
        Renderer[] renderers = protagonist.GetComponentsInChildren<Renderer>(true);
        foreach (var r in renderers)
        {
            r.enabled = isVisible;
        }

        // 3. Bật/tắt Colliders (3D & 2D)
        Collider[] colliders = protagonist.GetComponentsInChildren<Collider>(true);
        foreach (var c in colliders)
        {
            c.enabled = isVisible;
        }

        Collider2D[] colliders2D = protagonist.GetComponentsInChildren<Collider2D>(true);
        foreach (var c in colliders2D)
        {
            c.enabled = isVisible;
        }
    }

    private void InitMaterial()
    {
        if (_effectMaterial != null) return;

        Renderer ren = GetComponent<Renderer>();
        if (ren != null)
        {
            _effectMaterial = ren.material;
            return;
        }

        Graphic graphic = GetComponent<Graphic>();
        if (graphic != null)
        {
            _effectMaterial = graphic.material;
        }
    }
}
