using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.EventSystems;
using ArtScan.CoreModule;

namespace ArtScan.CamConfigLoaderModule
{
	[System.Serializable]
	public class CamConfigJSON : ConfigLoader.ConfigJSON
	{
		public string defaultCamera;
		public bool flipVertical;
		public bool flipHorizontal;

		public int brightness = 255;
		public int contrast = 127;

		public int edgeFindingMethod;

		public bool doSizeToFit;
		public bool doCropToBoundingBox;
		
	}

	public class CamConfigLoader : ConfigLoader
	{
		public RemoveBackgroundSettings settings;
		public GameEvent ConfigLoaded;
		public CamConfigJSON configData;

		protected override IEnumerator PopulateContent(string contentData)
		{
			configData = JsonConvert.DeserializeObject<CamConfigJSON>(contentData);

			if (configData == null)
				yield break;

			if (settings == null)
				settings = (RemoveBackgroundSettings)FindObjectOfType(typeof(RemoveBackgroundSettings));

			if (settings != null)
			{
				settings.brightness = configData.brightness;
				settings.contrast = configData.contrast;
				settings.edgeFindingMethod = (EdgeFindingMethod)configData.edgeFindingMethod;
				settings.doSizeToFit = configData.doSizeToFit;
				settings.doCropToBoundingBox = configData.doCropToBoundingBox;
			}

			yield return base.PopulateContent(contentData);

			if (ConfigLoaded != null)
				ConfigLoaded.Raise();
	
			yield break;
		}
	}
}



