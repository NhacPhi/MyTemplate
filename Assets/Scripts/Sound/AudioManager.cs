using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Tech.Logger;

public interface IAudioManager
{
    UniTask PlaySFXAsync(string audioID, bool stopPrevious = false);
    void ResetAudioSpeed();
}

public class AudioManager : MonoBehaviour, IAudioManager
{
    [SerializeField] private AudioSource _sfxSource;

    private Dictionary<string, AudioDataConfig> _masterConfigMap = new Dictionary<string, AudioDataConfig>();
    private Dictionary<string, float> _lastPlayedTimeMap = new Dictionary<string, float>();

    private void Update()
    {
        if (_sfxSource != null)
        {
            // Đồng bộ tốc độ âm thanh (pitch) với tốc độ game (Time.timeScale) khi fast forward
            // Nếu game pause (timeScale = 0), giữ pitch = 1 để âm thanh UI không bị rè hoặc tịt
            _sfxSource.pitch = Time.timeScale > 0f ? Time.timeScale : 1f;
        }
    }

    public void ResetAudioSpeed()
    {
        // Hàm hỗ trợ để đảm bảo đưa tốc độ game và âm thanh về mức bình thường (x1)
        Time.timeScale = 1f;
        if (_sfxSource != null)
        {
            _sfxSource.pitch = 1f;
        }
    }

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
