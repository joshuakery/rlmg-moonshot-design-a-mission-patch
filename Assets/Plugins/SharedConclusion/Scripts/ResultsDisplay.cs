using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using rlmg.logging;

public class ResultsDisplay : MonoBehaviour
{
    public MoonshotDataHandler _userData;
    private MoonshotDataHandler userData
    {
        get
        {
            if (_userData == null)
            {
                _userData = GetComponentInParent<MoonshotDataHandler>();
            }

            return _userData;
        }
    }

    public bool useServer = true;

    public bool updateConstantly = true;
    [Range(0, 4)]
    public static int teamNum = 0;
    public bool loadOnEnable = true;
    public bool doLoadResultsTest = false;

    public CanvasGroup[] loadingScreens;
    public bool hasFadedOutLoadScreens = false;
    public float minLoadScreenDisplayDur = 5f;
    private float loadStartTime;

    [Range(0, 2)]
    public int roverStepsCompleted = 0;

    [Range(0, 2)]
    public int mapRoundsCompleted = 0;

    [Range(0, 9)]
    public int huntNumFound = 0;

    [Range(-1, 1)]
    public float charterMeterDecisions;
    [Range(-1, 1)]
    public float charterMeterPriorities;
    [Range(-1, 1)]
    public float charterMeterStrictness;

    [HideInInspector]
    [Range(0, 1)]
    public float settlementShelterQuality;
    [HideInInspector]
    [Range(0, 1)]
    public float settlementCommsQuality;
    [HideInInspector]
    [Range(0, 1)]
    public float settlementSunQuality;
    [HideInInspector]
    [Range(0, 1)]
    public float settlementWaterQuality;

    public Image[] artworkImages;
    public Sprite[] artworkSprites;

    //public float durWaitBetweenImageSteps = 1f;
    public bool doTestUploadFirst = false;
    public string testUploadFolder = "TestImages";
    public string[] testArtworkFilenames;

    public string downloadFolder = "Images";

    private bool hasAtLeastTriedLoadingArtworks = false;

    void OnEnable()
    {
        if (loadOnEnable)
        {
            LoadTeamResults();
        }

        SetLoadingScreenAlpha(1f);

        foreach (CanvasGroup loadingScreen in loadingScreens)
        {
            if (loadingScreen != null)
            {
                loadingScreen.gameObject.SetActive(true);
            }
        }

        hasFadedOutLoadScreens = false;

        loadStartTime = Time.time;

        UpdateArtworks();
    }

    void Update()
    {
        if (doLoadResultsTest)
        {
            doLoadResultsTest = false;

            LoadTeamResults();
        }

        if (updateConstantly)
        {
            UpdateArtworks();
        }

        if (!hasFadedOutLoadScreens && Time.time >= loadStartTime + minLoadScreenDisplayDur && hasAtLeastTriedLoadingArtworks)
        {
            hasFadedOutLoadScreens = true;
            
            StartCoroutine(FadeOutLoadingScreen());
        }
    }

    void UpdateArtworks()
    {
        for (int i = 0; i < artworkImages.Length; i++)
        {
            if (artworkSprites != null && artworkSprites.Length > i && artworkSprites[i] != null)
            {
                artworkImages[i].sprite = artworkSprites[i];

                artworkImages[i].gameObject.SetActive(true);
            }
            else
            {
                artworkImages[i].gameObject.SetActive(false);
            }
        }
    }

    public void LoadTeamResults(int teamNumber)
    {
        teamNum = teamNumber;

        LoadTeamResults();
    }

    [ContextMenu("Load Team Results")]
    public void LoadTeamResults()
    {
        if (useServer == true)
        {
            if (Client.instance.team == null)
            {
                Client.instance.CreateNewTeam();

                Debug.LogError("Created new team data because none existed. This should only happen when testing the results scene in isolation.");
            }
            
            LoadTeamResults(Client.instance.team.MoonshotTeamData);
        }
        else if (userData != null)
        {
            userData.teamNum = teamNum;
            
            userData.onLoadingComplete += JsonLoadingCompleteCallback;

            userData.LoadExternalJson();
        }
    }

