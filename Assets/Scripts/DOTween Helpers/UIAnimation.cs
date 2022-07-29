using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UIAnimation", menuName = "UI Animation")]
public class UIAnimation : ScriptableObject
{
    public enum AnimationType
    {
        Fade
    }

    public AnimationType animationType;

    public float duration;
    public bool useFrom;
    public Vector3 from;
    public Vector3 to;
}
