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
        bool isEmpty = Array.TrueForAll(gameState.scans, scan => scan == null);
        if (isEmpty)
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
