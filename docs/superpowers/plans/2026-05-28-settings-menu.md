# Settings Menu Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add "Ajustes" button between "Jugar" and "Salir" in main menu with a popup settings panel (volume slider + fullscreen toggle, persisted via PlayerPrefs).

**Architecture:** Panel-overlay approach on the existing Canvas. New `SettingsMenu.cs` script manages show/hide and PlayerPrefs persistence. `MainMenu.cs` gains two wiring lines in `Start()`.

**Tech Stack:** Unity 6 LTS, URP 2D, uGUI Canvas, TextMeshPro, C# 6 (CodeDom), PlayerPrefs

---

### Task 1: Create SettingsMenu.cs

**Files:**
- Create: `Assets/Scripts/UI/SettingsMenu.cs`

- [ ] **Step 1: Create the script**

Use MCP `create_script` at path `Assets/Scripts/UI/SettingsMenu.cs`:

```csharp
using UnityEngine;
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
            volumeSlider.value = savedVolume;

        int savedFullscreen = PlayerPrefs.GetInt("Fullscreen", 1);
        bool isFullscreen = savedFullscreen == 1;
        Screen.fullScreen = isFullscreen;
        if (fullscreenToggle != null)
            fullscreenToggle.isOn = isFullscreen;

        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    private void Update()
    {
        if (settingsPanel != null && settingsPanel.activeSelf && UnityEngine.Input.GetKeyDown(KeyCode.Escape))
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
```

- [ ] **Step 2: Refresh Unity to compile**

MCP: `refresh_unity` with `mode=force`, `compile=request`, `wait_for_ready=true`

- [ ] **Step 3: Verify no compilation errors**

MCP: `read_console` with `types=["error"]`, `count=5`. Expected: no errors related to SettingsMenu.

---

### Task 2: Modify MainMenu.cs

**Files:**
- Modify: `Assets/Scripts/UI/MainMenu.cs`

- [ ] **Step 1: Read current file**

MCP: `manage_script` `action=read`, `name=MainMenu`, `path=Assets/Scripts/UI`

- [ ] **Step 2: Apply edit to add settings wiring to Start()**

MCP: `script_apply_edits` on `MainMenu`, `path=Assets/Scripts/UI`:

```json
{
  "name": "MainMenu",
  "path": "Assets/Scripts/UI",
  "edits": [
    {
      "op": "replace_method",
      "className": "MainMenu",
      "methodName": "Start",
      "replacement": "void Start()\n    {\n        var playBtn = GameObject.Find(\"PlayButton\")?.GetComponent<UnityEngine.UI.Button>();\n        if (playBtn != null) playBtn.onClick.AddListener(PlayGame);\n        var quitBtn = GameObject.Find(\"QuitButton\")?.GetComponent<UnityEngine.UI.Button>();\n        if (quitBtn != null) quitBtn.onClick.AddListener(QuitGame);\n        var settingsBtn = GameObject.Find(\"SettingsButton\")?.GetComponent<UnityEngine.UI.Button>();\n        var sm = FindObjectOfType<SettingsMenu>();\n        if (settingsBtn != null && sm != null) settingsBtn.onClick.AddListener(sm.OpenSettings);\n    }"
    }
  ]
}
```

- [ ] **Step 3: Refresh Unity**

MCP: `refresh_unity` with `mode=force`, `compile=request`, `wait_for_ready=true`

- [ ] **Step 4: Verify no compilation errors**

MCP: `read_console` with `types=["error"]`, `count=5`.

---

### Task 3: Reposition existing buttons + create SettingsButton

**Scene:** `Menu.unity`

- [ ] **Step 1: Move PlayButton up (y: 45 → 90)**

MCP: `manage_gameobject`, `action=modify`, target `PlayButton`, `search_method=by_name`, `position=[0, 90, 0]`

- [ ] **Step 2: Move QuitButton down (y: -45 → -90)**

MCP: `manage_gameobject`, `action=modify`, target `QuitButton`, `search_method=by_name`, `position=[0, -90, 0]`

- [ ] **Step 3: Create SettingsButton via execute_code (batch creation of button + label + styling)**

MCP: `execute_code`, `compiler=codedom`:

