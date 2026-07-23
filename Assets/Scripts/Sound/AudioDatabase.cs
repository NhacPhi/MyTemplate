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

    public AudioDataConfig GetRandomSFX(AudioDataConfig excludeConfig = null)
    {
        if (SFXList == null || SFXList.Count == 0) return null;
        if (SFXList.Count == 1 || excludeConfig == null)
        {
            int randomIndex = Random.Range(0, SFXList.Count);
            return SFXList[randomIndex];
        }

        var candidates = SFXList.FindAll(x => x != null && x != excludeConfig);
        if (candidates.Count == 0)
        {
            return SFXList[Random.Range(0, SFXList.Count)];
        }
        return candidates[Random.Range(0, candidates.Count)];
    }
}
