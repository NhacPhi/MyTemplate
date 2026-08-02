using SixLabors.ImageSharp;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using VContainer;

using Gameplay.Common;

public enum ProtagonistState
{
    Normal,     // Trạng thái bình thường (di chuyển, tấn công, tương tác)
    Dialogue,   // Đang trong hội thoại (Khóa di chuyển, ngắt input, tắt collision)
    Dead        // Nhân vật đã chết
}

public class Protagonist : MonoBehaviour, IDamageable
{
    public ProtagonistState CurrentState { get; private set; } = ProtagonistState.Normal;
    [Header("Player Stats")]
    public int Level = 1;
    public int MaxHP;
    public int CurrentHP { get; private set; }
    public int AttackDamage;

    public bool IsTargetable => playerCollider != null ? playerCollider.enabled : !isClone;

    [SerializeField] private TransformAnchor gameplayCameraTransform = default;
    [SerializeField] private TransformAnchor playerTranform = default;

    [SerializeField] private GameObject objPlayer;

    [SerializeField] private SpriteRenderer character;
    [SerializeField] private SpriteRenderer clone;

    [SerializeField] private Animator smoke;

    [SerializeField] private float velocity = 0;
    [SerializeField] private float timeToGetWeapon;

    [Header("Transformation Settings")]
    [SerializeField] private Collider playerCollider; // Collider cần bật/tắt
    [SerializeField] private float cloneSpeedMultiplier = 1.5f; // Hệ số buff tốc độ
    private float baseVelocity;

    [Header("Idle Voice Settings")]
    [SerializeField] private bool _enableIdleVoice = true;
    [SerializeField] private float _idleTimeThreshold = 5f;
    [SerializeField] private AudioSource _voiceAudioSource;
    [SerializeField] private AudioDataConfig[] _idleVoiceConfigs; // Gán 3 ScriptableObject AudioDataConfig vào đây để chỉnh âm lượng (Volume)
    [SerializeField] private AudioDatabase _idleVoiceDatabase;   // Hoặc gán 1 ScriptableObject AudioDatabase
    [SerializeField] private AudioClip[] _idleVoiceClips;         // Mảng AudioClip trực tiếp (dùng làm fallback)

    private float _idleTimer = 0f;
    private Vector3 _lastPosition;
    private int _lastVoiceIndex = -1;

    float countDown = 0;

    [SerializeField] private WeaponController weapon;

    private Vector3 moveVector = Vector3.zero;

    private bool equipWeapon = false;
    private bool isClone = false;

    public LayerMask groundLayer;

    private Vector2 movement;

    private void OnEnable()
    {
        GameEvent.OnPlayerMove += PlayerMovement;
        GameEvent.OnPlayerAttack += PlayerAttack;
        GameEvent.OnPlayerTransform += Transformation;
        GameEvent.OnStartDialogue += HandleStartDialogue;
        GameEvent.OnEndDialogue += HandleEndDialogue;
        playerTranform.Provide(transform);
    }

    private void OnDisable()
    {
        GameEvent.OnPlayerMove -= PlayerMovement;
        GameEvent.OnPlayerAttack -= PlayerAttack;
        GameEvent.OnPlayerTransform -= Transformation;
        GameEvent.OnStartDialogue -= HandleStartDialogue;
        GameEvent.OnEndDialogue -= HandleEndDialogue;
    }
    // Start is called before the first frame update
    void Update()
    {
        if (CurrentState != ProtagonistState.Normal) return; // Chỉ xử lý khi ở trạng thái Normal

        if (equipWeapon && countDown > 0)
        {
            countDown -= Time.deltaTime;
            if (countDown < 0)
            {
                equipWeapon = false;
                weapon.TakeOffWeapon();
            }
        }

#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_STANDALONE_WIN
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        // Hỗ trợ phím cứng WASD và Phím Mũi Tên cho bản Windows Build & Editor
        if (movement.sqrMagnitude == 0)
        {
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) movement.x = -1;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) movement.x = 1;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) movement.y = 1;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) movement.y = -1;
        }

        if (movement.magnitude > 0)
        {
            PlayerMovement(movement);
        }

        // Thêm phím tấn công (Space / Chuột trái)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (UnityEngine.EventSystems.EventSystem.current == null || !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                PlayerAttack();
            }
        }
