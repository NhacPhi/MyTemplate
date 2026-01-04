using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Settings", menuName = "Settings/Create new Settings SO")]
public class SettingSO : ScriptableObject
{
    [SerializeField] private int _fps = default;
    [SerializeField] private int _musicVolume = default;
    [SerializeField] private string _currentLocale = default;

    public int FPS => _fps;
    public int MusicVolume => _musicVolume;
    public string CurrentLocale => _currentLocale;

    public SettingSO() { }

}
