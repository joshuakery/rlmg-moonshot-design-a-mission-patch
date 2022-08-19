using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleAudio : MonoBehaviour
{
    public GameEvent toggleOn;
    public GameEvent toggleOff;

    public void RaiseEvent(bool value)
    {
        if (value) toggleOn.Raise();
        else toggleOff.Raise();
    }
}
