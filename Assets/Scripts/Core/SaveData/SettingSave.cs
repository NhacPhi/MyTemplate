using System;
using UnityEngine;

[Serializable]
public class SettingSave 
{
    private int fps = 60;
    private int musicVolune = 10;
    private string currentLocalized = "VIETNAMESE";

    public int FPS
    {
        get { return fps > 0 ? fps : 60; }
        set { fps = value; }
    }
    public int MusicVolune
    {
        get { return musicVolune >= 0 ? Mathf.Clamp(musicVolune, 0, 10) : 10; }
        set { musicVolune = Mathf.Clamp(value, 0, 10); }
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
        this.musicVolune = Mathf.Clamp(musicVolume, 0, 10);
    }

    public void SaveGraphicSettings(int fps)
    {
        this.fps = fps;
    }

    public void SaveMusicSettings(int volume)
    {
        this.musicVolune = Mathf.Clamp(volume, 0, 10);
    }

    public void SaveLanguageSettings(string localized)
    {
        this.currentLocalized = localized;
    }
}
