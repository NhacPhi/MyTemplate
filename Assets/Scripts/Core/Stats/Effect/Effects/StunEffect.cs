using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StunEffect : StatusEffect
{
    private EffectConfig _data;
    private string _effectID;

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
        // Không cần đụng vào Stats. Thường ở đây ta sẽ bật Animation choáng, hoặc hiện Icon lốc xoáy trên đầu.
        Debug.Log($"[Effect] {Target.EntityID} đã bị CHOÁNG (Stun) trong {_data.Duration} lượt!");

        // Ví dụ nếu bạn có State Manager, bạn có thể ép nó chuyển state luôn:
        // Target.GetComponent<Entity>().StateManager.ChangeState(EntityState.STUNNED);
    }

    public override void AddStack()
    {
        base.AddStack(); // Hàm cha lo việc reset biến 'turn' về 0 để làm mới thời gian choáng

        // Choáng thường không có Stack (Ví dụ: Không có "Choáng x2"). 
        // Việc đập thêm 1 chiêu choáng vào đứa đang bị choáng chỉ đơn giản là reset lại thời gian (Refresh duration).
        Debug.Log($"[Effect] {Target.EntityID} bị bồi thêm CHOÁNG, làm mới lại thời gian chịu phạt!");
    }

    protected override void OnStop()
    {
        // Khi hết choáng:
        // Tắt Animation choáng, xóa Icon trên đầu.
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
