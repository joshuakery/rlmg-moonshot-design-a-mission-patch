using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
//using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using rlmg.logging;

public class MoonshotDataHandler : MonoBehaviour
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

    public string jsonFilenamePrefix = "team_data_";
    public int teamNum = 0;

    public delegate void OnLoadingComplete(MoonshotTeamData teamData);
    public OnLoadingComplete onLoadingComplete;

    public MoonshotTeamData teamData;

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
        return JsonUtility.ToJson(teamData, true);
    }

    [ContextMenu("Load External Json Data")]
    public void LoadExternalJson()
    {
        StopCoroutine(LoadExternalJsonRoutine());
        StartCoroutine(LoadExternalJsonRoutine());
    }

    public void LoadExternalJson(int teamNumber)
    {
        teamNum = teamNumber;
        
        LoadExternalJson();
    }

    IEnumerator LoadExternalJsonRoutine()
	{
        string url = ContentLoader.fileProtocolPrefix + Path.Combine(DirectoryPathJson, jsonFilenamePrefix + teamNum + ".json");
        
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
            //Debug.LogError("External JSON data is empty.   data string = " + jsonData);
            RLMGLogger.Instance.Log("External JSON data is empty.   data string = " + jsonData, MESSAGETYPE.ERROR);
        }
        else
        {
            //Debug.Log("Loaded from external JSON.   data string = " + jsonData);
            RLMGLogger.Instance.Log("Loaded from external JSON.   data string = " + jsonData, MESSAGETYPE.INFO);
        }

        SetAllDataFromJson(jsonData);

        if (onLoadingComplete != null)
        {
            onLoadingComplete(teamData);
        }
    }
    
    public void SetAllDataFromJson(string jsonData)
    {
        JsonUtility.FromJsonOverwrite(jsonData, teamData);
    }

    [ContextMenu("Save External Json File")]
    public void SaveExternalJson()
    {
        string jsonString = GetAllDataAsJson();
        System.IO.File.WriteAllText(Path.Combine(DirectoryPathJson, jsonFilenamePrefix + teamNum + ".json"), jsonString);

        //Debug.Log("Saved to external JSON.   data string = " + jsonString);
        RLMGLogger.Instance.Log("Saved to external JSON.   data string = " + jsonString, MESSAGETYPE.INFO);
    }
}