    private void JsonLoadingCompleteCallback(MoonshotTeamData singleTeamData)
    {
        userData.onLoadingComplete -= JsonLoadingCompleteCallback;

        LoadTeamResults(singleTeamData);
    }

    public void LoadTeamResults(MoonshotTeamData singleTeamData)
    {
        if (singleTeamData == null)
        {
            return;
        }

        Debug.Log("Loading Team Results Data   teamName = " + singleTeamData.teamName);

        roverStepsCompleted = singleTeamData.roverStepsCompleted;

        huntNumFound = singleTeamData.huntNumFound;

        charterMeterDecisions = singleTeamData.charterMeterDecisions;
        charterMeterPriorities = singleTeamData.charterMeterPriorities;
        charterMeterStrictness = singleTeamData.charterMeterStrictness;

        settlementShelterQuality = singleTeamData.settlementShelterQuality;
        settlementCommsQuality = singleTeamData.settlementCommsQuality;
        settlementSunQuality = singleTeamData.settlementSunQuality;
        settlementWaterQuality = singleTeamData.settlementWaterQuality;

        mapRoundsCompleted = singleTeamData.mapRoundsCompleted;

        if (useServer)
        {
            StartCoroutine(DownloadThenLoadImages(singleTeamData));
        }
        else
        {
            StartCoroutine(LoadImagesViaFilenames(userData.DirectoryPathImages, singleTeamData.artworks));
        }
    }

    public IEnumerator DownloadThenLoadImages(MoonshotTeamData singleTeamData)
    {
        Debug.Log("ResultsDisplay.DownloadThenLoadImages() singleTeamData = " + singleTeamData);
        
        if (singleTeamData == null)
        {
            Debug.LogError("singleTeamData == null");
            
            yield break;
        }
        
        string directoryPath = Path.Join(Application.streamingAssetsPath, downloadFolder);

        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        //a bunch of stuff for crude and hacky testing -JY
        //todo: maybe use multi-threading and a coroutine like I now do with the download? it probably doesn't matter since this is just for testing.
        if (doTestUploadFirst)
        {
            Debug.Log("test artwork file(s) about to be uploaded.   Time.time = " + Time.time);

            yield return new WaitForSeconds(1f);  //just to let everything else start-up before it freezes

            singleTeamData.artworks = new string[testArtworkFilenames.Length];
            
            //foreach (string testArtworkFilename in testArtworkFilenames)
            for (int i = 0; i < testArtworkFilenames.Length; i++)
            {
                //ClientSend.SendFileToServer(Path.Join(directoryPath, testArtworkFilename));
                ClientSend.SendFileToServer(Path.Join(Path.Join(Application.streamingAssetsPath, testUploadFolder), testArtworkFilenames[i]));

                //yield return new WaitForSeconds(durWaitBetweenImageSteps);

                singleTeamData.artworks[i] = testArtworkFilenames[i];
            }

            //yield return new WaitForSeconds(durWaitBetweenImageSteps);
        }

        if (singleTeamData.artworks == null)
        {
            //Debug.LogError("singleTeamData.artworks == null");
            RLMGLogger.Instance.Log("Final results (ResultsDisplay.cs) is trying to load artworks but singleTeamData.artworks == null", MESSAGETYPE.ERROR);

            if (!updateConstantly)
            {
                UpdateArtworks();
            }

            hasAtLeastTriedLoadingArtworks = true;
            
            yield break;
        }

        artworkSprites = new Sprite[singleTeamData.artworks.Length];


        //Debug.Log("artwork files about to be downloaded.   Time.time = " + Time.time);

        // for (int i = 0; i < singleTeamData.artworks.Length; i++)
        // {
        //     ClientSend.GetFileFromServer(singleTeamData.artworks[i], directoryPath);
        // }


        yield return StartCoroutine(GetComponent<SaveScansHelper>().DownloadScansCoroutine(null));

        //Debug.Log("artwork files have been downloaded.   Time.time = " + Time.time);

        //TODO: I might need/want to add some wait or check that they are in fact downloaded. Maybe use Josh's multi-threaded approach?

        StartCoroutine(LoadImagesViaFilenames(directoryPath, singleTeamData.artworks));
    }

