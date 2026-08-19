using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using VContainer.Unity;
using VContainer;
using Tech.Pool;
using UnityEngine.AddressableAssets;
public class CombatText : IInitializable, IDisposable
{
    [Inject] IObjectResolver _objectResolver;

    public const string Address = "Combat Text";
    private CombatTextUI popupPrefab;
    public void Initialize()
    {
        UIEvent.DamagePopup += CreateDamagePopup;
        UIEvent.HealPopup += CreateHealPopup;
        UIEvent.TextPopup += CreateTextPopup;
        _ = WaitLoading();
    }

    private async UniTaskVoid WaitLoading()
    {
        while(!AddressablesManager.Instance)
        {
            await UniTask.Yield();
        }

        var prefab = await Addressables.LoadAssetAsync<GameObject>(Address);
        if (prefab != null)
        {
            popupPrefab = prefab.GetComponent<CombatTextUI>();
        }

        //var clone = PoolManager.Instance.SpawnObject(popupPrefab, Vector3.zero, Quaternion.identity);
        //AddressablesManager.Instance.RemoveAsset(Address);
    }
    public void CreateDamagePopup(float damage, Vector3 position, bool isCris)
    {
        var clone = PoolManager.Instance.SpawnObject(popupPrefab, position, Quaternion.identity);
        clone.SetAnimationEnabled(true); // Bật animation cho sát thương
        clone.SetValue(damage);
        clone.SetCritical(isCris);
        if(isCris)
            clone.TMP.color = Color.yellow;
        else
            clone.TMP.color = Color.white;

        var jump = clone.GetComponent<NumberJumpAnimation>();
        if (jump != null) jump.PlayAnimation(isCris);
    }

    public void CreateHealPopup(float heal, Vector3 position)
    {
        if (heal < 0) return;

        var clone = PoolManager.Instance.SpawnObject(popupPrefab, position, Quaternion.identity);
        clone.SetAnimationEnabled(true); // Bật animation cho hồi máu
        clone.SetValue(heal);
        clone.SetCritical(false);
        clone.TMP.color = Color.green;

        var jump = clone.GetComponent<NumberJumpAnimation>();
        if (jump != null) jump.PlayAnimation(false);
    }

    public void CreateTextPopup(string text, Vector3 position)
    {
        if (popupPrefab == null) return;
        var clone = PoolManager.Instance.SpawnObject(popupPrefab, position, Quaternion.identity);
        clone.SetAnimationEnabled(true);
        clone.SetText(text);
        clone.SetCritical(false);

        // Đặt màu sắc nổi bật, sang trọng theo ngữ cảnh
        if (text.Contains("Phản Kích") || text.Contains("Counter"))
        {
            clone.TMP.color = new Color(1f, 0.85f, 0.2f); // Vàng kim rực rỡ
        }
        else if (text.Contains("Càn Quét") || text.Contains("Sweep"))
        {
            clone.TMP.color = new Color(1f, 0.55f, 0.1f); // Vàng cam phong hỏa
        }
        else if (text.Contains("Choáng") || text.Contains("Stun") || text.Contains("Băng") || text.Contains("Frozen"))
        {
            clone.TMP.color = new Color(0.4f, 0.9f, 1f); // Xanh băng tuyết
        }
        else if (text.Contains("Độc") || text.Contains("Poison"))
        {
            clone.TMP.color = new Color(0.6f, 1f, 0.35f); // Xanh ngọc độc
        }
        else
        {
            clone.TMP.color = new Color(0.35f, 0.85f, 1f); // Xanh Cyan tinh tế
        }

        var jump = clone.GetComponent<NumberJumpAnimation>();
        if (jump != null)
        {
            // Scale nhỏ lại (0.6f) để chữ không bị quá to như số sát thương, thời lượng 0.95s mượt mà
            jump.PlayTextAnimation(scaleMultiplier: 0.8f, totalDuration: 0.5f);
        }
    }
    public void Dispose()
    {
        UIEvent.DamagePopup -= CreateDamagePopup;
        UIEvent.HealPopup -= CreateHealPopup;
        UIEvent.TextPopup -= CreateTextPopup;
    }
}
