using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrozenEffect : StatusEffect
{
    private EffectConfig _data;
    private string _effectID;

    private GameObject _frozenEff;

    // Load prefab FrozenEff qua Addressables
    private const string frozenAddress = "FrozenEff";

    public FrozenEffect(string effectID, EffectConfig data, StatsController target, StatsController caster = null)
        : base(target)
    {
        this._effectID = effectID;
        this._data = data;
    }

    protected override void OnStart()
    {
        Debug.Log($"[Effect] {Target.EntityID} đã bị ĐÓNG BĂNG (Frozen) trong {_data.Duration} lượt!");
        SpawnVFXAsync().Forget();
    }

    private async UniTaskVoid SpawnVFXAsync()
    {
        GameObject vfxPrefab = await AddressablesManager.Instance.LoadAssetAsync<GameObject>(frozenAddress);

        if (this.IsStop || Target == null || vfxPrefab == null)
        {
            return;
        }

        _frozenEff = GameObject.Instantiate(vfxPrefab, Target.gameObject.transform);
        _frozenEff.transform.localPosition = Vector3.zero;
    }

    public override void AddStack(int currentTurnID)
    {
        base.AddStack(currentTurnID); 

        // Việc đập thêm 1 chiêu đóng băng vào đứa đang bị đóng băng chỉ đơn giản là reset lại thời gian (Refresh duration).
        Debug.Log($"[Effect] {Target.EntityID} bị bồi thêm ĐÓNG BĂNG, làm mới lại thời gian chịu phạt!");
    }

    protected override void OnStop()
    {
        if (_frozenEff != null)
        {
            GameObject.Destroy(_frozenEff);
            _frozenEff = null;
        }
        Debug.Log($"[Effect] {Target.EntityID} đã PHÁ BĂNG, lấy lại ý thức!");
    }

    // --- Các property bắt buộc phải override ---
    public override EffectConfig Data => _data;

    public override string ID => _effectID;

    public override StatusEffect Clone()
    {
        return new FrozenEffect(this.ID, this._data, this.Target);
    }
}
