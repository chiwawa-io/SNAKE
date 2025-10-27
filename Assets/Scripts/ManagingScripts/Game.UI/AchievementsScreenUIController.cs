using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game.UI
{
    public class AchievementsScreenUIController : BaseUIController
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
