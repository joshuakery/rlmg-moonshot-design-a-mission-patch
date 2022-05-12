using System.Collections;
using UnityEngine;
using System.IO;
using UnityEngine.Video;

public class AttractLoop : MonoBehaviour
{
	public AttractScreen attractScreen;

	public VideoPlayerManager videoPlayerManager;
	public CanvasGroup attractUICanvasGroup;

    public float fadeInDuration = 1f;
    public float fadeOutDuration = 1f;

    public VideoClip attractVideoClip;
    public string attractVideoPath;

	void Awake()
	{
		if (attractScreen == null)
			attractScreen = GetComponent<AttractScreen>();

		if (attractScreen != null)
		{
			attractScreen.onOpen += OnOpen;
			attractScreen.onClose += OnClose;
		}

		if (videoPlayerManager == null)
			videoPlayerManager = GetComponentInChildren<VideoPlayerManager>();

		if (attractUICanvasGroup != null)
		{
			attractUICanvasGroup.alpha = 1f;
			attractUICanvasGroup.gameObject.SetActive(true);
		}
	}

	//void OnEnable()
	void OnOpen(AttractScreen attract)
	{
		StopAllCoroutines();

		StartCoroutine(StartAttractLoop());
	}

	private IEnumerator StartAttractLoop()
	{
		if (attractUICanvasGroup != null)
			attractUICanvasGroup.gameObject.SetActive(true);

		if (!videoPlayerManager.videoPlayer.isPlaying)
		{
			if (attractVideoClip != null)
            {
                videoPlayerManager.LoadAndPlayVideo(attractVideoClip);
            }
            else if (!string.IsNullOrEmpty(attractVideoPath))
            {
                videoPlayerManager.LoadAndPlayVideo(attractVideoPath);
            }
            else
            {
                Debug.LogError("No attract video clip or url found.");
            }


			while (!videoPlayerManager.videoPlayer.isPrepared)
			{
				yield return null;
			}

            Debug.Log("Video finished preparing! Continue on.");
        }
        else
		{
            Debug.Log("Video is already playing. Continue on.");
        }

        if (attractUICanvasGroup != null && attractUICanvasGroup.alpha < 1f)
			StartCoroutine(FadeInUI());
	}

	private IEnumerator FadeInUI()
	{
		if (attractUICanvasGroup == null)
			yield break;

		//attractUICanvasGroup.gameObject.SetActive(true);

		float t = 0f;

		while (t < fadeInDuration)
		{
			attractUICanvasGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeInDuration);

			t += Time.deltaTime;

			yield return null;
		}

		attractUICanvasGroup.alpha = 1f;
	}

	private IEnumerator FadeOutUI()
	{
		if (attractUICanvasGroup == null)
			yield break;

		float t = 0f;

		while (t < fadeOutDuration)
		{
			attractUICanvasGroup.alpha = Mathf.Lerp(1f, 0f, t / fadeOutDuration);

			t += Time.deltaTime;

			yield return null;
		}

		attractUICanvasGroup.alpha = 0f;

		//attractUICanvasGroup.interactable = attractUICanvasGroup.blocksRaycasts = false;
		attractUICanvasGroup.gameObject.SetActive(false);


		videoPlayerManager.videoPlayer.Stop();
	}

	//void OnDisable()
	void OnClose(AttractScreen attract)
	{
//		if (!quitApp)
//		{
			StopAllCoroutines();

			//reset things here
//		}

		StartCoroutine(FadeOutUI());
	}

//	private bool quitApp = false;
//
//	void OnApplicationQuit()
//	{
//		quitApp = true;
//	}
}
