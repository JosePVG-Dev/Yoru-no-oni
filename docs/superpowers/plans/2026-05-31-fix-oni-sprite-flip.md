# Fix Oni Sprite Flip — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Fix the Oni sprite facing the wrong direction by switching from `localScale.x` flipping to `SpriteRenderer.flipX`.

**Architecture:** Replace the `transform.localScale.x` sign flip in `OniAI.MoveTowardsTarget()` with `SpriteRenderer.flipX`. Add a cached `SpriteRenderer` reference for the flipX property.

**Tech Stack:** Unity 6 LTS, C#, URP 2D

---

### Task 1: Add SpriteRenderer reference and replace flip logic

**Files:**
- Modify: `Assets/Scripts/OniAI.cs`

- [ ] **Step 1: Read current OniAI.cs to identify exact lines to change**

Read the file at `Assets/Scripts/OniAI.cs` and locate the `MoveTowardsTarget()` method and the field declarations.

- [ ] **Step 2: Add `SpriteRenderer spriteRenderer` field**

Near the top fields declaration, add:

```csharp
private SpriteRenderer spriteRenderer;
```

- [ ] **Step 3: Cache SpriteRenderer in Awake/Start**

In `Start()` or `Awake()`, add:

```csharp
spriteRenderer = GetComponent<SpriteRenderer>();
```

- [ ] **Step 4: Replace flip logic in MoveTowardsTarget()**

Find the existing localScale flip code in `MoveTowardsTarget()` and replace it.

**Current code pattern** (looks like):
```csharp
if (direction.x > 0)
    transform.localScale = new Vector3(-absScale, scale.y, scale.z);
else if (direction.x < 0)
    transform.localScale = new Vector3(absScale, scale.y, scale.z);
```

**Replace with:**
```csharp
if (spriteRenderer != null)
{
    spriteRenderer.flipX = direction.x < 0;
}
```

- [ ] **Step 5: Set default facing direction**

The Oni walks right by default. `flipX = false` means facing right. `flipX = true` means facing left (mirrored). Verify this matches: Oni walks toward Shrine (center), so Oni spawned on left side walks right (flipX=false), Oni spawned on right side walks left (flipX=true).

If the sprite is still inverted, swap the condition: `spriteRenderer.flipX = direction.x > 0;`

- [ ] **Step 6: Verify in Unity Editor**

1. Enter Play Mode in `Game.unity`
2. Wait for an Oni to spawn from both left and right spawn points
3. Verify the Oni faces the direction it walks (toward the Shrine in center)
4. Verify the Oni turns to face the Samurai when fighting

- [ ] **Step 7: Commit**

```bash
git add Assets/Scripts/OniAI.cs
git commit -m "fix: replace Oni localScale flip with SpriteRenderer.flipX"
```
