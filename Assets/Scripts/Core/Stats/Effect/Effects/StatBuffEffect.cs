using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatBuffEffect : StatusEffect
{
    private EffectConfig _data;

    private Modifier _currentModifier;

    private StatType _targetStat;

    private string _effectID;

    public StatBuffEffect(string effectID, EffectConfig data, StatsController target, StatsController caster = null)
        : base(target)
    {
        this._effectID = effectID;
        this._data = data;

        _targetStat = _data.TargetStat;
    }

    protected override void OnStart()
    {
        if (_targetStat == StatType.None) return;

        // Tính toán lượng buff thực tế (Giá trị gốc * số Stack hiện tại)
        float buffValue = _data.Value * CurrentStack;

        // Tạo cục Modifier mới
        _currentModifier = new Modifier(buffValue, Data.ModifyType);

        // Gọi hàm của bạn để đắp chỉ số vào nhân vật
        Target.AddModifier(_targetStat, _currentModifier);

        Debug.Log($"[Effect] Bắt đầu BUFF {buffValue} {_targetStat} cho {Target.EntityID}");
    }

    public override void AddStack()
    {
        base.AddStack(); // Hàm cha sẽ lo việc tăng biến CurrentStack và reset Turn về 0

        if (_targetStat == StatType.None || _currentModifier == null) return;

        // 1. Phải GỠ cục buff cũ ra trước (Nếu không nhân vật sẽ được cộng cả cũ lẫn mới thành buff kép)
        Target.RemoveModifier(_targetStat, _currentModifier);

        // 2. Tính lại giá trị buff mới mạnh hơn
        float newBuffValue = _data.Value * CurrentStack;
        _currentModifier = new Modifier(newBuffValue, Data.ModifyType);

        // 3. Đắp cục buff mới vào lại
        Target.AddModifier(_targetStat, _currentModifier);

        Debug.Log($"[Effect] Tăng Stack BUFF lên {CurrentStack}. Tổng cộng đang buff: {newBuffValue} {_targetStat}");
    }

    protected override void OnStop()
    {
        // Khi hết thời gian, BẮT BUỘC phải gỡ Modifier để trả lại chỉ số gốc!
        if (_targetStat != StatType.None && _currentModifier != null)
        {
            Target.RemoveModifier(_targetStat, _currentModifier);
            Debug.Log($"[Effect] Đã HẾT HẠN, thu hồi buff {_targetStat} của {Target.EntityID}");
        }
    }

    public override EffectConfig Data => _data;

    public override string ID => _effectID;

    public override StatusEffect Clone()
    {
        return new StatBuffEffect(this.ID, this._data, this.Target);
    }
}
