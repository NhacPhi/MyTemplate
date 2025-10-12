using System;

public class SaveSystem
{
    private string _saveSettingsFileName = "settings.json";

    private SettingSave _settings;

    public SettingSave Settings => _settings;

    public void Init()
    {
        _settings = new SettingSave();
    }

    public bool LoadSaveDataFromDisk()
    {
        if (FileManager.LoadFromFile(_saveSettingsFileName, out _settings))
        {
            return true;
        }
        return false;
    }

    public void SaveDataToDisk()
    {
        FileManager.WriteToFile(_saveSettingsFileName, _settings);
    }
}
