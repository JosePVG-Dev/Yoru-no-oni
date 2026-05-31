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

        config.name = "AutoWave_" + waveNumber;
        config.enemyPrefabs = new System.Collections.Generic.List<GameObject> { defaultEnemyPrefab };
        config.enemyCount = count;
        config.spawnInterval = interval;
        config.postWaveDelay = timeBetweenWaves;
        config.isBossWave = false;
        config.requireDefeat = true;

        return config;
    }
}