```csharp
var panel = GameObject.Find("Canvas/ButtonPanel");
if (panel == null) { return "ERROR: ButtonPanel not found"; }

var settingsBtn = new GameObject("SettingsButton", typeof(RectTransform));
settingsBtn.transform.SetParent(panel.transform, false);
var btnRt = settingsBtn.GetComponent<RectTransform>();
btnRt.sizeDelta = new Vector2(240, 72);
btnRt.anchoredPosition = new Vector2(0, 0);
btnRt.localScale = Vector3.one;

var img = settingsBtn.AddComponent<UnityEngine.UI.Image>();
img.color = new Color(0.290f, 0.102f, 0.420f, 1.0f);

var btn = settingsBtn.AddComponent<UnityEngine.UI.Button>();
var colors = btn.colors;
colors.normalColor = new Color(0.290f, 0.102f, 0.420f, 1.0f);
colors.highlightedColor = new Color(0.420f, 0.165f, 0.610f, 1.0f);
colors.pressedColor = new Color(0.177f, 0.106f, 0.306f, 1.0f);
colors.selectedColor = new Color(0.961f, 0.961f, 0.961f, 1.0f);
colors.disabledColor = new Color(0.784f, 0.784f, 0.784f, 0.502f);
btn.colors = colors;

var outline = settingsBtn.AddComponent<UnityEngine.UI.Outline>();
outline.effectColor = new Color(0.831f, 0.627f, 0.090f, 0.8f);
outline.effectDistance = new Vector2(2, -2);

var label = new GameObject("Label", typeof(RectTransform));
label.transform.SetParent(settingsBtn.transform, false);
var labelRt = label.GetComponent<RectTransform>();
labelRt.anchorMin = Vector2.zero;
labelRt.anchorMax = Vector2.one;
labelRt.sizeDelta = Vector2.zero;
labelRt.anchoredPosition = Vector2.zero;

var text = label.AddComponent<TMPro.TextMeshProUGUI>();
text.text = "Ajustes";
text.color = new Color(1.0f, 0.843f, 0.0f, 1.0f);
text.fontSize = 30;
text.alignment = TMPro.TextAlignmentOptions.Center;
text.font = UnityEditor.AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>("Assets/Fonts/VT323/VT323-Regular SDF.asset");

return "SettingsButton created";
```

- [ ] **Step 4: Save scene**

MCP: `manage_scene`, `action=save`

---

### Task 4: Create SettingsPanel hierarchy

**Scene:** `Menu.unity`

- [ ] **Step 1: Create the SettingsPanel parent with all children via execute_code**

MCP: `execute_code`, `compiler=codedom`:

