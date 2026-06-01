# Game Over Screen Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a Game Over overlay UI inside Game.unity when player dies or shrine is destroyed.

**Architecture:** New `GameOverUI.cs` script handles a Canvas panel (dark overlay + text + buttons). WaveSpawner exposes `currentWave`. SamuraiController/Shrine call `GameOverUI.Show()` instead of loading scene directly. `Time.timeScale = 0` while panel is active.

**Tech Stack:** Unity 6 LTS, URP 2D, TextMeshPro, Input System, C#

---

### Task 1: Create GameOverUI.cs script

**Files:**
- Create: `Assets/Scripts/UI/GameOverUI.cs`

- [ ] **Step 1: Write GameOverUI.cs**

```csharp
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text causeText;
    [SerializeField] private TMP_Text waveText;

    private void Start()
    {
        var retryBtn = GameObject.Find("RetryButton")?.GetComponent<UnityEngine.UI.Button>();
        if (retryBtn != null) retryBtn.onClick.AddListener(Reiniciar);

        var menuBtn = GameObject.Find("MenuButton")?.GetComponent<UnityEngine.UI.Button>();
        if (menuBtn != null) menuBtn.onClick.AddListener(IrAlMenu);

        if (panel != null)
            panel.SetActive(false);
    }

    public void Show(string cause)
    {
        if (panel != null)
            panel.SetActive(true);

        Time.timeScale = 0f;

        if (causeText != null)
            causeText.text = cause;

        var spawner = FindFirstObjectByType<WaveSpawner>();
        int wave = spawner != null ? spawner.CurrentWave : 1;
        if (waveText != null)
            waveText.text = "Oleada alcanzada: " + wave;
    }

    public void Reiniciar()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Game");
    }

    public void IrAlMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }
}
```

- [ ] **Step 2: Verify compilation**

Run: monitor Unity Editor console for compilation errors after the script is created.

---

### Task 2: Expose CurrentWave in WaveSpawner

**Files:**
- Modify: `Assets/Scripts/WaveSpawner.cs` (add public property for `currentWave`)

- [ ] **Step 1: Add public property**

Add after line 27 (`private WaveConfig activeConfig;`):
```csharp
    public int CurrentWave => currentWave;
```

- [ ] **Step 2: Verify compilation**

Check Unity console for errors.

---

### Task 3: Modify SamuraiController to trigger Game Over instead of loading Menu

**Files:**
- Modify: `Assets/Scripts/SamuraiController.cs`

- [ ] **Step 1: Replace death scene load with GameOverUI call**

Change lines 282-286 from:
```csharp
        if (currentHealth <= 0)
        {
            Debug.Log("[Samurai] Defeated!");
            UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
        }
```

To:
```csharp
        if (currentHealth <= 0)
        {
            Debug.Log("[Samurai] Defeated!");
            var gameOver = FindFirstObjectByType<GameOverUI>();
            if (gameOver != null)
                gameOver.Show("El Samurai ha caido");
        }
```

- [ ] **Step 2: Verify compilation**

Check Unity console for errors.

---

### Task 4: Modify Shrine to trigger Game Over instead of loading Menu

**Files:**
- Modify: `Assets/Scripts/Shrine.cs`

- [ ] **Step 1: Replace death scene load with GameOverUI call**

Change lines 40-44 from:
```csharp
        if (currentHealth <= 0)
        {
            isDead = true;
            StartCoroutine(GameOverRoutine());
        }
```

To:
```csharp
        if (currentHealth <= 0)
        {
            isDead = true;
            var gameOver = FindFirstObjectByType<GameOverUI>();
            if (gameOver != null)
                gameOver.Show("El Santuario ha sido destruido");
        }
```

Remove the entire `GameOverRoutine` coroutine (lines 120-124):
```csharp
    private IEnumerator GameOverRoutine()
    {
        yield return new WaitForSeconds(deathDelay);
        SceneManager.LoadScene("Menu");
    }
```

Also remove the `deathDelay` field (line 18-20) and its `using UnityEngine.SceneManagement;` at line 3 since it's no longer needed.

- [ ] **Step 2: Verify compilation**

Check Unity console for errors.

---

### Task 5: Create GameOverPanel UI in Game.unity

- [ ] **Step 1: Stop play mode if running**

Ensure Unity editor is not in play mode.

- [ ] **Step 2: Create GameOverPanel as child of HUD_Canvas**

Create a new GameObject `GameOverPanel` under `HUD_Canvas` with components: `RectTransform`, `CanvasRenderer`, `Image`.
- Set Image color to black with alpha ~0.75 (overlay)
- Set RectTransform anchors to stretch full screen (min 0,0; max 1,1) with sizeDelta (0,0)

- [ ] **Step 3: Add Title text "GAME OVER"**

Create child TextMeshPro `GameOverTitle` under GameOverPanel.
- Text: "GAME OVER"
- Font: VT323 SDF
- Font Size: 72
- Color: #D4A017 (gold)
- Alignment: Center
- Anchors: center, y=0.6

- [ ] **Step 4: Add Cause of death text**

Create child TextMeshPro `CauseText` under GameOverPanel.
- Text: "" (set by script)
- Font: VT323 SDF
- Font Size: 36
- Color: #C71585 (magenta)
- Alignment: Center
- Anchors: center, y=0.45

- [ ] **Step 5: Add Wave text**

Create child TextMeshPro `WaveText` under GameOverPanel.
- Text: "" (set by script)
- Font: VT323 SDF
- Font Size: 28
- Color: white
- Alignment: Center
- Anchors: center, y=0.35

- [ ] **Step 6: Add Retry button**

Create child Button `RetryButton` under GameOverPanel.
- Button child Text: "REINTENTAR"
- Font: VT323 SDF, size 32
- Color: #D4A017
- Anchors: center, y=0.2

- [ ] **Step 7: Add Menu button**

Create child Button `MenuButton` under GameOverPanel.
- Button child Text: "MENU PRINCIPAL"
- Font: VT323 SDF, size 32
- Color: #C71585
- Anchors: center, y=0.05

- [ ] **Step 8: Attach GameOverUI script**

Create or reuse an empty GameObject (e.g. add to `WaveSpawner` or create `GameOverLogic` empty) and attach `GameOverUI.cs`.
- Assign `panel` → the GameOverPanel GameObject
- Assign `titleText` → GameOverTitle
- Assign `causeText` → CauseText
- Assign `waveText` → WaveText
- Set GameOverPanel to inactive initially

- [ ] **Step 9: Verify Game Over works in play mode**

Run Game scene, let the player die or shrine be destroyed. Verify:
- Panel appears with correct text
- Game is paused (enemies frozen)
- Retry reloads Game scene fresh
- Menu goes back to Menu scene
