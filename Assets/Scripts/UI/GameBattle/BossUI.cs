using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using TMPro;

public class BossUI : MonoBehaviour
{
    [SerializeField] private SkillBossUI _baseSkill;

    [SerializeField] private SkillBossUI _majorSkill;

    [SerializeField] private SkillBossUI _ultimateSkill;

    [SerializeField] private TextMeshProUGUI _txtBossName;

    [SerializeField] private Image _bossAvatar;

    [SerializeField] Slider _bossHP;

    [Inject] private GameDataBase _gameData;

    private void Awake()
    {
        var tooltipUI = GetComponentInChildren<SkillTooltipUI>(true);
        if (tooltipUI == null)
        {
            tooltipUI = FindFirstObjectByType<SkillTooltipUI>(FindObjectsInactive.Include);
        }
        if (tooltipUI != null && !tooltipUI.gameObject.activeSelf)
        {
            tooltipUI.gameObject.SetActive(true);
            tooltipUI.Hide();
        }
    }

    private void Start()
    {
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.CheckBattleHasBosss();
            bool hasBoss = BattleManager.Instance.Boss != null;
            gameObject.SetActive(hasBoss);
            if (hasBoss)
            {
                UpdateSkillBossUI(BattleManager.Instance.Boss);
            }
        }
    }

    private void OnEnable()
    {
        UIEvent.OnUpdateBossUI += UpdateSkillBossUI;
        if (BattleManager.Instance != null && BattleManager.Instance.Boss != null)
        {
            UpdateSkillBossUI(BattleManager.Instance.Boss);
        }
    }

    private void OnDisable()
    {
        UIEvent.OnUpdateBossUI -= UpdateSkillBossUI;
    }

    public void UpdateSkillBossUI(Entity boss)
    {
        if (boss == null) return;
        var characterConfig = _gameData.GetCharacterConfig(boss.GetEntityID());
        if (characterConfig == null) return;

        var avatar = characterConfig.Icon;

        var baseSkill = characterConfig.BaseSkillIcon;
        var majorSkill = characterConfig.MajorSkillIcon;
        var ultimateSKill = characterConfig.UltimateSkillIcon;

        string bossID = boss.GetEntityID();

        if (_baseSkill != null)
        {
            _baseSkill.SetIconSkill(baseSkill);
            _baseSkill.SetCharacterID(bossID);
        }
        if (_majorSkill != null)
        {
            _majorSkill.SetIconSkill(majorSkill);
            _majorSkill.SetCharacterID(bossID);
        }
        if (_ultimateSkill != null)
        {
            _ultimateSkill.SetIconSkill(ultimateSKill);
            _ultimateSkill.SetCharacterID(bossID);
        }

        var skillConfig = characterConfig.Skills;

        if (skillConfig != null)
        {
            foreach (var kvp in skillConfig)
            {
                SkillCharacter type = kvp.Key;
                SkillComponent data = kvp.Value;

                var entitySkill = boss.GetCoreComponent<EntitySkill>();
                if (entitySkill == null) continue;

                int currentCD = entitySkill.GetCurrentCooldown(type);

                switch (type)
                {
                    case SkillCharacter.Base:
                        if (_baseSkill != null) _baseSkill.UpdateSkillUI(data, currentCD);
                        break;
                    case SkillCharacter.Major:
                        if (_majorSkill != null) _majorSkill.UpdateSkillUI(data, currentCD);
                        break;
                    case SkillCharacter.Ultimate:
                        if (_ultimateSkill != null) _ultimateSkill.UpdateSkillUI(data, currentCD);
                        break;
                }
            }
        }

        if (_txtBossName != null && characterConfig.Name != null)
            _txtBossName.text = LocalizationManager.Instance.GetLocalizedValue(characterConfig.Name);

        var entityStats = boss.GetCoreComponent<EntityStats>();
        if (entityStats != null && _bossHP != null)
        {
            var characterHp = entityStats.GetAttribute(AttributeType.Hp);
            if (characterHp != null)
            {
                _bossHP.minValue = 0;
                _bossHP.maxValue = characterHp.MaxValue;
                _bossHP.value = characterHp.Value;
            }
        }

        if (_bossAvatar != null)
            _bossAvatar.sprite = avatar;

    }
}
