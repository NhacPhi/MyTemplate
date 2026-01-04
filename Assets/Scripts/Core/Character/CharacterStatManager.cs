using System.Collections.Generic;
using System;
using VContainer;
public class CharacterStatManager 
{
    [Inject] GameDataBase gameDataBase;
    [Inject] SaveSystem save;

    //List<CharacterStatConfig> characterStats = new();

    public void Init()
    {
        foreach(var data in save.Player.Characters)
        {
            

            var characterConfig = gameDataBase.GetCharacterConfig(data.ID);

            if (characterConfig != null)
            {
                //characterConfig.ID = stat.ID;

                int weaponHP = 0;
                int weaponATK = 0;
                if (data.Weapon != "")
                {
                    //weaponHP = characterConfig.GetWeaponConfig(data.Weapon).HP;
                    //weaponATK = characterConfig.GetWeaponConfig(data.Weapon).ATK;
                }
                //    character.HP = GetStat(stat.HP, upgrade.GrowthHP, data.Level) + weaponHP;
                //    character.ATK = GetStat(stat.ATK, upgrade.GrowthATK, data.Level) + weaponATK;
                //    character.DEF = GetStat(stat.DEF, upgrade.GrowthDEF, data.Level);
                //    character.DEFShred = stat.DEFShred;
                //    character.SPD = stat.SPD;
                //    character.CRITDMG = stat.CRITDMG;
                //    character.CRITRate = stat.CRITRate;
                //    character.Penetration = stat.Penetration;
                //    character.CRITDMGRes = stat.CRITDMGRes;
                //    characterStats.Add(character);
                //}

            }
        }
    }

    //public CharacterStatConfig GetCharacterStat(string id)
    //{
    //    return characterStats.Find(character => character.ID == id);
    //}

    public int GetCharacterPower(string id)
    {
        //CharacterStatConfig character = GetCharacterStat(id);
        //float power = 0;
        //power = 1.5f * character.ATK + 2.2f * character.DEF + 1.2f * character.HP + 3 * character.SPD + 
        //    + character.DEFShred * 1.6f + 1.5f *  character.CRITDMG + character.CRITRate * 2f + character.Penetration * 1.3f + character.CRITDMGRes;
        //return Convert.ToInt32(power);
        return 0;
    }

    private float GetStat(float baseStat, float growth, int level)
    {
        return baseStat + Utility.GetStatGrowthLevel(level, growth);
    }
}
