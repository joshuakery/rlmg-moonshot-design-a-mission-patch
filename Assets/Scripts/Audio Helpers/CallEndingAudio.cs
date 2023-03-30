using System;
using UnityEngine;
using ArtScan;

public class CallEndingAudio : MonoBehaviour
{
    public GameState gameState;
    public UISequenceManager uiSequenceManager;

    public GameEvent CongratulationsAudioEvent;
    public GameEvent TooBadAudioEvent;

    public void CallAudioEvent()
    {
        if (gameState.allScansEmpty)
        {
            TooBadAudioEvent.Raise();
        }
        else
        {
            CongratulationsAudioEvent.Raise();
        }
    }

    public void AppendAsCallback()
    {
        uiSequenceManager.AppendCallback(CallAudioEvent);
    }
}
