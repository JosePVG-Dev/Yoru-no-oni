# Damage Cone + Oni Health Bars — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a visual cyan semi-transparent attack cone to the Samurai and health bars above Oni enemies.

**Architecture:** Two new self-contained MonoBehaviour components (`DamageCone`, `HealthBar`) each generating their visuals procedurally (mesh + material for cone; textured sprites for health bars). No external assets required. `SamuraiController` gains a serialized reference to call `DamageCone.Show()` on attack. `Enemy` exposes `maxHealth` for `HealthBar` to read.

**Tech Stack:** Unity 6 LTS, URP 2D Renderer, C# 6 (CodeDom compiler)

---

### Task 1: Create `DamageCone.cs`

**Files:**
- Create: `Assets/Scripts/DamageCone.cs`

- [ ] **Step 1: Write the script**

```csharp
using UnityEngine;
using System.Collections;

public class DamageCone : MonoBehaviour
{
    [SerializeField] private int coneSegments = 20;
    [SerializeField] private float displayDuration = 0.2f;
    [SerializeField] private float fadeDuration = 0.12f;

    private float coneRange = 2f;
    private float coneAngle = 60f;
    private Color coneColor = new Color(0.29f, 0.57f, 0.85f, 0.35f);
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Material coneMaterial;
    private Coroutine showCoroutine;

    private void Awake()
    {
        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshRenderer = gameObject.AddComponent<MeshRenderer>();

        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();

        coneMaterial = new Material(Shader.Find("Sprites/Default"));
        coneMaterial.mainTexture = tex;
        coneMaterial.color = coneColor;
        meshRenderer.material = coneMaterial;
        meshRenderer.sortingOrder = 5;
        meshRenderer.enabled = false;

        GenerateMesh();
    }

    public void Initialize(float range, float angleDegrees)
    {
        coneRange = range;
        coneAngle = angleDegrees;
        GenerateMesh();
    }

    private void GenerateMesh()
    {
        int vertexCount = coneSegments + 2;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[coneSegments * 3];

        vertices[0] = Vector3.zero;

        float halfAngle = coneAngle * 0.5f * Mathf.Deg2Rad;
        float angleStep = (coneAngle * Mathf.Deg2Rad) / coneSegments;
        float startAngle = -halfAngle;

        for (int i = 0; i <= coneSegments; i++)
        {
            float angle = startAngle + angleStep * i;
            vertices[i + 1] = new Vector3(Mathf.Cos(angle) * coneRange, Mathf.Sin(angle) * coneRange, 0f);
        }

        for (int i = 0; i < coneSegments; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = i + 2;
        }

        Mesh mesh = new Mesh { name = "DamageCone" };
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        meshFilter.mesh = mesh;
    }

    public void Show()
    {
        if (showCoroutine != null)
            StopCoroutine(showCoroutine);
        showCoroutine = StartCoroutine(ShowRoutine());
    }

    private IEnumerator ShowRoutine()
    {
        meshRenderer.enabled = true;
        Color c = coneMaterial.color;
        c.a = coneColor.a;
        coneMaterial.color = c;

        yield return new WaitForSeconds(displayDuration);

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(coneColor.a, 0f, elapsed / fadeDuration);
            coneMaterial.color = c;
            yield return null;
        }

        meshRenderer.enabled = false;
    }

    private void OnDestroy()
    {
        if (coneMaterial != null)
            Destroy(coneMaterial);
    }
}
```

- [ ] **Step 2: Validate the script compiles**

Run: MCP `unityMCP_validate_script` with `uri="Assets/Scripts/DamageCone.cs"` and `level="standard"`.
Expected: no errors.

- [ ] **Step 3: Refresh Unity**

Run: MCP `unityMCP_refresh_unity` with `mode="if_dirty"`, `scope="all"`, `compile="request"`, `wait_for_ready=true`.
Expected: compilation succeeds with no errors.

---

### Task 2: Create `HealthBar.cs`

**Files:**
- Create: `Assets/Scripts/HealthBar.cs`

- [ ] **Step 1: Write the script**

