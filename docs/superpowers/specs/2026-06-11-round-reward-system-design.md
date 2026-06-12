# Round Reward System — Design Spec

**Date:** 2026-06-11
**Status:** Approved
**Project:** Yoru no Oni (Unity 6 LTS)

---

## 1. Goal

Replace the automatic healing every 4 rounds with a player-choice reward system that activates after each round.

---

## 2. Removed Behavior

- **OLD**: Every 4th wave (`currentWave % 4 == 0`), Samurai heals 1 HP and Shrine gains +5 max/current HP (lines 107-113 in WaveSpawner.cs)
- **REMOVED**: This entire block is deleted

---

## 3. New System

### 3.1 Rewards (always all 3 available)

| Rewards | Effect | Stackable? |
|---------|--------|------------|
| **Restore Health** | `currentHealth = maxHealth` (full heal) | Instant, no stack |
| **+1 Damage** | `bonusDamage += 1` | Yes, unbounded |
| **+Attack Speed** | `attackCooldown *= 0.90` | Yes, unbounded |

### 3.2 Fixed Bonus

- Every 5 rounds (`currentWave % 5 == 0`): Shrine `+5 maxHealth` and `+5 currentHealth` (automatic, independent of reward choice)

### 3.3 Flow per Round

```
Round completed → postWaveDelay → waveActive = false
                                      ↓
                              Time.timeScale = 0 (pause)
                              Show RewardPanel UI (3 buttons)
                              Player clicks 1 button
                                      ↓
                              SamuraiController.ApplyReward(type)
                              If round % 5 == 0: shrine +5 HP
                              Time.timeScale = 1
                              Hide panel
                                      ↓
                              Next round
```

---

## 4. Files to Modify

### 4.1 `Assets/Scripts/SamuraiController.cs`

**New fields:**
```csharp
[Header("Base Stats")]
[SerializeField] private int   baseDamage = 1;
[SerializeField] private float baseAttackCooldown = 0.35f;

private int   bonusDamage;
private float attackSpeedMultiplier = 1f;
```

**Effective properties:**
```csharp
public int   EffectiveDamage       => baseDamage + bonusDamage;
public float EffectiveAttackCooldown => baseAttackCooldown * attackSpeedMultiplier;
```

**Reward method:**
```csharp
public void ApplyReward(RewardType type)
{
    switch (type)
    {
        case RewardType.RestoreHealth:
            currentHealth = maxHealth;
            break;
        case RewardType.BonusDamage:
            bonusDamage++;
            break;
        case RewardType.AttackSpeed:
            attackSpeedMultiplier *= 0.90f;
            break;
    }
}
```

**Usage changes:**
- `PerformAttack()` line 192: `attackCooldownTimer = EffectiveAttackCooldown;`
- `PerformAttack()` line 209: `enemy.TakeDamage(EffectiveDamage);`
- `maxHealth` field stays unchanged (no bonus max health needed)

### 4.2 `Assets/Scripts/WaveSpawner.cs`

**Remove:** Lines 107-113 (the `if (currentWave % 4 == 0)` healing block)

**New fields:**
```csharp
[Header("Rewards")]
[SerializeField] private RewardPanel rewardPanel;
private bool waitingForReward;
```

**Flow change** after `waveActive = false` (line 105):
```csharp
waveActive = false;

// Show reward selection
waitingForReward = true;
rewardPanel.Show(OnRewardSelected);
while (waitingForReward)
    yield return null;

// Shrine bonus every 5 rounds
if (currentWave % 5 == 0)
{
    if (shrine != null)
        shrine.IncreaseMaxHealth(5);
}
```

**New callback:**
```csharp
private void OnRewardSelected(RewardType type)
{
    if (samurai != null)
        samurai.ApplyReward(type);
    waitingForReward = false;
}
```

### 4.3 `Assets/Scripts/UI/RewardPanel.cs` (NEW)

```csharp
using System;
using UnityEngine;
using UnityEngine.UI;

public enum RewardType { RestoreHealth, BonusDamage, AttackSpeed }

public class RewardPanel : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Button healthButton;
    [SerializeField] private Button damageButton;
    [SerializeField] private Button speedButton;

    private Action<RewardType> callback;

    private void Awake()
    {
        healthButton.onClick.AddListener(() => Select(RewardType.RestoreHealth));
        damageButton.onClick.AddListener(() => Select(RewardType.BonusDamage));
        speedButton.onClick.AddListener(() => Select(RewardType.AttackSpeed));

        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    public void Show(Action<RewardType> onSelected)
    {
        callback = onSelected;
        panelRoot.SetActive(true);
        Time.timeScale = 0f;
    }

    private void Select(RewardType type)
    {
        panelRoot.SetActive(false);
        Time.timeScale = 1f;
        callback?.Invoke(type);
    }
}
```

---

## 5. Scene Changes (Game.unity)

### 5.1 New GameObject in Canvas: `RewardPanel`

```
Canvas
  └── RewardPanel (inactive by default)
        ├── DarkBackground (Image, black 80% opacity, covers full screen)
        ├── Title (TMP_Text: "Elige una recompensa", centered)
        ├── HealthButton (Button + TMP_Text: "Restaurar vida")
        ├── DamageButton (Button + TMP_Text: "+1 Daño")
        └── SpeedButton  (Button + TMP_Text: "+Velocidad ataque")
```

### 5.2 WaveSpawner reference

- Add `rewardPanel` field reference on WaveSpawner pointing to the RewardPanel GameObject

---

## 6. Data Flow Summary

```
WaveSpawner              SamuraiController           RewardPanel
    |                          |                          |
    |-- waveActive=false       |                          |
    |-- rewardPanel.Show(cb)-> |                          |-- panelRoot.SetActive(true)
    |                          |                          |-- Time.timeScale=0
    |                          |                          |
    |                          |                          |<-- Player clicks button
    |                          |                          |
    |<-- cb(RewardType) ------|                          |-- Select(type)
    |                          |                          |-- Time.timeScale=1
    |-- samurai.ApplyReward()> |-- ApplyReward(type)     |
    |                          |   (modify stats)        |
    |-- shrine.IncreaseMax()   |                          |
    |-- waitingForReward=false |                          |
```

---

## 7. Edge Cases

- **Attack speed approaching 0**: Each stack is `*0.90`. After 20 stacks: `0.35 * 0.90^20 ≈ 0.042s`. Still functional but very fast. No cap needed — difficulty scaling handles balance.
- **Full health + Restore Health**: Harmless — sets currentHealth to maxHealth (same value).
- **RewardPanel shown while no enemies**: Safe — game is paused via `Time.timeScale = 0`.
- **Scene reload / Game Over**: BonusDamage and attackSpeedMultiplier reset with the MonoBehaviour (new instance on scene load).

---

## 8. What Does NOT Change

- Enemy stats, spawn logic, wave types
- Samurai movement, dash, jump, invulnerability frames
- Shrine behavior (except healing removed from WaveSpawner)
- HealthBar, CameraFollow, AudioManager, UI (GameOver, Settings, etc.)
