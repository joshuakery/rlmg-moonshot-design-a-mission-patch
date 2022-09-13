using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace MoonshotTimer
{
    public class TimerDisplay : MonoBehaviour
    {
        public Timer timer;
        public TMP_Text timerDisplay;

        public bool displayAsMinutes;

        public string defaultTimerName;

        public float mspace = 0.7f;

        private void Start()
        {
            if (!timer && !String.IsNullOrEmpty(defaultTimerName))
                timer = GameObject.Find(defaultTimerName).GetComponent<Timer>();
        }

        private void Update()
        {
            string display;
            if (displayAsMinutes)
            {
                TimeSpan timeSpan = TimeSpan.FromSeconds(Mathf.Ceil(timer.time));
                display = String.Format(@"<mspace={1}em>{0:%m}</mspace>:<mspace={1}em>{0:ss}</mspace>", timeSpan, mspace);
            }
            else
            {
                display = Mathf.Round(timer.time).ToString();
            }

            timerDisplay.text = display;
        }
    }
}


