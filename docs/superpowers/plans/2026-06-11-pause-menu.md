# Pause Menu — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add pause menu (Enter/Escape) with controls display, settings, and return-to-menu confirmation.

**Architecture:** PauseMenu.cs orchestrates a root panel with sub-panels for controls and confirmation. SettingsMenu.cs gets a PauseContext flag to delegate Escape handling. All UI lives in Game.unity Canvas.

**Tech Stack:** Unity 6 LTS, URP 2D, uGUI (Canvas), TMPro, Input System

**Spec:** `docs/superpowers/specs/2026-06-11-pause-menu-design.md`

---

### Task 1: Modify SettingsMenu.cs — add IsOpen and PauseContext

**Files:**
- Modify: `Assets/Scripts/UI/SettingsMenu.cs`

- [ ] **Step 1: Add `IsOpen` property and `PauseContext` field**

After the existing fields (after line 9 `[SerializeField] private Toggle fullscreenToggle;`), insert:

```csharp
    public bool IsOpen => settingsPanel != null && settingsPanel.activeSelf;
    public bool PauseContext { get; set; }
```

- [ ] **Step 2: Wrap Escape check in Update with PauseContext guard**

Change the Update method. Currently lines 39-44:
```csharp
    private void Update()
    {
        if (settingsPanel != null && settingsPanel.activeSelf && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CloseSettings();
        }
    }
```

Change to:
```csharp
    private void Update()
    {
        if (settingsPanel != null && settingsPanel.activeSelf &&
            !PauseContext &&
            Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CloseSettings();
        }
    }
```

- [ ] **Step 3: Reset PauseContext in CloseSettings**

Add `PauseContext = false;` at the start of `CloseSettings()`:

Change:
```csharp
    public void CloseSettings()
    {
        PlayerPrefs.SetFloat("MasterVolume", AudioListener.volume);
```
To:
```csharp
    public void CloseSettings()
    {
        PauseContext = false;
        PlayerPrefs.SetFloat("MasterVolume", AudioListener.volume);
```

- [ ] **Step 4: Verify compilation**

Use `unityMCP_refresh_unity` with `compile=request`, `wait_for_ready=true`. Check `unityMCP_read_console` for errors.

---

### Task 2: Create PauseMenu.cs

**Files:**
- Create: `Assets/Scripts/UI/PauseMenu.cs`

- [ ] **Step 1: Create the script**

Use `unityMCP_create_script` with path `Assets/Scripts/UI/PauseMenu.cs` and content:

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

- [ ] **Step 2: Verify compilation**

Use `unityMCP_refresh_unity` with `compile=request`, `wait_for_ready=true`. Check `unityMCP_read_console` for errors.

---

### Task 3: Create PausePanel UI in Game.unity scene

**Files:**
- Modify: `Assets/Scenes/Game.unity`

**Prerequisites:** Stop play mode, load Game.unity.

- [ ] **Step 1: Ensure scene loaded and play mode off**

Use `manage_editor stop`. Use `manage_scene load` with path=`Assets/Scenes/Game.unity`.

- [ ] **Step 2: Find Canvas**

Use `find_gameobjects` with `search_term=UnityEngine.Canvas`, `search_method=by_component`. Note instance ID.

- [ ] **Step 3: Create PausePanel root**

Use `manage_gameobject`:
- `action=create`, `name=PausePanel`, `parent=<canvas_id>`
- `components_to_add=["RectTransform","Image","PauseMenu"]`

- [ ] **Step 4: Configure PausePanel RectTransform and Image**

Use `manage_components` on PausePanel:
- RectTransform: `anchorMin=[0,0]`, `anchorMax=[1,1]`, `anchoredPosition=[0,0]`, `sizeDelta=[0,0]`
- Image: `color=[0,0,0,0.8]`

- [ ] **Step 5: Create Title (PausePanel child)**

Use `manage_gameobject`:
- `action=create`, `name=Title`, `parent=<pausepanel_id>`
- `components_to_add=["RectTransform","TextMeshProUGUI"]`

Configure:
- RectTransform: `anchorMin=[0.5,1]`, `anchorMax=[0.5,1]`, `anchoredPosition=[0,-120]`, `sizeDelta=[400,80]`
- TextMeshProUGUI: `text=Pausa`, `fontSize=48`, `alignment=Center`, `color=white`

- [ ] **Step 6: Create 4 main buttons (PausePanel children)**

