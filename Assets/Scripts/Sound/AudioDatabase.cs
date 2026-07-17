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

    public AudioDataConfig GetRandomSFX()
    {
        if (SFXList == null || SFXList.Count == 0) return null;
        int randomIndex = Random.Range(0, SFXList.Count);
        return SFXList[randomIndex];
    }
}