```csharp
using UnityEngine;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private float barWidth = 1.2f;
    [SerializeField] private float barHeight = 0.12f;
    [SerializeField] private float yOffset = 1.8f;
    [SerializeField] private Color bgColor = new Color(0.1f, 0.04f, 0.18f, 0.8f);
    [SerializeField] private Color fillColor = new Color(0.78f, 0.08f, 0.52f, 0.9f);

    private Enemy enemy;
    private SpriteRenderer bgRenderer;
    private SpriteRenderer fillRenderer;
    private Transform fillTransform;

    private void Awake()
    {
        enemy = GetComponentInParent<Enemy>();
        if (enemy == null)
        {
            Debug.LogWarning($"[HealthBar] No Enemy component found in parent of {name}");
            Destroy(gameObject);
            return;
        }

        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();

        Sprite bgSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 100f);

        GameObject bgObj = new GameObject("BG");
        bgObj.transform.SetParent(transform, false);
        bgObj.transform.localPosition = new Vector3(0f, yOffset, 0f);
        bgRenderer = bgObj.AddComponent<SpriteRenderer>();
        bgRenderer.sprite = bgSprite;
        bgRenderer.color = bgColor;
        bgRenderer.sortingOrder = 5;
        bgObj.transform.localScale = new Vector3(barWidth, barHeight, 1f);

        Sprite fillSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0f, 0.5f), 100f);

        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(bgObj.transform, false);
        fillObj.transform.localPosition = new Vector3(-0.5f, 0f, 0f);
        fillRenderer = fillObj.AddComponent<SpriteRenderer>();
        fillRenderer.sprite = fillSprite;
        fillRenderer.color = fillColor;
        fillRenderer.sortingOrder = 6;
        fillTransform = fillObj.transform;
        fillTransform.localScale = Vector3.one;
    }

    private void Update()
    {
        if (enemy == null) return;

        float pct = Mathf.Clamp01((float)enemy.Health / enemy.MaxHealth);
        Vector3 s = fillTransform.localScale;
        s.x = pct;
        fillTransform.localScale = s;
    }
}
```

- [ ] **Step 2: Validate the script compiles**

Run: MCP `unityMCP_validate_script` with `uri="Assets/Scripts/HealthBar.cs"` and `level="standard"`.
Expected: no errors.

- [ ] **Step 3: Refresh Unity**

Run: MCP `unityMCP_refresh_unity` with `mode="if_dirty"`, `scope="all"`, `compile="request"`, `wait_for_ready=true`.
Expected: compilation succeeds with no errors.

---

### Task 3: Add `maxHealth` field to `Enemy.cs`

**Files:**
- Modify: `Assets/Scripts/Enemy.cs`

- [ ] **Step 1: Add `maxHealth` field, `Health` and `MaxHealth` properties**

Replace the field declaration and `Awake`:

**Before:**
```csharp
    [SerializeField] private int health = 3;
    protected Animator animator;

    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void TakeDamage(int amount)
    {
        health -= amount;
```

**After:**
```csharp
    [SerializeField] private int maxHealth = 3;
    private int health;
    protected Animator animator;

    public int Health => health;
    public int MaxHealth => maxHealth;

    protected virtual void Awake()
    {
        health = maxHealth;
        animator = GetComponent<Animator>();
    }

    public void TakeDamage(int amount)
    {
        health -= amount;
```

The `TakeDamage` body, `Die()`, and `DieRoutine()` remain unchanged.

- [ ] **Step 2: Validate the script compiles**

Run: MCP `unityMCP_validate_script` with `uri="Assets/Scripts/Enemy.cs"` and `level="standard"`.
Expected: no errors.

- [ ] **Step 3: Refresh Unity**

Run: MCP `unityMCP_refresh_unity` with `mode="if_dirty"`, `scope="all"`, `compile="request"`, `wait_for_ready=true`.
Expected: compilation succeeds.

---

### Task 4: Wire `DamageCone` into `SamuraiController.cs`

**Files:**
- Modify: `Assets/Scripts/SamuraiController.cs`

- [ ] **Step 1: Add the serialized field**

Add after the existing Attack header fields (line 23):

```csharp
    [SerializeField] private DamageCone damageCone;
```

- [ ] **Step 2: Initialize the DamageCone in Awake**

Insert after `if (dashGhostTrail != null)` block (after line 67):

