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
        public Timer secondaryTimer;
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
            float timeToDisplay;
            //primaryTimer takes precedence
            if (timer.isCounting)
            {
                timeToDisplay = timer.time;
            }
            //use the secondaryTimer as a backup
            else if (secondaryTimer != null && secondaryTimer.isCounting)
            {
                timeToDisplay = secondaryTimer.time;
            }
            //if neither are counting, use the state of the primaryTimer
            else
            {
                timeToDisplay = timer.time;
            }

            string display;
            if (displayAsMinutes)
            {
                TimeSpan timeSpan = TimeSpan.FromSeconds(Mathf.Ceil(timeToDisplay));
                display = String.Format(@"<mspace={1}em>{0:%m}</mspace>:<mspace={1}em>{0:ss}</mspace>", timeSpan, mspace);
            }
            else
            {
                display = Mathf.Round(timeToDisplay).ToString();
            }

            timerDisplay.text = display;
        }
    }
}


