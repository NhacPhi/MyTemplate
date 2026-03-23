using System;
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
        this._targetStat = _data.TargetStat;
    }

    protected override void OnStart()
    {
        // Khi Effect bắt đầu, gọi hàm tính toán đắp Modifier vào
        UpdateModifier();
        Debug.Log($"[Effect] Bắt đầu BUFF {_data.Value * CurrentStack} {_targetStat} cho {Target.name}");
    }

    public override void AddStack(int currentTurnID)
    {
        base.AddStack(currentTurnID); // Hàm cha tăng CurrentStack và reset Turn

        // Cập nhật lại Modifier sau khi tăng stack
        UpdateModifier();
        Debug.Log($"[Effect] Tăng Stack lên {CurrentStack}. Tổng buff: {_data.Value * CurrentStack} {_targetStat}");
    }

    // 🌟 ĐÂY LÀ PHẦN BẠN CÒN THIẾU: Xử lý khi bị rớt Stack 🌟
    public override int RemoveStack()
    {
        int remain = base.RemoveStack(); // Hàm cha giảm CurrentStack xuống

        // Cập nhật lại Modifier sau khi rớt stack (chỉ số sẽ giảm xuống)
        UpdateModifier();

        return remain;
    }

    protected override void OnStop()
    {
        // Gỡ sạch sẽ Modifier khi Effect hết hạn hoàn toàn
        if (_targetStat != StatType.None && _currentModifier != null)
        {
            Target.RemoveModifier(_targetStat, _currentModifier);
            _currentModifier = null;
            Debug.Log($"[Effect] Đã HẾT HẠN, thu hồi toàn bộ buff {_targetStat} của {Target.name}");
        }
    }

    // --- HÀM TỐI ƯU: Gom chung logic xử lý Modifier ---
    private void UpdateModifier()
    {
        if (_targetStat == StatType.None) return;

        // 1. Nếu trên người đang có cục buff cũ -> Gỡ nó ra trước
        if (_currentModifier != null)
        {
            Target.RemoveModifier(_targetStat, _currentModifier);
            _currentModifier = null;
        }

        // 2. Nếu số Stack > 0, tạo và đắp cục buff mới (Trường hợp RemoveStack về 0 sẽ không đắp nữa)
        if (CurrentStack > 0)
        {
            float newBuffValue = _data.Value * CurrentStack;
            _currentModifier = new Modifier(newBuffValue, Data.ModifyType);

            Target.AddModifier(_targetStat, _currentModifier);
        }
    }

    public override EffectConfig Data => _data;
    public override string ID => _effectID;

    public override StatusEffect Clone()
    {
        return new StatBuffEffect(this.ID, this._data, this.Target);
    }
}