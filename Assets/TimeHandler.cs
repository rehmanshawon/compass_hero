using System;
using UnityEngine;
using UnityEngine.UI;

public class TimeHandler : MonoBehaviour
{
    public Text InfoText;
    public GameObject TournamentPanel;
    public bool IfCheckForTime;

    void OnEnable()
    {
        if (IfCheckForTime)
        {
            DateTime pstTime = GetPacificTime();

            Debug.Log("UTC Time: " + DateTime.UtcNow);
            Debug.Log("PST Time: " + pstTime);
            Debug.Log("Day PST: " + pstTime.DayOfWeek);

            if (IsInTournamentTime(pstTime))
            {
                PerformTournamentActionOnTime();
            }
            else
            {
                PerformTournamentActionNonTime();
            }
        }
        else
        {
            PerformTournamentActionOnTime();
        }
    }

    // Manual Pacific Time calculation (works in WebGL)
    DateTime GetPacificTime()
    {
        DateTime utcNow = DateTime.UtcNow;

        // Simplified DST check (true for March to November)
        bool isDaylightSaving = utcNow.Month > 3 && utcNow.Month < 11 ||
                               (utcNow.Month == 3 && utcNow.Day >= 8) ||
                               (utcNow.Month == 11 && utcNow.Day <= 1);

        // PDT = UTC-7 (with DST), PST = UTC-8 (without DST)
        TimeSpan offset = isDaylightSaving ? new TimeSpan(-7, 0, 0) : new TimeSpan(-8, 0, 0);
        return utcNow + offset;
    }

    bool IsInTournamentTime(DateTime pstTime)
    {
        TimeSpan startTime = new TimeSpan(18, 30, 0); // 6:30 PM PST
        TimeSpan endTime = new TimeSpan(19, 0, 0);    // 7:00 PM PST
        TimeSpan currentTime = pstTime.TimeOfDay;

        return currentTime >= startTime && currentTime <= endTime;
    }

    void PerformTournamentActionOnTime()
    {
        InfoText.color = Color.green;
        Debug.Log("Tournament time! Performing action...");
        TournamentPanel.SetActive(true);
        InfoText.text = "Available Tournaments";
    }

    void PerformTournamentActionNonTime()
    {
        InfoText.color = Color.red;
        InfoText.text = "No available Tournaments!";
        TournamentPanel.SetActive(false);
    }
}