#endif

        CheckIdleVoice();
    }

    private void CheckIdleVoice()
    {
        if (!_enableIdleVoice || CurrentState != ProtagonistState.Normal)
        {
            _idleTimer = 0f;
            return;
        }

        // 1. Reset timer nếu vị trí nhân vật đang thay đổi (đang di chuyển)
        bool isMoving = (transform.position - _lastPosition).sqrMagnitude > 0.0001f;
        _lastPosition = transform.position;

        if (isMoving)
        {
            _idleTimer = 0f;
            return;
        }

        // 2. Chỉ tính thời gian khi ở Màn chơi chính (Không ở trong UI full-screen hay Battle)
        if (!IsInMainGameplay())
        {
            _idleTimer = 0f;
            return;
        }

        // 3. Nếu âm thanh voice trước đó đang phát -> Giữ timer = 0, chưa đếm 5s tiếp theo
        if (_voiceAudioSource != null && _voiceAudioSource.isPlaying)
        {
            _idleTimer = 0f;
            return;
        }

        // 4. Tăng thời gian idle (chỉ đếm sau khi âm thanh trước đã phát xong hoàn toàn)
        _idleTimer += Time.deltaTime;

        if (_idleTimer >= _idleTimeThreshold)
        {
            PlayRandomIdleVoice();
            _idleTimer = 0f; // Reset timer để tiếp tục đếm cho lần idle tiếp theo
        }
    }

    private bool IsInMainGameplay()
    {
        // Trả về false nếu game đang Pause (TimeScale = 0)
        if (Time.timeScale == 0f) return false;

        // Trả về false nếu đang trong màn Trận đánh (Battle)
        if (BattleManager.Instance != null && BattleManager.Instance.gameObject.activeInHierarchy)
        {
            return false;
        }

        // Trả về false nếu đang mở bất kỳ UI Window nào khác ngoài GamePlay (Inventory, Shop, Quests, Settings...)
        if (UIManager.Instance != null)
        {
            return UIManager.Instance.IsInMainGameplay();
        }

        return true;
    }

    private void PlayRandomIdleVoice()
    {
        PlayRandomIdleVoiceAsync().Forget();
    }

    private async UniTaskVoid PlayRandomIdleVoiceAsync()
    {
        // 1. Ưu tiên phát từ mảng ScriptableObject AudioDataConfig (_idleVoiceConfigs)
        if (_idleVoiceConfigs != null && _idleVoiceConfigs.Length > 0)
        {
            List<int> validIndices = new List<int>();
            for (int i = 0; i < _idleVoiceConfigs.Length; i++)
            {
                if (_idleVoiceConfigs[i] != null) validIndices.Add(i);
            }

            if (validIndices.Count > 0)
            {
                List<int> candidateIndices = new List<int>(validIndices);
                if (candidateIndices.Count > 1 && _lastVoiceIndex >= 0)
                {
                    candidateIndices.Remove(_lastVoiceIndex);
                }

                int chosenIndex = candidateIndices[Random.Range(0, candidateIndices.Count)];
                _lastVoiceIndex = chosenIndex;
                AudioDataConfig config = _idleVoiceConfigs[chosenIndex];

                await PlayAudioDataConfig(config);
                return;
            }
        }

        // 2. Sử dụng ScriptableObject AudioDatabase (_idleVoiceDatabase)
        if (_idleVoiceDatabase != null && _idleVoiceDatabase.SFXList != null && _idleVoiceDatabase.SFXList.Count > 0)
        {
            List<int> validIndices = new List<int>();
            for (int i = 0; i < _idleVoiceDatabase.SFXList.Count; i++)
            {
                if (_idleVoiceDatabase.SFXList[i] != null) validIndices.Add(i);
            }

            if (validIndices.Count > 0)
            {
                List<int> candidateIndices = new List<int>(validIndices);
                if (candidateIndices.Count > 1 && _lastVoiceIndex >= 0)
                {
                    candidateIndices.Remove(_lastVoiceIndex);
                }

                int chosenIndex = candidateIndices[Random.Range(0, candidateIndices.Count)];
                _lastVoiceIndex = chosenIndex;
                AudioDataConfig config = _idleVoiceDatabase.SFXList[chosenIndex];

                await PlayAudioDataConfig(config);
                return;
            }
        }

        // 3. Fallback: Phát từ mảng _idleVoiceClips (Gán trực tiếp AudioClip)
        if (_idleVoiceClips != null && _idleVoiceClips.Length > 0)
        {
            List<int> validIndices = new List<int>();
            for (int i = 0; i < _idleVoiceClips.Length; i++)
            {
                if (_idleVoiceClips[i] != null) validIndices.Add(i);
            }

            if (validIndices.Count > 0)
            {
                List<int> candidateIndices = new List<int>(validIndices);
                if (candidateIndices.Count > 1 && _lastVoiceIndex >= 0)
                {
                    candidateIndices.Remove(_lastVoiceIndex);
                }

                int chosenIndex = candidateIndices[Random.Range(0, candidateIndices.Count)];
                _lastVoiceIndex = chosenIndex;
                AudioClip clipToPlay = _idleVoiceClips[chosenIndex];

                PlayDirectClip(clipToPlay, 1f);
                Debug.Log($"[IdleVoice] Playing non-repeating voice clip (Index {chosenIndex}): {clipToPlay.name}");
                return;
            }
        }

        Debug.LogWarning("[IdleVoice] Chưa gán bất kỳ ScriptableObject AudioDataConfig hay AudioClip nào vào 'Idle Voice Settings' trên Inspector!");
    }

    private async UniTask PlayAudioDataConfig(AudioDataConfig config)
    {
        if (config == null) return;

        AudioClip clipToPlay = null;

        // 1. Kiểm tra DirectClip (Nếu gán trực tiếp file .mp3/.wav vào ScriptableObject)
        if (config.DirectClip != null)
        {
            clipToPlay = config.DirectClip;
        }

        // 2. Kiểm tra ClipRef (Addressable AssetReference)
        if (clipToPlay == null && config.ClipRef != null)
        {
            if (config.ClipRef.RuntimeKeyIsValid())
            {
                clipToPlay = await AddressablesManager.Instance.LoadAssetAsync<AudioClip>(config.ClipRef);
            }
#if UNITY_EDITOR
            if (clipToPlay == null && config.ClipRef.editorAsset != null)
            {
                clipToPlay = config.ClipRef.editorAsset as AudioClip;
            }
#endif
        }

        // 3. Nếu tìm thấy AudioClip -> Phát ngay lập tức với Volume của ScriptableObject
        if (clipToPlay != null)
        {
            PlayDirectClip(clipToPlay, config.Volume);
            Debug.Log($"[IdleVoice] Playing ScriptableObject '{config.name}' - Clip: {clipToPlay.name} (Volume: {config.Volume})");
            return;
        }

        // 4. Nếu không có clip trực tiếp -> Thử phát qua AudioManager nếu ID đã đăng ký
        var rootScope = FindObjectOfType<RootScope>();
        if (rootScope != null && rootScope.Container != null)
        {
            var audioMgr = rootScope.Container.Resolve(typeof(IAudioManager)) as IAudioManager;
            if (audioMgr != null && !string.IsNullOrEmpty(config.AudioID))
            {
                await audioMgr.PlaySFXAsync(config.AudioID);
                Debug.Log($"[IdleVoice] Playing AudioDataConfig via AudioManager: {config.AudioID} (Volume: {config.Volume})");
                return;
            }
        }

        Debug.LogWarning($"[IdleVoice] ScriptableObject '{config.name}' chưa được gán file âm thanh (DirectClip / ClipRef)!");
    }

    private void PlayDirectClip(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;
        if (_voiceAudioSource == null)
        {
            _voiceAudioSource = GetComponent<AudioSource>();
            if (_voiceAudioSource == null)
            {
                _voiceAudioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        _voiceAudioSource.spatialBlend = 0f; // Cấu hình 2D Sound để phát rõ ràng
        _voiceAudioSource.volume = volume;
        _voiceAudioSource.PlayOneShot(clip, volume);
    }

    private void PlayerMovement(Vector2 input)
    {
        if (CurrentState != ProtagonistState.Normal) return;

        if(input.magnitude > 0)
        {
            _idleTimer = 0f;

            if (input.x > 0) objPlayer.transform.localScale = new Vector3(-1, 1, 1);
            else if (input.x < 0) objPlayer.transform.localScale = new Vector3(1, 1, 1);

            Vector3 moveDir = new Vector3(input.x, 0, input.y).normalized;

            //transform.position = transform.position + moveDir * velocity * Time.deltaTime;

            Vector3 nextPosition = transform.position + moveDir * velocity * Time.deltaTime;

            // Dùng SphereCast (bắn tia hình cầu có độ dày 0.4f) thay cho Raycast (tia siêu mỏng)
            // Việc này giúp nhân vật không bị lọt tia check xuống khe nứt và đi lướt qua rãnh nhỏ
            Vector3 origin = nextPosition - Vector3.forward + Vector3.up * 0.5f; 
            if (Physics.SphereCast(origin, 0.4f, Vector3.down, out RaycastHit hit, 5f, groundLayer))
            {
                transform.position = nextPosition;
            }
        }

    }

    private void PlayerAttack()
    {
        if (CurrentState != ProtagonistState.Normal) return;

        _idleTimer = 0f;

        if (!equipWeapon && !isClone)
        {
            weapon.WeaponDoSomething(1);
            equipWeapon = true;
            countDown = timeToGetWeapon;

        }
        else if(equipWeapon)
        {
            countDown = timeToGetWeapon;
            weapon.WeaponDoSomething(2);
        }
        
    }

    private void Transformation()
    {
        if (CurrentState != ProtagonistState.Normal) return;

        _idleTimer = 0f;

        if (equipWeapon)
        {
            equipWeapon = false;
            weapon.TakeOffWeapon();
        }

        smoke.SetTrigger("Start");
        
        isClone = !isClone; // Đảo trạng thái trước
        
        character.gameObject.SetActive(!isClone);
        clone.gameObject.SetActive(isClone);
        
        // Bật/Tắt va chạm (deactivate collision)
        if (playerCollider != null)
        {
            playerCollider.enabled = !isClone;
        }

        // Buff thêm tốc độ di chuyển
        if (isClone)
        {
            velocity = baseVelocity * cloneSpeedMultiplier;
        }
        else
        {
            velocity = baseVelocity;
        }
    }

    public void UpdateStats()
    {
        MaxHP = 100 + (Level * 20);
        AttackDamage = 40 + (Level * 5);
        CurrentHP = MaxHP;
        UIEvent.OnUpdatePlayerHP?.Invoke(CurrentHP, MaxHP);
    }

    private void Start()
    {
        UpdateStats();
        baseVelocity = velocity; // Lưu tốc độ cơ bản
        _lastPosition = transform.position;

        // Tự động tìm Collider nếu bạn quên chưa kéo vào Inspector
        if (playerCollider == null) playerCollider = GetComponent<Collider>();
    }

    public void TakeDamage(int damage)
    {
        CurrentHP -= damage;
        Debug.Log($"Protagonist HP: {CurrentHP}/{MaxHP}");
        UIEvent.OnUpdatePlayerHP?.Invoke(CurrentHP, MaxHP);

        if (CurrentHP <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        ChangeState(ProtagonistState.Dead);
        Debug.Log("Protagonist Died!");
        // Sử dụng hệ thống load scene của game thay vì SceneManager mặc định
        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.RestartCurrentScene();
        }
    }

    public void ChangeState(ProtagonistState newState)
    {
        if (CurrentState == newState) return;

        ProtagonistState previousState = CurrentState;
        CurrentState = newState;

        OnExitState(previousState);
        OnEnterState(newState);
    }

    private void OnEnterState(ProtagonistState state)
    {
        switch (state)
        {
            case ProtagonistState.Normal:
                SetCollisionActive(!isClone);
                break;
            case ProtagonistState.Dialogue:
                _idleTimer = 0f;
                SetCollisionActive(false);
                break;
            case ProtagonistState.Dead:
                SetCollisionActive(false);
                break;
        }
    }

    private void OnExitState(ProtagonistState state)
    {
        switch (state)
        {
            case ProtagonistState.Dialogue:
                SetCollisionActive(!isClone);
                break;
        }
    }

    private void HandleStartDialogue(DialogueConfig dialogueConfig)
    {
        ChangeState(ProtagonistState.Dialogue);
    }

    private void HandleEndDialogue(DialogueType type)
    {
        if (CurrentState == ProtagonistState.Dialogue)
        {
            ChangeState(ProtagonistState.Normal);
        }
    }

    private void SetCollisionActive(bool active)
    {
        if (playerCollider != null)
        {
            playerCollider.enabled = active;
        }

        var colliders = GetComponents<Collider>();
        foreach (var col in colliders)
        {
            col.enabled = active;
        }
    }
}
