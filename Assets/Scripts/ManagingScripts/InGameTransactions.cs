using UnityEngine;
using Luxodd.Game.Scripts.Network;
using Game.Core;
public class InGameTransactions : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private GameStateManager gameStateManager;
    [SerializeField] private NetworkManager networkManager;
    [SerializeField] private PlayerDataManager playerDataManager;
    
    private void OnEnable()
    {
        GameManager.OnGameOver += ShowSessionOptions;
    }

    private void OnDisable()
    {
        GameManager.OnGameOver -= ShowSessionOptions;
    }

    private void ShowSessionOptions(int a)
    {
        gameStateManager.SetState(GameState.Loading);
        networkManager.WebSocketService.SendSessionOptionContinue(OnSessionOptionContinueSuccess);
    }

    private void OnSessionOptionContinueSuccess(SessionOptionAction sessionOptionAction)
    {
        switch (sessionOptionAction)
        {
            case SessionOptionAction.Restart:
                RestartGame();
                break;
            case SessionOptionAction.Continue:
                gameManager.RequestContinueGame();
                break;
            case SessionOptionAction.End:
            case SessionOptionAction.Cancel:
                EndGameSession();
                break;
            default:
                Debug.LogError("SessionOptionAction not implemented");
                break;
        }
    }

    private void RestartGame()
    {
        gameManager.RequestRestartGame(false);
    }

    private void EndGameSession()
    {
        networkManager.WebSocketCommandHandler.SendLevelEndRequestCommand(0, gameManager.CurrentScore, OnLevelEndSuccess, OnLevelEndFail);
    }

    private void OnLevelEndSuccess()
    {
        networkManager.WebSocketService.BackToSystem();
    }

    private void OnLevelEndFail(int code, string message)
    {
        GameManager.OnErrorOccurred?.Invoke(code, message);
    }
}
