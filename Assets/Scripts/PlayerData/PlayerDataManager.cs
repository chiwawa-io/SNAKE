// Copyright (C) 2025 Tencent Games. All rights reserved.
//
// AchievementManager.cs
//
// Purpose: Manages loading, checking, and saving player achievements.
// All operations involving network communication are asynchronous to prevent
// blocking the main thread and ensure smooth gameplay.
//

using System;
using System.Collections.Generic;
using Game.Core; // Assuming GameManager and LoadingComplete are here
using Luxodd.Game.Scripts.Network; // Assuming NetworkManager is here
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

    public class PlayerDataManager : MonoBehaviour
    {
        #region Events

        public event Action OnInitialized;
        public event Action<string> OnAchievementCompleted;
        public event Action DataSaveSuccess;

        #endregion

        #region Fields

        [Header("Dependencies")]
        [SerializeField, Tooltip("Reference to the core network manager for server communication.")]
        private NetworkManager networkManager;

        public bool IsInitialized { get; private set; }
        private bool isLoading;

        private readonly HashSet<string> _completedAchievementIds = new();
        
        #endregion

        #region Public API

        public void Initialize()
        {
            if (IsInitialized || isLoading)
            {
                return;
            }

            isLoading = true;
            networkManager.WebSocketCommandHandler.SendGetUserDataRequestCommand(OnDataLoadSuccess, OnDataLoadError);
        }

        public bool IsAchievementCompleted(string achievementId)
        {
            if (!IsInitialized)
            {
                Debug.LogWarning("[AchievementManager] Checked for achievement before system was initialized.");
                return false;
            }
            
            return !string.IsNullOrEmpty(achievementId) && _completedAchievementIds.Contains(achievementId);
        }

        public void CompleteAchievement(string achievementId)
        {
            if (!IsInitialized)
            {
                Debug.LogError("[AchievementManager] Attempted to complete achievement before system was initialized.");
                return;
            }

            if (string.IsNullOrEmpty(achievementId)) return;
            
            if (_completedAchievementIds.Add(achievementId))
            {
                Debug.Log($"[AchievementManager] Achievement Completed: {achievementId}. Saving data.");
                OnAchievementCompleted?.Invoke(achievementId);
                SaveDataAsync();
            }
        }

        #endregion

        #region Data Handling

        private void SaveDataAsync()
        {
            var dataToSave = new AchievementData(_completedAchievementIds);
            networkManager.WebSocketCommandHandler.SendSetUserDataRequestCommand(dataToSave, OnDataSaveSuccess, OnDataSaveError);
        }

        private void InitializeFromData(AchievementData data)
        {
            _completedAchievementIds.Clear();
            if (data?.CompletedAchievementIds != null)
            {
                foreach (var id in data.CompletedAchievementIds)
                {
                    _completedAchievementIds.Add(id);
                }
            }
        }
        
        private void InitializeEmpty()
        {
            _completedAchievementIds.Clear();
            IsInitialized = true;
            isLoading = false;
            Debug.Log("[AchievementManager] Initialized with no existing data.");
            LoadingComplete.LoadingCompleteAction?.Invoke();
            OnInitialized?.Invoke();
        }
        
        #endregion

        #region Network Callbacks

        private void OnDataLoadSuccess(object response)
        {
            if (response is not UserDataPayload userDataPayload)
            {
                InitializeEmpty();
                return;
            }

            try
            {
                var userDataJson = (JObject)userDataPayload.Data;
                if (userDataJson != null && userDataJson.TryGetValue("user_data", out var dataToken) && dataToken.Type != JTokenType.Null)
                {
                    var loadedData = JsonConvert.DeserializeObject<AchievementData>(dataToken.ToString());
                    InitializeFromData(loadedData);
                }
                else
                {
                    InitializeEmpty();
                    return;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AchievementManager] Failed to parse achievement data: {ex.Message}");
                InitializeEmpty(); // Fallback to a safe empty state.
                return;
            }

            IsInitialized = true;
            isLoading = false;
            Debug.Log("[AchievementManager] Data loaded and initialized successfully.");
            
            // Notify other systems that loading is complete.
            LoadingComplete.LoadingCompleteAction?.Invoke();
            OnInitialized?.Invoke();
        }

        private void OnDataLoadError(int code, string message)
        {
            Debug.LogError($"[AchievementManager] Data load failed. Code: {code}, Message: {message}. Initializing with empty data.");
            InitializeEmpty(); // Ensure the game can continue by initializing with a default state.
            GameManager.OnErrorOccurred?.Invoke(code, message);
        }

        private void OnDataSaveSuccess()
        {
            DataSaveSuccess?.Invoke();
        }

        private void OnDataSaveError(int code, string message)
        {
            Debug.LogError($"[AchievementManager] Data save failed. Code: {code}, Message: {message}");
            GameManager.OnErrorOccurred?.Invoke(code, message);
        }

        #endregion
    }

    /// <summary>
    /// Data Transfer Object (DTO) for achievement data serialization.
    /// </summary>
    [Serializable]
    public class AchievementData
    {
        // Using a List for JSON serialization compatibility, as Unity's JsonUtility doesn't handle HashSets well,
        // and some backends prefer arrays. Newtonsoft.Json can handle HashSets, but this is safer.
        public HashSet<string> CompletedAchievementIds;

        public AchievementData(HashSet<string> completedIds)
        {
            CompletedAchievementIds = completedIds;
        }
    }