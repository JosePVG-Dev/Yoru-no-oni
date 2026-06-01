# Story + Controls Intro — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace the direct Play→Game flow with a 3-step intro: Story screen (typewriter text from a .txt file) → Controls screen (key bindings display) → Game.

**Architecture:** Two new UI scripts (`StoryIntro.cs`, `ControlsIntro.cs`) with corresponding Canvas panels in Menu.unity. `MainMenu.PlayGame()` shows Story panel instead of loading the game directly. Story panel advances to Controls on click, Controls "Comenzar" button loads Game scene.

**Tech Stack:** Unity 6 LTS, C#, UGUI, TextMeshPro, VT323 font

---

### Task 1: Create story.txt resource file

**Files:**
- Create: `Assets/Resources/story.txt`

- [ ] **Step 1: Create story.txt**

Create `Assets/Resources/story.txt` with placeholder content:

```
En las profundidades de la noche eterna, donde las sombras susurran leyendas olvidadas, un guerrero se alza contra la oscuridad.

Un samurai sin nombre, portador de la luz ancestral, protege el ultimo santuario sagrado de las fuerzas demoniacas.

Los oni, bestias del inframundo, avanzan en oleadas interminables. Cada ataque resuena como el trueno, cada caida de un enemigo es un verso en la cancion de la resistencia.

El santuario debe permanecer en pie. La llama no debe apagarse.
```

- [ ] **Step 2: Verify the file is readable at runtime**

In Unity, files under `Assets/Resources/` are accessible via `Resources.Load<TextAsset>("story")`. Verify with `execute_code`:

```csharp
var text = Resources.Load<TextAsset>("story");
return text != null ? "Found: " + text.text.Length + " chars" : "NOT FOUND";
```

- [ ] **Step 3: Commit**

```bash
git add Assets/Resources/story.txt Assets/Resources/story.txt.meta
git commit -m "feat: add story.txt resource for intro narrative"
```

---

### Task 2: Create StoryIntro.cs

**Files:**
- Create: `Assets/Scripts/UI/StoryIntro.cs`

- [ ] **Step 1: Write StoryIntro.cs**

```csharp
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class StoryIntro : MonoBehaviour
{
    [Header("Text")]
    [Tooltip("TextAsset from Resources (e.g. 'story' loads Resources/story.txt)")]
    public string resourceName = "story";
    [Range(0.01f, 0.2f)]
    public float typewriterSpeed = 0.04f;
    public TMP_Text storyText;

    [Header("UI")]
    public GameObject storyPanel;
    public GameObject controlsPanel;

    [Header("Audio (optional)")]
    public AudioSource typewriterAudio;

    private string fullText;
    private Coroutine typewriterRoutine;
    private bool textFullyShown = false;

    private void Start()
    {
        LoadAndStartStory();
    }

    public void LoadAndStartStory()
    {
        var textAsset = Resources.Load<TextAsset>(resourceName);
        if (textAsset != null)
        {
            fullText = textAsset.text;
        }
        else
        {
            fullText = "No story file found at Resources/" + resourceName + ".txt";
        }

        storyPanel.SetActive(true);
        if (controlsPanel != null)
            controlsPanel.SetActive(false);

        storyText.text = "";
        textFullyShown = false;
        typewriterRoutine = StartCoroutine(TypewriterEffect());
    }

    private IEnumerator TypewriterEffect()
    {
        for (int i = 0; i < fullText.Length; i++)
        {
            storyText.text += fullText[i];

            if (typewriterAudio != null && fullText[i] != ' ' && fullText[i] != '\n')
                typewriterAudio.Play();

            yield return new WaitForSeconds(typewriterSpeed);
        }

        textFullyShown = true;
    }

    private void Update()
    {
        if (storyPanel.activeSelf && (Input.GetMouseButtonDown(0) || Input.anyKeyDown))
        {
            if (!textFullyShown && typewriterRoutine != null)
            {
                StopCoroutine(typewriterRoutine);
                storyText.text = fullText;
                textFullyShown = true;
            }
            else if (textFullyShown)
            {
                ShowControls();
            }
        }
    }

    private void ShowControls()
    {
        storyPanel.SetActive(false);
        if (controlsPanel != null)
            controlsPanel.SetActive(true);
    }
}
```

- [ ] **Step 2: Commit**

```bash
git add Assets/Scripts/UI/StoryIntro.cs
git commit -m "feat: add StoryIntro with typewriter effect from Resources txt"
```

---

### Task 3: Create ControlsIntro.cs

**Files:**
- Create: `Assets/Scripts/UI/ControlsIntro.cs`

- [ ] **Step 1: Write ControlsIntro.cs**

```csharp
using UnityEngine;
using UnityEngine.SceneManagement;

public class ControlsIntro : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            StartGame();
        }
    }

    public void StartGame()
    {
        SceneManager.LoadScene("Game");
    }
}
```

- [ ] **Step 2: Commit**

```bash
git add Assets/Scripts/UI/ControlsIntro.cs
git commit -m "feat: add ControlsIntro to show controls before game"
```

---

### Task 4: Create UI Panels in Menu.unity

**Files:**
- Modify: `Assets/Scenes/Menu.unity`

- [ ] **Step 1: Create StoryPanel Canvas child**

Create a full-screen dark panel under the existing Canvas in Menu.unity with:

