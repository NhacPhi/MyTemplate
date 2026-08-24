using Cysharp.Threading.Tasks;
using UnityEngine;

public class SilenceEffect : StatusEffect
{
    private EffectConfig _data;
    private string _effectID;

    public SilenceEffect(string effectID, EffectConfig data, StatsController target, StatsController caster = null)
        : base(target)
    {
        this._effectID = effectID;
        this._data = data;
    }

    protected override void OnStart()
    {
        Debug.Log($"[Effect] {Target.EntityID} đã bị CÂM LẶNG (Silence) trong {_data.Duration} lượt - Không thể dùng Tuyệt kỹ / Kỹ năng!");
    }

    protected override void OnStop()
    {
        Debug.Log($"[Effect] {Target.EntityID} đã kết thúc trạng thái CÂM LẶNG!");
    }

    public override void AddStack(int currentTurnID)
    {
        base.AddStack(currentTurnID);
        Debug.Log($"[Effect] {Target.EntityID} bị bồi thêm CÂM LẶNG, làm mới thời gian!");
    }

    // --- Các property bắt buộc phải override ---
    public override EffectConfig Data => _data;

    public override string ID => _effectID;

    public override StatusEffect Clone()
    {
        return new SilenceEffect(this.ID, this._data, this.Target);
    }
}
