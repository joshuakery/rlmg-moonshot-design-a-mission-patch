using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
[RequireComponent(typeof(Button))]
public class FlexibleUIButton : FlexibleUI
{

    Button button;
    Image image;

    public override void Awake()
    {
        button = GetComponent<Button>();
        image = GetComponent<Image>();

        base.Awake();
    }

    protected override void OnSkinUI()
    {
        button.transition = Selectable.Transition.SpriteSwap;
        button.targetGraphic = image;

        image.type = Image.Type.Sliced;
        image.sprite = skinData.buttonSprite;
        button.spriteState = skinData.buttonSpriteState;

    }
}