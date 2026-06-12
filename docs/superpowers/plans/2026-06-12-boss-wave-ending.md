# Boss Wave Ending Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a finite 10-wave sequence ending with a boss wave that spawns a giant red BossOni that one-shots the Shrine if it reaches it.

**Architecture:** BossOni is a new `Enemy` subclass that walks toward the Shrine (ignoring the Samurai) and one-shots it on contact. `WaveSpawner` iterates through a finite wave sequence instead of infinite `while(true)`. `GameOverUI` gains a `ShowVictory()` method for the victory screen.

**Tech Stack:** Unity 6 LTS, C#, URP 2D

---

### Task 1: Create BossOni.cs

**Files:**
- Create: `Assets/Scripts/BossOni.cs`

- [ ] **Step 1: Write the BossOni script**

```csharp
using UnityEngine;

public class BossOni : Enemy
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 0.4f; // Slower than Tank (1.5)

    [Header("Shrine Detection")]
    [SerializeField] private float attackRange = 1.5f;

    [Header("Health")]
    [SerializeField] private int bossMaxHealth = 45;

    private Transform shrine;
    private Rigidbody2D rb;

    protected override void Awake()
    {
        base.Awake();
        InitHealth(bossMaxHealth);
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        GameObject shrineObj = GameObject.Find("Shrine");
        if (shrineObj != null)
            shrine = shrineObj.transform;

        sr.color = Color.red;
    }

    private void Update()
    {
        if (isDead || shrine == null) return;

        float dist = Vector2.Distance(transform.position, shrine.position);
        if (dist <= attackRange)
        {
            var shrineComp = shrine.GetComponent<Shrine>();
            if (shrineComp != null)
                shrineComp.TakeDamage(shrineComp.maxHealth);
        }
    }

    private void FixedUpdate()
    {
        if (isDead || rb == null || shrine == null) return;

        Vector2 direction = ((Vector2)shrine.position - rb.position).normalized;
        rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);

        var sr = GetComponent<SpriteRenderer>();
        if (sr != null && Mathf.Abs(direction.x) > 0.1f)
            sr.flipX = direction.x < 0;
    }

    protected override IEnumerator DieRoutine()
    {
        isDead = true;

        if (animator != null)
            animator.SetFloat("Dead", 1f);

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        yield return new WaitForSeconds(1f);

        if (waveSpawner != null)
            waveSpawner.OnEnemyDied();

        enabled = false;
    }
}
```

---

### Task 2: Add ShowVictory to GameOverUI

**Files:**
- Modify: `Assets/Scripts/UI/GameOverUI.cs`

- [ ] **Step 1: Add ShowVictory method**

```csharp
public void ShowVictory(int wave)
{
    if (panel != null)
        panel.SetActive(true);

    Time.timeScale = 0f;
    Cursor.visible = true;
    Cursor.lockState = CursorLockMode.None;

    if (causeText != null)
        causeText.text = "¡Salvaste el santuario!";

    if (waveText != null)
        waveText.text = "Oleada alcanzada: " + wave;

    var retryBtn = panel.transform.Find("RetryButton")?.gameObject;
    if (retryBtn != null)
        retryBtn.SetActive(false);
}
```

Insert this method after the existing `Show(string cause)` method (around line 43).

---

### Task 3: Modify WaveSpawner — Finite Waves + Victory

**Files:**
- Modify: `Assets/Scripts/WaveSpawner.cs`

- [ ] **Step 1: Change RunWaves() loop to finite**

Replace the `while (true)` at line 51 with:

```csharp
int totalWaves = waveSequence != null && waveSequence.waves.Count > 0
    ? waveSequence.waves.Count
    : int.MaxValue;

while (currentWave < totalWaves)
```

- [ ] **Step 2: Add victory trigger after reward selection**

After the reward selection block (the `waitingForReward` section, around line 108), insert:

```csharp
if (activeConfig.isBossWave)
{
    ShowVictory();
    yield break;
}
```

