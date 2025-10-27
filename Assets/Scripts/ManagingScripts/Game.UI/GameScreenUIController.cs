// GameHUDController.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Game.Core;

namespace Game.UI
{
    public class GameHUDController : BaseUIController
    {
        [Header("Basic")]
        [SerializeField] private GameManager gameManager;
        
        [Header("HUD Elements")]
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private List<GameObject> lifeIcons;
        [SerializeField] private int lifeCount = 3;
        
        [Header("Floating Text")]
        [SerializeField] private TextMeshProUGUI addedScoreText;
        
        [Header("Notifications")]
        [SerializeField] private GameObject achievementUINotification;
        [SerializeField] private TextMeshProUGUI achievementNameText;
        
        [Header("GameOver")]
        [SerializeField] private GameObject gameOverText;
        [Space]
        [SerializeField] private GameObject gameOverPopup;
        [Space]
        [SerializeField] private TextMeshProUGUI bestScoreText;
        [SerializeField] private TextMeshProUGUI yourScoreText;

        private int _currentScore;

        private void OnEnable()
        {
            Player.UpdateScore += OnScoreUpdated;
            Player.UpdateLife += OnLifeUpdated;
            AchievGameListener.OnAchievementCompleted += OnAchievementCompleted;
            GameManager.OnGameReset += ResetUI;
            GameManager.OnGameOver += OnGameOver;
        }

        private void OnDisable()
        {
            Player.UpdateScore -= OnScoreUpdated;
            Player.UpdateLife -= OnLifeUpdated;
            AchievGameListener.OnAchievementCompleted -= OnAchievementCompleted;
            GameManager.OnGameReset -= ResetUI;
            GameManager.OnGameOver -= OnGameOver;
        }
        
        protected override void OnShow()
        {
            base.OnShow();
            
            ResetUI();
        }

        private void ResetUI()
        {
            _currentScore = gameManager.CurrentScore;
            scoreText.text = _currentScore.ToString("D10");
            gameOverPopup.SetActive(false);
            OnLifeUpdated(lifeCount); // starting lives is 3
        }

        private void OnScoreUpdated(int scoreToAdd, Vector2 screenPosition)
        {
            _currentScore += scoreToAdd * 100;
            scoreText.text = _currentScore.ToString("D10");
            
            ShowFloatingText($"+{scoreToAdd*100}", screenPosition);
        }
        
        private void OnLifeUpdated(int currentLives)
        {
            currentLives = Mathf.Max(0, currentLives);
            for (var i = 0; i < lifeIcons.Count; i++)
            {
                lifeIcons[i].SetActive(i < currentLives);
            }
        }

        private void OnAchievementCompleted(string achievementName)
        {
            achievementNameText.text = achievementName;
            StartCoroutine(ShowAndHide(achievementUINotification, 2.0f));
        }

        private void ShowFloatingText(string text, Vector2 position)
        {
            addedScoreText.gameObject.transform.position = position;
            addedScoreText.text = text;
            StartCoroutine(ShowAndHide(addedScoreText.gameObject, 0.5f));
        }

        private void OnGameOver(int yourScore)
        {
            Debug.LogWarning("GameOverUIController.OnGameOver");
            // var bestScore = playerDataManager.BestScore;
            //
            // StartCoroutine(ShowAndHide(gameOverText, gameOverTextAnimationTime, true));
            // bestScoreText.text = bestScore.ToString("D10");
            // yourScoreText.text = yourScore.ToString("D10");
        }
        
        private IEnumerator ShowAndHide(GameObject obj, float duration, bool gameOver = false)
        {
            obj.SetActive(true);
            yield return new WaitForSeconds(duration);
            obj.SetActive(false);
            
        }

    }
}