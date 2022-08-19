using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Clock : MonoBehaviour 
{
	public bool isVisible = false;
	//public bool isCounting = false;
	public bool _isCounting = false;
	public bool isCounting
	{
		get
		{
			return _isCounting;
		}
		set
		{
			bool wasCounting = _isCounting;
			
			_isCounting = value;

			if (_isCounting && !wasCounting)
			{
				isHiddenForPauseBlink = false;

				UpdateDisplay();
			}
			else if (!_isCounting && wasCounting)
			{
				//Debug.Log("start pause blink");
				
				lastPauseBlinkTime = Time.time;

				if (pauseBlinkInterval > 0)
				{
					isHiddenForPauseBlink = true;
				}

				UpdateDisplay();
			}
		}
	}

	public bool countUp = true;

	public delegate void OnCountdownComplete();
    public OnCountdownComplete onCountdownComplete;

	public float time = 0f;
    //public Text[] textUIs;
    public TextMeshProUGUI[] textUIs;

	public float monoSpacing = 2.75f;

	public float pauseBlinkInterval = 1f;
	private float lastPauseBlinkTime;
	private bool isHiddenForPauseBlink = false;

	private int prevRoundedTime;
	private string prevFormattedTime;

    void Update()
	{
		// if (Input.GetKeyDown("c"))  //for testing the pause without networking
		// {
		// 	isCounting = !isCounting;
		// }
		
		if (isCounting)
		{
			if (countUp)
				time += Time.deltaTime;
			else
				time -= Time.deltaTime;

			if (!countUp && time <= 0f)
			{
				time = 0f;
			}
		}
		else
		{
			if (pauseBlinkInterval > 0 && Time.time >= lastPauseBlinkTime + pauseBlinkInterval)
			{
				isHiddenForPauseBlink = !isHiddenForPauseBlink;

				lastPauseBlinkTime = Time.time;

				//Debug.Log("toggled pause blink.   isHiddenForPauseBlink="+isHiddenForPauseBlink);
			}
		}

		UpdateDisplay();

		if (isCounting)
		{
			if (!countUp && time <= 0f)
			{
				StopClock();
				
				if (onCountdownComplete != null)
				{
					Debug.Log("Clock.onCountdownComplete()   time = " + time + "   roundedTime = " + Mathf.CeilToInt(time));
					
					onCountdownComplete();
				}
			}
		}
	}

	private void UpdateDisplay()
	{
		if (isVisible && !isHiddenForPauseBlink)
		{
			int roundedTime = Mathf.CeilToInt(time);  //this should be fine for full seconds

			if (roundedTime != prevRoundedTime)  //a way of optimizing a bit to avoid generating a bunch of string garbage https://answers.unity.com/questions/1334070/how-to-avoid-garbage-with-strings.html
			{
				prevFormattedTime = FormatTime(time);
			}

			prevRoundedTime = roundedTime;

			foreach (TextMeshProUGUI text in textUIs)
			{
				text.text = prevFormattedTime;
			}
		}
		else
		{
			foreach (TextMeshProUGUI text in textUIs)
			{
				text.text = "";
			}
		}
	}

	public void StartClock()
	{
		Debug.Log("StartClock()");
		
		time = 0.0f;

		prevRoundedTime = -999;  //intentionally a dummy number that's just different than the actual time to force a refresh

		isCounting = true;
		isVisible = true;
		countUp = true;

		UpdateDisplay();
	}

	public void StopClock()
	{
		isCounting = false;
	}

	public void StopAndHideClock()
	{
		isCounting = false;
		isVisible = false;

		UpdateDisplay();
	}

	public string FormatTime(float timeInSeconds)
	{
		int minutes = Mathf.FloorToInt( (timeInSeconds / 60) % 60 );
		int seconds = Mathf.CeilToInt( timeInSeconds % 60 );

		//return string.Format("{0:00}:{1:00}", minutes, seconds);
		return string.Format("<mspace="+monoSpacing+"em>" + "{0:D1}" + "</mspace>" + ":" + "<mspace="+monoSpacing+"em>" + "{1:D2}" + "</mspace>", minutes, seconds);
	}
}
