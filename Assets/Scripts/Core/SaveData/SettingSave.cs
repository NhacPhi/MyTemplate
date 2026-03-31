using System;


[Serializable]
public class SettingSave 
{
    private int fps;
    private int musicVolune;
    private string currentLocalized;

    public int FPS
    {
        get { return fps; }
        set { fps = value; }
    }
    public int MusicVolune
    {
        get { return musicVolune; }
        set { musicVolune = value; }
    }

    public string CurrentLocalized
    {
        get { return currentLocalized; }
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
