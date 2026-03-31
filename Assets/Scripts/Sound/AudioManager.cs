using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Tech.Logger;

public interface IAudioManager
{
    UniTask PlaySFXAsync(string audioID, bool stopPrevious = false);
}

public class AudioManager : MonoBehaviour, IAudioManager
{
    [SerializeField] private AudioSource _sfxSource;

    private Dictionary<string, AudioDataConfig> _masterConfigMap = new Dictionary<string, AudioDataConfig>();
    private Dictionary<string, float> _lastPlayedTimeMap = new Dictionary<string, float>();
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
                    LogCommon.LogWarning($"[Audio] duplocated audio UUID: {config.AudioID}!");
                }
            }
        }
    }

    public async UniTask PlaySFXAsync(string audioID, bool stopPrevious = false)
    {


        if (!_masterConfigMap.TryGetValue(audioID, out var config))
        {
            LogCommon.LogWarning($"[Audio] Can't found config id: {audioID}");
            return;
        }
        
        AudioClip clipToPlay = await AddressablesManager.Instance.LoadAssetAsync<AudioClip>(config.ClipRef);

        if (clipToPlay != null)
        {
            if (stopPrevious)
            {
                _sfxSource.Stop();
                _sfxSource.clip = clipToPlay;
                _sfxSource.volume = config.Volume;
                _sfxSource.Play();
            }
            else
            {
                _sfxSource.PlayOneShot(clipToPlay, config.Volume);
            }
        }
    }
}
