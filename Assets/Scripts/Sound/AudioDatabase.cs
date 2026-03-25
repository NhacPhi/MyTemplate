using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioDatabase", menuName = "GameConfig/Audio DataBase")]
public class AudioDatabase : ScriptableObject
{
    public List<AudioDataConfig> SFXList;

    public AudioDataConfig GetSFXConfig(string id)
    {
        return SFXList.Find(x => x.AudioID == id);
    }
}
