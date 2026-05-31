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
        if (config == null) yield break;

        for (int i = 0; i < config.enemyCount; i++)
        {
            SpawnEnemy(config);
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

    private void UpdateHUD()
    {
        if (waveActive && activeConfig != null && activeConfig.requireDefeat)
        {
            waveStatusText.text = "Enemies: " + enemiesAlive;
        }
    }

    private void Update()
    {
        UpdateHUD();
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
