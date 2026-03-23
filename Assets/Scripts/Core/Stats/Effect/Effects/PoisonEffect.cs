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
        GameObject vfxPrefab = await AddressablesManager.Instance.LoadAssetAsync<GameObject>(poisonAddress);

        if (this.IsStop || Target == null || vfxPrefab == null)
        {
            return;
        }

        _posionVFX = GameObject.Instantiate(vfxPrefab, Target.gameObject.transform);
        _posionVFX.transform.localPosition = Vector3.zero;
    }


    public override void OnStartOfTurn() 
    {
        float poisonDamage = 0;

        // 1. Kiểm tra xem Độc này trừ máu Thẳng hay trừ theo % Máu Tối Đa
        if (_data.ModifyType == ModifyType.Percent)
        {
            // Nếu là Percent: Value 5 có nghĩa là mất 5% HP tối đa mỗi hiệp
            // (Giả sử bạn có hàm GetMaxHP hoặc thuộc tính MaxHP trong StatsController)
            float maxHP = Target.GetStat(StatType.HP).BaseValue; // Tùy vào code lấy MaxHP của bạn
            poisonDamage = maxHP * (_data.Value / 100f);
        }
        else
        {
            // Nếu là Constant: Trừ thẳng Value (VD: 50 máu)
            poisonDamage = _data.Value;
        }

        // 2. Nhân với số Stack hiện tại (VD: 2 lớp Độc thì đau gấp đôi)
        poisonDamage *= CurrentStack;

        // 3. Trừ máu mục tiêu! 
        // (Giả sử bạn có hàm TakeDamage. Nên truyền thêm Caster vào để game biết ai là hung thủ giết quái)
        if (Target is EntityStats entityStats)
        {
            entityStats.TakeDamage(poisonDamage, entityStats.gameObject.transform);
            UIEvent.DamagePopup(poisonDamage, entityStats.transform.position, false);
        }


        Debug.Log($"[Effect] {Target.EntityID} bị mất {poisonDamage} máu do ĐỘC! (Đang có {CurrentStack} Stack)");
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
