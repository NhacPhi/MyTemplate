using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EffectControllUI : MonoBehaviour
{
    [SerializeField] private EffectUI _effectUIPrefab;
    [SerializeField] private GameObject _containt;

    [SerializeField] Sprite _iconAtk;
    [SerializeField] Sprite _iconDef;
    [SerializeField] Sprite _iconPoison;
    [SerializeField] Sprite _iconSpeed;
    [SerializeField] Sprite _iconExac;

    [SerializeField] Sprite _iconBuff;
    [SerializeField] Sprite _iconDebuff;

    Dictionary<string, EffectControllUI> _effects = new Dictionary<string, EffectControllUI>();

    private EntityStats _targetStats;

    private Dictionary<string, EffectUI> _activeUIEffects = new Dictionary<string, EffectUI>();

    private void Awake()
    {
        _targetStats = GetComponentInParent<EntityStats>();


        if (_targetStats != null)
        {
            _targetStats.OnEffectAdded += UpdateEffectUI;
            _targetStats.OnEffectUpdated += UpdateEffectUI; // Dùng chung hàm UpdateEffectUI cũng được
            _targetStats.OnEffectRemoved += RemoveEffectUI;

            // Cập nhật giao diện lần đầu (Lỡ như nhân vật có sẵn buff từ trước)
            RefreshAllUI();
        }
    }

    private void OnDestroy()
    {
        if (_targetStats != null)
        {
            _targetStats.OnEffectAdded -= UpdateEffectUI;
            _targetStats.OnEffectUpdated -= UpdateEffectUI;
            _targetStats.OnEffectRemoved -= RemoveEffectUI;
        }
    }

    private void RefreshAllUI()
    {
        ClearAll();
        if (_targetStats == null) return;

        foreach (var effect in _targetStats.StatusEffect)
        {
            UpdateEffectUI(effect);
        }
    }

    public void UpdateEffectUI(StatusEffect effect)
    {
        string id = effect.GetID();

        if(effect.Data.IsEffectUI())
        {
            // Nếu UI của Effect này đã tồn tại -> Chỉ việc cập nhật số
            if (_activeUIEffects.TryGetValue(id, out EffectUI uiItem))
            {
                uiItem.UpdateEffectUI(effect.Duration - effect.Turn);
            }
            else
            {
                // Nếu chưa tồn tại -> Sinh ra Prefab mới
                EffectUI newItem = Instantiate(_effectUIPrefab, _containt.gameObject.transform);

                // Tìm đúng cái hình Icon để gắn vào
                Sprite icon = GetIconByEffectID(effect);


                Sprite tick = effect.Data.IsBuff() ? _iconBuff : _iconDebuff;

                // Cài đặt thông số lần đầu
                newItem.Setup(icon, effect.Duration, tick);

                // Cất vào kho quản lý
                _activeUIEffects.Add(id, newItem);
            }
        }
    }

    // 2. Hàm gọi khi nhân vật BỊ XÓA Effect (Hết hạn hoặc bị thanh tẩy)
    public void RemoveEffectUI(StatusEffect effect)
    {
        string id = effect.GetID();

        if (_activeUIEffects.TryGetValue(id, out EffectUI uiItem))
        {
            // Phá hủy GameObject
            Destroy(uiItem.gameObject);

            // Xóa khỏi danh sách quản lý
            _activeUIEffects.Remove(id);
        }
    }

    // 3. Hàm gọi khi nhân vật chết hoặc qua màn (Làm sạch toàn bộ)
    public void ClearAll()
    {
        foreach (var item in _activeUIEffects.Values)
        {
            if (item != null) Destroy(item.gameObject);
        }
        _activeUIEffects.Clear();
    }

    // --- HÀM HỖ TRỢ LẤY ĐÚNG ICON ---
    private Sprite GetIconByEffectID(StatusEffect effect)
    {
        var data = effect.Data;

        switch (data.Type)
        {
            case EffectType.Poison:
                return _iconPoison;

            case EffectType.StatDebuff or EffectType.StatBuff:
                switch (data.TargetStat)
                {
                    case StatType.ATK: return _iconAtk;
                    case StatType.DEF: return _iconDef;
                    case StatType.SPEED: return _iconSpeed;
                }
                return null; 

            default:
                return null;
        }
    }
}
