using System.Collections.Generic;
using TMPro;
using UIFramework;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using VContainer;

public class PopupBattleResultController : WindowController
{
    [Header("Result State UI")]
    [SerializeField] private GameObject winIcon;
    [SerializeField] private GameObject loseIcon;

    [Header("MVP Section")]
    [SerializeField] private Image mvpImage;

    [Header("EXP Section")]
    [SerializeField] private Slider expSlider;
    [SerializeField] private TextMeshProUGUI txtLevel;
    [SerializeField] private TextMeshProUGUI txtExpProgress;

    [Header("Characters Section")]
    [SerializeField] private Transform characterGrid;
    [SerializeField] private GameObject characterResultPrefab;

    [Header("Buttons")]
    [SerializeField] private Button btnNext;

    [Inject] private GameDataBase _gameDataBase;

    private BattleResultProperties _properties;
    private List<GameObject> _spawnedCharacters = new List<GameObject>();
    private Tween _expTween;

    protected override void AddListeners()
    {
        base.AddListeners();
        btnNext.onClick.AddListener(OnNextClicked);
    }

    protected override void RemoveListeners()
    {
        base.RemoveListeners();
        btnNext.onClick.RemoveListener(OnNextClicked);
    }

    protected override void OnPropertiesSet()
    {
        base.OnPropertiesSet();
        _properties = Properties as BattleResultProperties;
        if (_properties != null)
        {
            SetupUI();
        }
    }

    private void SetupUI()
    {
        // 1. Win/Lose Icon Animation
        if (winIcon != null) winIcon.SetActive(false);
        if (loseIcon != null) loseIcon.SetActive(false);

        GameObject activeIcon = _properties.IsWin ? winIcon : loseIcon;
        if (activeIcon != null)
        {
            activeIcon.SetActive(true);
            activeIcon.transform.localScale = Vector3.zero;
            activeIcon.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
        }

        // 2. MVP Image Animation
        if (mvpImage != null && !string.IsNullOrEmpty(_properties.MvpCharacterId))
        {
            var charConfig = _gameDataBase.GetCharacterConfig(_properties.MvpCharacterId);
            if (charConfig != null)
            {
                mvpImage.sprite = charConfig.BigIcon;
                mvpImage.gameObject.SetActive(true);

                mvpImage.transform.localScale = Vector3.zero;
                mvpImage.transform.DOScale(Vector3.one, 0.6f).SetEase(Ease.OutBack).SetDelay(0.3f);
            }
        }

        // 3. Clear old grid and spawn Character UI
        foreach (var child in _spawnedCharacters)
        {
            Destroy(child);
        }
        _spawnedCharacters.Clear();

        if (characterGrid != null && characterResultPrefab != null)
        {
            foreach (var charData in _properties.Characters)
            {
                var go = Instantiate(characterResultPrefab, characterGrid);
                var ui = go.GetComponent<CharacterUIResult>();
                if (ui != null)
                {
                    var itemConfig = _gameDataBase.GetItemConfig(charData.CharacterId);
                    if (itemConfig != null)
                    {
                        Sprite icon = itemConfig.Icon;
                        Sprite bg = _gameDataBase.GetBGItemByRare(itemConfig.Rarity);
                        // Mock level 1 & upgrade 0 for now since we just display battle status, or fetch from SaveSystem
                        ui.Init(charData.CharacterId, itemConfig.Rarity, icon, bg, 1, 0, charData.CurrentHp, charData.MaxHp);
                    }
                }
                _spawnedCharacters.Add(go);
            }
        }

        // 4. Animate EXP Bar
        if (txtLevel != null) txtLevel.text = "Lv. " + _properties.CurrentLevel;
        if (expSlider != null)
        {
            expSlider.maxValue = _properties.MaxExpForCurrentLevel;
            expSlider.value = _properties.ExpBefore;
            
            UpdateExpText(Mathf.RoundToInt(expSlider.value));

            // Animation
            _expTween?.Kill();
            int targetExp = _properties.ExpBefore + _properties.ExpAdded;
            
            // Check level up (nếu exp vượt quá maxValue, reset về 0)
            if (targetExp >= _properties.MaxExpForCurrentLevel)
            {
                // Phức tạp hơn thì làm 2 sequence tween, đơn giản thì cứ tăng rồi reset
                _expTween = expSlider.DOValue(_properties.MaxExpForCurrentLevel, 1f).OnUpdate(() =>
                {
                    UpdateExpText(Mathf.RoundToInt(expSlider.value));
                }).OnComplete(() =>
                {
                    txtLevel.text = "Lv. " + (_properties.CurrentLevel + 1);
                    expSlider.value = 0;
                    expSlider.maxValue = _properties.MaxExpForCurrentLevel; // Cần công thức update MaxEXP nếu lên cấp
                    expSlider.DOValue(targetExp - _properties.MaxExpForCurrentLevel, 0.5f).OnUpdate(() =>
                    {
                        UpdateExpText(Mathf.RoundToInt(expSlider.value));
                    });
                });
            }
            else
            {
                _expTween = expSlider.DOValue(targetExp, 1f).OnUpdate(() =>
                {
                    UpdateExpText(Mathf.RoundToInt(expSlider.value));
                });
            }
        }
    }

    private void UpdateExpText(int currentVal)
    {
        if (txtExpProgress != null)
        {
            txtExpProgress.text = $"{currentVal}/{_properties.MaxExpForCurrentLevel}";
        }
    }

    private void OnNextClicked()
    {
        _properties?.OnNextClicked?.Invoke();
    }
}
