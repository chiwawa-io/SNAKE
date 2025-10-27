using System;
using System.Collections.Generic;
using UnityEngine;

public class AchievGameListener : MonoBehaviour
{
    [SerializeField] private List<AchievementSO> achievementsList = new();
    [SerializeField] private PlayerDataManager playerDataManager;
    
    private readonly List<string> _completedAchievements = new();
    
    public static Action<string> OnAchievementCompleted;
    
    private void OnEnable()
    {
        Player.OnAchieved += AchievementComplete;
        LoadCompletedAchievements();
    }

    private void OnDisable()
    {
        Player.OnAchieved -= AchievementComplete;
    }

    private void LoadCompletedAchievements()
    {
        foreach (var achievement in achievementsList)
        {
            if (playerDataManager.IsAchievementCompleted(achievement.id))
                _completedAchievements.Add(achievement.id);
        }
    }

    private void AchievementComplete(string id)
    {
        playerDataManager.CompleteAchievement(id);

        var achievementData = GetAchievementById(id);
        
        if (achievementData != null && !_completedAchievements.Contains(id)) OnAchievementCompleted?.Invoke(achievementData.displayName);
        
        if (!_completedAchievements.Contains(id)) _completedAchievements.Add(id);
    }

    private AchievementSO GetAchievementById(string id)
    {
        foreach (var achievementData in achievementsList)
        {
            if (achievementData.id == id) return achievementData;
            
            Debug.LogWarning("Not found the Achievement");
        }
        return null;
    }
}