```csharp
var canvas = GameObject.Find("Canvas");
if (canvas == null) { return "ERROR: Canvas not found"; }

// --- Find VT323 font asset ---
var font = UnityEditor.AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>("Assets/Fonts/VT323/VT323-Regular SDF.asset");

// --- SettingsPanel root ---
var root = new GameObject("SettingsPanel", typeof(RectTransform));
root.transform.SetParent(canvas.transform, false);
var rootRt = root.GetComponent<RectTransform>();
rootRt.anchorMin = Vector2.zero;
rootRt.anchorMax = Vector2.one;
rootRt.sizeDelta = Vector2.zero;
rootRt.anchoredPosition = Vector2.zero;
root.SetActive(false);

// --- Overlay ---
var overlay = new GameObject("Overlay", typeof(RectTransform));
overlay.transform.SetParent(root.transform, false);
var ovRt = overlay.GetComponent<RectTransform>();
ovRt.anchorMin = Vector2.zero;
ovRt.anchorMax = Vector2.one;
ovRt.sizeDelta = Vector2.zero;
var ovImg = overlay.AddComponent<UnityEngine.UI.Image>();
ovImg.color = new Color(0, 0, 0, 0.5f);
ovImg.raycastTarget = true;

// --- PanelFrame ---
var frame = new GameObject("PanelFrame", typeof(RectTransform));
frame.transform.SetParent(root.transform, false);
var frRt = frame.GetComponent<RectTransform>();
frRt.anchorMin = new Vector2(0.5f, 0.5f);
frRt.anchorMax = new Vector2(0.5f, 0.5f);
frRt.sizeDelta = new Vector2(420, 380);
frRt.anchoredPosition = Vector2.zero;
var frImg = frame.AddComponent<UnityEngine.UI.Image>();
frImg.color = new Color(0.10f, 0.04f, 0.18f, 0.95f);
var frOl = frame.AddComponent<UnityEngine.UI.Outline>();
frOl.effectColor = new Color(0.831f, 0.627f, 0.090f, 0.8f);
frOl.effectDistance = new Vector2(2, -2);

// --- Title ---
var titleGo = new GameObject("Title", typeof(RectTransform));
titleGo.transform.SetParent(frame.transform, false);
var titleRt = titleGo.GetComponent<RectTransform>();
titleRt.anchorMin = new Vector2(0.5f, 1f);
titleRt.anchorMax = new Vector2(0.5f, 1f);
titleRt.pivot = new Vector2(0.5f, 1f);
titleRt.sizeDelta = new Vector2(300, 50);
titleRt.anchoredPosition = new Vector2(0, -20);
var titleTxt = titleGo.AddComponent<TMPro.TextMeshProUGUI>();
titleTxt.text = "Ajustes";
titleTxt.color = new Color(1.0f, 0.843f, 0.0f, 1.0f);
titleTxt.fontSize = 42;
titleTxt.alignment = TMPro.TextAlignmentOptions.Center;
titleTxt.font = font;

// --- VolumeLabel ---
var volLabel = new GameObject("VolumeLabel", typeof(RectTransform));
volLabel.transform.SetParent(frame.transform, false);
var volLbRt = volLabel.GetComponent<RectTransform>();
volLbRt.anchorMin = new Vector2(0.5f, 1f);
volLbRt.anchorMax = new Vector2(0.5f, 1f);
volLbRt.pivot = new Vector2(0.5f, 1f);
volLbRt.sizeDelta = new Vector2(300, 36);
volLbRt.anchoredPosition = new Vector2(0, -85);
var volLbTxt = volLabel.AddComponent<TMPro.TextMeshProUGUI>();
volLbTxt.text = "Volumen";
volLbTxt.color = Color.white;
volLbTxt.fontSize = 28;
volLbTxt.alignment = TMPro.TextAlignmentOptions.Center;
volLbTxt.font = font;

// --- VolumeSlider (simplified: just background + fill area) ---
var volSlider = new GameObject("VolumeSlider", typeof(RectTransform));
volSlider.transform.SetParent(frame.transform, false);
var vsRt = volSlider.GetComponent<RectTransform>();
vsRt.anchorMin = new Vector2(0.5f, 1f);
vsRt.anchorMax = new Vector2(0.5f, 1f);
vsRt.pivot = new Vector2(0.5f, 1f);
vsRt.sizeDelta = new Vector2(300, 30);
vsRt.anchoredPosition = new Vector2(0, -120);

var sliderComp = volSlider.AddComponent<UnityEngine.UI.Slider>();
sliderComp.minValue = 0;
sliderComp.maxValue = 1;
sliderComp.value = 1;
sliderComp.wholeNumbers = false;

// Slider Background
var sliderBg = new GameObject("Background", typeof(RectTransform));
sliderBg.transform.SetParent(volSlider.transform, false);
var sbgRt = sliderBg.GetComponent<RectTransform>();
sbgRt.anchorMin = Vector2.zero;
sbgRt.anchorMax = Vector2.one;
sbgRt.sizeDelta = Vector2.zero;
var sbgImg = sliderBg.AddComponent<UnityEngine.UI.Image>();
sbgImg.color = new Color(0.177f, 0.106f, 0.306f, 1.0f);

// Slider Fill Area
var fillArea = new GameObject("Fill Area", typeof(RectTransform));
fillArea.transform.SetParent(volSlider.transform, false);
var faRt = fillArea.GetComponent<RectTransform>();
faRt.anchorMin = new Vector2(0, 0.25f);
faRt.anchorMax = new Vector2(1, 0.75f);
faRt.sizeDelta = new Vector2(-20, 0);
faRt.anchoredPosition = Vector2.zero;

var fillGo = new GameObject("Fill", typeof(RectTransform));
fillGo.transform.SetParent(fillArea.transform, false);
var fRt = fillGo.GetComponent<RectTransform>();
fRt.anchorMin = Vector2.zero;
fRt.anchorMax = Vector2.one;
fRt.sizeDelta = Vector2.zero;
var fImg = fillGo.AddComponent<UnityEngine.UI.Image>();
fImg.color = new Color(0.831f, 0.627f, 0.090f, 1.0f);

// Slider Handle Area
var handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
handleArea.transform.SetParent(volSlider.transform, false);
var haRt = handleArea.GetComponent<RectTransform>();
haRt.anchorMin = new Vector2(0, 0);
haRt.anchorMax = new Vector2(1, 1);
haRt.sizeDelta = new Vector2(-20, 0);
haRt.anchoredPosition = Vector2.zero;

var handleGo = new GameObject("Handle", typeof(RectTransform));
handleGo.transform.SetParent(handleArea.transform, false);
var hRt = handleGo.GetComponent<RectTransform>();
hRt.sizeDelta = new Vector2(24, 24);
hRt.anchoredPosition = Vector2.zero;
var hImg = handleGo.AddComponent<UnityEngine.UI.Image>();
hImg.color = new Color(0.831f, 0.627f, 0.090f, 1.0f);

sliderComp.fillRect = fRt;
sliderComp.handleRect = hRt;
sliderComp.targetGraphic = hImg;

// --- FullscreenLabel ---
var fsLabel = new GameObject("FullscreenLabel", typeof(RectTransform));
fsLabel.transform.SetParent(frame.transform, false);
var fsLbRt = fsLabel.GetComponent<RectTransform>();
fsLbRt.anchorMin = new Vector2(0.5f, 1f);
fsLbRt.anchorMax = new Vector2(0.5f, 1f);
fsLbRt.pivot = new Vector2(0.5f, 1f);
fsLbRt.sizeDelta = new Vector2(300, 36);
fsLbRt.anchoredPosition = new Vector2(0, -180);
var fsLbTxt = fsLabel.AddComponent<TMPro.TextMeshProUGUI>();
fsLbTxt.text = "Pantalla Completa";
fsLbTxt.color = Color.white;
fsLbTxt.fontSize = 28;
fsLbTxt.alignment = TMPro.TextAlignmentOptions.Center;
fsLbTxt.font = font;

// --- FullscreenToggle ---
var fsToggle = new GameObject("FullscreenToggle", typeof(RectTransform));
fsToggle.transform.SetParent(frame.transform, false);
var fstRt = fsToggle.GetComponent<RectTransform>();
fstRt.anchorMin = new Vector2(0.5f, 1f);
fstRt.anchorMax = new Vector2(0.5f, 1f);
fstRt.pivot = new Vector2(0.5f, 1f);
fstRt.sizeDelta = new Vector2(40, 40);
fstRt.anchoredPosition = new Vector2(0, -220);

var toggleComp = fsToggle.AddComponent<UnityEngine.UI.Toggle>();
toggleComp.isOn = true;

// Toggle Background
var toggleBg = new GameObject("Background", typeof(RectTransform));
toggleBg.transform.SetParent(fsToggle.transform, false);
var tbgRt = toggleBg.GetComponent<RectTransform>();
tbgRt.anchorMin = Vector2.zero;
tbgRt.anchorMax = Vector2.one;
tbgRt.sizeDelta = Vector2.zero;
var tbgImg = toggleBg.AddComponent<UnityEngine.UI.Image>();
tbgImg.color = new Color(0.177f, 0.106f, 0.306f, 1.0f);

// Toggle Checkmark
var checkmark = new GameObject("Checkmark", typeof(RectTransform));
checkmark.transform.SetParent(fsToggle.transform, false);
var ckmRt = checkmark.GetComponent<RectTransform>();
ckmRt.anchorMin = new Vector2(0.1f, 0.1f);
ckmRt.anchorMax = new Vector2(0.9f, 0.9f);
ckmRt.sizeDelta = Vector2.zero;
var ckmImg = checkmark.AddComponent<UnityEngine.UI.Image>();
ckmImg.color = new Color(0.831f, 0.627f, 0.090f, 1.0f);

toggleComp.graphic = ckmImg;
toggleComp.targetGraphic = tbgImg;

// --- BackButton ---
var backBtn = new GameObject("BackButton", typeof(RectTransform));
backBtn.transform.SetParent(frame.transform, false);
var bbRt = backBtn.GetComponent<RectTransform>();
bbRt.anchorMin = new Vector2(0.5f, 0f);
bbRt.anchorMax = new Vector2(0.5f, 0f);
bbRt.pivot = new Vector2(0.5f, 0f);
bbRt.sizeDelta = new Vector2(240, 72);
bbRt.anchoredPosition = new Vector2(0, 30);

var bbImg = backBtn.AddComponent<UnityEngine.UI.Image>();
bbImg.color = new Color(0.102f, 0.039f, 0.180f, 0.7f);

var bbBtn = backBtn.AddComponent<UnityEngine.UI.Button>();
var bbColors = bbBtn.colors;
bbColors.normalColor = new Color(0.102f, 0.039f, 0.180f, 0.7f);
bbColors.highlightedColor = new Color(0.831f, 0.627f, 0.090f, 1.0f);
bbColors.pressedColor = new Color(0.177f, 0.106f, 0.306f, 1.0f);
bbBtn.colors = bbColors;

var bbOl = backBtn.AddComponent<UnityEngine.UI.Outline>();
bbOl.effectColor = new Color(0.290f, 0.290f, 0.416f, 1.0f);
bbOl.effectDistance = new Vector2(2, -2);

var bbLabel = new GameObject("Label", typeof(RectTransform));
bbLabel.transform.SetParent(backBtn.transform, false);
var bbLbRt = bbLabel.GetComponent<RectTransform>();
bbLbRt.anchorMin = Vector2.zero;
bbLbRt.anchorMax = Vector2.one;
bbLbRt.sizeDelta = Vector2.zero;
var bbTxt = bbLabel.AddComponent<TMPro.TextMeshProUGUI>();
bbTxt.text = "Volver";
bbTxt.color = new Color(0.722f, 0.663f, 0.788f, 1.0f);
bbTxt.fontSize = 30;
bbTxt.alignment = TMPro.TextAlignmentOptions.Center;
bbTxt.font = font;

return "SettingsPanel created successfully";
```

