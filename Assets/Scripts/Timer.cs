using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer : MonoBehaviour
{
    public float time;
    public float duration;

    public bool isCounting;

    public List<GameEvent> timeOutEvents;

    public Timer LessThanOrEqualToThisTimer;


    public bool isWarning;
    public GameEvent TimeWarning;
    [Tooltip("Time (in seconds) when time warnings are triggered.")]
    public List<int> warningTimes;
    public int warningAdvance = 0;

    // Start is called before the first frame update
    void Start()
    {
        isCounting = false;
        isWarning = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (isCounting)
        {
            time -= Time.deltaTime;
            if (time <= 0)
            {
                time = 0;
                isCounting = false;
                foreach (GameEvent gameEvent in timeOutEvents)
                {
                    gameEvent.Raise();
                }
            }
            else if (LessThanOrEqualToThisTimer != null &&
                     time > LessThanOrEqualToThisTimer.time)
            {
                time = LessThanOrEqualToThisTimer.time;
            }
            if (!isWarning && IsWarningTime())
            {
                TimeWarning.Raise();
                isWarning = true;

            }
            else if (isWarning && !IsWarningTime())
            {
                isWarning = false;
            }
        }
    }

    private bool IsWarningTime()
    {
        foreach(int warningTime in warningTimes)
        {
            if ((warningTime + warningAdvance) == (int)Mathf.Round(time))
            {
                return true;
            }
        }
        return false;
    }

    public void StartCounting()
    {
        isCounting = true;
    }

    public void PauseCounting()
    {
        isCounting = false;
    }

    public void SetToZero()
    {
        time = 0;
        isCounting = false;
    }

    public void Reset()
    {
        time = duration;
        isCounting = false;
    }
}

//using System;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class Timer : MonoBehaviour
//{
//    public DateTime start;
//    public float duration;

//    public float time
//    {
//        get
//        {
//            DateTime end = start.Add(TimeSpan.FromSeconds(duration));
//            float difference = (float)end.Subtract(DateTime.Now).TotalSeconds;

//            if (LessThanOrEqualToThisTimer != null &&
//                difference > LessThanOrEqualToThisTimer.time)
//            {
//                return LessThanOrEqualToThisTimer.time;
//            }
//            else if (difference > 0)
//            {
//                return difference;
//            }
//            else
//            {
//                return 0;
//            }

//        }
//    }


//    public bool isCounting;

//    public List<GameEvent> timeOutEvents;

//    public Timer LessThanOrEqualToThisTimer;


//    public bool isWarning;
//    public GameEvent TimeWarning;
//    [Tooltip("Time (in seconds) when time warnings are triggered.")]
//    public List<int> warningTimes;
//    public int warningAdvance = 0;

//    // Start is called before the first frame update
//    void Start()
//    {
//        isCounting = false;
//        isWarning = false;
//    }

//    // Update is called once per frame
//    void Update()
//    {
//        if (isCounting)
//        {
//            if (time <= 0)
//            {
//                isCounting = false;
//                foreach (GameEvent gameEvent in timeOutEvents)
//                {
//                    gameEvent.Raise();
//                }
//            }
//            if (!isWarning && IsWarningTime())
//            {
//                TimeWarning.Raise();
//                isWarning = true;

//            }
//            else if (isWarning && !IsWarningTime())
//            {
//                isWarning = false;
//            }
//        }
//    }

//    private bool IsWarningTime()
//    {
//        foreach (int warningTime in warningTimes)
//        {
//            if ((warningTime + warningAdvance) == (int)Mathf.Round(time))
//            {
//                return true;
//            }
//        }
//        return false;
//    }

//    public void StartCounting()
//    {
//        start = DateTime.Now;
//        isCounting = true;
//    }

//    public void PauseCounting()
//    {
//        //
//        isCounting = false;
//    }

//    public void SetToZero()
//    {
//        time = 0;
//        isCounting = false;
//    }

//    public void Reset()
//    {
//        time = duration;
//        isCounting = false;
//    }
//}
