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

    [Tooltip("Time limit in seconds for timer-based waves (requireDefeat=false)")]
    [Min(0f)]
    public float timeLimit = 60f;

    [Tooltip("Custom text shown at wave start (e.g. 'BOSS WAVE!')")]
    public string announcementText = "";
}