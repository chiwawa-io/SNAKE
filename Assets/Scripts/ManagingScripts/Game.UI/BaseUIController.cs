// BaseUIController.cs

using TMPro;
using UnityEngine;

namespace Game.UI
{
    public abstract class BaseUIController : MonoBehaviour
    {
        [SerializeField] 
        private GameObject screenRoot;

        [SerializeField] 
        protected TextMeshProUGUI timerText;
        
        public bool IsVisible => screenRoot.activeSelf;
        
        public virtual void Show()
        {
            screenRoot.SetActive(true);
            OnShow();
        }
        protected virtual void UpdateTimer(int timeLeft)
        {
            if (timerText != null)
            {
                timerText.text = timeLeft.ToString();
            }
        }
        
        public virtual void Hide()
        {
            OnHide();
            screenRoot.SetActive(false);
        }

        protected virtual void OnShow()
        {
            if (timerText != null)
            {
                InactivityDetector.OnTimerUpdate += UpdateTimer;
            }
        }


        protected virtual void OnHide()
        {
            if (timerText != null)
            {
                InactivityDetector.OnTimerUpdate -= UpdateTimer;
            }
        }
    }
}