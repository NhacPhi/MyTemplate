using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using VContainer.Unity;
using Tech.Pool;
using UnityEngine.AddressableAssets;
public class CombatText : IInitializable, IDisposable
{
    public const string Address = "Combat Text";
    private CombatTextUI popupPrefab;
    public void Initialize()
    {
        UIEvent.DamagePopup += CreateDamagePopup;
        UIEvent.HealPopup += CreateHealPopup;
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
        AddressablesManager.Instance.RemoveAsset(Address);
    }
    public void CreateDamagePopup(float damage, Vector3 position, bool isCris)
    {
        var clone = PoolManager.Instance.SpawnObject(popupPrefab, position, Quaternion.identity);
        clone.SetValue(damage);
        if(isCris)
            clone.TMP.color = Color.yellow;
        else
            clone.TMP.color = Color.white;
    }

    public void CreateHealPopup(float heal, Vector3 position)
    {
        if (heal < 0) return;

        var clone = PoolManager.Instance.SpawnObject(popupPrefab, position, Quaternion.identity);
        clone.SetValue(heal);
        clone.TMP.color = Color.green;
    }
    public void Dispose()
    {
        UIEvent.DamagePopup -= CreateDamagePopup;
        UIEvent.HealPopup -= CreateHealPopup;
    }
}
