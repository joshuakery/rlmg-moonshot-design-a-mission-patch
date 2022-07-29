using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

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

    public Image imageTemplate;

    private Image[] backgroundImages;
    private Image[] astronautImages;
    private Image[] buildingImages;
    private Image[] flagImages;
    private Image[] roverImages;

    public Sprite[] backgroundSprites;
    public Sprite[] astronautSprites;
    public Sprite[] buildingSprites;
    public Sprite[] flagSprites;
    public Color flagColorLeft = Color.blue;
    public Color flagColorRight = Color.red;
    public Sprite[] roverSprites;

    [Range(0, 2)]
    public int roverStepsCompleted = 0;

    [Range(0, 9)]
    public int huntNumFound = 0;

    [Range(-1, 1)]
    public float charterMeterDecisions;
    [Range(-1, 1)]
    public float charterMeterPriorities;
    [Range(-1, 1)]
    public float charterMeterStrictness;

    [Range(0, 1)]
    public float settlementShelterQuality;
    [Range(0, 1)]
    public float settlementCommsQuality;
    [Range(0, 1)]
    public float settlementSunQuality;
    [Range(0, 1)]
    public float settlementWaterQuality;

    public Color buildingPoorQualityColor = Color.black;

    public Image[] artworkImages;
    public Sprite[] artworkSprites;

    void Awake()
    {
        //Debug.Log(Application.dataPath);
        //Debug.Log(System.Environment.SpecialFolder.Desktop);

        if (imageTemplate == null)
            return;

        astronautImages = MakeImagesForSprites(astronautSprites);
        roverImages = MakeImagesForSprites(roverSprites);
        buildingImages = MakeImagesForSprites(buildingSprites);
        flagImages = MakeImagesForSprites(flagSprites);
        backgroundImages = MakeImagesForSprites(backgroundSprites);

        imageTemplate.gameObject.SetActive(false);
    }

    void OnEnable()
    {
        if (loadOnEnable)
        {
            LoadTeamResults();
        }

        UpdateMoonScene();
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
            UpdateMoonScene();
        }
    }

    void UpdateMoonScene()
    {
        for (int i = 0; i < astronautImages.Length; i++)
        {
            astronautImages[i].gameObject.SetActive(i + 1 <= huntNumFound);
        }

        for (int i = 0; i < roverImages.Length; i++)
        {
            roverImages[i].gameObject.SetActive(i + 1 <= roverStepsCompleted);
        }

        for (int i = 0; i < flagImages.Length; i++)
        {
            if (i == 0)
            {
                flagImages[i].color = Color.Lerp(flagColorLeft, flagColorRight, (charterMeterDecisions + 1f) / 2f);
            }
            else if (i == 1)
            {
                flagImages[i].color = Color.Lerp(flagColorLeft, flagColorRight, (charterMeterPriorities + 1f) / 2f);
            }
            else if (i == 2)
            {
                flagImages[i].color = Color.Lerp(flagColorLeft, flagColorRight, (charterMeterStrictness + 1f) / 2f);
            }
        }

        for (int i = 0; i < buildingImages.Length; i++)
        {
            if (i == 0)
            {
                buildingImages[i].color = Color.Lerp(buildingPoorQualityColor, Color.white, settlementShelterQuality);

                buildingImages[i].gameObject.SetActive(settlementShelterQuality > 0);
            }
            else if (i == 1)
            {
                buildingImages[i].color = Color.Lerp(buildingPoorQualityColor, Color.white, settlementSunQuality);

                buildingImages[i].gameObject.SetActive(settlementSunQuality > 0);
            }
            else if (i == 2)
            {
                buildingImages[i].color = Color.Lerp(buildingPoorQualityColor, Color.white, settlementCommsQuality);

                buildingImages[i].gameObject.SetActive(settlementCommsQuality > 0);
            }
            else if (i == 3)
            {
                buildingImages[i].color = Color.Lerp(buildingPoorQualityColor, Color.white, settlementWaterQuality);

                buildingImages[i].gameObject.SetActive(settlementWaterQuality > 0);
            }
        }

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

    //void MakeImagesForSprites(Sprite[] spritesArray, Image[] imagesArray)
    Image[] MakeImagesForSprites(Sprite[] spritesArray)
    {
        Image[] imagesArray = new Image[spritesArray.Length];

        for (int i = 0; i < spritesArray.Length; i++)
        {
            imagesArray[i] = Instantiate(imageTemplate, imageTemplate.transform.parent);

            imagesArray[i].sprite = spritesArray[i];

            imagesArray[i].transform.SetAsFirstSibling();

            imagesArray[i].name = spritesArray[i].name;
        }

        return imagesArray;
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

        //StartCoroutine(LoadImagesViaFilenames(userData.DirectoryPathImages, singleTeamData.artworks));
    }

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
    }
}
