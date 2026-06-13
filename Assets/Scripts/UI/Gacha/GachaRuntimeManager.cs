using System.Collections.Generic;
using VContainer;

/// <summary>
/// Quản lý dữ liệu tiến trình quay của người chơi (Pity, Mục tiêu...).
/// Tích hợp với SaveSystem để lưu trữ vĩnh viễn.
/// </summary>
public class GachaRuntimeManager
{
    private readonly SaveSystem _saveSystem;

    [Inject]
    public GachaRuntimeManager(SaveSystem saveSystem)
    {
        _saveSystem = saveSystem;
    }

    private Dictionary<string, BannerRuntimeData> BannerDataMap => _saveSystem.Player.Gacha.BannerDataMap;

    public BannerRuntimeData GetRuntimeData(string bannerId)
    {
        if (!BannerDataMap.ContainsKey(bannerId))
        {
            BannerDataMap[bannerId] = new BannerRuntimeData 
            { 
                BannerId = bannerId, 
                PityCount = 0, 
                IsNextSSRGuaranteed = false,
                TotalSummonCount = 0,
                SelectedTargetId = string.Empty
            };
            SaveData();
        }
        return BannerDataMap[bannerId];
    }

    public void SetSelectedTarget(string bannerId, string targetId)
    {
        var data = GetRuntimeData(bannerId);
        data.SelectedTargetId = targetId;
        SaveData();
    }

    public void SaveData()
    {
        _saveSystem.SaveDataToDisk(GameSaveType.PlayerInfo);
    }
}
