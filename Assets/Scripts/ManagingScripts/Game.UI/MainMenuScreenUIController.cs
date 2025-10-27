// MainMenuUIController.cs
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Game.Core;

namespace Game.UI
{
    public class MainMenuUIController : BaseUIController
    {
        [Header("References")]
        [SerializeField] private PlayerDataManager playerDataManager;
        [SerializeField] private Slider expBar;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private GameObject difficultyPopup;
        
        [Header("Button Selection")]
        [SerializeField] private Button defaultMenuButton;
        [SerializeField] private Button defaultPopupOpenButton;
        [SerializeField] private Button defaultPopupCloseButton;

        protected override void OnShow()
        {
            base.OnShow();
            
            UpdatePlayerInfo();
            difficultyPopup.SetActive(false);
            defaultMenuButton.Select();

            GameStateManager.OnStateChanged += ShowDifficultyPopup;
        }

        protected override void OnHide()
        {
            base.OnHide();
            
            GameStateManager.OnStateChanged -= ShowDifficultyPopup;
        }

        private void UpdatePlayerInfo()
        {
            // expBar.value = playerDataManager.GetExp();
            // levelText.text = playerDataManager.GetLevel().ToString();
        }

        private void ShowDifficultyPopup(GameState state)
        {
            if (state == GameState.DifficultySelect)
                SetDifficultyPopup(true);
            else if  (state == GameState.DifficultySelectClose)
                SetDifficultyPopup(false);
            else 
                return;
        }

        public void SetDifficultyPopup(bool isActive)
        {
            difficultyPopup.SetActive(isActive);
            if (isActive) defaultPopupOpenButton.Select();
            else defaultMenuButton.Select();
        }

    }
}