# Pause Menu — Design Spec

**Date:** 2026-06-11
**Status:** Approved
**Project:** Yoru no Oni (Unity 6 LTS)

---

## 1. Goal

Add a pause menu accessible mid-game via Enter/Escape that allows the player to view controls, adjust settings, or return to the main menu.

---

## 2. Input

| Key | Context | Action |
|-----|---------|--------|
| Enter | Game running, no other panels open | Open pause |
| Escape | Game running, no other panels open | Open pause |
| Escape | Pause root open, no sub-panel | Resume (close pause) |
| Escape | Sub-panel open (controls/confirm/settings) | Close sub-panel, return to pause root |

**Priority guard:** Pause does NOT open if RewardPanel or GameOverUI is currently active.

---

## 3. Flow

```
Game running
  │
  ├── Enter/Escape → Pause (Time.timeScale=0, PausePanel opens)
  │     │
  │     ├── Escape → Resume (Time.timeScale=1, PausePanel closes)
  │     ├── Reanudar button → Resume
  │     │
  │     ├── Controles button → Show ControlsPanel
  │     │     ├── Back button / Escape → Hide ControlsPanel (back to PausePanel)
  │     │
  │     ├── Ajustes button → SettingsMenu.OpenSettings()
  │     │     ├── Back button / Escape → SettingsMenu.CloseSettings() (back to PausePanel)
  │     │
  │     └── Volver al Menú button → Show ConfirmPanel
  │           ├── Sí → Time.timeScale=1, LoadScene("Menu")
  │           └── No / Escape → Hide ConfirmPanel (back to PausePanel)
```

---

## 4. New Script: `Assets/Scripts/UI/PauseMenu.cs`

```csharp
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

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
    }

    private void Start()
    {
        gameOverUI = FindFirstObjectByType<GameOverUI>();
    }

    private void Update()
    {
        if (gameOverUI != null && gameOverUI.gameObject.activeInHierarchy) return;
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
```

---

## 5. Modify: `Assets/Scripts/UI/SettingsMenu.cs`

Add a public `PauseContext` flag. When true, SettingsMenu skips its own Escape close — PauseMenu handles that.

```csharp
public bool IsOpen => settingsPanel != null && settingsPanel.activeSelf;
public bool PauseContext { get; set; }

// In Update(), wrap the Escape check:
if (settingsPanel != null && settingsPanel.activeSelf &&
    !PauseContext &&
    Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
{
    CloseSettings();
}
```

**PauseMenu usage:**
```csharp
public void OpenSettings()
{
    if (settingsMenu != null)
    {
        settingsMenu.PauseContext = true;
        settingsMenu.OpenSettings();
    }
}
// After CloseSettings (called by HandleEscapeInPause or Back button):
// settingsMenu.PauseContext = false;
```

CloseSettings itself should also reset `PauseContext = false`. Add at start of `CloseSettings()`: `PauseContext = false;`

---

## 6. Scene Changes (Game.unity)

### 6.1 New GameObject in Canvas: `PausePanel`

```
Canvas
  └── PausePanel (inactive by default)
        ├── PauseMenu component (script)
        ├── DarkBackground (Image, black 80% opacity, stretches full-screen)
        ├── Title (TMP: "Pausa", font 48, white, top-center)
        ├── ResumeButton (Button + TMP: "Reanudar")
        ├── ControlsButton (Button + TMP: "Controles")
        ├── SettingsButton (Button + TMP: "Ajustes")
        ├── MenuButton (Button + TMP: "Volver al Menú")
        │
        ├── ControlsPanel (inactive by default)
        │     ├── ControlsText (TMP: multi-line controls listing)
        │     └── ControlsBackBtn (Button + TMP: "Volver")
        │
        └── ConfirmPanel (inactive by default)
              ├── ConfirmText (TMP: "¿Perderás progreso. Volver al menú?")
              ├── YesBtn (Button + TMP: "Sí")
              └── NoBtn (Button + TMP: "No")
```

### 6.2 Reference wiring

- `PauseMenu.settingsMenu` → existing SettingsMenu component in scene
- `PauseMenu.rewardPanel` → RewardPanel GameObject (already exists from reward system)

### 6.3 Button layout

- ResumeButton: center, Y=150
- ControlsButton: center, Y=50
- SettingsButton: center, Y=-50
- MenuButton: center, Y=-150

### 6.4 SettingsMenu already exists

The SettingsMenu and its SettingsPanel already exist in the Game scene (from main menu flow). No need to recreate. Just wire the reference.

---

## 7. Controls Panel Content

```
=== Controles ===
WASD / ←↑↓→  —  Mover
Space         —  Saltar
Shift         —  Dash
Click / E     —  Atacar
Enter / Esc   —  Pausa
```

---

## 8. Interactions with Other Systems

| System | Behavior |
|--------|----------|
| **RewardPanel** | Pause blocked while reward panel is open (checked via `rewardPanel.gameObject.activeInHierarchy`) |
| **GameOverUI** | Pause blocked while game over is shown |
| **SettingsMenu** | Reused directly via `OpenSettings()`/`CloseSettings()` |
| **Time.timeScale** | Set to 0 during any pause or sub-panel; restored to 1 on resume or menu exit |

---

## 9. What Does NOT Change

- MainMenu.cs, StoryIntro.cs, ControlsIntro.cs (separate flow, untouched)
- WaveSpawner, SamuraiController, Shrine, Enemy, OniAI
- RewardPanel.cs
- AudioManager, CameraFollow
