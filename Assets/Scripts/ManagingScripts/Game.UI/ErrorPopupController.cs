using Game.Core;
using Game.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ErrorPopupController : BaseUIController
{
    [SerializeField] private TextMeshProUGUI errorMessage;
    [SerializeField] private TextMeshProUGUI errorCode;
    [SerializeField] private Button quitButton;
    
    protected override void OnShow()
    {
        base.OnShow();
        
        var message = GameManager.errorMessage;
        var code = GameManager.errorCode;
        
        errorMessage.text = message;
        errorCode.text = code.ToString();
        quitButton.Select();
    }
}