But this must come AFTER the reward and shrine bonus, BEFORE the post-wave delay. So the flow should be:

```csharp
waitingForReward = true;
if (rewardPanel != null)
    rewardPanel.Show(OnRewardSelected);
while (waitingForReward)
    yield return null;

if (activeConfig.isBossWave)
{
    ShowVictory();
    yield break;
}

if (currentWave % 5 == 0)
{
    if (shrine != null)
        shrine.IncreaseMaxHealth(5);
}
```

- [ ] **Step 3: Add ShowVictory method to WaveSpawner**

```csharp
private void ShowVictory()
{
    var gameOver = FindFirstObjectByType<GameOverUI>();
    if (gameOver != null)
        gameOver.ShowVictory(currentWave);
}
```

---

### Task 4: Create Boss Wave Config + Update DefaultWaveSequence

**Files:**
- Create: `Assets/Settings/Wave_10_Boss.asset` (WaveConfig)
- Modify: `Assets/Settings/DefaultWaveSequence.asset`

- [ ] **Step 1: Create the boss WaveConfig asset**

Use `manage_scriptable_object` to create a `WaveConfig` asset:
- Type: `WaveConfig`
- Folder: `Assets/Settings`
- Name: `Wave_10_Boss`
- Properties:
  - `enemyPrefabs`: array with 5 entries — Oni, FastOni, TankOni, JumperOni, BossOni (will add prefab reference after Task 5)
  - `enemyCount`: 12
  - `spawnInterval`: 1.5
  - `postWaveDelay`: 3
  - `isBossWave`: true
  - `requireDefeat`: true
  - `announcementText`: "¡El Oni Mayor ha llegado!"

- [ ] **Step 2: Update DefaultWaveSequence to use finite waves**

Modify `DefaultWaveSequence.asset`:
- Set `waves` array to have 10 slots (indices 0-9)
- Set slot 9 (wave 10) to reference `Wave_10_Boss`
- Leave slots 0-8 null (they'll fall back to auto-generation via `GetWaveConfig`)

---

### Task 5: Create BossOni Prefab + Scene Setup

**Files:**
- Create: `Assets/Prefabs/BossOni.prefab`
- Create: `Assets/Sprites/BossOni.png` (or reuse Oni_Sheet with red tint — already handled in code)
- Modify: Game.unity (scene) — wire up references

- [ ] **Step 1: Create BossOni sprite**

Create a red-tinted version of the Oni sprite, or reuse the existing `Oni_Sheet.png` and apply red tint via code (already done in `BossOni.Start()`). For simplicity, use a red texture generated procedurally or a copy of Oni_Sheet with red tint in the import settings.

Simplest: Just use `Oni_Sheet.png` — the `sr.color = Color.red` in BossOni.Start() tints the entire sprite red at runtime.

- [ ] **Step 2: Create BossOni prefab**

Duplicate the Oni.prefab structure:
- SpriteRenderer: `Oni_Sheet.png`
- Rigidbody2D: Dynamic, Gravity 1, Freeze Rotation Z
- CapsuleCollider2D: offset (0, 0.75), size (1.5, 1.5)
- Animator: `Oni.controller`
- Replace `OniAI` component with `BossOni` component
- Set `maxHealth` to 45
- Move to `Assets/Prefabs/BossOni.prefab`
- Scale: 2.0 (bigger than normal Oni at 1.5)

- [ ] **Step 3: Update Wave_10_Boss enemyPrefabs**

Add `BossOni.prefab` reference to `Wave_10_Boss` asset's `enemyPrefabs` list (slot index 4).

- [ ] **Step 4: Verify GameOverPanel in Game.unity has RetryButton**

Ensure the GameOverPanel in the scene has a child `RetryButton` (it needs to exist so `ShowVictory` can find and hide it). If the button already exists, the code handles hiding it. No scene changes needed otherwise.
