using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.Events;

public class TimeCounterOfRoom : MonoBehaviour
{
    public Text TimeText;
    public float ActualTime;
    private bool isCountingDown = true;

    public UnityEvent OnTimeEnd;

    public bool IfCheckForTime;

    void OnEnable()
    {
        if (IfCheckForTime)
        {
            DateTime pstTime = GetPacificTime();

            Debug.Log("UTC Time: " + DateTime.UtcNow);
            Debug.Log("PST Time: " + pstTime);

            TimeSpan startTime = new TimeSpan(18, 30, 0); // 6:30 PM
            TimeSpan endTime = new TimeSpan(19, 0, 0);  // 7:00:00 PM

            if (pstTime.TimeOfDay >= endTime)
            {
                ActualTime = 0;
                isCountingDown = false;
                TimeText.text = "End!";
                OnCountdownFinished();
                return;
            }

            if (pstTime.TimeOfDay <= startTime)
            {
                ActualTime = (float)(endTime - startTime).TotalSeconds;
            }
            else
            {
                ActualTime = (float)(endTime - pstTime.TimeOfDay).TotalSeconds;
            }

            UpdateTimerUI();
        }
    }

    private void FixedUpdate()
    {
        if (isCountingDown && ActualTime > 0)
        {
            ActualTime -= Time.deltaTime;
            UpdateTimerUI();
        }
        else if (isCountingDown && ActualTime <= 0)
        {
            ActualTime = 0;
            isCountingDown = false;
            TimeText.text = "End!";
            OnCountdownFinished();
        }
    }

    // Manual PST/PDT conversion (for WebGL)
    DateTime GetPacificTime()
    {
        DateTime utcNow = DateTime.UtcNow;

        // Simple DST check (adjust for your needs if needed)
        bool isDaylightSaving = utcNow.Month > 3 && utcNow.Month < 11 ||
                               (utcNow.Month == 3 && utcNow.Day >= 8) ||
                               (utcNow.Month == 11 && utcNow.Day <= 1);

        TimeSpan offset = isDaylightSaving ? new TimeSpan(-7, 0, 0) : new TimeSpan(-8, 0, 0);
        return utcNow + offset;
    }

    void UpdateTimerUI()
    {
        TimeSpan time = TimeSpan.FromSeconds(ActualTime);
        TimeText.text = string.Format("{0:D2}:{1:D2}", time.Minutes, time.Seconds);
    }

    void OnCountdownFinished()
    {
        Debug.Log("OnCountdownFinished");
        OnTimeEnd.Invoke();
    }
}
