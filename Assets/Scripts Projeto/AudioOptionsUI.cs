
using UnityEngine;
using UnityEngine.UI;

public class AudioOptionsUI : MonoBehaviour
{
    [Header("Sliders")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    private void Start()
    {
        // Configura os limites dos sliders.
        musicSlider.minValue = 0f;
        musicSlider.maxValue = 1f;

        sfxSlider.minValue = 0f;
        sfxSlider.maxValue = 1f;

        // Recupera os valores salvos.
        musicSlider.SetValueWithoutNotify(
            PlayerPrefs.GetFloat("MusicVolume", 1f)
        );

        sfxSlider.SetValueWithoutNotify(
            PlayerPrefs.GetFloat("SFXVolume", 1f)
        );

        // Registra os eventos.
        musicSlider.onValueChanged.AddListener(
            AudioSettingsManager.Instance.SetMusicVolume
        );

        sfxSlider.onValueChanged.AddListener(
            AudioSettingsManager.Instance.SetSFXVolume
        );
    }

    private void OnDestroy()
    {
        if (AudioSettingsManager.Instance == null)
            return;

        musicSlider.onValueChanged.RemoveListener(
            AudioSettingsManager.Instance.SetMusicVolume
        );

        sfxSlider.onValueChanged.RemoveListener(
            AudioSettingsManager.Instance.SetSFXVolume
        );
    }
}
