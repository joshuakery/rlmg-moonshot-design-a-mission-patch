using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TimerDisplay : MonoBehaviour
{
    public Timer timer;
    public TMP_Text timerDisplay;

    public bool displayAsMinutes;

    private void Start()
    {
        if (!timer) timer = GameObject.Find("Timer").GetComponent<Timer>();
    }

    private void Update()
    {
        string display;
        if (displayAsMinutes)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(Mathf.Round(timer.time));
            int seconds = timeSpan.Seconds;
            string secondsDisplay = seconds < 10 ? "0" + seconds.ToString() : seconds.ToString();

            float monoSpacing = 0.63f;
            display = "<mspace="+monoSpacing+"em>" + timeSpan.Minutes.ToString() + "</mspace>" + " : " + "<mspace="+monoSpacing+"em>" + secondsDisplay + "</mspace>";

            // display = timeSpan.Minutes.ToString() + ":" + secondsDisplay;
        }
        else
        {
            display = Mathf.Round(timer.time).ToString();
        }
        timerDisplay.text = display;
    }
}