- [ ] **Step 2: Save scene**

MCP: `manage_scene`, `action=save`

- [ ] **Step 3: Verify hierarchy**

MCP: `manage_scene`, `action=get_hierarchy`, `parent=SettingsPanel` (find by name first if needed)

---

### Task 5: Attach SettingsMenu component and wire references

**Scene:** `Menu.unity`

- [ ] **Step 1: Add SettingsMenu component to SettingsPanel**

MCP: `manage_components`, `action=add`, target `SettingsPanel`, `search_method=by_name`, `component_type=SettingsMenu`

- [ ] **Step 2: Wire serialized references via execute_code**

MCP: `execute_code`, `compiler=codedom`:

```csharp
var sm = Object.FindObjectOfType<SettingsMenu>();
if (sm == null) { return "ERROR: SettingsMenu not found"; }

var panel = GameObject.Find("Canvas/SettingsPanel");
var volumeSlider = GameObject.Find("Canvas/SettingsPanel/PanelFrame/VolumeSlider");
var fullscreenToggle = GameObject.Find("Canvas/SettingsPanel/PanelFrame/FullscreenToggle");

var so = new UnityEditor.SerializedObject(sm);
so.FindProperty("settingsPanel").objectReferenceValue = panel;
so.FindProperty("volumeSlider").objectReferenceValue = volumeSlider != null ? volumeSlider.GetComponent<UnityEngine.UI.Slider>() : null;
so.FindProperty("fullscreenToggle").objectReferenceValue = fullscreenToggle != null ? fullscreenToggle.GetComponent<UnityEngine.UI.Toggle>() : null;
so.ApplyModifiedProperties();

return "References wired";
```

