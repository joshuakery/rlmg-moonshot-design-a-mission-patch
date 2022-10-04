using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResultsDisplayCompositedScene : MonoBehaviour
{
    public ResultsDisplay resultsDisplay;
    
    public bool updateConstantly = true;
    
    public Image imageTemplate;

    //private Image[] backgroundImages;
    private Image[] astronautImages;
    private Image[] buildingImages;
    private Image[] flagImages;
    private Image[] roverImages;

    //public Sprite[] backgroundSprites;
    public Sprite[] astronautSprites;
    public Sprite[] buildingSprites;
    // public Sprite[] flagSpritesLeft;
    // public Sprite[] flagSpritesRight;
    public Sprite flagSpriteDecisionsLeft;
    public Sprite flagSpriteDecisionsRight;
    public Sprite flagSpritePrioritiesLeft;
    public Sprite flagSpritePrioritiesRight;
    public Sprite flagSpriteStrictnessLeft;
    public Sprite flagSpriteStrictnessRight;
    // public Color flagColorLeft = Color.blue;
    // public Color flagColorRight = Color.red;
    public Sprite[] roverSprites;

    //public Color buildingPoorQualityColor = Color.black;
    
    void Awake()
    {
        //Debug.Log(Application.dataPath);
        //Debug.Log(System.Environment.SpecialFolder.Desktop);

        if (imageTemplate == null)
            return;

        
        astronautImages = MakeImagesForSprites(astronautSprites);
        roverImages = MakeImagesForSprites(roverSprites);
        flagImages = MakeImagesForSprites(new Sprite[]{flagSpriteDecisionsLeft, flagSpritePrioritiesLeft, flagSpriteStrictnessLeft});
        //backgroundImages = MakeImagesForSprites(backgroundSprites);
        buildingImages = MakeImagesForSprites(buildingSprites);
        for (int i = 0; i < buildingImages.Length; i++)
        {
            buildingImages[i].transform.SetSiblingIndex(i);
        }

        imageTemplate.gameObject.SetActive(false);
    }
    
    void Start()
    {
        
    }

    void OnEnable()
    {
        UpdateMoonScene();
    }

    void Update()
    {
        if (updateConstantly)
        {
            UpdateMoonScene();
        }
    }

    void UpdateMoonScene()
    {
        if (resultsDisplay == null)
        {
            return;
        }
        
        for (int i = 0; i < astronautImages.Length; i++)
        {
            astronautImages[i].gameObject.SetActive(i + 1 <= resultsDisplay.huntNumFound);
        }

        for (int i = 0; i < roverImages.Length; i++)
        {
            roverImages[i].gameObject.SetActive(i + 1 <= resultsDisplay.roverStepsCompleted);
        }

        for (int i = 0; i < flagImages.Length; i++)
        {
            // if (i == 0)
            // {
            //     flagImages[i].color = Color.Lerp(flagColorLeft, flagColorRight, (resultsDisplay.charterMeterDecisions + 1f) / 2f);
            // }
            // else if (i == 1)
            // {
            //     flagImages[i].color = Color.Lerp(flagColorLeft, flagColorRight, (resultsDisplay.charterMeterPriorities + 1f) / 2f);
            // }
            // else if (i == 2)
            // {
            //     flagImages[i].color = Color.Lerp(flagColorLeft, flagColorRight, (resultsDisplay.charterMeterStrictness + 1f) / 2f);
            // }

            if (i == 0)
            {
                flagImages[i].sprite = GetCharterValueWithTieBreaker(resultsDisplay.charterMeterDecisions) < 0f ? flagSpriteDecisionsLeft : flagSpriteDecisionsRight;
            }
            else if (i == 1)
            {
                flagImages[i].sprite = GetCharterValueWithTieBreaker(resultsDisplay.charterMeterPriorities) < 0f ? flagSpritePrioritiesLeft : flagSpritePrioritiesRight;
            }
            else if (i == 2)
            {
                flagImages[i].sprite = GetCharterValueWithTieBreaker(resultsDisplay.charterMeterStrictness) < 0f ? flagSpriteStrictnessLeft : flagSpriteStrictnessRight;
            }
        }

        for (int i = 0; i < buildingImages.Length; i++)
        {
            // if (i == 0)
            // {
            //     buildingImages[i].color = Color.Lerp(buildingPoorQualityColor, Color.white, resultsDisplay.settlementShelterQuality);

            //     buildingImages[i].gameObject.SetActive(resultsDisplay.settlementShelterQuality > 0);
            // }
            // else if (i == 1)
            // {
            //     buildingImages[i].color = Color.Lerp(buildingPoorQualityColor, Color.white, resultsDisplay.settlementSunQuality);

            //     buildingImages[i].gameObject.SetActive(resultsDisplay.settlementSunQuality > 0);
            // }
            // else if (i == 2)
            // {
            //     buildingImages[i].color = Color.Lerp(buildingPoorQualityColor, Color.white, resultsDisplay.settlementCommsQuality);

            //     buildingImages[i].gameObject.SetActive(resultsDisplay.settlementCommsQuality > 0);
            // }
            // else if (i == 3)
            // {
            //     buildingImages[i].color = Color.Lerp(buildingPoorQualityColor, Color.white, resultsDisplay.settlementWaterQuality);

            //     buildingImages[i].gameObject.SetActive(resultsDisplay.settlementWaterQuality > 0);
            // }

            buildingImages[i].gameObject.SetActive(i <= resultsDisplay.mapRoundsCompleted);
        }

        // for (int i = 0; i < artworkImages.Length; i++)
        // {
        //     if (artworkSprites != null && artworkSprites.Length > i && artworkSprites[i] != null)
        //     {
        //         artworkImages[i].sprite = artworkSprites[i];

        //         artworkImages[i].gameObject.SetActive(true);
        //     }
        //     else
        //     {
        //         artworkImages[i].gameObject.SetActive(false);
        //     }
        // }
    }

    float GetCharterValueWithTieBreaker(float rawValue)
    {
        float noTieValue = rawValue;
        if (noTieValue == 0f)
        {
            if (updateConstantly)
            {
                noTieValue = 1f;
            }
            else
            {
                if (Random.value > 0.5f)
                {
                    noTieValue = 1f;
                }
                else
                {
                    noTieValue = -1f;
                }
            }
        }
        
        return noTieValue;
    }

    Image[] MakeImagesForSprites(Sprite[] spritesArray, bool frontToBack = true)
    {
        Image[] imagesArray = new Image[spritesArray.Length];

        for (int i = 0; i < spritesArray.Length; i++)
        {
            imagesArray[i] = Instantiate(imageTemplate, imageTemplate.transform.parent);

            imagesArray[i].sprite = spritesArray[i];

            if (frontToBack)
            {
                imagesArray[i].transform.SetAsFirstSibling();
            }
            else
            {
                imagesArray[i].transform.SetAsLastSibling();
            }

            imagesArray[i].name = spritesArray[i].name;
        }

        return imagesArray;
    }
}
