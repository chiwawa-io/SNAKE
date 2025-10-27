using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

// The ActiveItem class can remain the same or be moved into this file.
public class ActiveItem
{
    public GameItem Data { get; }
    public GameObject Instance { get; }
    public ActiveItem(GameItem data, GameObject instance)
    {
        Data = data;
        Instance = instance;
    }
}


public class ItemSpawner : MonoBehaviour
{
    [Header("Item Database")]
    [SerializeField] private List<GameItem> itemTypes;

    [Header("Pattern & Spawning Logic")]
    [SerializeField] private List<SpawnPointMap> spawnPointMaps = new();
    [SerializeField] private SpawnPattern startingFoodPattern;
    [SerializeField] private SpawnPattern speedUpPattern;
    [SerializeField] private SpawnPattern invulnerabilityPattern;
    [SerializeField] private SpawnPattern regularFoodPattern;
    [SerializeField] private SpawnPattern portalsPattern;
    [SerializeField] private List<SpawnPattern> easyChallengePatterns = new();
    [SerializeField] private List<SpawnPattern> mediumChallengePatterns = new();
    [SerializeField] private List<SpawnPattern> hardChallengePatterns = new();
    [SerializeField, Range(0, 1)] private float challengeChance = 0.2f;
    /*[SerializeField] private float portalSpawnInterval = 15f;*/


    // --- State Variables ---
    private readonly Dictionary<Vector2Int, ActiveItem> _activeItems = new();
    private readonly Dictionary<string, int> _lastSpawnPointIndex = new();
    /*private readonly Dictionary<Vector2Int, Vector2Int> _portalPairs = new();*/

    private List<Vector2Int> _playerBodyPositions; // A direct reference to the player's body list
    private Vector2Int _gridBounds;
    private bool _isSpawningActive;

    // --- Current Session Patterns ---
    private SpawnPointMap _currentSpawnPointMap;
    private SpawnPattern _currentChallengePattern;

    #region Initialization & Resetting

    public void Initialize(Vector2Int gridBounds, List<Vector2Int> playerBody, string difficulty)
    {
        _gridBounds = gridBounds;
        _playerBodyPositions = playerBody;
        _isSpawningActive = true;
        
        ItemLifeSpan.OnItemDestroyed += OnItemDestroyedByTimer;

        ChoosePatternsForDifficulty(difficulty);
        
        if (startingFoodPattern != null)
        {
            SpawnPattern(startingFoodPattern);
        }

        // Start timed spawners
        StartCoroutine(SpawnSpeedUpCoroutine());
        /*StartCoroutine(SpawnPortalCoroutine());*/
        StartCoroutine(SpawnInvulnerabilityCoroutine());
    }
    
    public void ResetSpawner()
    {
        StopAllCoroutines();
        _isSpawningActive = false;
        ItemLifeSpan.OnItemDestroyed -= OnItemDestroyedByTimer;

        foreach (ActiveItem item in _activeItems.Values.ToList())
        {
            if (item.Instance != null) Destroy(item.Instance);
        }

        _activeItems.Clear();
        _lastSpawnPointIndex.Clear();
        /*_portalPairs.Clear();*/
    }

    #endregion

    #region Public API (for Player script)

    public void OnFoodCollected()
    {
        if (!_isSpawningActive) return;

        if (Random.value < challengeChance && _currentChallengePattern != null)
        {
            SpawnPattern(_currentChallengePattern);
        }
        else
        {
            SpawnPattern(regularFoodPattern);
        }
    }
    
    public ActiveItem GetItemAt(Vector2Int position) => _activeItems.GetValueOrDefault(position);

    /*public Vector2Int GetPortalExit(Vector2Int entryPosition) => _portalPairs.GetValueOrDefault(entryPosition);*/

    public void RemoveItem(Vector2Int position)
    {
        _activeItems.Remove(position);
    }

    #endregion

    #region Core Spawning Logic

    private void SpawnItem(string itemName, Vector2Int spawnPos)
    {
        GameItem itemData = itemTypes.FirstOrDefault(i => i.objName == itemName);
        if (itemData == null)
        {
            Debug.LogError($"Item '{itemName}' not found in Item Types list.");
            return;
        }

        var instance = Instantiate(itemData.prefab, (Vector2)spawnPos, Quaternion.identity);
        _activeItems[spawnPos] = new ActiveItem(itemData, instance);
    }
    