Button config for each (use batch_execute):
- Size: `anchorMin=[0.5,0.5]`, `anchorMax=[0.5,0.5]`, `sizeDelta=[400,100]`
- Each needs a Text child with `anchorMin=[0,0]`, `anchorMax=[1,1]`, `sizeDelta=[0,0]`

Buttons (name, Y position, text, serialized field):
1. ResumeButton: Y=150, "Reanudar", `resumeButton`
2. ControlsButton: Y=50, "Controles", `controlsButton`
3. SettingsButton: Y=-50, "Ajustes", `settingsButton`
4. MenuButton: Y=-150, "Volver al Menú", `menuButton`

Create each with `manage_gameobject`:
- `action=create`, `name=<name>`, `parent=<pausepanel_id>`
- `components_to_add=["RectTransform","Image","Button"]`

Then create Text child for each button, and configure TMP text.

Then use `manage_components set_property` to wire each button's onClick to the PauseMenu methods via the serialized Button fields. The actual wiring is done by PauseMenu.Awake() via `Find` or `GetComponentInChildren`. Or better, use SerializedObject to persist onClick references.

**Note:** For onClick persistence, use `execute_code` with SerializedObject to add persistent calls to each button's `m_OnClick.m_PersistentCalls.m_Calls`. But actually, the simpler approach: in PauseMenu.Awake(), wire buttons via `GetComponentInChildren<Button>()` by name. Even simpler: add serialized Button fields to PauseMenu and wire them in the scene.

