# Boss Wave Ending — Design Spec

**Date:** 2026-06-12
**Status:** Draft
**Project:** Yoru no Oni (Unity 6 LTS)

---

## 1. Goal

Add a definitive ending to the game: a finite wave sequence culminating in a boss wave at wave 10. The boss wave spawns all enemy types plus a unique red boss Oni that one-shots the Shrine if it reaches it.

---

## 2. Current Behavior

- `WaveSpawner.RunWaves()` runs an infinite `while (true)` loop.
- No ending condition exists — game continues until Shrine or Samurai dies.
- No boss enemies.

---

## 3. New System

### 3.1 Wave Sequence (finite)

- `WaveSequence` ScriptableObject defines waves 1–10.
- Wave 10 is the boss wave (`isBossWave = true`).
- `WaveSpawner` iterates through the sequence and stops after the boss wave is cleared.

### 3.2 Boss Wave (Wave 10)

| Aspect | Detail |
|--------|--------|
| Enemies | All 4 Oni types (Balanced, Fast, Tank, Jumper) + Boss Rojo |
| Boss spawns | Mid-wave or near the end |
| requireDefeat | `true` (all enemies + boss must die to win) |
| announcementText | `"¡El Oni Mayor ha llegado!"` |

### 3.3 Boss Rojo — `BossOni` Component

**Sprite:** Red (as requested)
**Behavior:**
- Walks slowly toward Shrine (ignores Samurai — always target Shrine)
- When it reaches attack range of Shrine → one-shots it (`shrine.TakeDamage(shrine.maxHealth)`)
- Does NOT attack Samurai
- Cannot be distracted

**Stats:**
- HP: 40-50
- Speed: very slow (≈0.5-0.8 units/s)
- Attack: instant Shrine kill on contact

**Death:** When Boss Rojo dies, the existing `OniDeathTracker`→`WaveSpawner.OnEnemyDied()` system notifies the spawner. When `enemiesAlive` reaches 0, WaveSpawner detects the boss wave is complete and triggers Victory.

### 3.4 Victory Flow

```
Boss Rojo dies → waveActive = false → postWaveDelay (short) → VictoryUI
```

### 3.5 Victory via GameOverUI (reused)

- `GameOverUI.Show()` repurposed for victory with a `isVictory` parameter.
- Title text: `"¡Salvaste el santuario!"`
- Shows oleada alcanzada.
- Victory mode: hides "Reintentar" button, only shows "Menú Principal".
- No new UI script needed.

---

## 4. Files to Create

### 4.1 `Assets/Scripts/BossOni.cs` (NEW)

```csharp
public class BossOni : Enemy
{
    // Walks toward Shrine slowly
    // On reaching Shrine: shrine.TakeDamage(shrine.maxHealth)
    // On death: trigger victory
}
```

_No new UI script — GameOverUI handles both game over and victory._

---

## 5. Files to Modify

### 5.1 `Assets/Scripts/WaveSpawner.cs`

**Change `RunWaves()`:**
- Instead of `while (true)`, iterate over `waveSequence.waves` count.
- After defeating a wave with `isBossWave == true`, break out and trigger victory (instead of continuing loop).

**Changes to `RunWaves()`:**
- Loop condition: iterate through `waveSequence.waves.Count` instead of `while (true)`.
- After reward selection and shrine bonus (end-of-wave logic), check `activeConfig.isBossWave`.
- If boss wave and `enemiesAlive == 0`, call `ShowVictory()` and `yield break`.

**Pseudo-flow:**
```csharp
private IEnumerator RunWaves()
{
    yield return new WaitForSeconds(initialDelay);

    int totalWaves = waveSequence != null ? waveSequence.waves.Count : int.MaxValue;

    while (currentWave < totalWaves)
    {
        // ... spawn wave, wait for enemies, show reward ...

        if (activeConfig.isBossWave)
        {
            ShowVictory();
            yield break;
        }

        // ... post-wave delay ...
    }
}
```

**New method:**
```csharp
private void ShowVictory()
{
    var gameOver = FindFirstObjectByType<GameOverUI>();
    if (gameOver != null)
        gameOver.ShowVictory(currentWave);
}
```

### 5.2 `Assets/Resources/` — WaveSequence Asset

Create `WaveSequence` asset with 10 waves:
- Waves 1-9: progressive difficulty (more enemies, faster spawns, tougher types)
- Wave 10: all enemy types + BossRojo

### 5.3 `Assets/Scripts/UI/GameOverUI.cs` — Add victory mode

**New method:**
```csharp
public void ShowVictory(int wave)
{
    panel.SetActive(true);
    Time.timeScale = 0f;
    Cursor.visible = true;
    Cursor.lockState = CursorLockMode.None;
    causeText.text = "¡Salvaste el santuario!";
    waveText.text = "Oleada alcanzada: " + wave;
    // Hide Retry button for victory
    var retryBtn = panel.transform.Find("RetryButton")?.gameObject;
    if (retryBtn != null) retryBtn.SetActive(false);
}
```

### 5.4 Scene (Game.unity)

- Create or configure `WaveSpawner` to use the finite `WaveSequence`.

---

## 6. Boss Prefab Setup

- Create `BossOni` prefab with red sprite, `BossOni` component, `CapsuleCollider2D`, `Rigidbody2D`.
- Add to `WaveConfig.enemyPrefabs` for wave 10.

---

## 7. Edge Cases

- **Player dies before boss reaches Shrine**: Normal GameOver via `SamuraiController.TakeDamage()` → `GameOverUI.Show("El Samurai ha caido")`. No special handling.
- **Shrine destroyed by boss**: BossOni calls `shrine.TakeDamage(shrine.maxHealth)`. `Shrine.TakeDamage()` already calls `GameOverUI.Show("El Santuario ha sido destruido")` when health ≤ 0. Works automatically.
- **Boss reaches Shrine while other enemies alive**: Shrine dies → Game Over. Existing system handles this.
- **Multiple boss spawns**: Only one BossRojo in wave 10 config. Safe.
- **Victory triggers early**: Victory only triggers when `enemiesAlive == 0` after boss wave. Boss dying but other enemies alive → keeps fighting.
- **No WaveSequence assigned**: Falls back to infinite waves (legacy behavior). No boss wave, no ending.
- **GameOverUI not in scene**: `FindFirstObjectByType<GameOverUI>()` returns null — safe no-op, wave loop just ends silently.

---

## 8. What Does NOT Change

- SamuraiController, OniAI, Shrine behavior (except Shrine can be one-shot by boss)
- Reward system (still works after each wave, including wave 9)
- Pause menu, settings, main menu
