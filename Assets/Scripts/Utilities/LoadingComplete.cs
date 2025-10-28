using System;
using UnityEngine;
using Game.Core;

public class LoadingComplete : MonoBehaviour
{
    [SerializeField] private NetworkManager networkManager;
    [SerializeField] private PlayerDataManager playerDataManager;

    public static Action LoadingCompleteAction;
 
    private void Start()
    {
        LoadingStart();
    }
    private void LoadingStart()
    {
        networkManager.WebSocketService.ConnectToServer(OnConnectionSuccess, OnConnectionFailure);
    }

    private void OnConnectionSuccess()
    {
        networkManager.HealthStatusCheckService.Activate();
        playerDataManager.Initialize();
        LoadingCompleteAction?.Invoke();
        Debug.Log("Loading Complete");
    }

    private void OnConnectionFailure()
    {
        GameManager.OnErrorOccurred?.Invoke(1, "Connection Failure");
        Debug.LogWarning("Connection Failure");
    }

}
