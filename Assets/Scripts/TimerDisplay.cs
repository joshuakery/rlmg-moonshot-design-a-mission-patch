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

    public float mspace = 0.7f;

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
            display = String.Format(@"<mspace={1}em>{0:%m}</mspace>:<mspace={1}em>{0:ss}</mspace>", timeSpan, mspace);
        }
        else
        {
            display = Mathf.Round(timer.time).ToString();
        }
        
        timerDisplay.text = display;
    }
}
