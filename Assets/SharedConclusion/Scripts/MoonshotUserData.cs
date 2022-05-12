using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
//using Newtonsoft.Json;
using UnityEngine;

public class MoonshotUserData : MonoBehaviour
{
    public enum DirectoryLocation
	{
		StreamingAssets,
		Desktop,
		Application,
        Custom
	}

	public DirectoryLocation directoryLocation = DirectoryLocation.StreamingAssets;
    public string directoryPath;

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

    public string DirectoryPath
    {
        get
        {
            string path = "";

            if (directoryLocation == DirectoryLocation.StreamingAssets)
            {
                path = Application.streamingAssetsPath;
            }
            else if (directoryLocation == DirectoryLocation.Desktop)
            {
                path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            }
            else if (directoryLocation == DirectoryLocation.Application)
            {
                path = Path.Combine(Application.dataPath, "..");
            }

            return Path.Combine(path, directoryPath);
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
        StopCoroutine(LoadContentRoutine());
        StartCoroutine(LoadContentRoutine());
    }

    IEnumerator LoadContentRoutine()
	{
        WWW contentFile = new WWW(ContentLoader.fileProtocolPrefix + Path.Combine(DirectoryPath, jsonFilename));
        yield return contentFile;

        if (string.IsNullOrEmpty(contentFile.text))
        {
            Debug.LogError("External JSON data is empty.   data string = " + contentFile.text);
        }
        else
        {
            Debug.Log("Loaded from external JSON.   data string = " + contentFile.text);
        }

        //yield return StartCoroutine(PopulateContent(contentFile.text));
        SetAllDataFromJson(contentFile.text);


        // for (int i = 0; i < allTeamsData.Length; i++)
        // {
        //     yield return StartCoroutine(LoadImagesViaFilenames(allTeamsData[i].artworks, allTeamsData[i].artworkSprites));
        // }

        //yield return StartCoroutine(LoadImagesViaFilenames());

        if (onLoadingComplete != null)
        {
            onLoadingComplete(this);
        }
    }
    
    public void SetAllDataFromJson(string jsonData)
    {
        //allTeamsData = JsonConvert.DeserializeObject<AllTeamsData>(jsonData);
        JsonUtility.FromJsonOverwrite(jsonData, allTeamsData);

        //HACK FOR TESTING
        // MoonSceneManager moonSceneManager = GetComponent<MoonSceneManager>();
        // if (moonSceneManager != null)
        // {
        //     moonSceneManager.LoadTeamResults(moonSceneManager.teamNum, this);
        // }
    }

    [ContextMenu("Save External Json File")]
    public void SaveExternalJson()
    {
        //string jsonString = JsonConvert.SerializeObject(allTeamsData);
        //string jsonString = JsonConvert.SerializeObject(allTeamsData, Formatting.Indented);
        string jsonString = JsonUtility.ToJson(allTeamsData, true);
        System.IO.File.WriteAllText(Path.Combine(DirectoryPath, jsonFilename), jsonString);

        Debug.Log("Saved to external JSON.   data string = " + jsonString);
    }

    // public IEnumerator LoadImagesViaFilenames(string[] filenames, Sprite[] sprites)
    // {
    //     sprites = new Sprite[filenames.Length];
        
    //     for (int i = 0; i < filenames.Length; i++)
    //     {
    //         if (!string.IsNullOrEmpty(filenames[i]))
    //         {
    //             //string imgFilePath = "";
    //             string imgFilePath = ContentLoader.fileProtocolPrefix + Path.Combine(DirectoryPath, filenames[i]);
    //             //string imgFilePath = GetCachedFilePath(filenames[i], ContentDirectory);
                
    //             yield return StartCoroutine(ContentLoader.LoadSpriteFromFilepath(imgFilePath, result => sprites[i] = result));
    //         }
    //     }
    // }

    // public IEnumerator LoadImagesViaFilenames()
    // {
    //     for (int i = 0; i < allTeamsData.teamsData.Length; i++)
    //     {
    //         allTeamsData.teamsData[i].artworkSprites = new Sprite[allTeamsData.teamsData[i].artworks.Length];
            
    //         for (int j = 0; j < allTeamsData.teamsData[i].artworks.Length; j++)
    //         {
    //             if (!string.IsNullOrEmpty(allTeamsData.teamsData[i].artworks[j]))
    //             {
    //                 //string imgFilePath = "";
    //                 string imgFilePath = ContentLoader.fileProtocolPrefix + Path.Combine(DirectoryPath, allTeamsData.teamsData[i].artworks[j]);
    //                 //string imgFilePath = GetCachedFilePath(allTeamsData.teamsData[i].artworks[j], ContentDirectory);
                    
    //                 yield return StartCoroutine(ContentLoader.LoadSpriteFromFilepath(imgFilePath, result => allTeamsData.teamsData[i].artworkSprites[j] = result));
    //             }
    //         }
    //     }

    //     // yield break;
    // }
}
