using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game.Core;

public class Player : MonoBehaviour
{
    #region Configuration
    [Header("Managing scripts")]
    [SerializeField] private ItemSpawner itemSpawner;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private AudioManager audioManager;
    
    [Header("Snake Settings")]
    [SerializeField] private float frequency = 0.1f;
    [SerializeField] private float visualLerpSpeed = 8; 
    [SerializeField] private float powerUpSpeed = 0.05f;
    [SerializeField] private int maxHp = 3;
    [SerializeField] private float respawnInvulnerabilityTime = 2f; 
    [SerializeField] private Vector2 gridSize;
    
    [Header("Snake visuals")]
    [SerializeField] private GameObject snakeHeadVisual;
    [SerializeField] private GameObject snakeTailVisual;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private GameObject boomEffect;
    #endregion

    #region State Variables
    private readonly List<Vector2Int> _snakeBodyPositions = new();

    private Vector2Int _moveDirection, _nextMoveDirection;
    private Quaternion _headRotation;
    private bool _isGameOver;
    private Coroutine _speedUpCoroutine;
    private int _xBound, _yBound;
    
    private float _moveTimer; 
    private Vector2Int _lastTailPosition; 

    // Respawn 
    private bool _isPowerUpInvulnerable;
    private bool _isRespawnInvulnerable;
    private bool _isMovementPaused; 

    private WaitForSeconds _snakeDelay;

    private int _initMaxHp;
    private string _currentDifficulty;
    
    //Achievements
    private int _gemsCount;
    private int _speedUpCount;
    #endregion

    #region Actions
    public static event Action<int, Vector2> UpdateScore;
    public static event Action<int> UpdateLife;
    public static event Action<string, Vector2> EventMessages;
    public static event Action OnHitAction;
    public static event Action<string> AudioPlay;
    public static event Action<string> OnAchieved;
    public static event Action OnPreciousFoodEaten;
    #endregion

    #region UnityLifecycle

    private void OnEnable()
    {
        _initMaxHp = maxHp;
        
        GameManager.OnGameReset += ResetPlayer;
        _currentDifficulty = gameManager.CurrentDifficulty;
    }

    private void Start() => Init();

    private void Update()
    {
        if (_moveTimer < frequency)
        {
            _moveTimer += Time.deltaTime;
        }
        
        if (!_isGameOver)
        {
            InputHandler();
            UpdateVisual();
        }
    }

    private void Init()
    {

        _xBound = Mathf.RoundToInt(gridSize.x / 2);
        _yBound = Mathf.RoundToInt(gridSize.y / 2);

        ResetSnakeState();

        itemSpawner.Initialize(new Vector2Int(_xBound, _yBound), _snakeBodyPositions, _currentDifficulty);

        StartCoroutine(WaitAndMoveRoutine());
    }

    private void OnDisable()
    {
        GameManager.OnGameReset -= ResetPlayer;
    }

    #endregion
    
    #region Movement & Core Logic

    private void ResetPlayer()
    {
        _isGameOver = false;
        maxHp = _initMaxHp;
        

        ResetSnakeState(); 
        
        itemSpawner.Initialize(new Vector2Int(_xBound, _yBound), _snakeBodyPositions, _currentDifficulty);
        
        StartCoroutine(WaitAndMoveRoutine());
    }

    private void InputHandler()
    {
        if (_isMovementPaused) return; // Block input while paused

        var moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        
        if (moveInput.x > 0 && _moveDirection != Vector2Int.left) { _nextMoveDirection = Vector2Int.right; }
        else if (moveInput.x < 0 && _moveDirection != Vector2Int.right) { _nextMoveDirection = Vector2Int.left; }
        else if (moveInput.y > 0 && _moveDirection != Vector2Int.down) { _nextMoveDirection = Vector2Int.up; }
        else if (moveInput.y < 0 && _moveDirection != Vector2Int.up) { _nextMoveDirection = Vector2Int.down; }
    }

