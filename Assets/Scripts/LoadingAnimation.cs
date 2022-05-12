using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingAnimation : MonoBehaviour
{
    public GameEvent AfterLoadingEvent;

    public void RaiseAfterLoadingEvent()
    {
        AfterLoadingEvent.Raise();
    }
}
