using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
[RequireComponent(typeof(Image))]
public class FlexibleUIToggle : FlexibleUI
{
    Toggle toggle;
    Image image;

    public override void Awake()
    {
        toggle = GetComponent<Toggle>();
        image = GetComponent<Image>();

        base.Awake();
    }

    protected override void OnSkinUI()
    {
        toggle.transition = Selectable.Transition.ColorTint;
        toggle.targetGraphic = image;

        image.type = Image.Type.Sliced;
        image.sprite = skinData.toggleSprite;
        toggle.spriteState = skinData.toggleSpriteState;

        Image checkmark = toggle.graphic.gameObject.GetComponent<Image>();
        checkmark.sprite = skinData.toggleCheckmarkSprite;

        


    }
}