using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
//using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

public class MoonshotUserData : MonoBehaviour
{
    public enum DirectoryLocation
	{
		StreamingAssets,
		Desktop,
		Application,
        Custom
	}

	public DirectoryLocation directoryLocationJson = DirectoryLocation.StreamingAssets;
    public string directoryPathJson;
    public DirectoryLocation directoryLocationImages = DirectoryLocation.StreamingAssets;
    public string directoryPathImages;

    public string jsonFilename = "team_results_data.json";

    public delegate void OnLoadingComplete(MoonshotUserData data);
    public OnLoadingComplete onLoadingComplete;
    
    // public TeamData[] allTeamsData;
    public AllTeamsData allTeamsData;
    
    [System.Serializable]
    public class AllTeamsData
    {
        public TeamData[] teamsData;
    }

    // [Range(0, 2)]
    // public int rangeAttributeTest;

    [System.Serializable]
    public class TeamData
    {
        public string teamName;

        public string namesake;

        public bool didRoverActivity;
        [Range(0, 2)]
        public int roverStepsCompleted = 0;

        public bool didMapActivity;
        public int mapRoundsCompleted;
        [Range(0, 1)]
        public float settlementShelterQuality;
        [Range(0, 1)]
        public float settlementCommsQuality;
        [Range(0, 1)]
        public float settlementSunQuality;
        [Range(0, 1)]
        public float settlementWaterQuality;

        public bool didArtActivity;
        public string[] artworks;  //probably a list of URLs?
        //public Sprite[] artworkSprites;

        public bool didCharterActivity;
        public int charterQsCompleted;
        [Range(-1, 1)]
        public float charterMeterDecisions;
        [Range(-1, 1)]
        public float charterMeterPriorities;
        [Range(-1, 1)]
        public float charterMeterStrictness;

        public bool didHuntActivity;
        [Range(0, 9)]
        public int huntNumFound = 0;
    }

    public string DirectoryPathJson
    {
        get
        {
            string path = "";

            if (directoryLocationJson == DirectoryLocation.StreamingAssets)
            {
                path = Application.streamingAssetsPath;
            }
            else if (directoryLocationJson == DirectoryLocation.Desktop)
            {
                path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            }
            else if (directoryLocationJson == DirectoryLocation.Application)
            {
                path = Path.Combine(Application.dataPath, "..");
            }

            return Path.Combine(path, directoryPathJson);
        }
    }

    public string DirectoryPathImages
    {
        get
        {
            string path = "";

            if (directoryLocationImages == DirectoryLocation.StreamingAssets)
            {
                path = Application.streamingAssetsPath;
            }
            else if (directoryLocationImages == DirectoryLocation.Desktop)
            {
                path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            }
            else if (directoryLocationImages == DirectoryLocation.Application)
            {
                path = Path.Combine(Application.dataPath, "..");
            }

            return Path.Combine(path, directoryPathImages);
        }
    }

    public string GetAllDataAsJson()
    {
        //return JsonConvert.SerializeObject(allTeamsData);
        return JsonUtility.ToJson(allTeamsData, true);
    }

    [ContextMenu("Load External Json Data")]
    public void LoadExternalJson()
    {
        StopCoroutine(LoadExternalJsonRoutine());
        StartCoroutine(LoadExternalJsonRoutine());
    }

    IEnumerator LoadExternalJsonRoutine()
	{
        string url = ContentLoader.fileProtocolPrefix + Path.Combine(DirectoryPathJson, jsonFilename);
        
        // WWW contentFile = new WWW(url);
        // yield return contentFile;
        //string jsonData = contentFile.text;

        //var loaded = new UnityWebRequest(url);
        //UnityWebRequest loaded = new UnityWebRequest(url);
        UnityWebRequest loaded = UnityWebRequest.Get(url);
        //loaded.downloadHandler = new DownloadHandlerBuffer();
        yield return loaded.SendWebRequest();
        string jsonData = loaded.downloadHandler.text;

        if (string.IsNullOrEmpty(jsonData))
        {
            Debug.LogError("External JSON data is empty.   data string = " + jsonData);
        }
        else
        {
            Debug.Log("Loaded from external JSON.   data string = " + jsonData);
        }

        SetAllDataFromJson(jsonData);

        if (onLoadingComplete != null)
        {
            onLoadingComplete(this);
        }
    }
    
    public void SetAllDataFromJson(string jsonData)
    {
        //allTeamsData = JsonConvert.DeserializeObject<AllTeamsData>(jsonData);
        JsonUtility.FromJsonOverwrite(jsonData, allTeamsData);
    }

    [ContextMenu("Save External Json File")]
    public void SaveExternalJson()
    {
        string jsonString = GetAllDataAsJson();
        System.IO.File.WriteAllText(Path.Combine(DirectoryPathJson, jsonFilename), jsonString);

        Debug.Log("Saved to external JSON.   data string = " + jsonString);
    }
}
