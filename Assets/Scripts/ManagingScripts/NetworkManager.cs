using Luxodd.Game.Scripts.Network;
using Luxodd.Game.Scripts.Network.CommandHandler;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    [SerializeField] private WebSocketService websocketService;
    [SerializeField] private HealthStatusCheckService healthStatusCheckService;
    [SerializeField] private WebSocketCommandHandler webSocketCommandHandler;
    [SerializeField] private ReconnectService reconnectService;
    
    
    public WebSocketService WebSocketService => websocketService;
    public HealthStatusCheckService HealthStatusCheckService => healthStatusCheckService;
    public WebSocketCommandHandler WebSocketCommandHandler => webSocketCommandHandler;
    public ReconnectService ReconnectService => reconnectService;

}