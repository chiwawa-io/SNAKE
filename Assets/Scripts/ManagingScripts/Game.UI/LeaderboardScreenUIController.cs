using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class LeaderboardScreenUIController : BaseUIController
    {
        [SerializeField] private Button returnButton;
        
        protected override void OnShow()
        {
            base.OnShow();
            returnButton.Select();
        }

        protected override void OnHide()
        {
            base.OnHide();
        }
    }
}

