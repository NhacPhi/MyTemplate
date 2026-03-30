using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using VContainer;
public class PlayerCharacterManager 
{
    [Inject] SaveSystem _saveGame;
    [Inject] GameDataBase _gameDataBase;
    [Inject] InventoryManager _inventory;

    private Dictionary<string, CharacterProfileModel> _unlockedCharacters = new Dictionary<string, CharacterProfileModel>();

    public List<string> ActivePartyIDs { get; private set; } = new List<string>();
    public void Init()
    {
        _unlockedCharacters.Clear();

        if(_saveGame.Player.Roster.Characters != null)
        {
            foreach(var charSave in _saveGame.Player.Roster.Characters)
            {
                var profile = new CharacterProfileModel();
                profile.Init(charSave, _gameDataBase.GetCharacterConfig(charSave.ID), _gameDataBase, _inventory);
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
}
