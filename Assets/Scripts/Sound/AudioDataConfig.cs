using System;
using UnityEngine.AddressableAssets;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioDatabase", menuName = "GameConfig/Audio Config")]
public class AudioDataConfig : ScriptableObject
{
    public string AudioID;
    public AudioClip DirectClip; // Kéo thả file âm thanh .mp3/.wav trực tiếp vào đây
    public AssetReferenceT<AudioClip> ClipRef;
    [Range(0f, 1f)] public float Volume = 1;

    private void OnValidate()
    {
        if (AudioID != this.name)
        {
            AudioID = this.name;
        }
    }
}
