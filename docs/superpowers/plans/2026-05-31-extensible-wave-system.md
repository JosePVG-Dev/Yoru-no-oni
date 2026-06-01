# Extensible Wave System — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace hardcoded wave rules with a data-driven ScriptableObject system that supports custom wave sequences, enemy types, and boss waves.

**Architecture:** Three-layer design: `WaveConfig` (defines one wave), `WaveSequence` (ordered list of WaveConfigs), and `WaveSpawner` (consumes WaveSequence). Default auto-scaled behavior is preserved as a fallback.

**Tech Stack:** Unity 6 LTS, C#, ScriptableObjects, URP 2D

---

### Task 1: Create WaveConfig ScriptableObject

**Files:**
- Create: `Assets/Scripts/WaveConfig.cs`

- [ ] **Step 1: Write WaveConfig.cs**

```csharp
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "YoruNoOni/Wave Config", fileName = "Wave_")]
public class WaveConfig : ScriptableObject
{
    [Header("Enemies")]
    [Tooltip("Prefabs to spawn this wave (randomly selected per spawn)")]
    public List<GameObject> enemyPrefabs = new List<GameObject>();

    [Min(1)]
    public int enemyCount = 2;

    [Header("Timing")]
    [Tooltip("Seconds between each enemy spawn")]
    [Min(0.1f)]
    public float spawnInterval = 5f;

    [Tooltip("Seconds to wait after this wave before next wave")]
    [Min(0f)]
    public float postWaveDelay = 8f;

    [Header("Flags")]
    [Tooltip("Is this a boss wave? Boss spawns from both sides or center.")]
    public bool isBossWave = false;

    [Tooltip("If true, enemies must be defeated to advance; false = timer-based")]
    public bool requireDefeat = true;

    [Tooltip("Custom text shown at wave start (e.g. 'BOSS WAVE!')")]
    public string announcementText = "";
}
```

- [ ] **Step 2: Commit**

```bash
git add Assets/Scripts/WaveConfig.cs
git commit -m "feat: add WaveConfig ScriptableObject"
```

---

### Task 2: Create WaveSequence ScriptableObject

**Files:**
- Create: `Assets/Scripts/WaveSequence.cs`

- [ ] **Step 1: Write WaveSequence.cs**

```csharp
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "YoruNoOni/Wave Sequence", fileName = "WaveSequence_")]
public class WaveSequence : ScriptableObject
{
    [Tooltip("Ordered list of waves. Wave 1 = index 0.")]
    public List<WaveConfig> waves = new List<WaveConfig>();

    [Header("Auto-Generation (used when no sequence assigned)")]
    [Min(1)]
    public int baseEnemiesPerWave = 2;

    [Min(1)]
    public int enemiesIncreasePerWave = 1;

    [Min(0.5f)]
    public float baseSpawnInterval = 5f;

    [Min(0f)]
    public float spawnIntervalDecrease = 0.5f;

    [Min(1f)]
    public float minSpawnInterval = 1f;

    [Min(0f)]
    public float timeBetweenWaves = 8f;

    public GameObject defaultEnemyPrefab;

    /// <summary>
    /// Generates an auto-scaled WaveConfig for the given wave number (1-indexed).
    /// Uses the legacy hardcoded scaling formula.
    /// </summary>
    public WaveConfig GenerateWave(int waveNumber)
    {
        var config = ScriptableObject.CreateInstance<WaveConfig>();
        int count = baseEnemiesPerWave + (waveNumber - 1) * enemiesIncreasePerWave;
        float interval = Mathf.Max(minSpawnInterval, baseSpawnInterval - (waveNumber - 1) * spawnIntervalDecrease);

        config.name = $"AutoWave_{waveNumber}";
        config.enemyPrefabs = new System.Collections.Generic.List<GameObject> { defaultEnemyPrefab };
        config.enemyCount = count;
        config.spawnInterval = interval;
        config.postWaveDelay = timeBetweenWaves;
        config.isBossWave = false;
        config.requireDefeat = true;

        return config;
    }
}
```

- [ ] **Step 2: Commit**

```bash
git add Assets/Scripts/WaveSequence.cs
git commit -m "feat: add WaveSequence ScriptableObject with auto-generation"
```

---

### Task 3: Modify WaveSpawner to use WaveSequence

**Files:**
- Modify: `Assets/Scripts/WaveSpawner.cs`

- [ ] **Step 1: Read current WaveSpawner.cs**

Read the full file at `Assets/Scripts/WaveSpawner.cs` to understand the exact current implementation.

- [ ] **Step 2: Add WaveSequence field and rewrite spawn logic**

Replace the full contents of `WaveSpawner.cs`:

