using UnityEngine;
using UnityEngine.UI;
using Game.Core;

namespace Game.Buttons{
    public class ButtonManager : MonoBehaviour
    {
        [Header("Basic")]
        [SerializeField] private GameStateManager gameStateManager;
        [SerializeField] private GameManager gameManager;
        
        [Header("Menu Buttons")]
        [SerializeField] private Button menuStartButton;
        [SerializeField] private Button menuLeaderboardButton;
        [SerializeField] private Button menuAchievementButton;
        [SerializeField] private Button menuExitButton;
        [Space]
        [SerializeField] private Button easyDifficultyButton;
        [SerializeField] private Button mediumDifficultyButton;
        [SerializeField] private Button hardDifficultyButton;
        [SerializeField] private Button returnDifficultyButton;
        
        [Header("Achievements Buttons")]
        [SerializeField] private Button achievementReturnButton;
        
        [Header("Leaderboard Buttons")]
        [SerializeField] private Button leaderboardReturnButton;
        
        [Header("Error Panel Buttons")]
        [SerializeField] private Button errorPanelExit;
        
        private void OnEnable()
        {
            menuStartButton.onClick.AddListener(() => gameStateManager.SetState(GameState.DifficultySelect));
            menuLeaderboardButton.onClick.AddListener(() => gameStateManager.SetState(GameState.Leaderboard));
            menuAchievementButton.onClick.AddListener(() => gameStateManager.SetState(GameState.Achievements));
            menuExitButton.onClick.AddListener(() => gameStateManager.SetState(GameState.Exit));
            easyDifficultyButton.onClick.AddListener(() => gameManager.SetDifficulty("Easy"));
            mediumDifficultyButton.onClick.AddListener(() => gameManager.SetDifficulty("Medium"));
            hardDifficultyButton.onClick.AddListener(() => gameManager.SetDifficulty("Hard"));
            returnDifficultyButton.onClick.AddListener(() => gameStateManager.SetState(GameState.DifficultySelectClose));
            achievementReturnButton.onClick.AddListener(() => gameStateManager.SetState(GameState.MainMenu));
            leaderboardReturnButton.onClick.AddListener(() => gameStateManager.SetState(GameState.MainMenu));
            errorPanelExit.onClick.AddListener(() => gameManager.RequestExitWithError());
        }

        private void OnDisable()
        {
            menuStartButton.onClick.RemoveListener(() => gameStateManager.SetState(GameState.DifficultySelect));
            menuLeaderboardButton.onClick.RemoveListener(() => gameStateManager.SetState(GameState.Leaderboard));
            menuAchievementButton.onClick.RemoveListener(() => gameStateManager.SetState(GameState.Achievements));
            menuExitButton.onClick.RemoveListener(() => gameStateManager.SetState(GameState.Exit));
            easyDifficultyButton.onClick.RemoveListener(() => gameManager.SetDifficulty("Easy"));
            mediumDifficultyButton.onClick.RemoveListener(() => gameManager.SetDifficulty("Medium"));
            hardDifficultyButton.onClick.RemoveListener(() => gameManager.SetDifficulty("Hard"));
            achievementReturnButton.onClick.RemoveListener(() => gameStateManager.SetState(GameState.MainMenu));
            leaderboardReturnButton.onClick.RemoveListener(() => gameStateManager.SetState(GameState.MainMenu));
            errorPanelExit.onClick.AddListener(() => gameManager.RequestExitWithError()); 
        }
    }
}
