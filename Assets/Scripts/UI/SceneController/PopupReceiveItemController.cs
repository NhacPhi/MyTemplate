using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UIFramework;
using VContainer;
using DG.Tweening;
using System.Linq;

public class RewardItemData
{
    public string ItemId;
    public int Amount;

    public RewardItemData(string itemId, int amount)
    {
        ItemId = itemId;
        Amount = amount;
    }
}

public class ReceiveItemProperties : WindowProperties
{
    public List<RewardItemData> Rewards;

    public ReceiveItemProperties(List<RewardItemData> rewards)
    {
        Rewards = rewards;
    }
}

public class PopupReceiveItemController : WindowController
{
    [SerializeField] private Button btnBackgroundClose; // Nút vô hình phủ toàn màn hình để bấm đóng
    [SerializeField] private Transform itemGridParent;
    [SerializeField] private GameObject gameItemPrefab;
    [SerializeField] private float animationDelay = 0.15f; // Delay between each item appearance
    [SerializeField] private float animationDuration = 0.3f; // Duration of slam animation

    [Inject] private GameDataBase gameDataBase;

    private List<GameObject> spawnedItems = new List<GameObject>();
    private Sequence animationSequence;

    private void Start()
    {
        if (btnBackgroundClose != null) btnBackgroundClose.onClick.AddListener(OnClose);
    }

    protected override void AddListeners()
    {
        base.AddListeners();
    }

    protected override void RemoveListeners()
    {
        base.RemoveListeners();
    }

    protected override void OnPropertiesSet()
    {
        base.OnPropertiesSet();
        
        // Stop any running animations
        if (animationSequence != null && animationSequence.IsActive())
        {
            animationSequence.Kill();
        }

        if (Properties is ReceiveItemProperties props)
        {
            DisplayRewards(props.Rewards);
        }
    }

    private void DisplayRewards(List<RewardItemData> rewards)
    {
        // Clear previous items
        foreach (var item in spawnedItems)
        {
            Destroy(item);
        }
        spawnedItems.Clear();

        if (rewards == null || rewards.Count == 0) return;

        animationSequence = DOTween.Sequence().SetUpdate(true);
        
        // Thời gian chờ ban đầu để khung Popup mở lên hoàn toàn (ví dụ: 0.4 giây)
        animationSequence.AppendInterval(0.4f);

        // Instantiate items and set up animation
        foreach (var reward in rewards)
        {
            var itemConfig = gameDataBase.GetItemConfig(reward.ItemId);
            if (itemConfig == null) continue;

            var obj = Instantiate(gameItemPrefab, itemGridParent);
            var itemUI = obj.GetComponent<GameItemUI>();

            if (itemUI != null)
            {
                itemUI.Setup(reward.ItemId, itemConfig.Rarity, itemConfig.Icon, itemConfig.IconBG);
                itemUI.SetAmount(reward.Amount);
                itemUI.CanClick = false; 
            }

            // Setup CanvasGroup for Alpha Fade
            CanvasGroup canvasGroup = obj.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = obj.AddComponent<CanvasGroup>();
            }

            // Initial state for Slam Animation: Large Scale, 0 Alpha
            obj.transform.localScale = Vector3.one * 3f;
            canvasGroup.alpha = 0f;

            // Animate using DOTween Sequence
            Sequence itemSeq = DOTween.Sequence().SetUpdate(true);
            
            // Fade in Alpha
            itemSeq.Join(canvasGroup.DOFade(1f, animationDuration * 0.8f));
            
            // Scale Slam (From 3 to 1) with Ease.OutBounce or Ease.InExpo
            itemSeq.Join(obj.transform.DOScale(Vector3.one, animationDuration).SetEase(Ease.OutBounce));

            // Append to main sequence with delay
            animationSequence.Append(itemSeq);
            animationSequence.AppendInterval(animationDelay);

            spawnedItems.Add(obj);
        }

        animationSequence.Play();
    }

    private void OnClose()
    {
        if (animationSequence != null && animationSequence.IsActive())
        {
            animationSequence.Complete();
            animationSequence.Kill();
        }
        UI_Close();
    }
}
