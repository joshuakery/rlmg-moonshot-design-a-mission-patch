using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using rlmg.logging;
using UnityEngine.Serialization;
using System.IO;
using System;
using System.Text.RegularExpressions;
using UnityEngine.Networking;
using rlmg.logging;

public class ContentLoader : MonoBehaviour
{
	public bool loadOnAwake = true;
	public bool loadFromExternal = true;
	public bool useInEditor = true;

    public LoadingScreen loadingScreen;

    public string contentFolderName = "";
    [FormerlySerializedAs("localFilename")]
    [FormerlySerializedAs("databaseName")]
    public string contentFilename = "config.json";

	public string cmsUrl;
	//public string cmsFilename;

	public bool tryDownloadFromCMS = false;
	public float cmsTimeOut = 60f;
	private float cmsTimer = 0f;
	protected bool didSuccessfullyDownloadFromCMS = false;
	[FormerlySerializedAs("saveFromCMS")]
	public bool cacheFromCMS = false;
	public string cachedFileExtensions = ".jpeg|.png|.jpg|.ogg";

	private bool _hasLoadedContent = false;
	public bool HasLoadedContent
	{
		get
		{
			return _hasLoadedContent;
		}
	}

	public static string fileProtocolPrefix
	{
		get
		{
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
			return "file:///";
#else
			return "file://";
#endif
        }
    }

	public enum CONTENT_LOCATION
	{
		StreamingAssets,
		Desktop,
		Application
	}

	public CONTENT_LOCATION contentLocation = CONTENT_LOCATION.Application;

    protected string ContentDirectory
	{
		get
		{
			string path = "";

            if (contentLocation == CONTENT_LOCATION.StreamingAssets)
            {
                path = Application.streamingAssetsPath;
            }
            if (contentLocation == CONTENT_LOCATION.Desktop)
            {
                path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            }
            else if (contentLocation == CONTENT_LOCATION.Application)
            {
                path = Path.Combine(Application.dataPath, "..");
            }

            //add parent folder to keep organized
            if (!string.IsNullOrEmpty(contentFolderName))
                path = Path.Combine(path, contentFolderName);

            if (contentLocation != CONTENT_LOCATION.StreamingAssets && !Directory.Exists(path))
                Directory.CreateDirectory(path);

			return path;
		}
	}

	protected virtual void Awake()
	{
		#if UNITY_EDITOR
		contentLocation = CONTENT_LOCATION.StreamingAssets;

		// if (!useInEditor)
        //     return;
		#endif

		if (loadOnAwake)
			LoadContent();
	}

	public virtual void LoadContent()
	{
		StopAllCoroutines();

		StartCoroutine(LoadContentRoutine());
	}

