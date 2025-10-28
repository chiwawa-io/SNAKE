using UnityEngine;
using System.Collections.Generic;
using Game.Core;

namespace Game.UI
{
    public class GameUI : MonoBehaviour
    {
        [Header("Screen Controllers")]
        [SerializeField] private BaseUIController loadingScreen;
        [SerializeField] private BaseUIController mainMenuScreen;
        [SerializeField] private BaseUIController gameScreen;
        [SerializeField] private BaseUIController leaderboardScreen;
        [SerializeField] private BaseUIController achievementsScreen;

        [Header("Panel Controllers")]
        [SerializeField] private BaseUIController errorPanel;
        [SerializeField] private BaseUIController savingPanel;

        private List<BaseUIController> _allScreens;

        private void Awake()
        {
            _allScreens = new List<BaseUIController>
            {
                loadingScreen, mainMenuScreen, gameScreen, 
                leaderboardScreen, achievementsScreen
            };
        }

        private void OnEnable()
        {
            GameStateManager.OnStateChanged += StateChange;
            LoadingComplete.LoadingCompleteAction += ShowMainMenuScreen;
        }

        private void OnDisable()
        {
            GameStateManager.OnStateChanged -= StateChange;
            LoadingComplete.LoadingCompleteAction -= ShowMainMenuScreen;
        }

        private void StateChange(GameState state)
        {
            switch (state)
            {
                case GameState.MainMenu:
                    ShowMainMenuScreen();
                    break;
                case GameState.Loading:
                    SetActiveScreen(loadingScreen);
                    break;
                case GameState.InGame:
                    SetActiveScreen(gameScreen);
                    break;
                case GameState.Leaderboard:
                    SetActiveScreen(leaderboardScreen);
                    break;
                case GameState.Achievements:
                    SetActiveScreen(achievementsScreen);
                    break;
                case GameState.Save:
                    ShowSavingPanel();
                    break;
                case GameState.Error:
                    SetActiveScreen(errorPanel);
;                   break;
            }
        }

        private void ShowMainMenuScreen()
        {
            SetActiveScreen(mainMenuScreen);
        }

        private void ShowSavingPanel()
        {
            savingPanel.Show();
        }
        private void SetActiveScreen(BaseUIController activeScreen)
        {
            foreach (var screen in _allScreens)
            {
                if (screen != activeScreen && screen.IsVisible)
                {
                    screen.Hide();
                }
            }
            
            if (activeScreen != null && !activeScreen.IsVisible)
            {
                activeScreen.Show();
            }
        }
    }
}