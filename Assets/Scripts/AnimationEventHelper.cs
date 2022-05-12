using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AnimationEventHelper : MonoBehaviour
{
    public void Raise(GameEvent myEvent)
    {
        myEvent.Raise();
    }
}
