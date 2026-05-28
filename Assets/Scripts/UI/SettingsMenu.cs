using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Toggle fullscreenToggle;

    private void Awake()
    {
        float savedVolume = PlayerPrefs.GetFloat("MasterVolume", 1.0f);
        AudioListener.volume = savedVolume;
        if (volumeSlider != null)
        {
            volumeSlider.value = savedVolume;
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }

        int savedFullscreen = PlayerPrefs.GetInt("Fullscreen", 1);
        bool isFullscreen = savedFullscreen == 1;
        Screen.fullScreen = isFullscreen;
        if (fullscreenToggle != null)
        {
            fullscreenToggle.isOn = isFullscreen;
            fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
        }

        if (settingsPanel != null)
            settingsPanel.SetActive(false);
        
        var backBtn = transform.Find("PanelFrame/BackButton")?.GetComponent<UnityEngine.UI.Button>();
        if (backBtn != null)
            backBtn.onClick.AddListener(CloseSettings);
    }

    private void Update()
    {
        if (settingsPanel != null && settingsPanel.activeSelf && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CloseSettings();
        }
    }

    public void OpenSettings()
    {
        if (settingsPanel == null) return;
        settingsPanel.SetActive(true);
        if (volumeSlider != null)
            volumeSlider.value = AudioListener.volume;
        if (fullscreenToggle != null)
            fullscreenToggle.isOn = Screen.fullScreen;
    }

    public void CloseSettings()
    {
        PlayerPrefs.SetFloat("MasterVolume", AudioListener.volume);
        PlayerPrefs.SetInt("Fullscreen", Screen.fullScreen ? 1 : 0);
        PlayerPrefs.Save();
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    public void OnVolumeChanged(float value)
    {
        AudioListener.volume = value;
    }

    public void OnFullscreenChanged(bool value)
    {
        Screen.fullScreen = value;
    }
}
