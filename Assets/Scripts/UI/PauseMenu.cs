using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject pauseRoot;
    [SerializeField] private GameObject controlsPanel;
    [SerializeField] private GameObject confirmPanel;

    [Header("External")]
    [SerializeField] private SettingsMenu settingsMenu;
    [SerializeField] private RewardPanel rewardPanel;

    private bool isPaused;
    private GameOverUI gameOverUI;

    private void Awake()
    {
        if (pauseRoot != null) pauseRoot.SetActive(false);
        if (controlsPanel != null) controlsPanel.SetActive(false);
        if (confirmPanel != null) confirmPanel.SetActive(false);

        var root = pauseRoot != null ? pauseRoot.transform : transform;
        var resumeBtn = root.Find("ResumeButton")?.GetComponent<UnityEngine.UI.Button>();
        if (resumeBtn != null) resumeBtn.onClick.AddListener(Resume);

        var controlsBtn = root.Find("ControlsButton")?.GetComponent<UnityEngine.UI.Button>();
        if (controlsBtn != null) controlsBtn.onClick.AddListener(ShowControls);

        var settingsBtn = root.Find("SettingsButton")?.GetComponent<UnityEngine.UI.Button>();
        if (settingsBtn != null) settingsBtn.onClick.AddListener(OpenSettings);

        var menuBtn = root.Find("MenuButton")?.GetComponent<UnityEngine.UI.Button>();
        if (menuBtn != null) menuBtn.onClick.AddListener(ShowConfirm);

        var controlsBackBtn = root.Find("ControlsPanel/ControlsBackBtn")?.GetComponent<UnityEngine.UI.Button>();
        if (controlsBackBtn != null) controlsBackBtn.onClick.AddListener(HideControls);

        var yesBtn = root.Find("ConfirmPanel/YesBtn")?.GetComponent<UnityEngine.UI.Button>();
        if (yesBtn != null) yesBtn.onClick.AddListener(GoToMenu);

        var noBtn = root.Find("ConfirmPanel/NoBtn")?.GetComponent<UnityEngine.UI.Button>();
        if (noBtn != null) noBtn.onClick.AddListener(HideConfirm);
    }

    private void Start()
    {
        gameOverUI = FindFirstObjectByType<GameOverUI>();
    }

    private void Update()
    {
        if (gameOverUI != null && gameOverUI.IsOpen) return;
        if (rewardPanel != null && rewardPanel.gameObject.activeInHierarchy) return;

        if (!isPaused && (Keyboard.current?.enterKey.wasPressedThisFrame == true ||
                          Keyboard.current?.escapeKey.wasPressedThisFrame == true))
        {
            Pause();
        }
        else if (isPaused && Keyboard.current?.escapeKey.wasPressedThisFrame == true)
        {
            HandleEscapeInPause();
        }
    }

    private void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f;
        if (pauseRoot != null) pauseRoot.SetActive(true);
    }

    public void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f;
        if (pauseRoot != null) pauseRoot.SetActive(false);
    }

    private void HandleEscapeInPause()
    {
        if (confirmPanel != null && confirmPanel.activeSelf)
            HideConfirm();
        else if (controlsPanel != null && controlsPanel.activeSelf)
            HideControls();
        else if (settingsMenu != null && settingsMenu.IsOpen)
            settingsMenu.CloseSettings();
        else
            Resume();
    }

    public void ShowControls()
    {
        if (controlsPanel != null) controlsPanel.SetActive(true);
    }

    public void HideControls()
    {
        if (controlsPanel != null) controlsPanel.SetActive(false);
    }

    public void ShowConfirm()
    {
        if (confirmPanel != null) confirmPanel.SetActive(true);
    }

    public void HideConfirm()
    {
        if (confirmPanel != null) confirmPanel.SetActive(false);
    }

    public void OpenSettings()
    {
        if (settingsMenu != null)
        {
            settingsMenu.PauseContext = true;
            settingsMenu.OpenSettings();
        }
    }

    public void GoToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }
}
