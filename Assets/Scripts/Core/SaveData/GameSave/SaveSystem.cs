using System;

public enum GameSaveType
{
    GameSetting,
    PlayerInfo,
    All
}

public class SaveSystem
{
    public static SaveSystem Instance { get; private set; }

    private string saveSettingsFileName = "settings.json";
    private string savePlayerFileName = "Player.json";

    private SettingSave settings;
    private PlayerSave player;

    public SettingSave Settings => settings ?? (settings = new SettingSave());
    public PlayerSave Player => player ?? (player = new PlayerSave());

    public void Init()
    {
        Instance = this;
        settings = new SettingSave();
        player = new PlayerSave();
    }

    public void LoadSaveDataFromDisk()
    {
        FileManager.LoadFromFile(saveSettingsFileName, out settings);
        FileManager.LoadFromFile(savePlayerFileName, out player);
    }

    public void SaveDataToDisk(GameSaveType type)
    {
        switch (type)
        {
            case GameSaveType.GameSetting:
                FileManager.WriteToFile(saveSettingsFileName, settings);
                break;
            case GameSaveType.PlayerInfo:
                FileManager.WriteToFile(savePlayerFileName, player);
                break;
            default:
                FileManager.WriteToFile(saveSettingsFileName, settings);
                FileManager.WriteToFile(savePlayerFileName, player);
                break;
        }
    }
}
