using Game.UI;
using TMPro;
using UnityEngine;

namespace Game.UI {
    public class ErrorPopupController : ErrorUIController
    {
        [SerializeField] private TextMeshProUGUI errorCodeText;
        [SerializeField] private TextMeshProUGUI errorMessageText;

        public override void ShowError(int code, string message)
        {
            
        }
    }
}
