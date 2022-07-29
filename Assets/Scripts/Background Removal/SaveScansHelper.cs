using UnityEngine;
using System.Collections;
using System.IO;
using ArtScan;
using ArtScan.ScanSavingModule;
using rlmg.logging;

public class SaveScansHelper : MonoBehaviour
{
    public GameState gameState;

    public void DownloadScans(GameEvent callbackEvent)
    {
        string dirPath = Path.Join(Application.streamingAssetsPath, gameState.settings.saveDir);

        if (gameState.currentTeam == null)
        {
            RLMGLogger.Instance.Log("There is no current team. Cannot download scans.", MESSAGETYPE.ERROR);
            return;
        }

        //asynchronous
        StartCoroutine(ScanSaving.DownloadScansCoroutine(dirPath, gameState.currentTeam.artworks, true, callbackEvent));
    }
}

