# Shrine 4-State Sprites — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add 4 visual states to the Shrine (full, damaged, critical, destroyed) by swapping sprites based on health percentage.

**Architecture:** Add 4 serialized `Sprite` fields to `Shrine.cs`. On `TakeDamage()`, calculate health % and swap the `SpriteRenderer.sprite`. All fields default to the current `Shrine.png` as a placeholder. The Game Over transition still happens after the destroyed sprite is shown briefly.

**Tech Stack:** Unity 6 LTS, C#, SpriteRenderer

---

### Task 1: Modify Shrine.cs for 4-state sprites

**Files:**
- Modify: `Assets/Scripts/Shrine.cs`

- [ ] **Step 1: Read current Shrine.cs**

Read the full file at `Assets/Scripts/Shrine.cs` to understand current implementation.

- [ ] **Step 2: Replace Shrine.cs with 4-state version**

Replace the entire file:

```csharp
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Shrine : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 10;
    public int currentHealth;

    [Header("Sprites")]
    public Sprite fullSprite;
    public Sprite damagedSprite;
    public Sprite criticalSprite;
    public Sprite destroyedSprite;

    [Header("Death Delay")]
    [Min(0f)]
    public float deathDelay = 1.5f;

    private SpriteRenderer spriteRenderer;
    private bool isDead = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentHealth = maxHealth;
        UpdateSprite();
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        currentHealth = Mathf.Max(0, currentHealth);

        UpdateSprite();

        if (currentHealth <= 0)
        {
            isDead = true;
            StartCoroutine(GameOverRoutine());
        }
    }

    public void Heal(int amount)
    {
        if (isDead) return;

        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        UpdateSprite();
    }

    private void UpdateSprite()
    {
        if (spriteRenderer == null) return;

        float healthPercent = maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;

        Sprite targetSprite;

        if (currentHealth <= 0 && destroyedSprite != null)
        {
            targetSprite = destroyedSprite;
        }
        else if (healthPercent <= 0.33f && criticalSprite != null)
        {
            targetSprite = criticalSprite;
        }
        else if (healthPercent <= 0.66f && damagedSprite != null)
        {
            targetSprite = damagedSprite;
        }
        else if (fullSprite != null)
        {
            targetSprite = fullSprite;
        }
        else
        {
            return;
        }

        spriteRenderer.sprite = targetSprite;
    }

    private IEnumerator GameOverRoutine()
    {
        yield return new WaitForSeconds(deathDelay);
        SceneManager.LoadScene("Menu");
    }
}
```

- [ ] **Step 3: Update Shrine in Game.unity via inspector**

1. Select the Shrine GameObject
2. In the Inspector, drag `Shrine.png` (or existing sprite) into all 4 sprite fields:
   - `fullSprite` = current Shrine sprite
   - `damagedSprite` = current Shrine sprite (placeholder)
   - `criticalSprite` = current Shrine sprite (placeholder)
   - `destroyedSprite` = current Shrine sprite (placeholder)

Use `manage_components` to set them:
```json
[
  {
    "action": "set_property",
    "target": "Shrine",
    "component_type": "Shrine",
    "property": "fullSprite",
    "value": { "path": "Assets/Sprites/Shrine.png" }
  },
  {
    "action": "set_property",
    "target": "Shrine",
    "component_type": "Shrine",
    "property": "damagedSprite",
    "value": { "path": "Assets/Sprites/Shrine.png" }
  },
  {
    "action": "set_property",
    "target": "Shrine",
    "component_type": "Shrine",
    "property": "criticalSprite",
    "value": { "path": "Assets/Sprites/Shrine.png" }
  },
  {
    "action": "set_property",
    "target": "Shrine",
    "component_type": "Shrine",
    "property": "destroyedSprite",
    "value": { "path": "Assets/Sprites/Shrine.png" }
  }
]
```

- [ ] **Step 4: Verify health threshold mapping**

Health thresholds:
| Condition | Sprite |
|-----------|--------|
| >66% HP | `fullSprite` |
| 33-66% HP | `damagedSprite` |
| 1-33% HP | `criticalSprite` |
| 0 HP | `destroyedSprite` |

With `maxHealth=10`:
- HP 7-10 → full
- HP 4-6 → damaged
- HP 1-3 → critical
- HP 0 → destroyed

- [ ] **Step 5: Test in Play Mode**

1. Enter Play Mode
2. Let Oni enemies attack the Shrine
3. Verify sprite changes at ~6 HP (damaged), ~3 HP (critical), 0 HP (destroyed)
4. Verify game over loads Menu scene after death delay
5. Verify the destroyed sprite is visible before the scene transition

- [ ] **Step 6: Commit**

```bash
git add Assets/Scripts/Shrine.cs
git commit -m "feat: add 4-state shrine sprite swapping based on health"
```
