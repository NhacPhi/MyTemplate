using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PoisonEffect : StatusEffect
{
    private EffectConfig _data;
    private string _effectID;
    private GameObject _posionVFX;

    private const string poisonAddress = "PoisonVFX";

    // Truyền thêm Caster (Người tung chiêu) cực kỳ quan trọng đối với Độc
    public PoisonEffect(string effectID, EffectConfig data, StatsController target, StatsController caster = null)
        : base(target)
    {
        this._effectID = effectID;
        this._data = data;
    }

    protected override void OnStart()
    {
        // Khi vừa dính độc (thường chưa mất máu ngay, chỉ bật hiệu ứng VFX)
        //Debug.Log($"[Effect] {Target.EntityID} bị NHIỄM ĐỘC từ chiêu của {(Caster != null ? Caster.EntityID : "Vô danh")}!");

        // Target.GetComponent<Entity>().PlayVFX("Poison_Bubbles");
        SpawnVFXAsync().Forget();
    }

    private async UniTaskVoid SpawnVFXAsync()
    {
        try
        {
            if (string.IsNullOrEmpty(poisonAddress) || AddressablesManager.Instance == null) return;

            GameObject vfxPrefab = await AddressablesManager.Instance.LoadAssetAsync<GameObject>(poisonAddress);

            if (this.IsStop || Target == null || vfxPrefab == null)
            {
                return;
            }

            _posionVFX = GameObject.Instantiate(vfxPrefab, Target.gameObject.transform);
            _posionVFX.transform.localPosition = Vector3.zero;
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[PoisonEffect] Không thể load VFX độc: {ex.Message}");
        }
    }


    public override void OnStartOfTurn() 
    {
        try
        {
            float poisonDamage = 0f;

            // 1. Kiểm tra xem Độc này trừ máu Thẳng hay trừ theo % Máu Tối Đa
            if (_data != null && _data.ModifyType == ModifyType.Percent)
            {
                var hpStat = Target != null ? Target.GetStat(StatType.HP) : null;
                float maxHP = (hpStat != null && hpStat.Value > 0) 
                    ? hpStat.Value 
                    : ((hpStat != null && hpStat.BaseValue > 0) ? hpStat.BaseValue : 1000f);
                poisonDamage = maxHP * (_data.Value / 100f);
            }
            else if (_data != null)
            {
                poisonDamage = _data.Value;
            }
            else
            {
                poisonDamage = 50f;
            }

            // 2. Nhân với số Stack hiện tại
            poisonDamage *= Mathf.Max(1, CurrentStack);

            // 3. Trừ máu mục tiêu (Gắn tag DoT, Poison để không làm ngắt animation hoặc làm gián đoạn State Machine)
            if (Target is EntityStats entityStats && entityStats != null && !entityStats.IsDead)
            {
                var dotTags = new System.Collections.Generic.HashSet<string> { "DoT", "Poison" };
                entityStats.TakeDamage(poisonDamage, entityStats.gameObject.transform, dotTags);
                UIEvent.DamagePopup?.Invoke(poisonDamage, entityStats.transform.position, false);
            }

            Debug.Log($"[Effect] {Target?.EntityID} bị mất {poisonDamage} máu do ĐỘC! (Đang có {CurrentStack} Stack)");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[PoisonEffect] Lỗi khi xử lý sát thương độc: {ex}");
        }
    }

    public override void OnEndOfTurn() 
    {

    }

    public override void AddStack(int currentTurnID)
    {
        base.AddStack(currentTurnID); // Reset lại thời gian (Turn = 0) và tăng CurrentStack lên 1

        Debug.Log($"[Effect] {Target.EntityID} bị tích thêm ĐỘC! Lên Stack: {CurrentStack}");
    }

    protected override void OnStop()
    {
        GameObject.Destroy(_posionVFX);
        _posionVFX = null;
        // Tắt hiệu ứng VFX bong bóng độc 
        Debug.Log($"[Effect] {Target.EntityID} đã hết thời gian NHIỄM ĐỘC.");
    }

    // --- Các property bắt buộc phải override ---
    public override EffectConfig Data => _data;

    public override string ID => _effectID;

    public override StatusEffect Clone()
    {
        return new PoisonEffect(this.ID, this._data, this.Target);
    }
}
