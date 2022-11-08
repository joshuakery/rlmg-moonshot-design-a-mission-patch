using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.EventSystems;
using ArtScan.CoreModule;
using ArtScan.ErrorDisplayModule;
using rlmg.logging;

namespace ArtScan.CamConfigLoaderModule
{
	[System.Serializable]
	public class CamConfigJSON : ConfigLoader.ConfigJSON
	{
		public string saveDir;
		public string trashDir;
		public bool clearSaveDirOnQuit;
		public bool clearTrashDirOnQuit;
		public bool doSaveCroppedToBoundingBox;

		public string defaultCamera;
		public bool flipVertical;
		public bool flipHorizontal;

		public int brightness = 255;
		public int contrast = 127;

		public int edgeFindingMethod;

		public bool doSizeToFit;
		public bool doCropToBoundingBox;

		public RemoveBackgroundSettings.PostProcessingSettings postProcessingSettings;

		public ErrorDisplaySettings errorDisplaySettings;
		
	}

	public class CamConfigLoader : ConfigLoader
	{
		public RemoveBackgroundSettings settings;
		public ErrorDisplaySettingsSO errorDisplaySettingsSO;
		public GameEvent ConfigLoaded;
		public CamConfigJSON configData;

		protected override IEnumerator PopulateContent(string contentData)
		{
			configData = JsonConvert.DeserializeObject<CamConfigJSON>(contentData);

			if (configData == null)
			{
				RLMGLogger.Instance.Log("Config data empty", MESSAGETYPE.INFO);
				yield break;
			}

			if (settings != null)
			{
				settings.saveDir = configData.saveDir;
				settings.trashDir = configData.trashDir;
				settings.clearSaveDirOnQuit = configData.clearSaveDirOnQuit;
				settings.clearTrashDirOnQuit = configData.clearTrashDirOnQuit;
				settings.doSaveCroppedToBoundingBox = configData.doSaveCroppedToBoundingBox;

				settings.brightness = configData.brightness;
				settings.contrast = configData.contrast;
				settings.edgeFindingMethod = (EdgeFindingMethod)configData.edgeFindingMethod;
				settings.doSizeToFit = configData.doSizeToFit;
				settings.doCropToBoundingBox = configData.doCropToBoundingBox;

				settings.postProcessingSettings = configData.postProcessingSettings;
			}

			if (errorDisplaySettingsSO != null)
				errorDisplaySettingsSO.errorDisplaySettings = configData.errorDisplaySettings;

			yield return base.PopulateContent(contentData);

			if (ConfigLoaded != null)
				ConfigLoaded.Raise();
	
			yield break;
		}
	}
}



