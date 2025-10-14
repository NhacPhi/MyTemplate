using System;

public class SaveSystem
{
    private string saveSettingsFileName = "settings.json";
    private string savePlayerFileName = "player.json";

    private SettingSave settings;
    private PlayerSave player;

    public SettingSave Settings => settings;
    public PlayerSave Player => player;

    public void Init()
    {
        settings = new SettingSave();
        player = new PlayerSave();
    }

    public void LoadSaveDataFromDisk()
    {
        FileManager.LoadFromFile(saveSettingsFileName, out settings);
        FileManager.LoadFromFile(savePlayerFileName, out player);
    }

    public void SaveDataToDisk()
    {
        FileManager.WriteToFile(saveSettingsFileName, settings);
        FileManager.WriteToFile(savePlayerFileName, player);
    }
}
