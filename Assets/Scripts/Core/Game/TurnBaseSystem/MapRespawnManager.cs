using System;
using VContainer.Unity;

public class MapRespawnManager : IInitializable, IDisposable
{
    private readonly SaveSystem _saveSystem;

    public MapRespawnManager(SaveSystem saveSystem)
    {
        _saveSystem = saveSystem;
    }

    public void Initialize()
    {
        GameEvent.OnCheckRespawnStatus += CheckRespawnStatus;
        GameEvent.OnRecordResourceDestroyed += RecordResourceDestroyed;
    }

    public void Dispose()
    {
        GameEvent.OnCheckRespawnStatus -= CheckRespawnStatus;
        GameEvent.OnRecordResourceDestroyed -= RecordResourceDestroyed;
    }

    private bool CheckRespawnStatus(string resourceID, RespawnType type, float cooldownMinutes)
    {
        if (_saveSystem == null || _saveSystem.Player == null || _saveSystem.Player.WorldState == null)
            return true; // Nếu chưa có save data thì mặc định sinh ra

        return _saveSystem.Player.WorldState.HasRespawned(resourceID, type, cooldownMinutes);
    }

    private void RecordResourceDestroyed(string resourceID)
    {
        if (_saveSystem == null || _saveSystem.Player == null || _saveSystem.Player.WorldState == null)
            return;

        _saveSystem.Player.WorldState.RecordDestroyed(resourceID);
        // Lưu ngay để tránh mất dữ liệu nếu crash, hoặc có thể đợi AutoSave
        _saveSystem.SaveDataToDisk(GameSaveType.PlayerInfo);
    }
}
