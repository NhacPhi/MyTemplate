using System.Collections;
using System.Collections.Generic;
using System.Threading;
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

    private CancellationTokenSource _bgmCts;
    private AudioDatabase _currentBgmDatabase;
    private AudioDataConfig _lastPlayedBgmConfig;

    private void Awake()
    {
        if (_musicSource == null)
        {
            _musicSource = gameObject.AddComponent<AudioSource>();
            _musicSource.playOnAwake = false;
        }
    }

    private void OnDestroy()
    {
        CancelBgmLoop();
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
                if (config == null) continue;
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
        // Nếu ID truyền vào là tên của một Database (ví dụ: AudioDB_Environment) và cần loop
        if (_databases.TryGetValue(audioID, out var db))
        {
            if (loop)
            {
                // Nếu cùng 1 database BGM đang chạy và không yêu cầu stopPrevious, không ngắt bài
                if (!stopPrevious && _currentBgmDatabase == db && _bgmCts != null && !_bgmCts.IsCancellationRequested && _musicSource != null && _musicSource.isPlaying)
                {
                    return;
                }

                CancelBgmLoop();
                _currentBgmDatabase = db;
                _bgmCts = new CancellationTokenSource();
                PlayBgmLoopTask(db, _bgmCts.Token).Forget();
                return;
            }
            else
            {
                AudioDataConfig randomConfig = db.GetRandomSFX();
                if (randomConfig != null)
                {
                    await PlaySingleClipAsync(randomConfig, stopPrevious, false);
                }
                return;
            }
        }

        if (!_masterConfigMap.TryGetValue(audioID, out var config) || config == null)
        {
            LogCommon.LogWarning($"[Audio] Can't found config id: {audioID}");
            return;
        }

        if (loop)
        {
            CancelBgmLoop();
        }

        await PlaySingleClipAsync(config, stopPrevious, loop);
    }

    private async UniTaskVoid PlayBgmLoopTask(AudioDatabase db, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            AudioDataConfig config = db.GetRandomSFX(_lastPlayedBgmConfig);
            if (config == null) break;

            _lastPlayedBgmConfig = config;

            AudioClip clipToPlay = await AddressablesManager.Instance.LoadAssetAsync<AudioClip>(config.ClipRef);
            if (cancellationToken.IsCancellationRequested) break;

            if (clipToPlay != null && _musicSource != null)
            {
                float volumeRatio = 1f;
                if (_saveSystem != null && _saveSystem.Settings != null)
                {
                    volumeRatio = _saveSystem.Settings.MusicVolune / 100f;
                }

                _activeConfigVolume = config.Volume;
                _musicSource.loop = false;
                _musicSource.Stop();
                _musicSource.clip = clipToPlay;
                _musicSource.volume = config.Volume * volumeRatio;
                _musicSource.Play();

                // Chờ cho đến khi bài hát phát xong hoặc có yêu cầu hủy
                bool canceled = await UniTask.WaitUntil(
                    () => _musicSource == null || !_musicSource.isPlaying || _musicSource.clip != clipToPlay,
                    PlayerLoopTiming.Update,
                    cancellationToken
                ).SuppressCancellationThrow();

                if (canceled || cancellationToken.IsCancellationRequested)
                {
                    break;
                }
            }
            else
            {
                bool canceled = await UniTask.Delay(1000, cancellationToken: cancellationToken).SuppressCancellationThrow();
                if (canceled || cancellationToken.IsCancellationRequested) break;
            }
        }
    }

    private async UniTask PlaySingleClipAsync(AudioDataConfig config, bool stopPrevious, bool loop)
    {
        AudioClip clipToPlay = await AddressablesManager.Instance.LoadAssetAsync<AudioClip>(config.ClipRef);
        if (clipToPlay == null) return;

        AudioSource activeSource = loop ? _musicSource : _sfxSource;
        if (activeSource == null) return;

        if (loop && activeSource.isPlaying && activeSource.loop && activeSource.clip != null)
        {
            return;
        }

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

    public void StopSFX()
    {
        CancelBgmLoop();
        if (_musicSource != null)
        {
            _musicSource.Stop();
            _musicSource.clip = null;
            _musicSource.loop = false;
        }
    }

    private void CancelBgmLoop()
    {
        if (_bgmCts != null)
        {
            if (!_bgmCts.IsCancellationRequested)
            {
                _bgmCts.Cancel();
            }
            _bgmCts.Dispose();
            _bgmCts = null;
        }
        _currentBgmDatabase = null;
    }

    public void UpdateVolume(float volumeRatio)
    {
        if (_musicSource != null)
        {
            _musicSource.volume = _activeConfigVolume * volumeRatio;
        }
    }
}