	IEnumerator LoadContentRoutine()
	{
		_hasLoadedContent = false;

        if (loadingScreen != null)
            loadingScreen.gameObject.SetActive(true);

        //if (loadFromExternal)
        if (loadFromExternal && (useInEditor || !Application.isEditor))
		{
			//RLMGLogger.Instance.Log("Loading external content...", MESSAGETYPE.INFO);

            WWW contentFile = null;

			if (tryDownloadFromCMS)
			{
				contentFile = new WWW(cmsUrl);
				//yield return jsonFile;

				bool failedByTimeOut = false;

				while (!contentFile.isDone)
				{
					if (cmsTimer > cmsTimeOut)
					{
						//Debug.Log("timed out");

						failedByTimeOut = true;
						break;
					}
					cmsTimer += Time.deltaTime;
					yield return null;
				}

				if (failedByTimeOut || !string.IsNullOrEmpty(contentFile.error))
				{
					if (failedByTimeOut)
						Debug.Log("Download from CMS failed by time out. Duration = "+cmsTimer);
					else
						Debug.Log("Download from CMS failed. Error = "+contentFile.error);

					contentFile.Dispose();
					contentFile = null;

					//yield break;
				}
				else
				{
					Debug.Log("Successfully downloaded from CMS. Download time = " + cmsTimer);

					didSuccessfullyDownloadFromCMS = true;
				}

				if (cacheFromCMS && contentFile != null && !string.IsNullOrEmpty(contentFile.text))  //should I be checking to see if it has any differences before saving?
				{
					Debug.Log("Saving downloaded CMS to local directory.");

					//System.IO.File.WriteAllText(contentDirectory + localFilename, jsonFile.text);
					//System.IO.File.WriteAllText(Application.streamingAssetsPath + "/" + contentFilename, contentFile.text);

					if (!Application.isEditor && contentLocation != CONTENT_LOCATION.StreamingAssets)
					{
						//File.Copy(Application.streamingAssetsPath + "/" + contentFilename, ContentDirectory + "/" + contentFilename, true);

						if (!Directory.Exists(ContentDirectory))
                			Directory.CreateDirectory(ContentDirectory);

						System.IO.File.WriteAllText(ContentDirectory + "/" + contentFilename, contentFile.text);
					}
					else
					{
						if (!Directory.Exists(Application.streamingAssetsPath + "/" + contentFolderName))
                			Directory.CreateDirectory(Application.streamingAssetsPath + "/" + contentFolderName);
						
						System.IO.File.WriteAllText(Application.streamingAssetsPath + "/" + contentFolderName + "/" + contentFilename, contentFile.text);
					}


					//using a function I largely got from ChrisW, store the images (and eventually audio?) locally for easier access
					yield return StartCoroutine(CacheContent(contentFile.text, ContentDirectory, cachedFileExtensions));
				}
			}

            //Debug.Log("contentFile = "+contentFile);

			//use local or already downloaded version
            if (contentFile == null || string.IsNullOrEmpty(contentFile.text))  
			{
                Debug.Log("Loading locally stored content: " + contentFilename);

                CopyOverIfNecessary();  //this copies the original from streaming assets to the designated folder, assuming the designated folder isn't already streaming assets

                contentFile = new WWW(fileProtocolPrefix + Path.Combine(ContentDirectory, contentFilename));
				yield return contentFile;
			}

			yield return StartCoroutine(PopulateContent(contentFile.text));

			//RLMGLogger.Instance.Log("Finished loading external content.", MESSAGETYPE.INFO);
		}

        if (loadingScreen != null)
            loadingScreen.FadeOut();

        _hasLoadedContent = true;

		FinishedLoadingContent();
	}

    protected virtual void CopyOverIfNecessary()
    {
        string desiredFilePath;
        desiredFilePath = Path.Combine(ContentDirectory, contentFilename);

        //if not there, and not already checking streaming assets, check for a backup in streaming assets
        if (!File.Exists(desiredFilePath) && contentLocation != CONTENT_LOCATION.StreamingAssets)
        {
            string backupFilePath = Path.Combine(Application.streamingAssetsPath, contentFilename);

            //if backup in streaming assets does exist, copy it to the desired external location
            if (File.Exists(backupFilePath))
            {
                File.Copy(backupFilePath, desiredFilePath, true);
            }
        }
    }

