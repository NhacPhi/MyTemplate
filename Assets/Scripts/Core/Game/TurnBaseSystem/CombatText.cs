using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using VContainer.Unity;
using VContainer;
using Tech.Pool;
using UnityEngine.AddressableAssets;

public enum CombatPopupType
{
    Damage,
    Heal,
    Text
}

public class CombatPopupRequest
{
    public CombatPopupType Type;
    public float Value;
    public string Text;
    public Vector3 Position;
    public bool IsCritical;
    public Color Color;
    public int Priority; // 0 = Damage/Heal (first), 1 = Debuff/Buff (second), 2 = Passive/Special (third)
}

public class CombatText : IInitializable, IDisposable
{
    public static CombatText Instance { get; private set; }

    [Inject] IObjectResolver _objectResolver;

    public const string Address = "Combat Text";
    private CombatTextUI popupPrefab;

    private readonly List<CombatPopupRequest> _popupQueue = new List<CombatPopupRequest>();
    private bool _isProcessingQueue = false;
    private readonly CancellationTokenSource _cts = new CancellationTokenSource();

    // Lưu vị trí và số lần popup liên tiếp trên cùng 1 vị trí để tính Stagger Offset Y chống đè chữ
    private Vector3 _lastSpawnPos = Vector3.zero;
    private int _consecutiveStackCount = 0;
    private float _lastSpawnTime = 0f;

    // Đếm số lượng popup đang hiển thị
    private int _activePopupsCount = 0;

    public void Initialize()
    {
        Instance = this;
        UIEvent.DamagePopup += CreateDamagePopup;
        UIEvent.HealPopup += CreateHealPopup;
        UIEvent.TextPopup += CreateTextPopup;
        _ = WaitLoading();
    }

    private async UniTaskVoid WaitLoading()
    {
        while (!AddressablesManager.Instance)
        {
            await UniTask.Yield();
        }

        var prefab = await Addressables.LoadAssetAsync<GameObject>(Address);
        if (prefab != null)
        {
            popupPrefab = prefab.GetComponent<CombatTextUI>();
        }
    }

    public void CreateDamagePopup(float damage, Vector3 position, bool isCris)
    {
        EnqueuePopup(new CombatPopupRequest
        {
            Type = CombatPopupType.Damage,
            Value = damage,
            Position = position + new Vector3(0.35f, 0.5f, 0f), // Số damage nảy ở tầm thân hơi lệch sang phải
            IsCritical = isCris,
            Color = isCris ? new Color(1f, 0.85f, 0.15f) : Color.white,
            Priority = 0 // Ưu tiên số damage hiển thị trước
        });
    }

    public void CreateHealPopup(float heal, Vector3 position)
    {
        if (heal <= 0) return;

        EnqueuePopup(new CombatPopupRequest
        {
            Type = CombatPopupType.Heal,
            Value = heal,
            Text = $"+{Mathf.CeilToInt(heal)}",
            Position = position + new Vector3(0f, 0.9f, 0f),
            IsCritical = false,
            Color = new Color(0.25f, 1f, 0.35f),
            Priority = 0
        });
    }

    public void CreateTextPopup(string text, Vector3 position, Color? color = null)
    {
        if (string.IsNullOrEmpty(text)) return;

        EnqueuePopup(new CombatPopupRequest
        {
            Type = CombatPopupType.Text,
            Text = text,
            Position = position + new Vector3(-0.3f, 0.4f, 0f), // Chữ hiệu ứng hiển thị ở đỉnh đầu lệch sang trái
            IsCritical = false,
            Color = color ?? new Color(0.35f, 0.85f, 1f),
            Priority = 1 // Chữ hiệu ứng hiển thị tiếp theo sau số damage
        });
    }

    private void EnqueuePopup(CombatPopupRequest request)
    {
        lock (_popupQueue)
        {
            _popupQueue.Add(request);
        }

        if (!_isProcessingQueue)
        {
            ProcessQueueAsync().Forget();
        }
    }

