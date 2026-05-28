# Spec: Settings Menu -- Yoru no Oni

**Date:** 2026-05-28
**Feature:** Settings button on main menu with volume and fullscreen toggle

## Summary

Add a third "Ajustes" button between "Jugar" and "Salir" in the main menu. Clicking it opens an overlay panel with a master volume slider and fullscreen toggle, persisted via PlayerPrefs.

---

## 1. Button Layout Changes

In `Canvas/ButtonPanel`, reposition existing buttons and add the new one:

| Button | y (current) | y (new) |
|--------|-------------|---------|
| PlayButton (Jugar) | +45 | +90 |
| **SettingsButton (Ajustes)** | -- | **0** |
| QuitButton (Salir) | -45 | -90 |

- All: same size 240x72 px
- SettingsButton: same style as PlayButton (purple bg, gold outline, VT323-Regular gold text)
- ButtonPanel height: auto-adjusts (~260 px)

## 2. SettingsPanel Hierarchy

```
Canvas
  SettingsPanel (GameObject, disabled by default)
    ├── Overlay (Image, black alpha=0.5, fullscreen, RaycastTarget)
    ├── PanelFrame (Image, 400x340 px, centered, dark purple bg, gold outline)
    │   ├── Title (TextMeshPro "Ajustes", VT323, gold, 42pt)
    │   ├── VolumeLabel (TextMeshPro "Volumen", VT323, white, 28pt)
    │   ├── VolumeSlider (Slider, 0-1, default 1.0)
    │   ├── FullscreenLabel (TextMeshPro "Pantalla Completa", VT323, white, 28pt)
    │   ├── FullscreenToggle (Toggle, default true)
    │   └── BackButton (Button "Volver", QuitButton style)
```

## 3. Behavior

- **Open:** Click "Ajustes" -> `SettingsPanel.SetActive(true)`, sync slider/toggle
- **Close:** Click "Volver" or ESC key -> save to PlayerPrefs, `SetActive(false)`
- **Volume:** Slider modifies `AudioListener.volume` in real-time
- **Fullscreen:** Toggle calls `Screen.fullScreen` -- WebGL only works on user click

## 4. Scripts

### SettingsMenu.cs (new, Assets/Scripts/UI/)

Component on the `SettingsPanel` GameObject.

```
[SerializeField] Slider volumeSlider;
[SerializeField] Toggle fullscreenToggle;
[SerializeField] GameObject settingsPanel;

void Awake()   -- load PlayerPrefs, apply initial values
void Update()  -- detect ESC to close
void OpenSettings()   -- show panel, sync controls
void CloseSettings()  -- save, hide panel
void OnVolumeChanged(float value)
void OnFullscreenChanged(bool value)
```

### MainMenu.cs (modify)

Only add to `Start()`:

```
var settingsBtn = GameObject.Find("SettingsButton")?.GetComponent<Button>();
var backBtn = GameObject.Find("BackButton")?.GetComponent<Button>();
var sm = FindObjectOfType<SettingsMenu>();
settingsBtn?.onClick.AddListener(() => sm.OpenSettings());
backBtn?.onClick.AddListener(() => sm.CloseSettings());
```

Do not touch `PlayGame()` or `QuitGame()`.

## 5. Persistence (PlayerPrefs)

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| `MasterVolume` | float | 1.0 | Master volume (0-1) |
| `Fullscreen` | int | 1 | 1=fullscreen, 0=windowed |

Saved on panel close. Loaded in `Awake()`.

## 6. Visual Style

Matches existing menu:
- **Panel bg:** RGBA(0.10, 0.04, 0.18, 0.95)
- **Outline:** gold #D4A017, distance (2, -2)
- **Font:** VT323-Regular SDF
- **Button colors:** PlayButton-style for SettingsButton, QuitButton-style for BackButton
- **Slider:** gold fill, dark purple background
- **Overlay:** RGBA(0, 0, 0, 0.5)
