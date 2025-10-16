using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using TMPro;

public class TimeCounterOfRoom : MonoBehaviour
{
    [Header("UI Elements")]

    [Header("Timer Settings")]
    [SerializeField] private float countdownDuration = 180f;
    private float actualTime;
    private bool isCountingDown = false;
    public TextMeshProUGUI timerText;

    [Header("Time Logic")]

    [Header("Events")]
    public UnityEvent OnTimeEnd;
    private void OnEnable()
    {
        if (TournamentLobbyCreator.Instance.IsPaidUser)
        {
            return;
        }

        SetupTimeConditions();
    }
    private void SetupTimeConditions()
    {
        // Local time logic (non-counting display only)
        if (TournamentLobbyCreator.Instance.checkLocalTime)
        {
            actualTime = 0;
            isCountingDown = false;
            return; // Skip further setup
        }

        if (TournamentLobbyCreator.Instance.checkPSTTime)
        {
            DateTime pstTime = GetPacificTime();

            TimeSpan startTime = new TimeSpan(18, 50, 0); // 6:50 PM
            TimeSpan endTime = new TimeSpan(19, 0, 0);    // 7:00 PM

            if (pstTime.TimeOfDay >= endTime)
            {
                actualTime = 0;
                isCountingDown = false;
                return;
            }

            if (pstTime.TimeOfDay <= startTime)
            {
                actualTime = (float)(endTime - startTime).TotalSeconds;
            }
            else
            {
                actualTime = (float)(endTime - pstTime.TimeOfDay).TotalSeconds;
            }

            isCountingDown = true;
            UpdateTimerUI();
        }
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.End))
        {
            if (TournamentLobbyCreator.Instance.isTournamentOwner)
            {
                StartTimer();
            }
        }

        if (TournamentLobbyCreator.Instance.RoundCount > 0)
        {
            timerText.gameObject.SetActive(false);
        }
    }

    private void FixedUpdate()
    {
        if (!isCountingDown) return;

        actualTime -= Time.deltaTime;

        if (actualTime <= 0)
        {
            actualTime = 0;
            isCountingDown = false;
            OnCountdownFinished();
        }
        else
        {
            UpdateTimerUI();
        }
    }

    private void StartTimer()
    {
        actualTime = countdownDuration;
        isCountingDown = true;
        UpdateTimerUI();
    }
    private void UpdateTimerUI()
    {
        TimeSpan time = TimeSpan.FromSeconds(actualTime);
        string formattedTime = string.Format("{0:D2}:{1:D2}", time.Minutes, time.Seconds);
        timerText.text = formattedTime;
        Debug.Log("Timer: " + formattedTime);
    }
    private void OnCountdownFinished()
    {
        Debug.Log("OnCountdownFinished");
        OnTimeEnd?.Invoke();
    }
    private DateTime GetPacificTime()
    {
        DateTime utcNow = DateTime.UtcNow;

        bool isDaylightSaving = utcNow.Month > 3 && utcNow.Month < 11 ||
                                (utcNow.Month == 3 && utcNow.Day >= 8) ||
                                (utcNow.Month == 11 && utcNow.Day <= 1);
        

        TimeSpan offset = isDaylightSaving ? new TimeSpan(-7, 0, 0) : new TimeSpan(-8, 0, 0);
        return utcNow + offset;
    }
}
