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
        if (jump != null) jump.PlayAnimation();
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
        if (jump != null) jump.PlayAnimation();
    }

    public void CreateTextPopup(string text, Vector3 position)
    {
        if (popupPrefab == null) return;
        var clone = PoolManager.Instance.SpawnObject(popupPrefab, position, Quaternion.identity);
        clone.SetAnimationEnabled(false); // Tắt animation cho các hiệu ứng chữ/mất lượt
        clone.SetText(text);
        clone.SetCritical(false);
        // Đặt màu tím nhạt/xanh cyan cho hiệu ứng trạng thái
        clone.TMP.color = new Color(0.3f, 0.9f, 1f); 
    }
    public void Dispose()
    {
        UIEvent.DamagePopup -= CreateDamagePopup;
        UIEvent.HealPopup -= CreateHealPopup;
        UIEvent.TextPopup -= CreateTextPopup;
    }
}
