using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class UIEvent
{
    public static Action OnLanguageChanged;
    public static Action<string> OnClickNavigationButton;
    public static Action<string> OnChanageAvatarPopup;
    public static Action<string> OnChanageAvatarPanel;

    //Inventory
    public static Action<ItemType> OnSelectToggleInventoryTap;
    public static Action<string> OnSelectInventoryItem;

    //Character
    public static Action<string> OnSelectCharacterAvatar;
    public static Action<CharacterTap> OnSelectToggleCharacterTap;
    public static Action<string> OnSelectCharacterArmorUI;
    public static Action<string> OnSelectCharacterChangeWeapon;
    public static Action<string> OnSelectCharacterChangeArmor;

    //weapon
    public static Action<bool> OnCloseCharacterWeapon;
    public static Action<string> OnSelectWeaponCard;

    public static Action<bool> OnSlectectRelicTap;
    public static Action<WeaponTap> OnSelectToggleWeaponTap;
    public static Action<string> OnSlelectWeaponEnchance;

    // Armor
    public static Action<ArmorPart> OnShowCharacterCategoryArmor;
    public static Action<ArmorPart> OnUpdateCharacterCategoryArmor;
    public static Action<ArmorPart> OnClickArmorIconCatergory;
    public static Action OnCloseCharacterCategoryArmor;
    public static Action<string> OnClickArmorCategoryUI;

    public static Action<string> OnUpdateArmorTooltipUI;

    //Tooltop
    public static Action<bool> OnShowTooltipUI;
    public static Action OnHideAllToolTipUI;

    // GamePlay
    public static Action<bool, InteractionType> OnInterationUI;

    // Map
    public static Action<LocationID> OnOpenMapUIWithCurrentLocation;
    public static Action<GameSceneSO, Sprite> OnSelectToggleMap;
    public static Action<bool> OnToggleLoadingScene;
    public static Action OnPrepareBattleData;

    public static Action<CurrencyType, int> OnCurrencyChanged;

    // TextPopup
    public static Action<float, Vector3, bool> DamagePopup;
    public static Action<float, Vector3> HealPopup;

    // Battle
    public static Action<SkillCharacter> OnChooseSkillCharacter;
    public static Action<Entity> OnChooseTargetEnemy;
    public static Action OnExecuteSkill;
    public static Action<Entity> OnUpdateSkillCharacterUI;
    public static Action<List<Entity>> OnUpdateEntityPrediction;
    public static Action<bool> OnSwithActiveSkilCharacter;
    public static Action<BattleResult> OnShowBattleResultUI;
}
