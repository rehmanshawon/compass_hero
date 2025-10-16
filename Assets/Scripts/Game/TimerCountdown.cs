using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimerCountdown : MonoBehaviour {

	public static TimerCountdown instance;
	public float startTime;
	public float timeRemaining;
	public Text countDownTimeText;
	Animation myAnim;
	bool startCounter;

	public float levelTagTimer = 1.5f;

	void Start() {
		myAnim = GetComponent<Animation>();				
	}

	void Update() {
        if (MainUI.Instance.isTimeRefreshed)
        {
			print("here");
			MainUI.Instance.isTimeRefreshed = false;
			startTime = (float)MainUI.Instance.startingTime;
			timeRemaining = startTime;
		}

		if(startCounter) {
			if(timeRemaining != 0)
            {
				CountDown();
			}			
		} else {
			// Use unscaled time so countdown continues even when the app loses focus
			if(Time.unscaledTime >= levelTagTimer) {
				startCounter = true;
			}
		}
	}

	void CountDown() {
		// Use unscaled time so countdown isn't affected by timeScale or focus changes
		timeRemaining = startTime - (Time.unscaledTime - levelTagTimer);

		if(timeRemaining <= 0) {
			timeRemaining = 0;
			myAnim.Stop("Timer blink");
			if (MainUI.Instance != null)
        	MainUI.Instance.isEndEvent = false; // prevent unwanted leave/reset
			print("Timer stopped, switching turn (unscaledTime-based)...");

			// 🧠 Use MultiEngine for networked turn switch
			if (MultiEngine.Instance != null)
			{
				MultiEngine.Instance.HandleTimerTurnEnd();
			}
			else if (GameEngine.Instance != null)
			{
				// fallback for offline/AI
				GameEngine.Instance.ChangeTurn();
			}

			// // ✅ Optional: reset the timer for the new player (if needed)
			// startCounter = false;
			// MainUI.Instance.isTimeRefreshed = true;
		}

		if(timeRemaining <= 10) {
			myAnim.Play("Timer blink");
		} else if(timeRemaining <= 30) {
			countDownTimeText.color = Color.magenta;
		}

		ShowTime();
	}

	// Reset and start the countdown for a new turn
	public void StartNewTurn(float durationSeconds)
	{
		if (durationSeconds <= 0)
		{
			return;
		}
		startTime = durationSeconds;
		timeRemaining = startTime;
		// Anchor to current unscaled time to avoid drift/pauses
		levelTagTimer = Time.unscaledTime;
		startCounter = true;
		if (countDownTimeText != null)
		{
			countDownTimeText.color = Color.white;
		}
		if (myAnim != null)
		{
			myAnim.Stop("Timer blink");
		}
		print($"TimerCountdown: StartNewTurn with duration={durationSeconds}s at t={Time.unscaledTime}");
		ShowTime();
	}

	void ShowTime() {
		int minutes;
		int seconds;
		string timeString;

		minutes = (int)timeRemaining / 60;
		seconds = (int)timeRemaining % 60;
		timeString = "0" + minutes.ToString() + ":" + seconds.ToString("d2");
		countDownTimeText.text = "0" + minutes.ToString() + ":" + seconds.ToString("d2");
	}
}