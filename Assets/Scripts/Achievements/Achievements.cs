using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 

public class Achievements : MonoBehaviour
{
    [Header("AchievementsDataBase")]
    [SerializeField] private List<AchievementSO> achievementsList = new();
    [SerializeField] private PlayerDataManager playerDataManager;

    [Header("AchievementsUI")]
    [SerializeField] private Transform achievementsUIParent;
    [SerializeField] private GameObject achievementsUIPrefab;

    [Header("Pagination")]
    [SerializeField] private int achievementsPerPage = 3; 
    
    //state variables
    private int _currentPage;
    private int _totalPages;

    private void OnEnable()
    {
        // Calculate total pages
        _totalPages = Mathf.CeilToInt((float)achievementsList.Count / achievementsPerPage);

        LoadAchievements();
    }

    public void NextPage()
    {
        if (_currentPage >= _totalPages - 1)
            return;
        
        _currentPage++;
        LoadAchievements();
    }

    public void PreviousPage()
    {
        if (_currentPage <= 0)
            return;
        
        _currentPage--;
        LoadAchievements();

    }

    private void LoadAchievements()
    {
        foreach (Transform child in achievementsUIParent)
        {
            Destroy(child.gameObject);
        }

        var startIndex = _currentPage * achievementsPerPage;
        var endIndex = Mathf.Min(startIndex + achievementsPerPage, achievementsList.Count);

        for (var i = startIndex; i < endIndex; i++)
        {
            var achievementData = achievementsList[i];
            var achievementRow = Instantiate(achievementsUIPrefab, achievementsUIParent);

            var isCompleted = playerDataManager.IsAchievementCompleted(achievementData.id);

            var rowUI = achievementRow.GetComponent<AchievementsRowUI>();
            rowUI.Setup(achievementData, isCompleted);
        }
    }
}