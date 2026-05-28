using UnityEngine;
using TMPro;

public class WaveSpawner : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private GameObject oniPrefab;

    [Header("Spawn Points")]
    [SerializeField] private Transform leftSpawn;
    [SerializeField] private Transform rightSpawn;

    [Header("Wave Settings")]
    [SerializeField] private int baseEnemiesPerWave = 2;
    [SerializeField] private float baseSpawnInterval = 5f;
    [SerializeField] private float timeBetweenWaves = 8f;
    [SerializeField] private int enemiesIncreasePerWave = 1;
    [SerializeField] private float spawnIntervalDecrease = 0.5f;

    [Header("HUD")]
    [SerializeField] private TMP_Text waveText;
    [SerializeField] private TMP_Text waveStatusText;

    private int currentWave = 0;
    private int enemiesToSpawn;
    private int enemiesSpawned;
    private int enemiesAlive;
    private float spawnTimer;
    private float waveTimer;
    private float initialDelay = 2f;
    private bool waveActive;
    private bool waitingBetweenWaves;

    private void Start()
    {
        Debug.Log("[WaveSpawner] Start() called");
        if (oniPrefab == null)
        {
            Debug.LogError("[WaveSpawner] Oni Prefab not assigned!");
            return;
        }
        Debug.Log("[WaveSpawner] oniPrefab=" + oniPrefab.name);
    }

    private void Update()
    {
        if (initialDelay > 0f)
        {
            initialDelay -= Time.deltaTime;
            if (initialDelay <= 0f)
            {
                Debug.Log("[WaveSpawner] Initial delay complete, starting wave");
                StartNextWave();
            }
            return;
        }

        if (waveActive)
        {
            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0f && enemiesSpawned < enemiesToSpawn)
            {
                SpawnOni();
                spawnTimer = GetCurrentSpawnInterval();
            }
        }

        if (waitingBetweenWaves)
        {
            waveTimer -= Time.deltaTime;
            if (waveTimer <= 0f)
            {
                waitingBetweenWaves = false;
                StartNextWave();
            }
            UpdateWaveStatusTimer();
        }
    }

    private void StartNextWave()
    {
        currentWave++;
        enemiesToSpawn = baseEnemiesPerWave + (currentWave - 1) * enemiesIncreasePerWave;
        enemiesSpawned = 0;
        enemiesAlive = 0;
        spawnTimer = 0f;
        waveActive = true;

        UpdateHUD();
        Debug.Log("[WaveSpawner] Wave " + currentWave + " started! " + enemiesToSpawn + " enemies");
    }

    private void SpawnOni()
    {
        Transform spawnPoint = GetSpawnPoint();
        if (spawnPoint == null)
        {
            Debug.LogError("[WaveSpawner] No spawn point!");
            return;
        }

        Debug.Log("[WaveSpawner] Spawning Oni at " + spawnPoint.position);
        GameObject oni = Instantiate(oniPrefab, spawnPoint.position, Quaternion.identity);
        enemiesSpawned++;
        enemiesAlive++;

        OniDeathTracker tracker = oni.AddComponent<OniDeathTracker>();
        tracker.spawner = this;
    }

    private Transform GetSpawnPoint()
    {
        bool useLeft = Random.value < 0.5f;
        if (useLeft && leftSpawn != null) return leftSpawn;
        if (!useLeft && rightSpawn != null) return rightSpawn;
        return leftSpawn ?? rightSpawn;
    }

    private float GetCurrentSpawnInterval()
    {
        return Mathf.Max(1f, baseSpawnInterval - (currentWave - 1) * spawnIntervalDecrease);
    }

    public void OnEnemyDied()
    {
        enemiesAlive--;
        CheckWaveComplete();
    }

    private void CheckWaveComplete()
    {
        if (enemiesSpawned >= enemiesToSpawn && enemiesAlive <= 0)
        {
            waveActive = false;
            waitingBetweenWaves = true;
            waveTimer = timeBetweenWaves;

            if (waveStatusText != null)
                waveStatusText.text = "Wave " + currentWave + " Complete!";

            Debug.Log("[WaveSpawner] Wave " + currentWave + " complete!");
        }
    }

    private void UpdateWaveStatusTimer()
    {
        if (waveStatusText != null)
            waveStatusText.text = "Next Wave in " + Mathf.CeilToInt(waveTimer) + "s";
    }

    private void UpdateHUD()
    {
        if (waveText != null)
            waveText.text = "Wave " + currentWave;
        if (waveStatusText != null)
            waveStatusText.text = "Enemies: " + enemiesToSpawn;
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
