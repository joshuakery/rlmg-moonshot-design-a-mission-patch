using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[CreateAssetMenu(menuName = "Flexible UI Data")]
public class FlexibleUIData : ScriptableObject
{
    public Sprite buttonSprite;
    public SpriteState buttonSpriteState;

    public Sprite toggleSprite;
    public SpriteState toggleSpriteState;
    public Sprite toggleCheckmarkSprite;

    public TMP_FontAsset regularFont;
    public TMP_FontAsset italicFont;
    public TMP_FontAsset boldFont;
    public TMP_FontAsset boldItalicFont;
    public TMP_FontAsset blackFont;
    public TMP_FontAsset blackItalicFont;

    public Sprite panelBackground;
    public Sprite panelTiling;
    public Sprite panelBorder;
    
}
