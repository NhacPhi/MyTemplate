using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UIFramework;
using VContainer;

public class BattleViewScene : WindowController
{
    [Inject] private UIManager _uiManager;
    [Inject] private GameDataBase _gameData;
    [Inject] private BattleSessionContext _battleSessionContext;

    [Header("Battle Info")]
    [SerializeField] private TextMeshProUGUI _txtBattleName;
    [SerializeField] private Image _imgBattleContent;
    [SerializeField] private TextMeshProUGUI _txtBattleDes;
    [SerializeField] private RectTransform _desContent;

    [Header("Lists & Prefabs")]
    [SerializeField] private Transform _enemyContainer;
    [SerializeField] private EnemyIconUI _enemyIconPrefab;
    [SerializeField] private Transform _rewardContainer;
    [SerializeField] private GameItemUI _rewardItemPrefab;

    [Header("Scroll Views")]
    [SerializeField] private ScrollRect _scrollRect;

    [Header("Buttons")]
    [SerializeField] private Button _btnPrepareBattle;
    [SerializeField] private Button _btnClose;

    private readonly List<GameObject> _instantiatedItems = new List<GameObject>();

    private void OnEnable()
    {
        Time.timeScale = 0f;
        UIEvent.OnPrepareBattleData += InitData;
        InitData();
    }

    private void OnDisable()
    {
        Time.timeScale = 1f;
        UIEvent.OnPrepareBattleData -= InitData;
        CleanData();
    }

    private void Start()
    {
        if (_btnPrepareBattle != null)
        {
            _btnPrepareBattle.onClick.AddListener(OnPrepareBattleClicked);
        }

        if (_btnClose != null)
        {
            _btnClose.onClick.AddListener(OnCloseClicked);
        }
    }

    public void InitData()
    {
        CleanData();

        if (_gameData == null || _battleSessionContext == null || string.IsNullOrEmpty(_battleSessionContext.PendingBattleID))
        {
            return;
        }

        string battleId = _battleSessionContext.PendingBattleID;
        var battleConfig = _gameData.GetBattleConfig(battleId);
        if (battleConfig == null)
        {
            Debug.LogError($"[BattleViewScene] No battle config found for ID '{battleId}'.");
            return;
        }

        // 1. Battle Name
        if (_txtBattleName != null)
        {
            _txtBattleName.text = LocalizationManager.Instance != null 
                ? LocalizationManager.Instance.GetLocalizedValue(battleConfig.Name) 
                : battleConfig.Name.ToString();
        }

        // 2. Battle Sprite Content
        if (_imgBattleContent != null)
        {
            Sprite battleSprite = _gameData.GetBattleSprite(battleId);
            if (battleSprite != null)
            {
                _imgBattleContent.sprite = battleSprite;
                _imgBattleContent.gameObject.SetActive(true);
            }
            else
            {
                _imgBattleContent.gameObject.SetActive(false);
            }
        }

        // 3. Battle Description
        if (_txtBattleDes != null)
        {
            _txtBattleDes.text = LocalizationManager.Instance != null 
                ? LocalizationManager.Instance.GetLocalizedValue(battleConfig.Description) 
                : battleConfig.Description.ToString();

            Canvas.ForceUpdateCanvases();

            // Ép chiều cao RectTransform của _txtBattleDes tự mở rộng theo độ dài chữ
            Vector2 desSize = _txtBattleDes.rectTransform.sizeDelta;
            desSize.y = _txtBattleDes.preferredHeight;
            _txtBattleDes.rectTransform.sizeDelta = desSize;

            LayoutRebuilder.ForceRebuildLayoutImmediate(_txtBattleDes.rectTransform);

            RectTransform contentRect = _desContent != null 
                ? _desContent 
                : _txtBattleDes.transform.parent as RectTransform;

            if (contentRect != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
            }
        }

        // 4. Enemy List (using EnemyIconUI, sorted by Rare and Level like CharacterScene)
        if (_enemyContainer != null && _enemyIconPrefab != null && battleConfig.Enemies != null)
        {
            var sortedEnemies = new List<StageEnemyComponent>(battleConfig.Enemies);
            sortedEnemies.Sort((a, b) =>
            {
                var configA = _gameData.GetCharacterConfig(a.EnemyID);
                var configB = _gameData.GetCharacterConfig(b.EnemyID);

                if (configA == null || configB == null) return 0;

                // 1. Rare (Giảm dần: B so với A)
                int rareComp = configB.Rare.CompareTo(configA.Rare);
                if (rareComp != 0) return rareComp;

                // 2. Level (Giảm dần)
                return b.EnemyLevel.CompareTo(a.EnemyLevel);
            });

            foreach (var enemy in sortedEnemies)
            {
                if (string.IsNullOrEmpty(enemy.EnemyID)) continue;

                var enemyConfig = _gameData.GetCharacterConfig(enemy.EnemyID);
                var itemConfig = _gameData.GetItemConfig(enemy.EnemyID);

                CharacterRare charRare = enemyConfig != null ? enemyConfig.Rare : CharacterRare.R;
                Rare itemRare = EnemyIconUI.ConvertToItemRare(charRare);

                Sprite icon = GetEnemyIcon(enemyConfig, itemConfig);
                Sprite bg = _gameData.GetBGItemByRare(itemRare);

                EnemyIconUI enemyIcon = Instantiate(_enemyIconPrefab, _enemyContainer);
                enemyIcon.Setup(enemy.EnemyID, charRare, icon, bg, enemy.EnemyLevel);
                _instantiatedItems.Add(enemyIcon.gameObject);
            }
        }

        // 5. Reward List (using GameItemUI)
        if (_rewardContainer != null && _rewardItemPrefab != null && !string.IsNullOrEmpty(battleConfig.Reward))
        {
            var rewardConfig = _gameData.GetRewardConfig(battleConfig.Reward);
            if (rewardConfig != null && rewardConfig.Rewards != null)
            {
                foreach (var reward in rewardConfig.Rewards)
                {
                    var rewardItemConfig = _gameData.GetItemConfig(reward.ItemID);
                    if (rewardItemConfig != null)
                    {
                        GameItemUI itemUI = Instantiate(_rewardItemPrefab, _rewardContainer);
                        itemUI.Setup(reward.ItemID, rewardItemConfig.Rarity, rewardItemConfig.Icon, rewardItemConfig.IconBG);
                        itemUI.SetAmount(reward.Amount);
                        if (itemUI is ItemUI customItemUI)
                        {
                            customItemUI.ActiveFragIcon(rewardItemConfig.Type == ItemType.Shard);
                        }
                        _instantiatedItems.Add(itemUI.gameObject);
                    }
                }
            }
        }

        ResetScrollView();
    }

