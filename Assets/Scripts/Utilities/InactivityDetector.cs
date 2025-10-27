using System;
using System.Collections;
using UnityEngine;

public class InactivityDetector : MonoBehaviour
{
    [SerializeField] private int inactivityTimeLimit = 30;

    private float _currentTime;
    private bool _isTimerRunning; 
    private Coroutine _timerCoroutine;

    public static event Action TimesUp;
    public static event Action<int> OnTimerUpdate;


    public void StartDetector()
    {
        _currentTime = 0f;
        _isTimerRunning = true;
        
        if (_timerCoroutine != null)
        {
            StopCoroutine(_timerCoroutine);
        }
        _timerCoroutine = StartCoroutine(UpdateTimerDisplay());
        
        Debug.LogWarning("Inactivity Detector Started.");
    }

    public void StopDetector()
    {
        _isTimerRunning = false;
        if (_timerCoroutine != null)
        {
            StopCoroutine(_timerCoroutine);
            _timerCoroutine = null;
        }

        Debug.LogWarning("Inactivity Detector Stopped.");
    }

    void Update()
    {
        if (!_isTimerRunning)
        {
            return;
        }

        _currentTime += Time.deltaTime;

        if (_currentTime >= inactivityTimeLimit)
        {
            Debug.LogWarning("Inactivity time limit reached!.");
            TimesUp?.Invoke();
            StopDetector(); 
        }
    }
    IEnumerator UpdateTimerDisplay()
    {
        while (_isTimerRunning)
        {
            var timeRemaining = Mathf.RoundToInt(inactivityTimeLimit - _currentTime);
            
            if (timeRemaining < 0) timeRemaining = 0;

            OnTimerUpdate?.Invoke(timeRemaining);
            
            yield return new WaitForSeconds(1f);
        }
    }
}