- [ ] **Step 3: Wire BackButton onClick**

MCP: `execute_code`, `compiler=codedom`:

```csharp
var sm = Object.FindObjectOfType<SettingsMenu>();
if (sm == null) { return "ERROR: SettingsMenu not found"; }

var backBtn = GameObject.Find("Canvas/SettingsPanel/PanelFrame/BackButton");
if (backBtn == null) { return "ERROR: BackButton not found"; }

var btn = backBtn.GetComponent<UnityEngine.UI.Button>();
btn.onClick.RemoveAllListeners();
btn.onClick.AddListener(sm.CloseSettings);

// Wire Slider's onValueChanged
var volSlider = GameObject.Find("Canvas/SettingsPanel/PanelFrame/VolumeSlider");
if (volSlider != null)
{
    var slider = volSlider.GetComponent<UnityEngine.UI.Slider>();
    slider.onValueChanged.AddListener(sm.OnVolumeChanged);
}

// Wire Toggle's onValueChanged
var fsToggle = GameObject.Find("Canvas/SettingsPanel/PanelFrame/FullscreenToggle");
if (fsToggle != null)
{
    var toggle = fsToggle.GetComponent<UnityEngine.UI.Toggle>();
    toggle.onValueChanged.AddListener(sm.OnFullscreenChanged);
}

return "Listeners wired";
```

- [ ] **Step 4: Save scene**

MCP: `manage_scene`, `action=save`

---

### Task 6: Verify and test

- [ ] **Step 1: Refresh Unity**

MCP: `refresh_unity`, `wait_for_ready=true`

- [ ] **Step 2: Check console for errors**

MCP: `read_console`, `types=["error"]`, `count=10`

- [ ] **Step 3: Enter play mode**

MCP: `manage_editor`, `action=play`

- [ ] **Step 4: Verify menu layout**

Take screenshot: MCP `manage_camera`, `action=screenshot`, `include_image=true`

Expected: Three buttons visible — Jugar (top), Ajustes (middle), Salir (bottom), all centered.

- [ ] **Step 5: Stop play mode**

MCP: `manage_editor`, `action=stop`

---
