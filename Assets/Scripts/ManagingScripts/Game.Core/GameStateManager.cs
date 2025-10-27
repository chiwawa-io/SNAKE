using System;
using UnityEngine;

namespace Game.Core
{
    public enum GameState
    {
        Loading,
        MainMenu,
        Leaderboard,
        Achievements,
        InGame,
        GameOver,
        Error,
        DifficultySelect,
        DifficultySelectClose,
        Save,
        Exit
    }

    /// <summary>
    /// Manages the global game state. Acts as the single source of truth for the application's state.
    /// </summary>
    public class GameStateManager : MonoBehaviour
    {
        public static event Action<GameState> OnStateChanged;

        public GameState CurrentState { get; private set; }

        private void Start()
        {
            SetState(GameState.Loading); 
        }

        public void SetState(GameState newState)
        {
            if (CurrentState == newState)
            {
                return;
            }

            CurrentState = newState;
            Debug.Log($"[GameStateManager] State changed to: {newState}");
            OnStateChanged?.Invoke(CurrentState);
        }
    }
}