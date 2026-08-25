using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

public class BurnEffect : StatusEffect
{
    private EffectConfig _data;
    private string _effectID;
    private GameObject _burnVFX;

    private const string burnAddress = "BurnVFX";

    public BurnEffect(string effectID, EffectConfig data, StatsController target, StatsController caster = null)
        : base(target)
    {
        this._effectID = effectID;
        this._data = data;
    }

    protected override void OnStart()
    {
        // Khi có BurnVFX sau này có thể bật SpawnVFXAsync()
    }

    public override void OnStartOfTurn()
    {
        try
        {
            float burnDamage = 0f;

            // 1. Tính toán sát thương Thiêu Đốt theo % Máu Tối Đa (Mặc định 8% Max HP mỗi stack)
            if (_data != null && _data.ModifyType == ModifyType.Percent)
            {
                var hpStat = Target != null ? Target.GetStat(StatType.HP) : null;
                float maxHP = (hpStat != null && hpStat.Value > 0)
                    ? hpStat.Value
                    : ((hpStat != null && hpStat.BaseValue > 0) ? hpStat.BaseValue : 1000f);
                burnDamage = maxHP * (_data.Value / 100f);
            }
            else if (_data != null)
            {
                burnDamage = _data.Value;
            }
            else
            {
                burnDamage = 50f;
            }

            // 2. Nhân với số Stack hiện tại
            burnDamage *= Mathf.Max(1, CurrentStack);

            // 3. Trừ máu mục tiêu (DoT Burn) và hiển thị Damage Popup
            if (Target is EntityStats entityStats && entityStats != null && !entityStats.IsDead)
            {
                var dotTags = new HashSet<string> { "DoT", "Burn", "Fire" };
                entityStats.TakeDamage(burnDamage, entityStats.gameObject.transform, dotTags);
                UIEvent.DamagePopup?.Invoke(burnDamage, entityStats.transform.position, false);
            }

            Debug.Log($"[BurnEffect] {Target?.EntityID} bị thiêu đốt mất {burnDamage} máu! (Stack: {CurrentStack})");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[BurnEffect] Lỗi khi xử lý sát thương thiêu đốt: {ex}");
        }
    }

    public override void AddStack(int currentTurnID)
    {
        base.AddStack(currentTurnID);
        Debug.Log($"[BurnEffect] {Target.EntityID} bị tích thêm tầng Thiêu Đốt! Tầng hiện tại: {CurrentStack}");
    }

    protected override void OnStop()
    {
        if (_burnVFX != null)
        {
            GameObject.Destroy(_burnVFX);
            _burnVFX = null;
        }
        Debug.Log($"[BurnEffect] {Target?.EntityID} đã hết thời gian bị Thiêu Đốt.");
    }

    public override EffectConfig Data => _data;
    public override string ID => _effectID;

    public override StatusEffect Clone()
    {
        return new BurnEffect(this.ID, this._data, this.Target);
    }
}
