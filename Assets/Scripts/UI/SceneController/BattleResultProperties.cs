using System;
using System.Collections.Generic;
using UIFramework;
using UnityEngine;

public class BattleResultProperties : WindowProperties
{
    public bool IsWin;
    public string MvpCharacterId;
    public int CurrentLevel;
    public int ExpBefore;
    public int ExpAdded;
    public int MaxExpForCurrentLevel;
    public List<CharacterResultData> Characters;
    public Action OnNextClicked;

    public BattleResultProperties(
        bool isWin, 
        string mvpCharacterId, 
        int currentLevel, 
        int expBefore, 
        int expAdded, 
        int maxExpForCurrentLevel, 
        List<CharacterResultData> characters, 
        Action onNextClicked)
    {
        IsWin = isWin;
        MvpCharacterId = mvpCharacterId;
        CurrentLevel = currentLevel;
        ExpBefore = expBefore;
        ExpAdded = expAdded;
        MaxExpForCurrentLevel = maxExpForCurrentLevel;
        Characters = characters;
        OnNextClicked = onNextClicked;
    }
}

public class CharacterResultData
{
    public string CharacterId;
    public int CurrentHp;
    public int MaxHp;
}
