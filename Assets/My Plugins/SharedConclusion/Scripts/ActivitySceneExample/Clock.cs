using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Clock : MonoBehaviour 
{
	public bool isVisible = false;
	public bool isCounting = false;

	public bool countUp = true;

	public delegate void OnCountdownComplete();
    public OnCountdownComplete onCountdownComplete;

	public float time = 0f;
    // public Text text;
    //public Text[] textUIs;
    public TextMeshProUGUI[] textUIs;

	public float monoSpacing = 2.75f;

    void Start()
    {
        // if (text == null)
        //     text = GetComponent<Text>();
    }

	// private int hours;
	// private int minutes;
	// private int seconds;
	//private int deciseconds;

	private int prevRoundedTime;

    void Update()
	{
		if (isCounting)
		{
			if (countUp)
				time += Time.deltaTime;
			else
				time -= Time.deltaTime;

			if (!countUp && time <= 0f)
			{
				time = 0f;
				
				// if (onCountdownComplete != null)
				// {
				// 	onCountdownComplete();
				// }
			}
		}

		if (isVisible)
		{
			//int roundedTime = Mathf.RoundToInt(time * 10);  //use this with the deciseconds, as it needs to update as often as the smallest increment
			//int roundedTime = Mathf.RoundToInt(time);  //this should be fine for full seconds
			int roundedTime = Mathf.CeilToInt(time);  //this should be fine for full seconds

			if (roundedTime != prevRoundedTime)  //a way of optimizing a bit to avoid generating a bunch of string garbage https://answers.unity.com/questions/1334070/how-to-avoid-garbage-with-strings.html
			{
				/*
				hours = Mathf.FloorToInt( time / 3600 );
				//minutes = Mathf.RoundToInt( time / 60 );
				minutes = Mathf.FloorToInt( (time / 60) % 60 );
				seconds = Mathf.FloorToInt( time % 60 );
				//deciseconds = Mathf.RoundToInt( (time * 10) % 10 );
				

				//text.text = string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, deciseconds);
				//text.text = string.Format("{0:00}:{1:00}:{2:00}:{3:00}", hours, minutes, seconds, deciseconds);
				text.text = string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
				*/

				foreach (TextMeshProUGUI text in textUIs)
				{
					text.text = FormatTime(time);
					//text.text = "<mspace="+monoSpacing+"em>" + FormatTime(time) + "</mspace>";
				}
			}

			prevRoundedTime = roundedTime;
		}
		else
		{
			foreach (TextMeshProUGUI text in textUIs)
			{
				text.text = "";
			}
		}

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

	public void StartClock()
	{
		Debug.Log("StartClock()");
		
		time = 0.0f;

		prevRoundedTime = -999;  //intentionally a dummy number that's just different than the actual time to force a refresh

		isCounting = true;
		isVisible = true;
		countUp = true;
	}

	public void StopClock()
	{
		isCounting = false;
	}

	public void StopAndHideClock()
	{
		isCounting = false;
		isVisible = false;
	}

	//public static string FormatTime(float timeInSeconds)
	public string FormatTime(float timeInSeconds)
	{
		int hours = Mathf.FloorToInt( timeInSeconds / 3600 );
		//int minutes = Mathf.RoundToInt( time / 60 );
		int minutes = Mathf.FloorToInt( (timeInSeconds / 60) % 60 );
		int seconds = Mathf.FloorToInt( timeInSeconds % 60 );
		//int deciseconds = Mathf.RoundToInt( (time * 10) % 10 );
				

		//return string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, deciseconds);
		//return string.Format("{0:00}:{1:00}:{2:00}:{3:00}", hours, minutes, seconds, deciseconds);
		//return string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
		//return string.Format("{0:00}:{1:00}", minutes, seconds);
		//return string.Format("<mspace="+monoSpacing+"em>" + "{0:00}" + "</mspace>" + ":" + "<mspace="+monoSpacing+"em>" + "{1:00}" + "</mspace>", minutes, seconds);
		//return string.Format("<mspace="+monoSpacing+"em>" + "{0:0}" + "</mspace>" + ":" + "<mspace="+monoSpacing+"em>" + "{1:00}" + "</mspace>", minutes, seconds);
		return string.Format("<mspace="+monoSpacing+"em>" + "{0:D1}" + "</mspace>" + ":" + "<mspace="+monoSpacing+"em>" + "{1:D2}" + "</mspace>", minutes, seconds);
	}
}
