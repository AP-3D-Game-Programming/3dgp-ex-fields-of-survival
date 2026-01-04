using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    [SerializeField] private GamePhase currentPhase = GamePhase.Plant;
    [SerializeField] private int currentRound = 1;
    [SerializeField] private Skybox skyboxController;

    [Header("Plant Phase Settings")]
    [SerializeField] private float plantPhaseDuration = 30f;
    private float plantPhaseTimer;

    [Header("Defense Phase Settings")]
    [SerializeField] private int baseEnemyCount = 5;
    [SerializeField] private float enemyCountMultiplier = 1.5f;
    [SerializeField] private Enemy[] enemyPrefabs;
    [SerializeField] private float spawnDelay = 1f;
    [SerializeField] private float nightTransitionTime = 5f;

    [Header("Field Settings")]
    [SerializeField] private Transform fieldCenter;
    [SerializeField] private Vector2 fieldSize = new Vector2(50f, 50f);
    [SerializeField] private FieldSide protectedSide = FieldSide.North;
    [SerializeField] private float spawnDistance = 5f;
    [SerializeField] private bool useGroundCheck = true;
    [SerializeField] private LayerMask groundLayer;

    private int remainingEnemies;
    private int enemiesToSpawn;
    private int totalEnemiesInWave;
    private bool isSpawning = false;
    private bool nightTransitionStarted = false;

    [Header("Events")]
    public UnityEvent OnPlantPhaseStart;
    public UnityEvent OnDefensePhaseStart;
    public UnityEvent<int> OnRoundChanged;

    [Header("Game Over")]
    [SerializeField] private bool isGameOver = false;
    [SerializeField] private GameObject gameOverCanvas;
    public UnityEvent OnGameOver;

    // Properties
    public GamePhase CurrentPhase => currentPhase;
    public int CurrentRound => currentRound;
    public int RemainingEnemies => remainingEnemies;
    public float PlantPhaseTimeRemaining => plantPhaseTimer;
    public bool IsGameOver => isGameOver;

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

            if (!nightTransitionStarted && plantPhaseTimer <= nightTransitionTime)
            {
                nightTransitionStarted = true;
                skyboxController.SetNight();
                Debug.Log("Night transition started...");
            }

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

        skyboxController.SetDay();

        Debug.Log($"Plant Phase Started - Round {currentRound}");
        OnPlantPhaseStart?.Invoke();
    }

    public void StartDefensePhase()
    {
        currentPhase = GamePhase.Defense;

        nightTransitionStarted = false;

        // Calculate enemies for this round
        enemiesToSpawn = Mathf.RoundToInt(baseEnemyCount * Mathf.Pow(enemyCountMultiplier, currentRound - 1));
        totalEnemiesInWave = enemiesToSpawn;
        remainingEnemies = 0;

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
        CheckWaveComplete();
    }

    private void SpawnRandomEnemy()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            Debug.LogError("No enemy prefabs assigned!");
            return;
        }

        if (fieldCenter == null)
        {
            Debug.LogError("No field center assigned!");
            return;
        }

        // Pick random enemy
        Enemy randomEnemy = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

        // Generate random position along one of the 3 open sides
        Vector3 spawnPosition = GetRandomSpawnPosition();

        // Make the enemy look towards the field center
        Vector3 directionToCenter = (fieldCenter.position - spawnPosition).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(directionToCenter.x, 0, directionToCenter.z));

        // Spawn the enemy
        Instantiate(randomEnemy, spawnPosition, lookRotation);
    }

    private Vector3 GetRandomSpawnPosition()
    {
        Vector3 centerPos = fieldCenter.position;
        Vector3 spawnPosition = centerPos;

        // Decide the 3 available spawn axis (except the protected axis)
        FieldSide[] availableSides = GetAvailableSpawnSides();

        // Choose a random axis
        FieldSide chosenSide = availableSides[Random.Range(0, availableSides.Length)];

        // Calculate position on chosen axis
        float halfWidth = fieldSize.x / 2f;
        float halfDepth = fieldSize.y / 2f;

        int maxAttempts = 20;
        int attempts = 0;

        do
        {
            switch (chosenSide)
            {
                case FieldSide.North: // +Z side
                    spawnPosition = centerPos + new Vector3(
                        Random.Range(-halfWidth, halfWidth),
                        0,
                        halfDepth + spawnDistance
                    );
                    break;

                case FieldSide.South: // -Z side
                    spawnPosition = centerPos + new Vector3(
                        Random.Range(-halfWidth, halfWidth),
                        0,
                        -(halfDepth + spawnDistance)
                    );
                    break;

                case FieldSide.East: // +X side
                    spawnPosition = centerPos + new Vector3(
                        halfWidth + spawnDistance,
                        0,
                        Random.Range(-halfDepth, halfDepth)
                    );
                    break;

                case FieldSide.West: // -X side
                    spawnPosition = centerPos + new Vector3(
                        -(halfWidth + spawnDistance),
                        0,
                        Random.Range(-halfDepth, halfDepth)
                    );
                    break;
            }

            // If ground check enabled, find real ground height
            if (useGroundCheck)
            {
                RaycastHit hit;
                if (Physics.Raycast(spawnPosition + Vector3.up * 100f, Vector3.down, out hit, 200f, groundLayer))
                {
                    spawnPosition = hit.point;
                }
            }

            attempts++;

        } while (attempts < maxAttempts && !IsValidSpawnPosition(spawnPosition));

        return spawnPosition;
    }

    private FieldSide[] GetAvailableSpawnSides()
    {
        // Return all sides except the protected side
        System.Collections.Generic.List<FieldSide> sides = new System.Collections.Generic.List<FieldSide>();

        if (protectedSide != FieldSide.North) sides.Add(FieldSide.North);
        if (protectedSide != FieldSide.South) sides.Add(FieldSide.South);
        if (protectedSide != FieldSide.East) sides.Add(FieldSide.East);
        if (protectedSide != FieldSide.West) sides.Add(FieldSide.West);

        return sides.ToArray();
    }

    private bool IsValidSpawnPosition(Vector3 position)
    {
        // Check if the spawn position has an obstacle
        if (Physics.CheckSphere(position, 1f))
        {
            return false;
        }

        // if ground check enabled, check if there's ground underneath
        if (useGroundCheck)
        {
            RaycastHit hit;
            if (!Physics.Raycast(position + Vector3.up * 2f, Vector3.down, out hit, 10f, groundLayer))
            {
                return false;
            }
        }

        return true;
    }

    public void OnEnemyKilled()
    {
        remainingEnemies--;
        CheckWaveComplete();
    }

    private void CheckWaveComplete()
    {
        // Only complete round when all enemies spawned + killed
        if (remainingEnemies <= 0 && !isSpawning)
        {
            // All enemies dead, start next round
            currentRound++;
            OnRoundChanged?.Invoke(currentRound);

            StartPlantPhase();
        }
    }

    public bool IsPlantPhase() => currentPhase == GamePhase.Plant;
    public bool IsDefensePhase() => currentPhase == GamePhase.Defense;

    public void GameOver()
    {
        if (isGameOver) return;

        isGameOver = true;

        Debug.Log("GAME OVER - Barn Destroyed!");

        Time.timeScale = 0f;

        if (gameOverCanvas != null)
        {
            gameOverCanvas.SetActive(true);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        OnGameOver?.Invoke();
    }

    //draws the spawn zones in the scene view
    private Vector3[] GetProtectedEdge(Vector3 center, float halfWidth, float halfDepth, FieldSide side)
    {
        Vector3[] edge = new Vector3[2];

        switch (side)
        {
            case FieldSide.North:
                edge[0] = center + new Vector3(-halfWidth, 0, halfDepth);
                edge[1] = center + new Vector3(halfWidth, 0, halfDepth);
                break;

            case FieldSide.South:
                edge[0] = center + new Vector3(-halfWidth, 0, -halfDepth);
                edge[1] = center + new Vector3(halfWidth, 0, -halfDepth);
                break;

            case FieldSide.East:
                edge[0] = center + new Vector3(halfWidth, 0, -halfDepth);
                edge[1] = center + new Vector3(halfWidth, 0, halfDepth);
                break;

            case FieldSide.West:
                edge[0] = center + new Vector3(-halfWidth, 0, -halfDepth);
                edge[1] = center + new Vector3(-halfWidth, 0, halfDepth);
                break;
        }

        return edge;
    }

    private Vector3[] GetSpawnZoneCorners(Vector3 center, float halfWidth, float halfDepth, FieldSide side)
    {
        Vector3[] corners = new Vector3[4];

        switch (side)
        {
            case FieldSide.North:
                corners[0] = center + new Vector3(-halfWidth, 0, halfDepth);
                corners[1] = center + new Vector3(halfWidth, 0, halfDepth);
                corners[2] = center + new Vector3(halfWidth, 0, halfDepth + spawnDistance);
                corners[3] = center + new Vector3(-halfWidth, 0, halfDepth + spawnDistance);
                break;

            case FieldSide.South:
                corners[0] = center + new Vector3(-halfWidth, 0, -halfDepth);
                corners[1] = center + new Vector3(halfWidth, 0, -halfDepth);
                corners[2] = center + new Vector3(halfWidth, 0, -(halfDepth + spawnDistance));
                corners[3] = center + new Vector3(-halfWidth, 0, -(halfDepth + spawnDistance));
                break;

            case FieldSide.East:
                corners[0] = center + new Vector3(halfWidth, 0, -halfDepth);
                corners[1] = center + new Vector3(halfWidth, 0, halfDepth);
                corners[2] = center + new Vector3(halfWidth + spawnDistance, 0, halfDepth);
                corners[3] = center + new Vector3(halfWidth + spawnDistance, 0, -halfDepth);
                break;

            case FieldSide.West:
                corners[0] = center + new Vector3(-halfWidth, 0, -halfDepth);
                corners[1] = center + new Vector3(-halfWidth, 0, halfDepth);
                corners[2] = center + new Vector3(-(halfWidth + spawnDistance), 0, halfDepth);
                corners[3] = center + new Vector3(-(halfWidth + spawnDistance), 0, -halfDepth);
                break;
        }

        return corners;
    }

    // Debug visualization in Scene view
    private void OnDrawGizmosSelected()
    {
        if (fieldCenter == null) return;

        Vector3 center = fieldCenter.position;
        float halfWidth = fieldSize.x / 2f;
        float halfDepth = fieldSize.y / 2f;

        // Draw the field (green)
        Gizmos.color = Color.green;
        Vector3[] fieldCorners = new Vector3[4]
        {
            center + new Vector3(-halfWidth, 0, -halfDepth),
            center + new Vector3(halfWidth, 0, -halfDepth),
            center + new Vector3(halfWidth, 0, halfDepth),
            center + new Vector3(-halfWidth, 0, halfDepth)
        };

        for (int i = 0; i < 4; i++)
        {
            Gizmos.DrawLine(fieldCorners[i], fieldCorners[(i + 1) % 4]);
        }

        // Draw spawn zones (red) on all 3 open sides
        Gizmos.color = Color.red;
        FieldSide[] availableSides = GetAvailableSpawnSides();

        foreach (FieldSide side in availableSides)
        {
            Vector3[] spawnCorners = GetSpawnZoneCorners(center, halfWidth, halfDepth, side);

            for (int i = 0; i < 4; i++)
            {
                Gizmos.DrawLine(spawnCorners[i], spawnCorners[(i + 1) % 4]);
            }
        }

        // draw the protected side (blauw = barn kant) - only the edge line
        Gizmos.color = Color.blue;
        Vector3[] barnEdge = GetProtectedEdge(center, halfWidth, halfDepth, protectedSide);
        Gizmos.DrawLine(barnEdge[0], barnEdge[1]);
    }
}

public enum GamePhase
{
    Plant,
    Defense
}

public enum FieldSide
{
    North,  // +Z
    South,  // -Z
    East,   // +X
    West    // -X
}