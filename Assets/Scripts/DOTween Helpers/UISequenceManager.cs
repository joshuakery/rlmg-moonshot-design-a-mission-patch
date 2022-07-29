using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[CreateAssetMenu(fileName = "UISequenceManager", menuName = "UI Sequence Manager")]
public class UISequenceManager : ScriptableObject
{
    public Sequence currentSequence;
}
