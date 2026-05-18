# Yoru no Oni — AGENTS.md

Unity 6 LTS (6000.3.11f1) • 2D pixel art • URP 2D Renderer

## Visual Style Guide

**Theme**: Dark fantasy Japanese folklore — samurai vs oni (demon)

### Art Direction
- **Style**: 2D pixel art, high-contrast, dark atmosphere
- **Color palette**:
  - Primary: Deep purple (#2D1B4E), magenta (#C71585), dark blue (#1A0A2E)
  - Accents: Gold (#D4A017) for horns/details, cyan (#4A90D9) for effects
  - Shadows: Near-black with purple tint
- **Sprites**: Pixel-perfect, no anti-aliasing (`filterMode: Point`)
- **Lighting**: Hard shadows only (`SoftShadowsSupported: 0`), no post-processing, no color grading

### Characters
| Character | Description |
|-----------|-------------|
| **Oni** (enemy) | Purple/magenta skin, golden horns, dark muscular body, purple-blue hair/accents. Sprite sheet: 12 frames (Idle×2, Walk×4, Jump×3, Death×3) |
| **Samurai** (player) | Traditional Japanese warrior. Single idle sprite so far |

### Environment
- **Backgrounds**: Full-screen static images (Game_Background.png, Menu.png)
- **Camera**: Orthographic, size 5. Backgrounds scaled to fill screen
- **No parallax** or dynamic environment elements yet

### Rendering Settings
- **Pipeline**: URP 2D Renderer (`Renderer2D.asset`)
- **MSAA**: 1x (no anti-aliasing — preserves pixel art crispness)
- **Shadows**: Hard only, 2048px main light shadowmap
- **No HDR**, no color grading, no post-processing volumes
- **Sprite import**: `filterMode: Point`, `mipmapEnabled: false`, `compressionQuality: 0` (uncompressed)

### Sprite Sheet Conventions
- **Grid layout**: 4 columns × variable rows, 384×256 base cell size
- **Bounds**: Tight to content, uniform within each animation cycle
- **Pivot**: `BottomCenter` (y=0) for consistent ground alignment
- **Collisions**: Use `CapsuleCollider2D` or `BoxCollider2D`, NOT `PolygonCollider2D` from sprite outline

## Project structure

```
Assets/
  Scenes/
    Menu.unity          ← Build index 0 (main menu)
    Game.unity          ← Build index 1 (gameplay)
  Scripts/UI/
    MainMenu.cs         ← PlayGame() / QuitGame()
  Sprites/
    Oni_Sheet.png       ← Enemy sprite sheet (12 frames: Idle×2, Walk×4, Jump×3, Death×3)
    Samurai_Idle.png    ← Player idle (single)
    Game_Background.png ← Game scene background
    Fondo.png           ← Alternative background
    Menu.png            ← Menu scene background (2549×1649)
  Settings/
    UniversalRP.asset   ← URP pipeline settings (hard shadows, no AA)
    Renderer2D.asset    ← 2D renderer config
  Screenshots/          ← Game captures
```

## Dev commands

No build/lint scripts exist yet. All work is done via Unity Editor + MCP.

## Architecture notes

- **Input System**: `activeInputHandler=1` (Input System Package). Legacy `Input` class (e.g. `Input.mousePosition`) will throw `InvalidOperationException`. Use `InputSystemUIInputModule` on the EventSystem, never `StandaloneInputModule`.
- **Render pipeline**: URP with 2D Renderer (`Assets/Settings/Renderer2D.asset`).
- **Camera**: Orthographic, size 5. Background is scaled via `Math.Max(camWidth/spriteWidth, camHeight/spriteHeight)` to fill screen.
- **UI**: ScreenSpaceOverlay Canvas with `ScaleWithScreenSize` at 1920×1080 reference.
- **Scene flow**: `Menu` → (Jugar) → `Game`, `Menu` → (Salir) → quit. Both in Build Settings.
- **`Application.Quit()`** does nothing in Editor. Use `#if UNITY_EDITOR / EditorApplication.isPlaying = false`.

## MCP tool quirks

### Canvas creation
- **Do NOT try to compile `UnityEngine.GraphicRaycaster` in `execute_code`** — CodeDom compiler can't resolve it. Create Canvas via `manage_gameobject` with `components_to_add: ["Canvas","CanvasScaler","GraphicRaycaster"]`, then configure render mode + scaler + children via `execute_code`.
- Canvas RectTransform anchor changes made via `execute_code` **do not persist** across scene save/reload. The Canvas auto-manages its RectTransform in ScreenSpaceOverlay mode. This is normal — the Canvas renders at the camera's `pixelRect` resolution regardless of anchor values.

### Button onClick persistence
- Runtime `AddListener(() => ...)` lambdas are non-persistent. To make onClick survive scene saves, wire buttons via `SerializedObject` on `m_OnClick.m_PersistentCalls.m_Calls`.
- As a fallback, `MainMenu.Start()` auto-wires buttons with `GameObject.Find("PlayButton/QuitButton")`.

### EventSystem
- Every UI scene needs `EventSystem` + `InputSystemUIInputModule`. Without it, button clicks are silently ignored.

### Scene operations
- `manage_scene save` fails with "cannot be used during play mode". Stop play mode first with `manage_editor stop`.
- `manage_asset move` / `rename` can fail silently. Use `execute_code` with `AssetDatabase.RenameAsset()` as fallback.

### execute_code compiler
- Use `compiler=codedom` (Roslyn is not installed). CodeDom is C# 6 only; many Unity.UI types require `manage_gameobject` instead.

## Resolution

- Target: 1920×1080. Set in Game View dropdown (not saved in project — per-session).
- Player Settings → Resolution → Default Width/Height: set to 1920×1080 for builds.

## Saved fixes

1. **UI buttons invisible**: Canvas was in WorldSpace mode + wrong RectTransform → recreated as ScreenSpaceOverlay.
2. **Buttons unresponsive**: Missing EventSystem → added with InputSystemUIInputModule.
3. **`InvalidOperationException: You are trying to read Input using UnityEngine.Input`**: StandaloneInputModule conflicts with Input System → swapped to InputSystemUIInputModule.
4. **`OnClick` lost after scene save**: Used SerializedObject to set persistent calls on Button components.
