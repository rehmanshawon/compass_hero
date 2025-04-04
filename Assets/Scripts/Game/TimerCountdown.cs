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
			if(Time.timeSinceLevelLoad >= levelTagTimer) {
				startCounter = true;
			}
		}
	}

	void CountDown() {
		timeRemaining = startTime - (Time.timeSinceLevelLoad - levelTagTimer);

		if(timeRemaining <= 0) {
			timeRemaining = 0;
			myAnim.Stop("Timer blink");
			MainUI.Instance.isEndEvent = true;
			print("stoped");
		}

		if(timeRemaining <= 10) {
			myAnim.Play("Timer blink");
		} else if(timeRemaining <= 30) {
			countDownTimeText.color = Color.magenta;
		}

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