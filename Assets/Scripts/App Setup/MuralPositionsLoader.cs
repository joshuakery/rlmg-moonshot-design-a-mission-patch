using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using rlmg.logging;

namespace ArtScan.MuralPositionsModule
{
	[System.Serializable]
	public class MuralPositionsJSON
    {
		public MuralPatch[] muralPatches;
    }

	[System.Serializable]
	public class MuralPatch
	{
		public Vector2 position;
		public Vector2 size;
	}

	public class MuralPositionsLoader : ContentLoader
	{
		public GameEvent MuralPositionsLoaded;
		public MuralPositionsJSON data;

		protected override IEnumerator PopulateContent(string contentData)
		{
			data = JsonConvert.DeserializeObject<MuralPositionsJSON>(contentData);

			if (data == null)
			{
				RLMGLogger.Instance.Log("Mural positions data empty", MESSAGETYPE.INFO);
				yield break;
			}

			yield return base.PopulateContent(contentData);

			if (MuralPositionsLoaded != null) { MuralPositionsLoaded.Raise(); }

			yield break;
		}

		public string GetSaveLocation()
		{
			return ContentDirectory + "/" + contentFilename;
		}
	}
}


