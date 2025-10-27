using System;
using System.Collections;
using UnityEngine;

namespace Game.Core
{
    public class GameManager : MonoBehaviour
    {
        #region Singleton

        public static GameManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        #endregion

        [Header("System References")]
        [SerializeField] private PlayerDataManager playerDataManager; // Updated from PlayerDataManager
        [SerializeField] private InactivityDetector inactivityDetector;
        [SerializeField] private NetworkManager networkManager;
        [SerializeField] private GameObject gameComponents;
        [SerializeField] private GameStateManager gameStateManager;

        // --- Events ---
        public static event Action<int> OnGameOver;
        public static event Action OnGameReset;
        public static Action<int, string> OnErrorOccurred;

        // --- Game Logic State ---
        public int CurrentScore { get; private set; }
        public string CurrentDifficulty { get; private set; }
        private float _gameTime;

        // --- Async Flow Control ---
        private bool _shouldContinue;
        private bool _canExitAfterSave;
        private bool _levelBeginRequestComplete;
        private bool _levelBeginRequestSuccess;
        private bool _dataSaveComplete;
        
        // --- Error State ---
        private int _errorCode;
        private string _errorMessage;

        #region Unity Lifecycle & Event Subscription

        private void OnEnable()
        {
            GameStateManager.OnStateChanged += HandleGameStateChanged;
            
            Player.UpdateScore += UpdateScore; 
            playerDataManager.DataSaveSuccess += OnDataSaveSuccess; // Assuming this event exists
            LoadingComplete.LoadingCompleteAction += RequestMainMenu;
            InactivityDetector.TimesUp += RequestExit;
            OnErrorOccurred += EnterErrorState;
        }

        private void OnDisable()
        {
            GameStateManager.OnStateChanged -= HandleGameStateChanged;

            Player.UpdateScore -= UpdateScore;
            playerDataManager.DataSaveSuccess -= OnDataSaveSuccess;
            LoadingComplete.LoadingCompleteAction -= RequestMainMenu;
            InactivityDetector.TimesUp -= RequestExit;
            OnErrorOccurred -= EnterErrorState;
        }

        private void Update()
        {
            // Check current state via the manager
            if (gameStateManager.CurrentState == GameState.InGame)
            {
                _gameTime += Time.deltaTime;
            }
        }

        #endregion

        #region State Change Handler

        private void HandleGameStateChanged(GameState newState)
        {
            inactivityDetector.StopDetector(); // Stop by default, re-enable in specific states

            switch (newState)
            {
                case GameState.MainMenu:
                    inactivityDetector.StartDetector();
                    gameComponents.SetActive(false);
                    break;
                case GameState.Achievements:
                case GameState.Leaderboard:
                    inactivityDetector.StartDetector();
                    break;
                case GameState.InGame:
                    if (!_shouldContinue) ResetGameLogic();
                    OnGameReset?.Invoke();
                    gameComponents.SetActive(true);
                    break;
                case GameState.GameOver:
                    // achievementManager.AddExperience(Mathf.RoundToInt(_gameTime));
                    break;
            }
        }

        #endregion

        #region Public Methods (Game Flow Control)

        public void SetDifficulty(string difficulty)
        {
            CurrentDifficulty = difficulty;
            RequestStartGame();
        }

        public void RequestStartGame()
        {
            StartCoroutine(LevelBeginRoutine());
        }

        public void RequestContinueGame()
        {
            _shouldContinue = true;
            gameStateManager.SetState(GameState.InGame);
        }


        public void TriggerGameOver()
        {
            if (gameStateManager.CurrentState != GameState.InGame) return;
            OnGameOver?.Invoke(CurrentScore);
            gameStateManager.SetState(GameState.GameOver);
        }

        public void RequestRestartGame(bool shouldExit)
        {
            _canExitAfterSave = shouldExit;
            StartCoroutine(LevelEndRoutine());
        }

        private void RequestMainMenu() => gameStateManager.SetState(GameState.MainMenu);

        private void RequestExit() => networkManager.WebSocketService.BackToSystem();

        public void RequestExitWithError()
        {
            networkManager.WebSocketService.BackToSystemWithError(_errorCode.ToString(), _errorMessage);
        }
        
        #endregion

        #region Private Logic
        
        private void EnterErrorState(int code, string message)
        {
            _errorCode = code;
            _errorMessage = message;
            gameStateManager.SetState(GameState.Error);
        }

        private void ResetGameLogic()
        {
            CurrentScore = 0;
            _gameTime = 0;
        }

        private void UpdateScore(int pointsToAdd, Vector2 position)
        {
            CurrentScore += pointsToAdd;
        }

        #endregion

        #region Asynchronous Routines

        private IEnumerator LevelBeginRoutine()
        {
            gameStateManager.SetState(GameState.Loading);
            
            _levelBeginRequestComplete = false;
            _levelBeginRequestSuccess = false;
            networkManager.WebSocketCommandHandler.SendLevelBeginRequestCommand(0, OnLevelBeginSuccess, OnLevelBeginFail);

            yield return new WaitUntil(() => _levelBeginRequestComplete);

            if (_levelBeginRequestSuccess)
            {
                gameStateManager.SetState(GameState.InGame);
            }
        }

        private void OnLevelBeginSuccess()
        {
            _levelBeginRequestSuccess = true;
            _levelBeginRequestComplete = true;
        }

        private void OnLevelBeginFail(int code, string message)
        {
            _levelBeginRequestSuccess = false;
            _levelBeginRequestComplete = true;
            EnterErrorState(code, message);
        }

        private IEnumerator LevelEndRoutine()
        {
            gameStateManager.SetState(GameState.Loading);
            _dataSaveComplete = false;
            
            networkManager.WebSocketCommandHandler.SendLevelEndRequestCommand(0, CurrentScore, OnLevelEndSuccess, OnLevelEndFail);
            
            yield return new WaitUntil(() => _dataSaveComplete);
            
            if (_levelBeginRequestSuccess && !_canExitAfterSave)
            {
                StartCoroutine(LevelBeginRoutine());
            }
            else if (_canExitAfterSave)
            {
                RequestExit();
            }
        }

        private void OnLevelEndSuccess()
        {
            Debug.LogWarning("LevelEnd reported to server successfully.");
            _dataSaveComplete = true; 
        }

        private void OnLevelEndFail(int code, string message)
        {
            EnterErrorState(code, message);
            _dataSaveComplete = true; 
        }

        private void OnDataSaveSuccess()
        {
        }

        #endregion
    }
}