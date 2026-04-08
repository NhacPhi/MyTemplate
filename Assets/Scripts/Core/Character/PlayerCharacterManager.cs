using System.Collections;
using System.Collections.Generic;
using System.Linq;
using VContainer;
using VContainer.Unity;
using System;
using UnityEngine;

public class PlayerCharacterManager : IInitializable, IDisposable
{
    [Inject] SaveSystem _saveGame;
    [Inject] GameDataBase _gameDataBase;
    [Inject] InventoryManager _inventory;
    [Inject] CurrencyManager _currency;

    private Dictionary<string, CharacterProfileModel> _unlockedCharacters = new Dictionary<string, CharacterProfileModel>();

    public List<string> ActivePartyIDs { get; private set; } = new List<string>();

    public void Initialize()
    {
        
    }

    public void Dispose()
    {
        UIEvent.OnRequestSwapWeapon -= HandleSwapWeapon;
        UIEvent.OnRequestSwapArmor -= HandleSwapArmor;
    }
    public void Init()
    {
        _unlockedCharacters.Clear();

        if(_saveGame.Player.Roster.Characters != null)
        {
            foreach(var charSave in _saveGame.Player.Roster.Characters)
            {
                var profile = new CharacterProfileModel();
                profile.Init(charSave, _gameDataBase.GetCharacterConfig(charSave.ID), _gameDataBase, _inventory, _currency);
                _unlockedCharacters.Add(charSave.ID, profile);  
            }
        }

        if(_saveGame.Player.Roster.ActiveSlots != null)
        {
            ActivePartyIDs.Clear();
            foreach (var charSlot in _saveGame.Player.Roster.ActiveSlots)
            {
                ActivePartyIDs.Add(charSlot.CharacterID);
            }
        }

        UIEvent.OnRequestSwapWeapon += HandleSwapWeapon;
        UIEvent.OnRequestSwapArmor += HandleSwapArmor;
    }

    private void HandleSwapWeapon(string victimID, string weaponToGiveThem)
    {
        _unlockedCharacters.TryGetValue(victimID, out CharacterProfileModel victim);

        if (victim != null)
        {
            victim.UnEquipWeapon(victim.SaveData.Weapon);

            if(string.IsNullOrEmpty(victim.SaveData.Weapon))
            {
                victim.EquipWeapon(weaponToGiveThem);
            }
        }
        else
        {
            var victimSave = _saveGame.Player.Roster.GetCharacter(victimID);
            if(victimSave != null)
            {
                victimSave.Weapon = "";

                if(!string.IsNullOrEmpty(weaponToGiveThem))
                {
                    victimSave.Weapon = weaponToGiveThem;

                    var weaponSave = _inventory.GetWeapon(weaponToGiveThem);
                    if (weaponSave != null) weaponSave.Equip = victimID;
                }
            }
        }
    }

    private void HandleSwapArmor(string victimID, string stonlentArmorUUID, string armorToGiveThem)
    {
        _unlockedCharacters.TryGetValue(victimID, out CharacterProfileModel victim);

        if (victim != null)
        {
            victim.UnequipArmor(stonlentArmorUUID);

            if (!string.IsNullOrEmpty(armorToGiveThem))
            {
                victim.EquipArmor(armorToGiveThem);
            }
        }
  
    }

    public CharacterProfileModel GetCharacter(string characterID)
    {
        _unlockedCharacters.TryGetValue(characterID, out var profile);
        return profile;
    }

    public CharacterProfileModel GetFirstCharacter()
    {
        if (_unlockedCharacters == null || _unlockedCharacters.Count == 0)
        {
            return null;
        }

        return _unlockedCharacters.FirstOrDefault().Value;
    }

    public List<CharacterProfileModel> GetActiveParty()
    {
        return ActivePartyIDs
            .Select(id => GetCharacter(id))
            .Where(profile => profile != null)
            .ToList();
    }

    public int GetCharacterPower(string id)
    {
        _unlockedCharacters.TryGetValue(id, out CharacterProfileModel profile);

        if (profile == null) return 0;

        float totalPower = 0f;

        totalPower += profile.GetTotalStat(StatType.HP) * 0.2f;

        totalPower += profile.GetTotalStat(StatType.ATK) * 2.5f;

        totalPower += profile.GetTotalStat(StatType.DEF) * 1.5f;

        // totalPower += profile.GetTotalStat(StatType.CritRate) * 1000f;

        //if (profile.SaveData.SkillLevels != null)
        //{
        //    foreach (var skill in profile.SaveData.SkillLevels)
        //    {
        //        int skillLevel = skill.Value;
        //     
        //        totalPower += (skillLevel * 100);
        //    }
        //}

        return Mathf.RoundToInt(totalPower);
    }
}