    private void ResetScrollView()
    {
        Canvas.ForceUpdateCanvases();

        if (_scrollRect != null)
        {
            _scrollRect.velocity = Vector2.zero;
            _scrollRect.verticalNormalizedPosition = 1f;
            _scrollRect.horizontalNormalizedPosition = 0f;
        }
        else
        {
            var scrollRects = GetComponentsInChildren<ScrollRect>(true);
            foreach (var sr in scrollRects)
            {
                if (sr != null)
                {
                    sr.velocity = Vector2.zero;
                    sr.verticalNormalizedPosition = 1f;
                    sr.horizontalNormalizedPosition = 0f;
                }
            }
        }
    }

    private Sprite GetEnemyIcon(CharacterConfig characterConfig, ItemConfig itemConfig)
    {
        if (characterConfig != null && characterConfig.Icon != null) return characterConfig.Icon;
        if (itemConfig != null && itemConfig.Icon != null) return itemConfig.Icon;
        return null;
    }

    private void CleanData()
    {
        foreach (var item in _instantiatedItems)
        {
            if (item != null) Destroy(item);
        }
        _instantiatedItems.Clear();
    }

    public void OnPrepareBattleClicked()
    {
        if (_uiManager != null)
        {
            _uiManager.CloseWindowScene(ScreenIds.BattleViewScene);
            _uiManager.OpenWindowScene(ScreenIds.PrepareBattleScene);
            UIEvent.OnPrepareBattleData?.Invoke();
        }
    }

    public void OnCloseClicked()
    {
        if (_uiManager != null)
        {
            _uiManager.CloseWindowScene(ScreenIds.BattleViewScene);
            _uiManager.OpenWindowScene(ScreenIds.GamePlayScene);
        }
        else
        {
            UI_Close();
        }
    }
}