    private void Move()
    {
        _moveDirection = _nextMoveDirection;
        var currentHead = _snakeBodyPositions[0];

        var newHeadPos = currentHead + _moveDirection;
        
        if (_snakeBodyPositions.Count > 0)
        {
            _lastTailPosition = _snakeBodyPositions.Last();
        }

        if (!_isRespawnInvulnerable)
        {
            if (Mathf.Abs(newHeadPos.x) > _xBound || Mathf.Abs(newHeadPos.y) > _yBound)
            {
                OnHit(); return;
            }

            for (int i = 1; i < _snakeBodyPositions.Count; i++)
            {
                if (newHeadPos == _snakeBodyPositions[i])
                {
                    OnHit(); return;
                }
            }
        }

        var snakeGrew = false;
        var item = itemSpawner.GetItemAt(newHeadPos);
        
        if (item != null && !item.Data.isPortal)
        {
            snakeGrew = HandleItemInteraction(item);
        }

        _snakeBodyPositions.Insert(0, newHeadPos);

        if (!snakeGrew)
        {
            _snakeBodyPositions.RemoveAt(_snakeBodyPositions.Count - 1);
        }
        else
        {
            if (_snakeBodyPositions.Count >= 60)
            {
                OnAchieved?.Invoke("Size_xxl");
                Debug.LogWarning("OnAchieved Size_xxl");
            }
            else if (_snakeBodyPositions.Count >= 40)
            {
                OnAchieved?.Invoke("Size_xx");
                Debug.LogWarning("OnAchieved Size_xx");
            }
            else if (_snakeBodyPositions.Count >= 25)
            {
                OnAchieved?.Invoke("Size_x");
                Debug.LogWarning("OnAchieved Size_x");
            }

        }
    }

    private void UpdateVisual()
    {
        if (_snakeBodyPositions.Count < 2) return;

        float lerpProgress = Mathf.Clamp01(_moveTimer / frequency);

        List<Vector3> visualPoints = new List<Vector3>();

        Vector2 visualHeadPos = Vector2.Lerp(_snakeBodyPositions[1], _snakeBodyPositions[0], lerpProgress);
        snakeHeadVisual.transform.position = visualHeadPos;
        visualPoints.Add(visualHeadPos);
        
        Vector2 headDirection = _snakeBodyPositions[0] - _snakeBodyPositions[1];
        float headAngle = Mathf.Atan2(headDirection.y, headDirection.x) * Mathf.Rad2Deg;
        Quaternion targetHeadRotation = Quaternion.Euler(0, 0, headAngle - 90);
        snakeHeadVisual.transform.rotation = Quaternion.Slerp(snakeHeadVisual.transform.rotation, targetHeadRotation, visualLerpSpeed * Time.deltaTime);

        for (int i = 1; i < _snakeBodyPositions.Count - 1; i++)
        {
            Vector2 prevPos = _snakeBodyPositions[i + 1];
            Vector2 currentPos = _snakeBodyPositions[i];
            visualPoints.Add(Vector2.Lerp(prevPos, currentPos, lerpProgress));
        }

        Vector2 visualTailPos = Vector2.Lerp(_lastTailPosition, _snakeBodyPositions.Last(), lerpProgress);
        snakeTailVisual.transform.position = visualTailPos;
        visualPoints.Add(visualTailPos);

        Vector2 tailDirection = (_snakeBodyPositions[^2] - _snakeBodyPositions.Last());
        float tailAngle = Mathf.Atan2(tailDirection.y, tailDirection.x) * Mathf.Rad2Deg;
        Quaternion targetTailRotation = Quaternion.Euler(0, 0, tailAngle - 90);
        snakeTailVisual.transform.rotation = Quaternion.Slerp(snakeTailVisual.transform.rotation, targetTailRotation, visualLerpSpeed * Time.deltaTime * 2);

        lineRenderer.positionCount = visualPoints.Count;
        lineRenderer.SetPositions(visualPoints.ToArray());
    }
    #endregion

    #region Collision, Death, and Respawn
    private void LifeCheck()
    {
        maxHp--;
        UpdateLife?.Invoke(maxHp); 

        if (maxHp > 0)
        {
            StartCoroutine(RespawnRoutine());
        }
        else
        {
            // No lives left, game over.
            OnGameOver();
            StopAllCoroutines();
            itemSpawner.ResetSpawner();
        }
    }

    private bool HandleItemInteraction(ActiveItem item)
    {
        var data = item.Data;
        Vector2 itemPosition = item.Instance.transform.position;

        itemSpawner.RemoveItem(Vector2Int.RoundToInt(itemPosition));
        Destroy(item.Instance);

        if (data.isCollectible)
        {
            _gemsCount++;
            
            if (_gemsCount >= 40) OnAchieved?.Invoke("FoodC_xx");
            else if (_gemsCount >= 25) OnAchieved?.Invoke("FoodC_x");
            else if (_gemsCount >= 15)
            {
                OnAchieved?.Invoke("FoodC_m");
                Debug.LogWarning("OnAchieved FoodC_m");
            }

            
            if (data.objName == "PreciousFood")
            {
                AudioPlay?.Invoke("preciousFoodCollect");
                OnPreciousFoodEaten?.Invoke();
            }
            else
                AudioPlay?.Invoke("foodCollect");
            
            UpdateScore?.Invoke(data.scoreValue * _snakeBodyPositions.Count, itemPosition);
            itemSpawner.OnFoodCollected();
            return true;
        }

        if (data.isObstacle && !_isPowerUpInvulnerable && !_isRespawnInvulnerable)
        {
            OnHit();

            if (data.objName == "Bomb")
            {
                AudioPlay?.Invoke("explode");
                OnAchieved?.Invoke("InstaDie");
            }
            else
                AudioPlay?.Invoke("rockHit");
            
            return false; 
        }

        if (data.isPowerUp)
        {
            ApplyPowerUp(data.effectType, data.effectDuration, itemPosition);
        }
        return false;
    }