Add these fields to PauseMenu (already in the spec above — they're the [SerializeField] fields).

Actually, looking at the PauseMenu script in the spec, the buttons are wired via serialized fields. The methods ShowControls, HideControls, ShowConfirm, HideConfirm, OpenSettings, Resume, GoToMenu are public. We need to persist button onClick calls.

AGENTS.md says: "To make onClick survive scene saves, wire buttons via SerializedObject on m_OnClick.m_PersistentCalls.m_Calls."

But since PauseMenu.Awake() can wire buttons via GetComponent InChildren at runtime, we don't need persistent wiring. The Awake method can search for buttons by name and add listeners.

Wait, the spec's Awake doesn't wire buttons. Let me handle this: either add button wiring in Awake or use serialized Button fields.

The simplest approach: add serialized Button fields to PauseMenu (not in the spec but necessary). Then wire them in the scene. Let me add these to the plan.

Actually, looking at the existing pattern (GameOverUI.cs, SettingsMenu.cs), they use `transform.Find("ButtonName")?.GetComponent<Button>()` to find and wire buttons. Let me follow this pattern.

- [ ] **Step 6a: Create ResumeButton**

Use `manage_gameobject`:
- `action=create`, `name=ResumeButton`, `parent=<pausepanel_id>`
- `components_to_add=["RectTransform","Image","Button"]`

Configure RectTransform: `anchorMin=[0.5,0.5]`, `anchorMax=[0.5,0.5]`, `anchoredPosition=[0,150]`, `sizeDelta=[400,100]`

Create Text child: `action=create`, `name=Text`, `parent=<resumebutton_id>`, `components_to_add=["RectTransform","TextMeshProUGUI"]`
Configure Text: stretch anchors, `text=Reanudar`, `fontSize=24`, `alignment=Center`, `color=white`

- [ ] **Step 6b: Create ControlsButton** (same, Y=50, text="Controles")

- [ ] **Step 6c: Create SettingsButton** (same, Y=-50, text="Ajustes")

- [ ] **Step 6d: Create MenuButton** (same, Y=-150, text="Volver al Menú")

- [ ] **Step 7: Create ControlsPanel (PausePanel child, inactive)**

Use `manage_gameobject`:
- `action=create`, `name=ControlsPanel`, `parent=<pausepanel_id>`
- `components_to_add=["RectTransform"]`
- `set_active=false`

Configure RectTransform: `anchorMin=[0,0]`, `anchorMax=[1,1]`, `sizeDelta=[0,0]`

Create ControlsText child: TMP, multi-line controls text (see spec section 7), centered, font 24.

Create ControlsBackBtn child: Button + Text "Volver", Y=-200.

- [ ] **Step 8: Create ConfirmPanel (PausePanel child, inactive)**

Use `manage_gameobject`:
- `action=create`, `name=ConfirmPanel`, `parent=<pausepanel_id>`
- `components_to_add=["RectTransform"]`
- `set_active=false`

Configure RectTransform: `anchorMin=[0,0]`, `anchorMax=[1,1]`, `sizeDelta=[0,0]`

Create ConfirmText child: TMP, "¿Perderás progreso. Volver al menú?", font 32, centered, Y=50.

Create YesBtn: Button + Text "Sí", Y=-50.

Create NoBtn: Button + Text "No", Y=-150.

- [ ] **Step 9: Set PausePanel inactive by default**

Use `manage_gameobject` with `action=modify`, `target=<pausepanel_id>`, `set_active=false`.

---

### Task 4: Wire button listeners in PauseMenu.Awake

**Files:**
- Modify: `Assets/Scripts/UI/PauseMenu.cs`

- [ ] **Step 1: Add button wiring logic to Awake**

After the existing inactive-setup code in Awake(), add button wiring using Find pattern:

```csharp
    private void Awake()
    {
        // ... existing setup code ...

        // Wire buttons via Transform.Find
        var resumeBtn = transform.Find("ResumeButton")?.GetComponent<UnityEngine.UI.Button>();
        if (resumeBtn != null) resumeBtn.onClick.AddListener(Resume);

        var controlsBtn = transform.Find("ControlsButton")?.GetComponent<UnityEngine.UI.Button>();
        if (controlsBtn != null) controlsBtn.onClick.AddListener(ShowControls);

        var settingsBtn = transform.Find("SettingsButton")?.GetComponent<UnityEngine.UI.Button>();
        if (settingsBtn != null) settingsBtn.onClick.AddListener(OpenSettings);

        var menuBtn = transform.Find("MenuButton")?.GetComponent<UnityEngine.UI.Button>();
        if (menuBtn != null) menuBtn.onClick.AddListener(ShowConfirm);

        var controlsBackBtn = transform.Find("ControlsPanel/ControlsBackBtn")?.GetComponent<UnityEngine.UI.Button>();
        if (controlsBackBtn != null) controlsBackBtn.onClick.AddListener(HideControls);

        var yesBtn = transform.Find("ConfirmPanel/YesBtn")?.GetComponent<UnityEngine.UI.Button>();
        if (yesBtn != null) yesBtn.onClick.AddListener(GoToMenu);

        var noBtn = transform.Find("ConfirmPanel/NoBtn")?.GetComponent<UnityEngine.UI.Button>();
        if (noBtn != null) noBtn.onClick.AddListener(HideConfirm);
    }
```

This follows the existing pattern from GameOverUI.cs (Transform.Find + GetComponent + AddListener).

---

### Task 5: Wire PauseMenu external references

**Files:**
- Modify: `Assets/Scenes/Game.unity`

- [ ] **Step 1: Find PauseMenu component and SettingsMenu**

Use `find_gameobjects` with `search_term=PausePanel`, `search_method=by_name`. Note instance ID.

Find SettingsMenu: use `find_gameobjects` with `search_term=SettingsMenu`, `search_method=by_component`.

Find RewardPanel: use `find_gameobjects` with `search_term=RewardPanel`, `search_method=by_name`.

- [ ] **Step 2: Wire references**

Use `manage_components set_property` on PauseMenu:
- `settingsMenu` → SettingsMenu instance ID
- `rewardPanel` → RewardPanel instance ID

---

### Task 6: Save, compile, play test, commit

- [ ] **Step 1: Refresh and compile**

Use `unityMCP_refresh_unity` with `compile=request`, `wait_for_ready=true`.

- [ ] **Step 2: Check for errors**

Use `unityMCP_read_console` with `types=["error"]`. Should be empty.

- [ ] **Step 3: Save scene**

Use `manage_scene save`.

- [ ] **Step 4: Enter play mode**

Use `manage_editor play`.

- [ ] **Step 5: Verify with execute_code**

```csharp
var pm = FindFirstObjectByType<PauseMenu>();
var sm = FindFirstObjectByType<SettingsMenu>();
var sb = new System.Text.StringBuilder();
sb.AppendLine("PauseMenu: " + (pm != null));
sb.AppendLine("SettingsMenu: " + (sm != null));
sb.AppendLine("IsOpen: " + (sm != null ? sm.IsOpen.ToString() : "N/A"));
sb.AppendLine("TimeScale: " + Time.timeScale);
return sb.ToString();
```
Expected: PauseMenu: True, SettingsMenu: True, IsOpen: False, TimeScale: 1

- [ ] **Step 6: Stop play mode**

Use `manage_editor stop`.

- [ ] **Step 7: Commit**

```bash
git add Assets/Scripts/UI/PauseMenu.cs Assets/Scripts/UI/SettingsMenu.cs Assets/Scenes/Game.unity
git commit -m "feat: pause menu with controls, settings, and return-to-menu"
```
