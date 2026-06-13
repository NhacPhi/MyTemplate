using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VContainer;

public class GachaBannerPanel : MonoBehaviour
{
    [SerializeField] private string bannerId;
    public string BannerId => bannerId;

    [Header("Action Buttons")]
    [SerializeField] private Button btnRoll1x;
    [SerializeField] private Button btnRollMulti;
    [SerializeField] private int multiRollCount = 10;
    
    [Header("Target UI")]
    [SerializeField] private GachaTargetSlotUI targetSlotUI;

    [Header("Pity Note")]
    [SerializeField] private TextMeshProUGUI txtPityNote;

    // Sự kiện gửi yêu cầu Roll lên GachaMainScene: (bannerId, rollCount)
    public Action<string, int> OnRequestRoll;

    public void Setup(string id)
    {
        this.bannerId = id;
    }

    public void UpdatePityNote(int remainingPity)
    {
        if (txtPityNote != null)
        {
            string formatStr = LocalizationManager.Instance.GetLocalizedValue("UI_PITY_NOTE");
            if (string.IsNullOrEmpty(formatStr))
            {
                formatStr = "Trong vòng {0} lần chắc chắn nhận được [SSR]";
            }

            try 
            {
                txtPityNote.text = string.Format(formatStr, remainingPity);
            }
            catch 
            {
                txtPityNote.text = $"Trong vòng {remainingPity} lần chắc chắn nhận được [SSR]";
            }
        }
    }

    private void Start()
    {
        if (btnRoll1x != null) btnRoll1x.onClick.AddListener(() => RequestRoll(1));
        if (btnRollMulti != null) btnRollMulti.onClick.AddListener(() => RequestRoll(multiRollCount));
    }

    private void RequestRoll(int count)
    {
        OnRequestRoll?.Invoke(bannerId, count);
    }
    
    /// <summary>
    /// Làm mới UI của ô chọn mục tiêu
    /// </summary>
    public void RefreshTargetSlot(string targetItemId, GameDataBase db)
    {
        if (targetSlotUI == null) return;

        if (string.IsNullOrEmpty(targetItemId))
        {
            targetSlotUI.SetupEmpty(bannerId);
        }
        else
        {
            var itemConfig = db.GetItemConfig(targetItemId);
            if (itemConfig != null)
            {
                targetSlotUI.SetupFilled(bannerId, targetItemId, itemConfig.Rarity, itemConfig.Icon, itemConfig.IconBG);
            }
        }
    }
    
    public GachaTargetSlotUI GetTargetSlotUI()
    {
        return targetSlotUI;
    }
}
