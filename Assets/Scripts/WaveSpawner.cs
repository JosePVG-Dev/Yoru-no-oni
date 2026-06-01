using System.Collections;
using System.Collections.Generic;
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

    [Header("Reward Targets")]
    public SamuraiController samurai;
    public Shrine shrine;

    [Header("Audio")]

    private int currentWave = 0;
    private int enemiesAlive = 0;
    private bool waveActive = false;
    private WaveConfig activeConfig;
    public int CurrentWave => currentWave;
    private List<GameObject> spawnedEnemies = new List<GameObject>();
    private int remainingEnemies = 0;

    private void Start()
    {
        StartCoroutine(RunWaves());
    }

    private IEnumerator RunWaves()
    {
        yield return new WaitForSeconds(initialDelay);

        while (true)
        {
            foreach (var go in spawnedEnemies)
            {
                if (go != null)
                {
                    var tracker = go.GetComponent<OniDeathTracker>();
                    if (tracker != null) tracker.spawner = null;
                    Destroy(go);
                }
            }
            spawnedEnemies.Clear();

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
                waveText.text = "Oleada " + currentWave;
            }

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayNewWave();

            remainingEnemies = activeConfig.enemyCount;

            yield return StartCoroutine(SpawnWave(activeConfig));

            if (activeConfig.requireDefeat)
            {
                while (enemiesAlive > 0)
                {
                    yield return null;
                }
            }
            else
            {
                while (enemiesAlive > 0)
                {
                    yield return null;
                }
            }

            float postTimer = activeConfig.postWaveDelay;
            while (postTimer > 0f)
            {
                postTimer -= Time.deltaTime;
                waveStatusText.text = string.Format("Siguiente oleada: {0}s", Mathf.CeilToInt(postTimer));
                yield return null;
            }

            waveActive = false;

            if (currentWave % 4 == 0)
            {
                if (samurai != null)
                    samurai.Heal(1);
                if (shrine != null)
                    shrine.IncreaseMaxHealth(5);
            }
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

        spawnedEnemies.Add(enemy);
        enemiesAlive++;

        var enemyComp = enemy.GetComponent<Enemy>();
        if (enemyComp != null)
            enemyComp.WaveSpawner = this;

        var tracker = enemy.AddComponent<OniDeathTracker>();
        tracker.spawner = this;
    }

    public void OnEnemyDied()
    {
        enemiesAlive--;
        enemiesAlive = Mathf.Max(0, enemiesAlive);
        remainingEnemies = Mathf.Max(0, remainingEnemies - 1);
    }

    private void UpdateHUD()
    {
        if (!waveActive || activeConfig == null) return;

        if (remainingEnemies > 0)
            waveStatusText.text = "Faltan " + remainingEnemies + " onis";
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
