using System.Collections.Generic;
using System.Linq;

public class SetBonusEvaluator 
{
    private readonly GameDataBase _gameData;

    public SetBonusEvaluator(GameDataBase gameData)
    {
        _gameData = gameData;
    }
    //Lấy danh sách các SetID và số lượng món tương ứng mà nhân vật đang mặc
    public Dictionary<string, int> GetEquippedSetCounts(EquipmentManager equipment)
    {
        var setCounts=  new Dictionary<string, int>();

        foreach(var kvp in equipment.EquippedItems)
        {
            if (kvp.Key == EquipSlot.Weapon) continue;

            var item = kvp.Value;

            string setID = item.BaseConfig.Armor?.ArmorSet;

            if(!string.IsNullOrEmpty(setID) )
            {
                if (!setCounts.ContainsKey(setID)) setCounts[setID] = 0;
                setCounts[setID]++;
            }
        }

        return setCounts;
    }

    //Trả về danh sách các Bonus đang được kích hoạt dựa trên trang hiên tại

    public List<SetBonusConfig> GetActiveSetBonuses(EquipmentManager equipment)
    {
        var  activeBonuses = new List<SetBonusConfig>();
        var setCounts = GetEquippedSetCounts(equipment);

        foreach (var setGroup in setCounts)
        {
            string setID = setGroup.Key;
            int count = setGroup.Value;

            // get config bonus of the set for database
            SetBonusConfig config = _gameData.GetSetBonusConfig(setID);

            if (config != null)
            {
                if (count >= config.Pieces)
                {
                    activeBonuses.Add(config);
                }
            }
        }

        return activeBonuses;
    }


}
