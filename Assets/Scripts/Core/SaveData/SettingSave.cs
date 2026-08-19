using System;


[Serializable]
public class SettingSave 
{
    private int fps = 60;
    private int musicVolune = 80;
    private string currentLocalized = "VIETNAMESE";

    public int FPS
    {
        get { return fps > 0 ? fps : 60; }
        set { fps = value; }
    }
    public int MusicVolune
    {
        get { return musicVolune >= 0 ? musicVolune : 80; }
        set { musicVolune = value; }
    }

    public string CurrentLocalized
    {
        get { return !string.IsNullOrEmpty(currentLocalized) ? currentLocalized : "VIETNAMESE"; }
        set { currentLocalized = value; }
    }

    public void SaveSetting(int fps, int musicVolume, string currentLocalizaed)
    {
        this.currentLocalized = currentLocalizaed;
        this.fps = fps;
        this.musicVolune = musicVolume;
    }

    public void SaveGraphicSettings(int fps)
    {
        this.fps = fps;
    }

    public void SaveMusicSettings(int volume)
    {
        this.musicVolune = volume;
    }

    public void SaveLanguageSettings(string localized)
    {
        this.currentLocalized = localized;
    }
}
