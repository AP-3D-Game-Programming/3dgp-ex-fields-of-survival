using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    [SerializeField] private GamePhase currentPhase = GamePhase.Plant;
    [SerializeField] private int currentRound = 1;

    [Header("Plant Phase Settings")]
    [SerializeField] private float plantPhaseDuration = 30f;
    private float plantPhaseTimer;

    [Header("Defense Phase Settings")]
    [SerializeField] private int baseEnemyCount = 5;
    [SerializeField] private float enemyCountMultiplier = 1.5f;
    [SerializeField] private Enemy[] enemyPrefabs;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float spawnDelay = 1f;

    private int remainingEnemies;
    private int enemiesToSpawn;
    private int totalEnemiesInWave;
    private bool isSpawning = false;

    [Header("Events")]
    public UnityEvent OnPlantPhaseStart;
    public UnityEvent OnDefensePhaseStart;
    public UnityEvent<int> OnRoundChanged;

    // Properties
    public GamePhase CurrentPhase => currentPhase;
    public int CurrentRound => currentRound;
    public int RemainingEnemies => remainingEnemies;
    public float PlantPhaseTimeRemaining => plantPhaseTimer;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        StartPlantPhase();
    }

    private void Update()
    {
        // Plant phase timer
        if (currentPhase == GamePhase.Plant)
        {
            plantPhaseTimer -= Time.deltaTime;

            if (plantPhaseTimer <= 0)
            {
                StartDefensePhase();
            }
        }
    }

    public void StartPlantPhase()
    {
        currentPhase = GamePhase.Plant;
        plantPhaseTimer = plantPhaseDuration;

        Debug.Log($"Plant Phase Started - Round {currentRound}");
        OnPlantPhaseStart?.Invoke();
    }

    public void StartDefensePhase()
    {
        currentPhase = GamePhase.Defense;

        // Calculate enemies for this round
        enemiesToSpawn = Mathf.RoundToInt(baseEnemyCount * Mathf.Pow(enemyCountMultiplier, currentRound - 1));
        totalEnemiesInWave = enemiesToSpawn;
        remainingEnemies = 0; // Will increase as enemies spawn

        Debug.Log($"Defense Phase Started - Round {currentRound} - Enemies: {totalEnemiesInWave}");
        OnDefensePhaseStart?.Invoke();

        // Start spawning enemies
        StartCoroutine(SpawnEnemies());
    }

    private System.Collections.IEnumerator SpawnEnemies()
    {
        isSpawning = true;

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            SpawnRandomEnemy();
            remainingEnemies++;
            yield return new WaitForSeconds(spawnDelay);
        }

        isSpawning = false;
    }

    private void SpawnRandomEnemy()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            Debug.LogError("No enemy prefabs assigned!");
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("No spawn points assigned!");
            return;
        }

        // Pick random enemy and spawn point
        Enemy randomEnemy = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        Transform randomSpawn = spawnPoints[Random.Range(0, spawnPoints.Length)];

        Instantiate(randomEnemy, randomSpawn.position, randomSpawn.rotation);
    }

    public void OnEnemyKilled()
    {
        remainingEnemies--;

        // Only complete round when all enemies spawned AND all killed
        if (remainingEnemies <= 0 && !isSpawning)
        {
            // All enemies dead, start next round
            currentRound++;
            OnRoundChanged?.Invoke(currentRound);

            Debug.Log($"All enemies defeated! Starting Round {currentRound}");
            StartPlantPhase();
        }
    }

    public bool IsPlantPhase() => currentPhase == GamePhase.Plant;
    public bool IsDefensePhase() => currentPhase == GamePhase.Defense;
}

public enum GamePhase
{
    Plant,
    Defense
}