    private void OnHit()
    {
        OnHitAction?.Invoke(); 
        AudioPlay?.Invoke("gameOver");
        LifeCheck();
    }

    private void OnGameOver()
    {
        _isGameOver = true;
        AudioPlay?.Invoke("gameOver");
        gameManager.TriggerGameOver();
        OnHitAction?.Invoke();
    }

    private void ResetSnakeState()
    {
        snakeHeadVisual.SetActive(true);
        snakeTailVisual.SetActive(true);
        lineRenderer.gameObject.SetActive(true);
        boomEffect.SetActive(false);
        
        SpeedChanger(frequency);

        _snakeBodyPositions.Clear();

        _snakeBodyPositions.Add(new Vector2Int(0, -5)); // Head position
        _snakeBodyPositions.Add(new Vector2Int(0, -6)); // "Neck" position
        _snakeBodyPositions.Add(new Vector2Int(0, -7)); // Body position
        _snakeBodyPositions.Add(new Vector2Int(0, -8)); // Tail position

        // Reset movement and rotation
        _moveDirection = Vector2Int.up;
        _nextMoveDirection = _moveDirection;
    }
    #endregion

    #region Item & Snake Management
    private void ApplyPowerUp(PowerUpEffectType effectType, float duration, Vector2 position)
    {
        AudioPlay?.Invoke("speedUp");
        switch (effectType)
        {
            case PowerUpEffectType.SpeedUp:
                if (_speedUpCoroutine != null) StopCoroutine(_speedUpCoroutine);
                _speedUpCoroutine = StartCoroutine(SpeedUpRoutine(duration, position));
                _speedUpCount++;
                if (_speedUpCount >= 40) OnAchieved?.Invoke("SpeedC_xxl");
                else if (_speedUpCount >= 25) OnAchieved?.Invoke("SpeedC_xx");
                else if (_speedUpCount >= 10) OnAchieved?.Invoke("SpeedC_x");
                else if (_speedUpCount >= 3) OnAchieved?.Invoke("SpeedC_m");
                break;
            case PowerUpEffectType.Invulnerable:
                StartCoroutine(InvulnerabilityRoutine(duration, position));
                break;
        }
    }

    private void SpeedChanger(float frequencyV) => _snakeDelay = new WaitForSeconds(frequencyV);
    #endregion

    #region Coroutines
    private IEnumerator WaitAndMoveRoutine()
    {
        while (!_isGameOver)
        {
            yield return _snakeDelay;
            
            _moveTimer = 0f;
            
            if (!_isMovementPaused) 
            {
                Move();
            }
        }
    }

    private IEnumerator RespawnRoutine()
    {
        _isMovementPaused = true;
        _speedUpCoroutine = null;

        snakeHeadVisual.SetActive(false);
        snakeTailVisual.SetActive(false);
        lineRenderer.gameObject.SetActive(false);
        
        boomEffect.transform.position = (Vector2)_snakeBodyPositions[0];
        boomEffect.SetActive(true);
        
        yield return new WaitForSeconds(1f); 

        ResetSnakeState(); 

        // temporary invulnerability
        EventMessages?.Invoke("Respawning!", _snakeBodyPositions[0]);
        _isRespawnInvulnerable = true;
        _isMovementPaused = false;
        OnHitAction?.Invoke(); 

        yield return new WaitForSeconds(respawnInvulnerabilityTime);

        _isRespawnInvulnerable = false;
    }

    private IEnumerator SpeedUpRoutine(float duration, Vector2 pos)
    {
        EventMessages?.Invoke("Speed Up", pos);
        SpeedChanger(powerUpSpeed);

        yield return new WaitForSeconds(duration);
        AudioPlay?.Invoke("speedDown");
        EventMessages?.Invoke("Speed Down", _snakeBodyPositions[0]);
        SpeedChanger(frequency);
        _speedUpCoroutine = null;
    }
    private IEnumerator InvulnerabilityRoutine(float duration, Vector2 pos)
    {
        EventMessages?.Invoke("Invulnerable", pos);
        _isPowerUpInvulnerable = true;

        yield return new WaitForSeconds(duration);
        AudioPlay?.Invoke("speedDown");
        EventMessages?.Invoke("Not invulnerable", _snakeBodyPositions[0]);
        _isPowerUpInvulnerable = false;
    }
    #endregion
}