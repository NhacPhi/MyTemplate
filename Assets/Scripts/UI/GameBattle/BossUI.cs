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

    [SerializeField] Slider _bossHP;

    [Inject] private GameDataBase _gameData;

    private void OnEnable()
    {
        UIEvent.OnUpdateBossUI += UpdateSkillBossUI;
    }

    private void OnDisable()
    {
        UIEvent.OnUpdateBossUI -= UpdateSkillBossUI;
    }

    public void UpdateSkillBossUI(Entity boss)
    {
        var characterConfig = _gameData.GetCharacterConfig(boss.GetEntityID());

        var baseSkill = characterConfig.BaseSkillIcon;
        var majorSkill = characterConfig.MajorSkillIcon;
        var ultimateSKill = characterConfig.UltimateSkillIcon;

        _baseSkill.SetIconSkill(baseSkill);
        _majorSkill.SetIconSkill(majorSkill);
        _ultimateSkill.SetIconSkill(ultimateSKill);

        var skillConfig = characterConfig.Skills;

        foreach (var kvp in skillConfig)
        {
            SkillCharacter type = kvp.Key;
            SkillComponent data = kvp.Value;

            int currentCD = boss.GetCoreComponent<EntitySkill>().GetCurrentCooldown(type);

            switch (type)
            {
                case SkillCharacter.Base:
                    _baseSkill.UpdateSkillUI(data, currentCD);
                    break;
                case SkillCharacter.Major:
                    _majorSkill.UpdateSkillUI(data, currentCD);
                    break;
                case SkillCharacter.Ultimate:
                    _ultimateSkill.UpdateSkillUI(data, currentCD);
                    break;
            }
        }

        _txtBossName.text = LocalizationManager.Instance.GetLocalizedValue(characterConfig.Name);

        var characterHp = boss.GetCoreComponent<EntityStats>().GetAttribute(AttributeType.Hp);

        _bossHP.minValue = 0;
        _bossHP.maxValue = characterHp.MaxValue;

        _bossHP.value = characterHp.Value;

    }
}
