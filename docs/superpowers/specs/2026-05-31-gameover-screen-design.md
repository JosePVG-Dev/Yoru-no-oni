# Game Over Screen — Design Spec

## Overview
Add a Game Over screen overlay inside `Game.unity` that appears when the player dies or the Shrine is destroyed. Shows cause of death, wave reached, and two buttons: Retry / Main Menu. Pauses the game while active.

## Requirements
- **Trigger 1**: `SamuraiController.TakeDamage()` → health ≤ 0 → show Game Over
- **Trigger 2**: `Shrine.GameOverRoutine()` → shrine health ≤ 0 → show Game Over
- Show **cause of death**: "El Samurai ha caido" / "El Santuario ha sido destruido"
- Show **wave reached**: WaveSpawner's `currentWave`
- **Retry button**: `SceneManager.LoadScene("Game")` — full reload = clean state
- **Menu button**: `SceneManager.LoadScene("Menu")`
- **Pause**: `Time.timeScale = 0` when panel shows; restore to 1 on scene load

## Implementation

### New Script: `GameOverUI.cs`
- Public method `Show(string cause)` — activates the panel, sets text, pauses time
- `Reiniciar()` — `Time.timeScale = 1; SceneManager.LoadScene("Game")`
- `IrAlMenu()` — `Time.timeScale = 1; SceneManager.LoadScene("Menu")`
- Start() auto-wires buttons via `GameObject.Find`

### Scene Changes (`Game.unity`)
- Add `GameOverPanel` GameObject under `HUD_Canvas` (or new Canvas)
  - Dark semi-transparent background Image (full screen, black, alpha ~0.7)
  - "GAME OVER" title TextMeshPro (VT323, large, gold/magenta)
  - Cause of death TextMeshPro
  - Wave reached TextMeshPro
  - "Reintentar" Button
  - "Menu Principal" Button
- Ensure EventSystem already has `InputSystemUIInputModule` (done)
- Attach `GameOverUI.cs` to a persistent GameObject (e.g. `WaveSpawner` or new empty)
- Set initial state: disabled (inactive)

### Modified Scripts
- **SamuraiController.cs** line 287: Replace `SceneManager.LoadScene("Menu")` with `FindObjectOfType<GameOverUI>()?.Show("El Samurai ha caido")`
- **Shrine.cs** lines 120-124: Replace `SceneManager.LoadScene("Menu")` with `FindObjectOfType<GameOverUI>()?.Show("El Santuario ha sido destruido")` and remove the deathDelay coroutine (instant handoff to Game Over)
- **WaveSpawner.cs**: Make `currentWave` accessible (public property or serialized field)

## Visual Style
- Match Menu canvas pattern: ScreenSpaceOverlay, 1920×1080 reference
- Center-aligned panel with VT323 font
- Colors: background overlay #00000080, title #D4A017 (gold), body text #C71585 (magenta)
- Buttons use same outline/style as Menu scene buttons

## Edge Cases
- If both player and shrine die simultaneously, first trigger wins
- WaveSpawner's `currentWave` starts at 1 on fresh scenes
- `Time.timeScale = 0` freezes Rigidbody2D/AI — no unwanted physics during Game Over
