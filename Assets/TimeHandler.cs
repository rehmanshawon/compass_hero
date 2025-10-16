using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class TimeHandler : MonoBehaviour
{
    public TextMeshProUGUI InfoText;
    public Text InfoText2;
    public GameObject TournamentPanel;
    public GameObject PeeWeePanel;

    void OnEnable()
    {
        
        if (TournamentLobbyCreator.Instance.IsPaidUser)
        {
            PeeWeePanel.SetActive(true);
        }
        else
        {
            PeeWeePanel.SetActive(false);
        }
        
        if (TournamentLobbyCreator.Instance.IfCheckForTime)
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
    private void Awake()
    {
      
    }
  
    DateTime GetPacificTime()
    {
        DateTime utcNow = DateTime.UtcNow;

        bool isDaylightSaving = utcNow.Month > 3 && utcNow.Month < 11 ||
                               (utcNow.Month == 3 && utcNow.Day >= 8) ||
                               (utcNow.Month == 11 && utcNow.Day <= 1);

        TimeSpan offset = isDaylightSaving ? new TimeSpan(-7, 0, 0) : new TimeSpan(-8, 0, 0);
        return utcNow + offset;
    }

    bool IsInTournamentTime(DateTime pstTime)
    {
        TimeSpan startTime = new TimeSpan(18, 50, 0); // 6:50 PM PST
        TimeSpan endTime = new TimeSpan(19, 0, 0);    // 7:00 PM PST
        TimeSpan currentTime = pstTime.TimeOfDay;

        return currentTime >= startTime && currentTime <= endTime;
    }

    void PerformTournamentActionOnTime()
    {
        InfoText.fontSize = 28;
        Debug.Log("Tournament time! Performing action...");
        TournamentPanel.SetActive(true);
        InfoText.text = "My Own Pee Wee Event";
        InfoText.color = Color.green;
        InfoText2.color = Color.green;
    }

    void PerformTournamentActionNonTime()
    {
        InfoText.fontSize = 23;
        InfoText.text = "The Pee Wee Shootout\n 6:50PM starts at 7PM PST.";
        TournamentPanel.SetActive(false);
    }
}