```csharp
        if (damageCone != null)
        {
            damageCone.Initialize(attackRange, attackConeAngle);
            damageCone.transform.localPosition = attackOriginOffset;
        }
```

- [ ] **Step 3: Call damageCone.Show() in OnAttack**

Insert at the beginning of `OnAttack()`, before the animator trigger (before line 159):

```csharp
        if (damageCone != null)
            damageCone.Show();
```

- [ ] **Step 4: Validate the script compiles**

Run: MCP `unityMCP_validate_script` with `uri="Assets/Scripts/SamuraiController.cs"` and `level="standard"`.
Expected: no errors.

- [ ] **Step 5: Refresh Unity**

Run: MCP `unityMCP_refresh_unity` with `mode="if_dirty"`, `scope="all"`, `compile="request"`, `wait_for_ready=true`.
Expected: compilation succeeds.

---

### Task 5: Add `DamageCone` child to Samurai in the scene

**Scene:** `Assets/Scenes/Game.unity`

- [ ] **Step 1: Create DamageCone child GameObject**

Run: MCP `unityMCP_manage_gameobject`:
```json
{
  "action": "create",
  "name": "AttackCone",
  "parent": "Samurai",
  "position": [0, 0, 0],
  "components_to_add": ["DamageCone"]
}
```

- [ ] **Step 2: Assign DamageCone reference in SamuraiController**

Run: MCP `unityMCP_manage_components`:
```json
{
  "action": "set_property",
  "target": "Samurai",
  "search_method": "by_name",
  "component_type": "SamuraiController",
  "property": "damageCone",
  "value": "AttackCone"
}
```
Specify the target as the AttackCone instance ID found from Step 1.

- [ ] **Step 3: Save the scene**

Run: MCP `unityMCP_manage_scene` with `action="save"`.

- [ ] **Step 4: Take a screenshot to verify**

Run: MCP `unityMCP_manage_camera` with `action="screenshot"` from Game view to confirm no errors.

---

### Task 6: Add `HealthBar` to Oni prefab

**Prefab:** The Oni prefab assigned in `WaveSpawner.oniPrefab`

- [ ] **Step 1: Find the Oni prefab**

Run: MCP `unityMCP_manage_asset` with `action="search"`, `path="Assets"`, `filter_type="Prefab"`, `search_pattern="Oni"`.
Note the prefab path (e.g., `Assets/Prefabs/Oni.prefab`).

- [ ] **Step 2: Add HealthBar component to the Oni prefab**

Run: MCP `unityMCP_manage_prefabs`:
```json
{
  "action": "modify_contents",
  "prefab_path": "<path from Step 1>",
  "components_to_add": ["HealthBar"]
}
```

- [ ] **Step 3: Verify the prefab hierarchy**

Run: MCP `unityMCP_manage_prefabs` with `action="get_hierarchy"` and `prefab_path="<path from Step 1>"`.
Expected: Oni prefab now has a `HealthBar` component.

- [ ] **Step 4: Save and refresh**

Run: MCP `unityMCP_refresh_unity` with `mode="force"`, `scope="all"`, `compile="request"`, `wait_for_ready=true`.

---

### Task 7: Integration test — play mode verification

- [ ] **Step 1: Clear the console**

Run: MCP `unityMCP_read_console` with `action="clear"`.

- [ ] **Step 2: Enter play mode**

Run: MCP `unityMCP_manage_editor` with `action="play"`.

- [ ] **Step 3: Wait a few seconds, then attack**

Wait 5 seconds for enemies to spawn. Then press the Attack key (X) via `unityMCP_execute_code` — or just take a screenshot.

Run: MCP `unityMCP_manage_camera` with `action="screenshot"`, `include_image=true`.
Expected: If an Oni is on screen, a health bar should be visible above it.

- [ ] **Step 4: Check console for errors**

Run: MCP `unityMCP_read_console` with `action="get"`, `types=["error","warning"]`.
Expected: no errors or warnings related to DamageCone or HealthBar.

- [ ] **Step 5: Stop play mode**

Run: MCP `unityMCP_manage_editor` with `action="stop"`.

- [ ] **Step 6: Save scene if modified**

Run: MCP `unityMCP_manage_scene` with `action="save"`.
