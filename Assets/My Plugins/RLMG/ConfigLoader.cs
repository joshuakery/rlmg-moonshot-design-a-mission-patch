using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.EventSystems;

public class ConfigLoader : ContentLoader
{
	public AttractScreen[] attractScreens;
	public EventSystem eventSystem;

    [System.Serializable]
	public class ConfigJSON
	{
        // public int screenWidth;
        // public int screenHeight;

        // public bool showMouseCursor;

        public float attractWaitTime = 60f;

		public int dragPixelThreshold;
    }

    protected override IEnumerator PopulateContent(string contentData)
	{
		ConfigJSON configData = JsonConvert.DeserializeObject<ConfigJSON>(contentData);

        if (configData == null)
            yield break;

        // Screen.SetResolution(configData.screenWidth, configData.screenHeight, true);

        // if (!Application.isEditor)
        // {
        //     Cursor.visible = configData.showMouseCursor;
        // }

        if (attractScreens == null || attractScreens.Length < 1)
			attractScreens = FindObjectsOfType<AttractScreen>();

        foreach (AttractScreen attractScreen in attractScreens)
        {
            if (attractScreen != null)
    			attractScreen.timeToActivate = configData.attractWaitTime;
        }

        if (eventSystem == null)
			eventSystem = (EventSystem)FindObjectOfType(typeof(EventSystem));

		if (eventSystem != null)
		{
			eventSystem.pixelDragThreshold = configData.dragPixelThreshold;
		}

        yield break;
	}
}