    private void SpawnPattern(SpawnPattern pattern)
    {
        if (pattern == null || _currentSpawnPointMap == null || pattern.items.Count == 0) return;

        string primaryItemName = pattern.items[0].itemName;
        List<Vector2Int> validPoints = _currentSpawnPointMap.GetPointsFor(primaryItemName);
        if (validPoints == null || validPoints.Count == 0) return;
        if (!_lastSpawnPointIndex.ContainsKey(primaryItemName)) _lastSpawnPointIndex[primaryItemName] = -1;

        // Try every valid spawn point until we find one that works
        for (int i = 0; i < validPoints.Count; i++)
        {
            int currentIndex = (_lastSpawnPointIndex[primaryItemName] + 1 + i) % validPoints.Count;
            Vector2Int anchorPos = validPoints[currentIndex];

            if (IsPatternLocationValid(pattern, anchorPos))
            {
                foreach (var item in pattern.items)
                {
                    SpawnItem(item.itemName, anchorPos + item.relativePosition);
                }
                _lastSpawnPointIndex[primaryItemName] = currentIndex;
                return;
            }
        }
    }
    
    /*private void SpawnPortalPattern()
    {
        if (portalsPattern == null || _currentSpawnPointMap == null) return;
        List<Vector2Int> allPoints = _currentSpawnPointMap.GetPointsFor("Portal");
        if (allPoints.Count < 2) return;

        for (int i = 0; i < 10; i++) // Try 10 times to find a valid spot
        {
            Vector2Int anchorPos = allPoints[Random.Range(0, allPoints.Count)];
            if (IsPatternLocationValid(portalsPattern, anchorPos))
            {
                var portalPositions = new List<Vector2Int>();
                foreach (var item in portalsPattern.items)
                {
                    Vector2Int spawnPos = anchorPos + item.relativePosition;
                    SpawnItem(item.itemName, spawnPos);
                    portalPositions.Add(spawnPos);
                }

                if (portalPositions.Count >= 2)
                {
                    _portalPairs[portalPositions[0]] = portalPositions[1];
                    _portalPairs[portalPositions[1]] = portalPositions[0];
                }
                return;
            }
        }
    }*/

    private bool IsPatternLocationValid(SpawnPattern pattern, Vector2Int anchorPos)
    {
        foreach (var item in pattern.items)
        {
            Vector2Int pos = anchorPos + item.relativePosition;
            if (Mathf.Abs(pos.x) >= _gridBounds.x || Mathf.Abs(pos.y) >= _gridBounds.y) return false;
            if (_playerBodyPositions.Contains(pos) || _activeItems.ContainsKey(pos)) return false;
        }
        return true;
    }

    private void ChoosePatternsForDifficulty(string difficulty)
    {
        T PickRandom<T>(List<T> pool) => (pool?.Count > 0) ? pool[Random.Range(0, pool.Count)] : default;

        _currentSpawnPointMap = PickRandom(spawnPointMaps);

        _currentChallengePattern = difficulty switch
        {
            "Easy" => PickRandom(easyChallengePatterns),
            "Medium" => PickRandom(mediumChallengePatterns),
            "Hard" => PickRandom(hardChallengePatterns),
            _ => PickRandom(easyChallengePatterns)
        };
    }

    // Called when an item's own lifespan timer runs out
    private void OnItemDestroyedByTimer(Vector2Int itemPos)
    {
        _activeItems.Remove(itemPos);
    }
    
    #endregion

    #region Timed Spawning Coroutines

    private IEnumerator SpawnSpeedUpCoroutine()
    {
        yield return new WaitForSeconds(7f);
        while (_isSpawningActive)
        {
            SpawnPattern(speedUpPattern);
            yield return new WaitForSeconds(14f);
        }
    }

    /*private IEnumerator SpawnPortalCoroutine()
    {
        while (_isSpawningActive)
        {
            yield return new WaitForSeconds(portalSpawnInterval);
            SpawnPortalPattern();
        }
    }*/
    
    private IEnumerator SpawnInvulnerabilityCoroutine()
    {
        yield return new WaitForSeconds(5f);
        while (_isSpawningActive)
        {
            SpawnPattern(invulnerabilityPattern);
            yield return new WaitForSeconds(25f);
        }
    }

    #endregion
}