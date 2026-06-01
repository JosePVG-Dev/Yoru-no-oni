using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "YoruNoOni/Wave Sequence", fileName = "WaveSequence_")]
public class WaveSequence : ScriptableObject
{
    [Tooltip("Ordered list of waves. Wave 1 = index 0.")]
    public List<WaveConfig> waves = new List<WaveConfig>();

    [Header("Auto-Generation (used when no sequence assigned)")]
    [Min(1)]
    public int baseEnemiesPerWave = 4;

    [Min(1)]
    public int enemiesIncreasePerWave = 1;

    [Min(0.5f)]
    public float baseSpawnInterval = 2.5f;

    [Min(0f)]
    public float spawnIntervalDecrease = 0.5f;

    [Min(1f)]
    public float minSpawnInterval = 1f;

    [Min(0f)]
    public float timeBetweenWaves = 8f;

    public List<GameObject> enemyPrefabs = new List<GameObject>();

    /// <summary>
    /// Generates an auto-scaled WaveConfig for the given wave number (1-indexed).
    /// Wave type cycles: 0=Balanced(Onis), 1=Fast(Rápidos), 2=Tank(Tanques), 3=Jumper(Saltarines).
    /// </summary>
    public WaveConfig GenerateWave(int waveNumber)
    {
        var config = ScriptableObject.CreateInstance<WaveConfig>();
        int baseCount = baseEnemiesPerWave + (waveNumber - 1) * enemiesIncreasePerWave;
        float interval = Mathf.Max(minSpawnInterval, baseSpawnInterval - (waveNumber - 1) * spawnIntervalDecrease);

        if (enemyPrefabs == null || enemyPrefabs.Count == 0)
        {
            Debug.LogWarning($"[WaveSequence] enemyPrefabs list is empty. Using default empty config for wave {waveNumber}.");
            config.name = "AutoWave_" + waveNumber;
            config.enemyPrefabs = new System.Collections.Generic.List<GameObject>();
            config.enemyCount = 0;
            config.spawnInterval = interval;
            config.postWaveDelay = timeBetweenWaves;
            config.isBossWave = false;
            config.requireDefeat = true;
            config.announcementText = "";
            return config;
        }

        int waveTypeIndex = (waveNumber - 1) % enemyPrefabs.Count;
        int prefabIndex = waveTypeIndex < enemyPrefabs.Count ? waveTypeIndex : 0;
        GameObject selectedPrefab = enemyPrefabs[prefabIndex];
        string announcement = GetWaveTypeName(waveNumber);
        int countMultiplier;

        switch (waveTypeIndex % 4)
        {
            case 0: countMultiplier = 1; break;
            case 1: countMultiplier = 3; break;
            case 2: countMultiplier = 1; break;
            case 3: countMultiplier = 2; break;
            default: countMultiplier = 1; break;
        }

        int count = baseCount * countMultiplier;

        config.name = "AutoWave_" + waveNumber + "_" + announcement.Replace("¡", "").Replace("!", "");
        config.enemyPrefabs = new List<GameObject> { selectedPrefab };
        config.enemyCount = count;
        config.spawnInterval = interval;
        config.postWaveDelay = timeBetweenWaves;
        config.isBossWave = false;
        config.requireDefeat = true;
        config.announcementText = announcement;

        return config;
    }

    /// <summary>
    /// Returns a localized display name for the wave type at the given wave number.
    /// </summary>
    public string GetWaveTypeName(int waveNumber)
    {
        int waveTypeIndex = (waveNumber - 1) % 4;
        switch (waveTypeIndex)
        {
            case 0: return "¡Onis!";
            case 1: return "¡Rápidos!";
            case 2: return "¡Tanques!";
            case 3: return "¡Saltarines!";
            default: return "¡Onis!";
        }
    }
}