using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillCharacterUIManager : MonoBehaviour
{
    [SerializeField] private ToggleSkillCharacterUI _baseSkill;

    [SerializeField] private ToggleSkillCharacterUI _majorSkill;

    [SerializeField] private ToggleSkillCharacterUI _ultimateSkill;
    private void OnEnable()
    {
        UIEvent.OnUpdateSkillCharacterUI += UpdateSkillCharacterUI;
    }

    private void OnDisable()
    {
        UIEvent.OnUpdateSkillCharacterUI -= UpdateSkillCharacterUI;
    }

    public void UpdateSkillCharacterUI(Sprite baseSkill, Sprite majorSkill, Sprite ultimateSkill)
    {
        _baseSkill.SetIconSkill(baseSkill);
        _majorSkill.SetIconSkill(majorSkill);
        _ultimateSkill.SetIconSkill(ultimateSkill);
    }
}
