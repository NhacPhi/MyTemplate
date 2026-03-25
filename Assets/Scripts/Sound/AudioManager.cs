using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Tech.Logger;

public interface IAudioManager
{
    UniTask PlaySFXAsync(string audioID);
}

public class AudioManager : MonoBehaviour, IAudioManager
{
    [SerializeField] private AudioSource _sfxSource;

    private Dictionary<string, AudioDataConfig> _masterConfigMap = new Dictionary<string, AudioDataConfig>();

    public void Init(List<AudioDatabase> databases)
    {
        foreach (var db in databases)
        {
            if (db == null) continue;

            foreach (var config in db.SFXList)
            {
                if (!_masterConfigMap.ContainsKey(config.AudioID))
                {
                    _masterConfigMap.Add(config.AudioID, config);
                }
                else
                {
                    LogCommon.LogWarning($"[Audio] duplocated audio ID: {config.AudioID}!");
                }
            }
        }
    }

    public async UniTask PlaySFXAsync(string audioID)
    {


        if (!_masterConfigMap.TryGetValue(audioID, out var config))
        {
            LogCommon.LogWarning($"[Audio] Can't found config id: {audioID}");
            return;
        }

        AudioClip clipToPlay = await AddressablesManager.Instance.LoadAssetAsync<AudioClip>(config.ClipRef);

        if (clipToPlay != null)
        {
            _sfxSource.PlayOneShot(clipToPlay, config.Volume);
        }
    }
}
