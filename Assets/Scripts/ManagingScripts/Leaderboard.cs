using System.Collections.Generic;
using System.Linq;
using Luxodd.Game.Scripts.Game.Leaderboard;
using TMPro;
using UnityEngine;
using Game.Core;

public class Leaderboard : MonoBehaviour
{
    [Header("Leaderboard")]
    [SerializeField] private NetworkManager networkManager;
    [SerializeField] private TextMeshProUGUI[] playerNameText;
    [SerializeField] private TextMeshProUGUI[] playerScoreText;
    [SerializeField] private int leaderboardSize;
    
    private int _currentPlayerRank;
    private bool _isCurrentPlayerShown;

    private void OnEnable()
    {
        networkManager.WebSocketCommandHandler.SendLeaderboardRequestCommand(OnGetLeaderboardSuccess, OnGetLeaderboardFail);
    }

    private void OnGetLeaderboardSuccess(LeaderboardDataResponse response)
    {
        if (response.CurrentUserData != null)
        {
            _currentPlayerRank = response.CurrentUserData.Rank;
        }
        
        if (response.Leaderboard != null)
        {
            var playerList = new List<LeaderboardData>();
            if (response.Leaderboard != null)
            {
                playerList = response.Leaderboard.ToList();
            }

            if (response.CurrentUserData != null && response.CurrentUserData.Rank <= _currentPlayerRank)
            {
                var currentPlayer = response.CurrentUserData;
                
                playerList.Insert(currentPlayer.Rank -1, currentPlayer);
                
                if (playerList.Count > leaderboardSize)
                {
                    playerList.RemoveAt(playerList.Count - 1);
                }
            }
            for (int i = 0; i < leaderboardSize; i++)
            {
                if (i < playerList.Count && playerList[i] != null)
                {
                    playerNameText[i].text = playerList[i].PlayerName;
                    playerScoreText[i].text = playerList[i].TotalScore.ToString("D10");
                }
                else
                {
                    playerNameText[i].text = "Empty";
                    playerScoreText[i].text = (0).ToString("D8");
                }
            }
        }

    }

    private void OnGetLeaderboardFail(int code, string message)
    {
        GameManager.OnErrorOccurred(code,message);
    }
}