```csharp
using System.Collections;
using UnityEngine;
using TMPro;

public class WaveSpawner : MonoBehaviour
{
    [Header("Sequence")]
    [Tooltip("Assign a WaveSequence asset. If null, auto-generates waves using legacy scaling.")]
    public WaveSequence waveSequence;

    [Header("Spawn Points")]
    public Transform leftSpawn;
    public Transform rightSpawn;

    [Header("Initial Delay")]
    [Min(0f)]
    public float initialDelay = 2f;

    [Header("HUD")]
    public TMP_Text waveText;
    public TMP_Text waveStatusText;

    private int currentWave = 0;
    private int enemiesAlive = 0;
    private int enemiesSpawned = 0;
    private bool waveActive = false;
    private WaveConfig activeConfig;

    private void Start()
    {
        StartCoroutine(RunWaves());
    }

    private IEnumerator RunWaves()
    {
        yield return new WaitForSeconds(initialDelay);

        while (true)
        {
            currentWave++;
            waveActive = true;
            enemiesSpawned = 0;
            enemiesAlive = 0;

            activeConfig = GetWaveConfig(currentWave);

            if (!string.IsNullOrEmpty(activeConfig.announcementText))
            {
                waveText.text = activeConfig.announcementText;
            }
            else
            {
                waveText.text = "Wave " + currentWave;
            }

            yield return StartCoroutine(SpawnWave(activeConfig));

            if (activeConfig.requireDefeat)
            {
                while (enemiesAlive > 0)
                {
                    waveStatusText.text = "Enemies: " + enemiesAlive;
                    yield return null;
                }
            }

            yield return new WaitForSeconds(activeConfig.postWaveDelay);
            waveActive = false;
        }
    }

    private WaveConfig GetWaveConfig(int waveNumber)
    {
        if (waveSequence != null && waveSequence.waves != null && waveNumber <= waveSequence.waves.Count)
        {
            var config = waveSequence.waves[waveNumber - 1];
            if (config != null)
                return config;
        }

        if (waveSequence != null)
            return waveSequence.GenerateWave(waveNumber);

        return null;
    }

    private IEnumerator SpawnWave(WaveConfig config)
    {
        for (int i = 0; i < config.enemyCount; i++)
        {
            SpawnEnemy(config);
            enemiesSpawned++;
            UpdateHUD();

            yield return new WaitForSeconds(config.spawnInterval);
        }
    }

    private void SpawnEnemy(WaveConfig config)
    {
        if (config.enemyPrefabs == null || config.enemyPrefabs.Count == 0)
            return;

        Transform spawnPoint = Random.value > 0.5f ? leftSpawn : rightSpawn;
        GameObject prefab = config.enemyPrefabs[Random.Range(0, config.enemyPrefabs.Count)];
        GameObject enemy = Instantiate(prefab, spawnPoint.position, Quaternion.identity);

        var tracker = enemy.AddComponent<OniDeathTracker>();
        tracker.spawner = this;
    }

    public void OnEnemyDied()
    {
        enemiesAlive--;
        enemiesAlive = Mathf.Max(0, enemiesAlive);
    }

    private void Update()
    {
        UpdateHUD();
    }

    private void UpdateHUD()
    {
        if (waveActive)
        {
            if (activeConfig != null && activeConfig.requireDefeat)
                waveStatusText.text = "Enemies: " + enemiesAlive;
        }
    }
}

public class OniDeathTracker : MonoBehaviour
{
    public WaveSpawner spawner;
    private void OnDestroy()
    {
        if (spawner != null)
            spawner.OnEnemyDied();
    }
}
```

- [ ] **Step 3: Remove old OniDeathTracker inner class from existing file**

If `OniDeathTracker` was previously a nested class, ensure it's now a top-level class as shown above. Verify no duplicate.

- [ ] **Step 4: Wire up references in Game.unity scene**

The existing `WaveSpawner` GameObject already has `leftSpawn`, `rightSpawn`, `waveText`, `waveStatusText` references. If they break during modification, re-assign them via `manage_components`:

```json
{
  "target": "WaveSpawner",
  "component_type": "WaveSpawner",
  "property": "leftSpawn",
  "value": { "ref": { "by_name": "LeftSpawn" } }
}
```

Repeat for `rightSpawn` (RightSpawn), `waveText` (WaveText), `waveStatusText` (WaveStatus).

- [ ] **Step 5: Create default WaveSequence asset**

Use `manage_scriptable_object` action=`create`:
```json
{
  "type_name": "WaveSequence",
  "folder_path": "Assets/Settings",
  "asset_name": "DefaultWaveSequence"
}
```

Then set its `defaultEnemyPrefab` to the Oni prefab:
```json
{
  "target": { "path": "Assets/Settings/DefaultWaveSequence.asset" },
  "patches": [
    { "path": "defaultEnemyPrefab", "value": { "path": "Assets/Prefabs/Oni.prefab" } }
  ]
}
```

Assign it to WaveSpawner's `waveSequence` field.

- [ ] **Step 6: Test in Play Mode**

1. Enter Play Mode
2. Verify waves spawn correctly with auto-generated configs (same behavior as before)
3. Verify wave text and enemy counter work
4. Verify enemies are defeated and wave advances
5. Test multiple waves to verify scaling (more enemies, faster spawns)

- [ ] **Step 7: Commit**

```bash
git add Assets/Scripts/WaveSpawner.cs Assets/Scripts/WaveConfig.cs Assets/Scripts/WaveSequence.cs Assets/Settings/DefaultWaveSequence.asset
git commit -m "feat: refactor wave system to use WaveConfig/WaveSequence ScriptableObjects"
```
