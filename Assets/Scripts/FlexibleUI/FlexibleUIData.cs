using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class CharacterSpacingOptions
{
    public int character = 0;
    public int word = 0;
    public int line = 0;
    public int paragraph = 0;
}

[System.Serializable]
public class TextType
{
    public int size = 36;
    public TMP_FontAsset font;
    public FontWeight weight = FontWeight.Regular;
    public CharacterSpacingOptions spacingOptions;
}
[System.Serializable]
public class Typography
{
    public TextType h1;
    public TextType h2;
    public TextType h3;
    public TextType p;
    public TextType caption;
}

[CreateAssetMenu(menuName = "Flexible UI Data")]
public class FlexibleUIData : ScriptableObject
{
    [Header("Color Palette")]
    public Color32 primaryColor;
    public Color32 secondaryColor;
    public Color32 whiteColor;
    public Color32 blackColor;
    public Color32 supportingNeutralColor;
    public Color32 accentColor;
    public TMP_ColorGradient primaryTextGradient;
    public TMP_ColorGradient secondaryTextGradient;

    [Header("Buttons")]
    public Sprite primaryButtonSprite;
    public SpriteState primaryButtonSpriteState;
    public Sprite secondaryButtonSprite;
    public SpriteState secondaryButtonSpriteState;

    [Header("Toggles")]
    public Sprite checkmark;
    public Sprite toggleBackground;
    


    // [Header("Font Style")]
    // public TMP_FontAsset primaryFont;
    // public TMP_FontAsset secondaryFont;

    [Header("Typography")]
    public Typography primaryTypography;
    public Typography secondaryTypography;

    [Header("Panels")]

    public Sprite panelBackground;
    public Sprite panelTiling;
    public Sprite panelBorder;

    [Header("Lines")]
    public Sprite primaryLineGradient;
    public Sprite secondaryLineGradient;
    
}