**Structure:**
```
Canvas/
└── StoryPanel (initially inactive)
    ├── Backdrop (Image, black, alpha 0.85)
    ├── ParchmentFrame (Image, Marco.png or dark purple panel, centered)
    │   └── StoryText (TMP_Text, gold #D4A017, VT323 font, 36pt, alignment: top-left, wrapping)
    └── ClickPrompt (TMP_Text, "Click para continuar...", gold, bottom-center, pulsing alpha)
```

**Components on StoryPanel:**
- `StoryIntro` script

**Steps via MCP:**
1. `manage_gameobject` action=`create` under Canvas with name="StoryPanel"
2. Add child `Backdrop` (Image, black 85% alpha, stretch to full screen)
3. Add child `ParchmentFrame` (Image, dark purple #2D1B4E, centered 900x600)
4. Add child `StoryText` under ParchmentFrame (TMP_Text, gold, VT323)
5. Add child `ClickPrompt` under StoryPanel
6. `manage_components` action=`add` to StoryPanel, componentType=`StoryIntro`
7. Wire `storyText` reference to the TMP_Text
8. Wire `storyPanel` to StoryPanel, `controlsPanel` (will be set in next step)
9. Set StoryPanel `set_active=false`

- [ ] **Step 2: Create ControlsPanel Canvas child**

```
Canvas/
└── ControlsPanel (initially inactive)
    ├── Backdrop (same as StoryPanel)
    ├── Title (TMP_Text, "CONTROLES", gold, large, centered top)
    ├── ControlsGrid (vertical layout)
    │   ├── ControlRow_Walk (Image + Icon text + "WASD / Flechas - Caminar")
    │   ├── ControlRow_Jump (Image + Icon text + "Espacio - Saltar")
    │   ├── ControlRow_Dash (Image + Icon text + "Z - Dash")
    │   └── ControlRow_Attack (Image + Icon text + "X / Click - Atacar")
    └── StartButton (Button, "COMENZAR", gold text, calls ControlsIntro.StartGame)
```

**Components on ControlsPanel:**
- `ControlsIntro` script

**Steps via MCP:**
1. `manage_gameobject` action=`create` under Canvas with name="ControlsPanel"
2. Add Backdrop, Title, grid rows using TMP_Text
3. Add `StartButton` (Button) with onClick → ControlsIntro.StartGame
4. `manage_components` action=`add` to ControlsPanel, componentType=`ControlsIntro`
5. Set ControlsPanel `set_active=false`

- [ ] **Step 3: Wire StoryIntro.controlsPanel reference**

Set `StoryIntro.controlsPanel` to the ControlsPanel GameObject reference:
```json
{
  "target": "StoryPanel",
  "component_type": "StoryIntro",
  "property": "controlsPanel",
  "value": { "ref": { "by_name": "ControlsPanel" } }
}
```

- [ ] **Step 4: Wire StartButton onClick persistent call**

Set ControlsPanel's StartButton onClick to call `ControlsIntro.StartGame()`:

Use `manage_scriptable_object` or `execute_code` to serialize the persistent call:
```csharp
var button = GameObject.Find("StartButton").GetComponent<UnityEngine.UI.Button>();
var so = new SerializedObject(button);
var calls = so.FindProperty("m_OnClick.m_PersistentCalls.m_Calls");
calls.arraySize = 1;
var call = calls.GetArrayElementAtIndex(0);
call.FindPropertyRelative("m_Target").objectReferenceValue = GameObject.Find("ControlsPanel").GetComponent<ControlsIntro>();
call.FindPropertyRelative("m_MethodName").stringValue = "StartGame";
call.FindPropertyRelative("m_Mode").enumValueIndex = 1; // EditorAndRuntime
so.ApplyModifiedProperties();
```

- [ ] **Step 5: Commit**

```bash
git add Assets/Scenes/Menu.unity
git commit -m "feat: add Story and Controls intro panels to Menu scene"
```

---

### Task 5: Modify MainMenu.cs PlayGame flow

**Files:**
- Modify: `Assets/Scripts/UI/MainMenu.cs`

- [ ] **Step 1: Read current MainMenu.cs**

Read the current file to understand the PlayGame method.

- [ ] **Step 2: Modify PlayGame() to show StoryPanel instead of loading Game**

Change the `PlayGame()` method:

```csharp
// OLD:
public void PlayGame()
{
    SceneManager.LoadScene("Game");
}

// NEW:
public void PlayGame()
{
    var storyPanel = GameObject.Find("StoryPanel");
    if (storyPanel != null)
    {
        storyPanel.SetActive(true);
        var story = storyPanel.GetComponent<StoryIntro>();
        if (story != null)
            story.LoadAndStartStory();
    }
    else
    {
        // Fallback: load game directly if StoryPanel not found
        SceneManager.LoadScene("Game");
    }
}
```

- [ ] **Step 3: Verify full flow in Play Mode**

1. Enter Play Mode from Menu.unity
2. Click "Jugar"
3. Story panel appears with typewriter text
4. Click to skip typewriter (shows full text)
5. Click again to advance to Controls panel
6. Controls panel shows 4 key bindings
7. Click "COMENZAR" or press Enter/Space
8. Game.unity loads

- [ ] **Step 4: Commit**

```bash
git add Assets/Scripts/UI/MainMenu.cs
git commit -m "feat: change Play button to show Story intro instead of direct load"
```
