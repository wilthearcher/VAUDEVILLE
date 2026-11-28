using UnityEngine;
using UnityEngine.Audio;

public class AudioSettingsManager : MonoBehaviour
{
    public static AudioSettingsManager Instance { get; private set; }

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    private const string MUSIC_KEY = "MusicVolume";
    private const string SFX_KEY = "SFXVolume";

    private const string MUSIC_PARAMETER = "MusicVolume";
    private const string SFX_PARAMETER = "SFXVolume";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadAudioSettings();
    }

    public void SetMusicVolume(float volume)
    {
        SetVolume(MUSIC_PARAMETER, volume);

        PlayerPrefs.SetFloat(MUSIC_KEY, volume);
    }

    public void SetSFXVolume(float volume)
    {
        SetVolume(SFX_PARAMETER, volume);

        PlayerPrefs.SetFloat(SFX_KEY, volume);
    }

    private void SetVolume(string parameter, float volume)
    {
        // Converte o valor do Slider (0 a 1) para decibéis.
        float db = volume <= 0.0001f
            ? -80f
            : Mathf.Log10(volume) * 20f;

        audioMixer.SetFloat(parameter, db);
    }

    private void LoadAudioSettings()
    {
        float musicVolume = PlayerPrefs.GetFloat(MUSIC_KEY, 1f);
        float sfxVolume = PlayerPrefs.GetFloat(SFX_KEY, 1f);

        SetVolume(MUSIC_PARAMETER, musicVolume);
        SetVolume(SFX_PARAMETER, sfxVolume);
    }
}