	IEnumerator CacheContent(string serverResponse, string cachePath, string extensions)
	{
		// use RegEx to find all image paths
		//Regex regex = new Regex("(http|ftp|https)://([\\w_-]+(?:(?:\\.[\\w_-]+)+))([\\w.,@?^=%&:/~+#-]*[\\w@?^=%&/~+#-])?(.jpeg|.png|.jpg)");
		Regex regex = new Regex("(http|ftp|https)://([\\w_-]+(?:(?:\\.[\\w_-]+)+))([\\w.,@?^=%&:/~+#-]*[\\w@?^=%&/~+#-])?(" + extensions + ")");

		MatchCollection matches = regex.Matches(serverResponse);
		if (matches.Count > 0)
		{
			foreach (Match match in matches)
			{
				string remotePath = match.Value;

				// parse out only filename
				string filename = remotePath.Substring(remotePath.LastIndexOf("/") + 1);
				string localPath = Path.Combine(cachePath, filename);

				// check if it exists
				if (!File.Exists(localPath))
				{
					UnityWebRequest www = UnityWebRequestTexture.GetTexture(remotePath);
					yield return www.SendWebRequest();

					if (www.isNetworkError)
					{
						Debug.Log(www.error);
					}
					else
					{
						// cache it if necessary
						CacheFile(localPath, www.downloadHandler.data);
					}
				}
			}

			// now clear out any local images that are NOT in json data
			var info = new DirectoryInfo (cachePath);
			var fileInfo = info.GetFiles ();

			foreach (FileInfo file in fileInfo)
			{
				if (serverResponse.IndexOf (file.Name) == -1 && regex.Match (file.Name).Success)
				{
					// doesn't exist in data anymore, so delete
					file.Delete ();
				}
			}
		}
	}

	static public void CacheFile(string path, byte[] bytes)
	{
		// cache image in target cache directory
		var file = File.Open(path, FileMode.Create);
		var binary = new BinaryWriter(file);
		binary.Write(bytes);
		file.Close();
	}

	static public void CacheFile(string path, string text)
	{
		// text write
		File.WriteAllText(path, text);
	}

	public string GetCachedFilePath(string remotePath, string cachePath)
	{
		if (remotePath == "" || remotePath == null)
		{
			return null;
		}

		string filename = remotePath.Substring (remotePath.LastIndexOf ("/") + 1);
		string localPath = Path.Combine (cachePath, filename);

		bool isCached = File.Exists (localPath);

		// set URL based on if local cached copy exists
		string url = isCached ? fileProtocolPrefix + localPath : remotePath;

		//Debug.Log("referencing local file at " + cachePath + " using remote url: " + remotePath);

		return url;
	}

    protected virtual IEnumerator PopulateContent(string contentData)
	{
		//override to do things!

		yield break;
	}

	protected virtual void FinishedLoadingContent()
	{
		//override to do things!
	}

    //https://forum.unity.com/threads/passing-ref-variable-to-coroutine.379640/
    //call via the following syntax: StartCoroutine(LoadSpriteFromFilepath(imgFilePath, result => spriteFileReference = result));
    public static IEnumerator LoadSpriteFromFilepath(string imgFilePath, Action<Sprite> spriteRef)
    {
        if (!string.IsNullOrEmpty(imgFilePath))
        {
            WWW externalImgFile = new WWW(imgFilePath);
            yield return externalImgFile;

            if (externalImgFile.error != null)
            {
                Debug.LogWarning(imgFilePath + " error = " + externalImgFile.error);
            }
            else
            {
                spriteRef(Utilities.TextureToMipMappedSprite(externalImgFile.texture));
            }
        }
    }

	public IEnumerator LoadAudioFileFromFilepath(string audioFilePath, Action<AudioClip> audioClipRef)
    {
        if (!string.IsNullOrEmpty(audioFilePath))
		{
			WWW externalAudioFile = new WWW(audioFilePath);
			yield return externalAudioFile;

			if (externalAudioFile.error != null)
			{
				Debug.LogWarning(audioFilePath + " error = " + externalAudioFile.error);
			}
			else
			{
				audioClipRef(externalAudioFile.GetAudioClip(false, false, GetAudioType(audioFilePath)));
			}
		}
    }

	protected AudioType GetAudioType(string audioFilename)
    {
        AudioType audioType = AudioType.UNKNOWN;

        if (Path.GetExtension(audioFilename).ToLower() == ".aiff")
            audioType = AudioType.AIFF;
        else if (Path.GetExtension(audioFilename).ToLower() == ".wav")
            audioType = AudioType.WAV;

        return audioType;
    }
}
