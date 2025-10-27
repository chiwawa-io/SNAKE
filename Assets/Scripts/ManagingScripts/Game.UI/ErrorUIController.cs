// BaseUIController.cs
using UnityEngine;

namespace Game.UI
{
    public abstract class ErrorUIController : MonoBehaviour
    {
        [SerializeField]
        private GameObject screenRoot;

        public bool IsVisible => screenRoot.activeSelf;

        public virtual void ShowError(int code, string message)
        {
            screenRoot.SetActive(true);
            OnShow();
        }

        public virtual void Hide()
        {
            OnHide();
            screenRoot.SetActive(false);
        }


        protected virtual void OnShow() { }
        
        protected virtual void OnHide() { }
    }
}