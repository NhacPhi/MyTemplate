using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StunEffect : StatusEffect
{
    private EffectConfig _data;
    private string _effectID;

    private GameObject _stunEff;

    private const string stunAddress = "StunEff";

    // Constructor giống hệt StatBuffEffect
    public StunEffect(string effectID, EffectConfig data, StatsController target, StatsController caster = null)
        : base(target) // Nếu class base của bạn có nhận thêm Caster thì truyền thêm vào nhé
    {
        this._effectID = effectID;
        this._data = data;
    }

    protected override void OnStart()
    {
        // Khi bắt đầu dính choáng: 


        Debug.Log($"[Effect] {Target.EntityID} đã bị CHOÁNG (Stun) trong {_data.Duration} lượt!");
        SpawnVFXAsync().Forget();
        // Target.GetComponent<Entity>().StateManager.ChangeState(EntityState.STUNNED);
    }

    private async UniTaskVoid SpawnVFXAsync()
    {
        GameObject vfxPrefab = await AddressablesManager.Instance.LoadAssetAsync<GameObject>(stunAddress);

        if (this.IsStop || Target == null || vfxPrefab == null)
        {
            return;
        }

        _stunEff = GameObject.Instantiate(vfxPrefab, Target.gameObject.transform);
        _stunEff.transform.localPosition = Vector3.zero;
    }

    public override void AddStack(int currentTurnID)
    {
        base.AddStack(currentTurnID); // Hàm cha lo việc reset biến 'turn' về 0 để làm mới thời gian choáng

        // Choáng thường không có Stack (Ví dụ: Không có "Choáng x2"). 
        // Việc đập thêm 1 chiêu choáng vào đứa đang bị choáng chỉ đơn giản là reset lại thời gian (Refresh duration).
        Debug.Log($"[Effect] {Target.EntityID} bị bồi thêm CHOÁNG, làm mới lại thời gian chịu phạt!");
    }

    protected override void OnStop()
    {
        GameObject.Destroy(_stunEff);
        _stunEff = null;
        Debug.Log($"[Effect] {Target.EntityID} đã HẾT CHOÁNG, lấy lại ý thức!");
    }

    // --- Các property bắt buộc phải override ---
    public override EffectConfig Data => _data;

    public override string ID => _effectID;

    public override StatusEffect Clone()
    {
        return new StunEffect(this.ID, this._data, this.Target);
    }
}