    private async UniTaskVoid ProcessQueueAsync()
    {
        _isProcessingQueue = true;

        try
        {
            while (true)
            {
                CombatPopupRequest nextRequest = null;
                lock (_popupQueue)
                {
                    if (_popupQueue.Count > 0)
                    {
                        // Lấy request có độ ưu tiên cao nhất trước (Damage/Heal trước, Text hiệu ứng sau)
                        int bestIndex = 0;
                        for (int i = 1; i < _popupQueue.Count; i++)
                        {
                            if (_popupQueue[i].Priority < _popupQueue[bestIndex].Priority)
                            {
                                bestIndex = i;
                            }
                        }

                        nextRequest = _popupQueue[bestIndex];
                        _popupQueue.RemoveAt(bestIndex);
                    }
                }

                if (nextRequest == null)
                {
                    break;
                }

                while (popupPrefab == null)
                {
                    await UniTask.Yield(_cts.Token);
                }

                // 1. Tính toán vị trí chống đè chữ (Stagger Offset Y khi xuất hiện nhiều popup liên tiếp)
                Vector3 spawnPos = nextRequest.Position;
                float now = Time.time;
                if (Vector3.Distance(spawnPos, _lastSpawnPos) < 1.5f && (now - _lastSpawnTime) < 1.5f)
                {
                    _consecutiveStackCount++;
                    // Dịch chuyển bậc thang lên trên để các chữ xếp chồng rõ ràng không che nhau
                    spawnPos += new Vector3((_consecutiveStackCount % 2 == 1 ? -0.2f : 0.2f), _consecutiveStackCount * 0.45f, 0f);
                }
                else
                {
                    _consecutiveStackCount = 0;
                }

                _lastSpawnPos = spawnPos;
                _lastSpawnTime = now;

                // 2. Sinh popup từ Pool
                SpawnPopup(nextRequest, spawnPos);

                // 3. Nghỉ một nhịp Stagger (280ms) chuẩn mực giữa các popup trên cùng mục tiêu
                // để người chơi kịp nhìn thấy số sát thương rồi đọc tiếp dòng hiệu ứng giải thích
                int remainingCount;
                lock (_popupQueue)
                {
                    remainingCount = _popupQueue.Count;
                }

                if (remainingCount > 0)
                {
                    await UniTask.Delay(280, cancellationToken: _cts.Token);
                }
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception e)
        {
            Debug.LogError($"[CombatText] Lỗi khi xử lý hàng đợi popup: {e}");
        }
        finally
        {
            _isProcessingQueue = false;
        }
    }

    private void SpawnPopup(CombatPopupRequest request, Vector3 position)
    {
        if (popupPrefab == null) return;

        var clone = PoolManager.Instance.SpawnObject(popupPrefab, position, Quaternion.identity);
        if (clone == null) return;

        clone.SetAnimationEnabled(true);
        clone.TMP.color = request.Color;

        var jump = clone.GetComponent<NumberJumpAnimation>();

        _activePopupsCount++;
        TrackPopupDuration(jump != null ? 0.85f : 0.6f).Forget();

        switch (request.Type)
        {
            case CombatPopupType.Damage:
                clone.SetValue(request.Value);
                clone.SetCritical(request.IsCritical);
                if (jump != null) jump.PlayAnimation(request.IsCritical);
                break;

            case CombatPopupType.Heal:
                clone.SetText(request.Text);
                clone.SetCritical(false);
                if (jump != null) jump.PlayAnimation(false);
                break;

            case CombatPopupType.Text:
                clone.SetText(request.Text);
                clone.SetCritical(false);
                if (jump != null) jump.PlayTextAnimation(scaleMultiplier: 0.8f, totalDuration: 1f);
                break;
        }
    }

    private async UniTaskVoid TrackPopupDuration(float duration)
    {
        try
        {
            await UniTask.Delay((int)(duration * 1000), cancellationToken: _cts.Token);
            _activePopupsCount = Mathf.Max(0, _activePopupsCount - 1);
        }
        catch { }
    }

    /// <summary>
    /// Chờ cho toàn bộ hàng đợi popup hiển thị xong mượt mà trước khi kết thúc Turn
    /// </summary>
    public async UniTask WaitForAllPopupsAsync(CancellationToken token = default)
    {
        float startTime = Time.time;
        // Đợi hàng đợi xả hết
        while (true)
        {
            bool hasQueued = false;
            lock (_popupQueue)
            {
                hasQueued = _popupQueue.Count > 0;
            }

            if (!hasQueued && !_isProcessingQueue)
            {
                break;
            }

            // Timeout an toàn tối đa 2.5s tránh treo game
            if (Time.time - startTime > 2.5f)
            {
                break;
            }

            await UniTask.Yield(token);
        }

        // Đợi thêm một nhịp ngắn (250ms) để người chơi kịp nhìn các chữ bay lên hoàn tất
        if (_activePopupsCount > 0)
        {
            await UniTask.Delay(250, cancellationToken: token);
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
        UIEvent.DamagePopup -= CreateDamagePopup;
        UIEvent.HealPopup -= CreateHealPopup;
        UIEvent.TextPopup -= CreateTextPopup;
        Instance = null;
    }
}