    public float loadingFadeOutDur = 1f;

    public IEnumerator FadeOutLoadingScreen()
    {
        float t = 0f;

        while (t < loadingFadeOutDur)
        {
            SetLoadingScreenAlpha(1f - (t / loadingFadeOutDur));

            t += Time.deltaTime;

            yield return null;
        }

        SetLoadingScreenAlpha(0f);

        foreach (CanvasGroup loadingScreen in loadingScreens)
        {
            if (loadingScreen != null)
            {
                loadingScreen.gameObject.SetActive(false);
            }
        }
    }

    private void SetLoadingScreenAlpha(float alpha)
    {
        foreach (CanvasGroup loadingScreen in loadingScreens)
        {
            if (loadingScreen != null)
            {
                loadingScreen.alpha = alpha;
            }
        }
    }

    // public IEnumerator LoadImageViaFilename(string directoryPath, string filename)
    // {
    //     if (!string.IsNullOrEmpty(filename))
    //     {
    //         string imgFilePath = ContentLoader.fileProtocolPrefix + System.IO.Path.Combine(directoryPath, filenames[i]);

    //         //Debug.Log("trying to load image from file path: " + imgFilePath + "   directory exists? " + Directory.Exists(Path.Combine(directoryPath, filenames[i])));
    //         Debug.Log("trying to load image from file path: " + imgFilePath + "   directory exists? " + Directory.Exists(Path.GetDirectoryName(Path.Combine(directoryPath, filenames[i]))));


    //         yield return StartCoroutine(ContentLoader.LoadSpriteFromFilepath(imgFilePath, result => artworkSprites[i] = result));

    //         Debug.Log("artworkSprites #" + i + " = " + artworkSprites[i]);
    //     }

    //     if (!updateConstantly)
    //     {
    //         UpdateMoonScene();
    //     }
    // }

    // public IEnumerator LoadImagesViaFilenames(string[] filenames, Sprite[] sprites)  //I couldn't get this passed sprite array reference to work
    // {
    //     sprites = new Sprite[filenames.Length];

    //     for (int i = 0; i < filenames.Length; i++)
    //     {
    //         if (!string.IsNullOrEmpty(filenames[i]))
    //         {
    //             string imgFilePath = ContentLoader.fileProtocolPrefix + System.IO.Path.Combine(Application.streamingAssetsPath, filenames[i]);

    //             yield return StartCoroutine(ContentLoader.LoadSpriteFromFilepath(imgFilePath, result => sprites[i] = result));
    //         }
    //     }
    // }

    public IEnumerator LoadImagesViaFilenames(string directoryPath, string[] filenames)
    {
        artworkSprites = new Sprite[filenames.Length];

        for (int i = 0; i < filenames.Length; i++)
        {
            if (!string.IsNullOrEmpty(filenames[i]))
            {
                string imgFilePath = ContentLoader.fileProtocolPrefix + System.IO.Path.Combine(directoryPath, filenames[i]);

                //Debug.Log("trying to load image from file path: " + imgFilePath + "   directory exists? " + Directory.Exists(Path.Combine(directoryPath, filenames[i])));
                Debug.Log("trying to load image from file path: " + imgFilePath + "   directory exists? " + Directory.Exists(Path.GetDirectoryName(Path.Combine(directoryPath, filenames[i]))));


                yield return StartCoroutine(ContentLoader.LoadSpriteFromFilepath(imgFilePath, result => artworkSprites[i] = result));

                Debug.Log("artworkSprites #" + i + " = " + artworkSprites[i]);
            }
        }

        if (!updateConstantly)
        {
            UpdateArtworks();
        }

        hasAtLeastTriedLoadingArtworks = true;

        //StartCoroutine(FadeOutLoadingScreen());
    }
}
