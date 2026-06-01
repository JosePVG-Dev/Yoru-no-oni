# Samurai Attack Animation — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Create a 5-frame attack animation for the Samurai using the new sprites (`1 Attack.PNG` through `5 Attack.PNG`) and wire it into the existing Attack state in the Animator Controller.

**Architecture:** Create a new `Samurai_Attack.anim` AnimationClip with the 5 sprites sequenced at 10 fps. Update `Samurai.controller`'s Attack state to reference the new clip. The existing trigger-based attack flow in `SamuraiController.cs` remains unchanged.

**Tech Stack:** Unity 6 LTS, Mecanim Animator, URP 2D

---

### Task 1: Create the Attack AnimationClip

**Files:**
- Create/Overwrite: `Assets/Animations/Samurai_Attack.anim`
- Modify: `Assets/Animations/Samurai.controller`

- [ ] **Step 1: Import the 5 attack sprites properly**

Using Unity MCP, verify the 5 sprites are imported correctly:
- `Assets/Sprites/Samurai/1 Attack.PNG`
- `Assets/Sprites/Samurai/2 Attack.PNG`
- `Assets/Sprites/Samurai/3 Attack.PNG`
- `Assets/Sprites/Samurai/4 Attack.PNG`
- `Assets/Sprites/Samurai/5 Attack.PNG`

Ensure each has `filterMode: Point`, `mipmapEnabled: false`, `pixelsToUnits: 100`, and is set as Sprite (2D and UI).

Run: use `manage_asset` action=`get_info` on each sprite to verify import settings.

- [ ] **Step 2: Create/overwrite Samurai_Attack.anim using manage_animation**

Use `manage_animation` action=`clip_create` with:
- `clip_path`: `Assets/Animations/Samurai_Attack.anim`
- `properties`:
```json
{
  "frameRate": 10,
  "wrapMode": "Once",
  "sprites": [
    "Assets/Sprites/Samurai/1 Attack.PNG",
    "Assets/Sprites/Samurai/2 Attack.PNG",
    "Assets/Sprites/Samurai/3 Attack.PNG",
    "Assets/Sprites/Samurai/4 Attack.PNG",
    "Assets/Sprites/Samurai/5 Attack.PNG"
  ]
}
```

If `clip_create` supports sprites as paths, use that. Otherwise use individual keyframe additions via `clip_*` actions or `execute_code`:

```csharp
var clip = new AnimationClip { frameRate = 10f, wrapMode = WrapMode.Once };
var binding = new EditorCurveBinding
{
    path = "",
    type = typeof(SpriteRenderer),
    propertyName = "m_Sprite"
};
var frames = new ObjectReferenceKeyframe[5];
var sprites = new string[] {
    "Assets/Sprites/Samurai/1 Attack.PNG",
    "Assets/Sprites/Samurai/2 Attack.PNG",
    "Assets/Sprites/Samurai/3 Attack.PNG",
    "Assets/Sprites/Samurai/4 Attack.PNG",
    "Assets/Sprites/Samurai/5 Attack.PNG"
};
for (int i = 0; i < 5; i++)
{
    frames[i] = new ObjectReferenceKeyframe
    {
        time = i / 10f,
        value = AssetDatabase.LoadAssetAtPath<Sprite>(sprites[i])
    };
}
AnimationUtility.SetObjectReferenceCurve(clip, binding, frames);
AssetDatabase.CreateAsset(clip, "Assets/Animations/Samurai_Attack.anim");
AssetDatabase.SaveAssets();
```

- [ ] **Step 3: Update Samurai.controller Attack state**

Use `manage_animation` action=`controller_*` to update the Attack state:
- Open the controller at `Assets/Animations/Samurai.controller`
- In the Attack state, set `Motion` to `Assets/Animations/Samurai_Attack.anim`

Or via `manage_prefabs` / `manage_animation`:
```json
{
  "action": "controller_set_state_motion",
  "controller_path": "Assets/Animations/Samurai.controller",
  "properties": {
    "stateName": "Attack",
    "motionPath": "Assets/Animations/Samurai_Attack.anim"
  }
}
```

- [ ] **Step 4: Verify Attack state transitions**

The Attack state in Samurai.controller has:
- Entry: AnyState → Attack on Attack trigger
- Exit: Attack → Idle after exit time or when grounded

Verify the exit time allows the full 0.5s animation (5 frames at 10 fps). The state's `speed` should be 1.0 and `exitTime` should be at least 0.5.

- [ ] **Step 5: Test in Play Mode**

1. Enter Play Mode in `Game.unity`
2. Press the Attack button (X key or left click)
3. Verify the 5-frame animation plays completely before returning to Idle
4. Verify the attack still hits enemies (damage cone + OverlapCircle)
5. Verify you can attack while walking and the animation plays correctly

- [ ] **Step 6: Commit**

```bash
git add Assets/Animations/Samurai_Attack.anim Assets/Animations/Samurai.controller
git commit -m "feat: add 5-frame Samurai attack animation with new sprites"
```
