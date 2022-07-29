using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using ArtScan.NamesakesModule;

public class NameResult : GenericWindow
{
    public TMP_Text fullNameDisplay;
    public TMP_Text moonbaseNameDisplay;
    public RawImage moonbaseNameDisplayImage;
    public TMP_Text descriptionDisplay;

    public RawImage profileImage;

    public void SetTexts()
    {
        if (gameState.namesakesData == null || gameState.namesakesData.Count == 0)
        {
            Debug.Log("NO NAMESAKES DATA");
            return;
        }

        string key = gameState.currentTeam.namesake;
        if (!String.IsNullOrEmpty(key))
        {
            Namesake namesake = gameState.namesakesData[key];

            if (fullNameDisplay != null)    
                fullNameDisplay.text = namesake.fullName;

            if (moonbaseNameDisplay != null)
                moonbaseNameDisplay.text = namesake.moonbaseName;

            if (moonbaseNameDisplayImage != null)
                moonbaseNameDisplayImage.texture = namesake.moonbaseNameImage;

            if (descriptionDisplay != null)
                descriptionDisplay.text = namesake.description;

            if (profileImage != null)
                profileImage.texture = namesake.texture;
        }

    }
}
