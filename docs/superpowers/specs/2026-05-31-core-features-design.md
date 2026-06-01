# Yoru no Oni — Core Features Design

> 2026-05-31 • 5 features covering sprite fixes, animation, waves, shrine states, and story intro

---

## Feature 1: Fix Oni Sprite Flip

**Problem:** The Oni sprite faces the opposite direction. Currently uses `transform.localScale.x` in `OniAI.MoveTowardsTarget()`.

**Solution:** Replace `localScale.x`-based flipping with `SpriteRenderer.flipX`. This is the standard 2D approach — doesn't affect colliders, children, or physics.

**File:** `Assets/Scripts/OniAI.cs` — modify `MoveTowardsTarget()` method.

---

## Feature 2: Samurai Attack Animation (5 frames)

**Problem:** The Samurai's Attack state reuses `jump.PNG` as a single frame. New sprites exist: `1 Attack.PNG` through `5 Attack.PNG`.

**Solution:** Create a new `Samurai_Attack.anim` using all 5 sprites at 8-10 fps. Replace the existing clip in `Samurai.controller`'s Attack state.

**Files:**
- Create: `Assets/Animations/Samurai_Attack.anim` (or overwrite existing)
- Modify: `Assets/Animations/Samurai.controller` — update Attack state clip reference

---

## Feature 3: Extensible Wave System

**Problem:** Current wave rules are hardcoded in `WaveSpawner.cs` with only Oni enemies and linear scaling.

**Solution:** Three-layer architecture using ScriptableObjects:
- **`WaveConfig`** — defines one wave: enemy count, spawn interval, enemy prefab(s), pause after wave, boss flag
- **`WaveSequence`** — ordered list of `WaveConfig`s
- **`WaveSpawner`** (modified) — consumes a `WaveSequence`, spawns wave by wave

Backward compatible: default auto-generated sequence preserves current behavior.

**Files:**
- Create: `Assets/Scripts/WaveConfig.cs` (ScriptableObject)
- Create: `Assets/Scripts/WaveSequence.cs` (ScriptableObject)
- Modify: `Assets/Scripts/WaveSpawner.cs`
- Remove: `OniDeathTracker` inner class (moved to its own file or refactored)

---

## Feature 4: Shrine 4-State Sprites

**Problem:** Shrine has 10 HP but only 1 sprite. No visual feedback for damage.

**Solution:** Add 4 `Sprite` fields to `Shrine.cs`: `fullSprite`, `damagedSprite`, `criticalSprite`, `destroyedSprite`. On `TakeDamage()`, swap sprite based on health percentage:
- \>66% → fullSprite
- 33–66% → damagedSprite
- 1–33% → criticalSprite
- 0 → destroyedSprite

Placeholder: all 4 fields default to current `Shrine.png` until custom sprites are provided.

**File:** `Assets/Scripts/Shrine.cs`

---

## Feature 5: Story + Controls Intro

**Problem:** Pressing "Jugar" loads the game directly with no narrative context.

**Solution:** Three-screen intro flow:

1. **Story Screen** — text from `Assets/Resources/story.txt`, typewriter effect (letter-by-letter), dark parchment background, click to advance
2. **Controls Screen** — shows 4 controls (WASD=Walk, Space=Jump, Z=Dash, X/Click=Attack), TMP-styled icons + text, "Comenzar" button to start game
3. **Game** — loads after controls screen

Both screens share the same dark parchment visual style using the VT323 pixel font with gold text.

**Files:**
- Create: `Assets/Resources/story.txt`
- Create: `Assets/Scripts/UI/StoryIntro.cs`
- Create: `Assets/Scripts/UI/ControlsIntro.cs`
- Modify: `Assets/Scripts/UI/MainMenu.cs` — change PlayGame() flow

---

## File Summary

| Action | File |
|--------|------|
| Modify | `Assets/Scripts/OniAI.cs` |
| Modify | `Assets/Scripts/SamuraiController.cs` |
| Modify | `Assets/Scripts/Shrine.cs` |
| Modify | `Assets/Scripts/WaveSpawner.cs` |
| Modify | `Assets/Scripts/UI/MainMenu.cs` |
| Modify | `Assets/Animations/Samurai.controller` |
| Create | `Assets/Animations/Samurai_Attack.anim` |
| Create | `Assets/Scripts/WaveConfig.cs` |
| Create | `Assets/Scripts/WaveSequence.cs` |
| Create | `Assets/Scripts/UI/StoryIntro.cs` |
| Create | `Assets/Scripts/UI/ControlsIntro.cs` |
| Create | `Assets/Resources/story.txt` |
