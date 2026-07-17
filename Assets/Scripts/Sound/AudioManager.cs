using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Tech.Logger;
using VContainer;

public interface IAudioManager
{
    UniTask PlaySFXAsync(string audioID, bool stopPrevious = false, bool loop = false);
    void StopSFX();
    void UpdateVolume(float volumeRatio);
    void ResetAudioSpeed();
}

public class AudioManager : MonoBehaviour, IAudioManager
{
    [Inject] private SaveSystem _saveSystem;

    [SerializeField] private AudioSource _sfxSource;
    [SerializeField] private AudioSource _musicSource;

    private Dictionary<string, AudioDatabase> _databases = new Dictionary<string, AudioDatabase>();
    private Dictionary<string, AudioDataConfig> _masterConfigMap = new Dictionary<string, AudioDataConfig>();
    private Dictionary<string, float> _lastPlayedTimeMap = new Dictionary<string, float>();

    private float _activeConfigVolume = 1f;

    private void Awake()
    {
        if (_musicSource == null)
        {
            _musicSource = gameObject.AddComponent<AudioSource>();
            _musicSource.playOnAwake = false;
        }
    }

    private void Update()
    {
        if (_sfxSource != null)
        {
            // Đồng bộ tốc độ âm thanh (pitch) với tốc độ game (Time.timeScale) khi fast forward
            // Nếu game pause (timeScale = 0), giữ pitch = 1 để âm thanh UI không bị rè hoặc tịt
            float pitch = Time.timeScale > 0f ? Time.timeScale : 1f;
            _sfxSource.pitch = pitch;
            if (_musicSource != null)
            {
                _musicSource.pitch = pitch;
            }
        }
    }

    public void ResetAudioSpeed()
    {
        // Hàm hỗ trợ để đảm bảo đưa tốc độ game và âm thanh về mức bình thường (x1)
        Time.timeScale = 1f;
        if (_sfxSource != null) _sfxSource.pitch = 1f;
        if (_musicSource != null) _musicSource.pitch = 1f;
    }

    public void Init(List<AudioDatabase> databases)
    {
        foreach (var db in databases)
        {
            if (db == null) continue;

            // Đăng ký database theo tên để có thể phát random
            _databases[db.name] = db;

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

    public async UniTask PlaySFXAsync(string audioID, bool stopPrevious = false, bool loop = false)
    {
        AudioDataConfig config = null;

        // Nếu ID truyền vào là tên của một Database (ví dụ: AudioDB_Environment), ta phát Random một âm thanh trong đó
        if (_databases.TryGetValue(audioID, out var db))
        {
            config = db.GetRandomSFX();
        }
        else if (!_masterConfigMap.TryGetValue(audioID, out config))
        {
            LogCommon.LogWarning($"[Audio] Can't found config id: {audioID}");
            return;
        }

        if (config == null)
        {
            LogCommon.LogWarning($"[Audio] Config is null or empty in database for: {audioID}");
            return;
        }
        
        AudioClip clipToPlay = await AddressablesManager.Instance.LoadAssetAsync<AudioClip>(config.ClipRef);

        if (clipToPlay != null)
        {
            // Chọn AudioSource tương ứng (loop -> _musicSource, sfx/voice -> _sfxSource)
            AudioSource activeSource = loop ? _musicSource : _sfxSource;

            if (activeSource == null) return;

            // Tránh ngắt quãng nếu nhạc lặp (nhạc nền/môi trường) đang chạy
            if (loop && activeSource.isPlaying && activeSource.loop && activeSource.clip != null)
            {
                return;
            }

            // Lấy tỉ lệ âm lượng từ SaveSystem (chỉ áp dụng cho nhạc lặp/môi trường)
            float volumeRatio = 1f;
            if (loop && _saveSystem != null && _saveSystem.Settings != null)
            {
                volumeRatio = _saveSystem.Settings.MusicVolune / 100f;
            }

            if (loop)
            {
                _activeConfigVolume = config.Volume;
            }

            activeSource.loop = loop;
            if (stopPrevious || loop)
            {
                activeSource.Stop();
                activeSource.clip = clipToPlay;
                activeSource.volume = config.Volume * volumeRatio;
                activeSource.Play();
            }
            else
            {
                activeSource.PlayOneShot(clipToPlay, config.Volume * volumeRatio);
            }
        }
    }

    public void StopSFX()
    {
        if (_musicSource != null)
        {
            _musicSource.Stop();
            _musicSource.clip = null;
            _musicSource.loop = false;
        }
    }

    public void UpdateVolume(float volumeRatio)
    {
        if (_musicSource != null)
        {
            _musicSource.volume = _activeConfigVolume * volumeRatio;
        }
    }
}
