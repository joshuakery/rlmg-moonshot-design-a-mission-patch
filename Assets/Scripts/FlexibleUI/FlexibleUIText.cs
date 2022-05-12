using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class FlexibleUIText : FlexibleUI
{
    protected TMP_Text tmp_text;

    public HeadingType headingType;

    public enum HeadingType
    {
        H1,
        H2,
        H3,
        P,
        Button,
        Caption
    }

    public override void Awake()
    {
        tmp_text = GetComponent<TMP_Text>();

        base.Awake();
    }

    protected override void OnSkinUI()
    {
        try
        {
            switch (headingType)
            {
                case HeadingType.H1:
                    tmp_text.font = skinData.blackFont;
                    break;
                case HeadingType.H2:
                    tmp_text.font = skinData.boldFont;
                    break;
                case HeadingType.H3:
                    tmp_text.font = skinData.regularFont;
                    break;
                case HeadingType.P:
                    tmp_text.font = skinData.regularFont;
                    break;
                case HeadingType.Button:
                    tmp_text.font = skinData.boldFont;
                    break;
                case HeadingType.Caption:
                    tmp_text.font = skinData.regularFont;
                    break;
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e, this);
        }

        // using fontWeight is for some reason making visible/serialized the Submesh GameObject
        // which is annoying because then none of the properties of either gameobject can be animated
        // tmp_text.fontWeight = FontWeight.Black;
        

        base.OnSkinUI();
    }
}