using UnityEngine;
using System.Collections;
using System.IO;
using ArtScan;
using ArtScan.ScanSavingModule;
using rlmg.logging;

public class SaveScansHelper : MonoBehaviour
{
    public GameState gameState;
    public DownloadThreadController downloadThreadController;

    public void DownloadScans(GameEvent callbackEvent)
    {
        string dirPath = Path.Join(Application.streamingAssetsPath, gameState.settings.saveDir);

        if (gameState.currentTeam == null)
        {
            RLMGLogger.Instance.Log("There is no current team. Cannot download scans.", MESSAGETYPE.ERROR);
            return;
        }

        //asynchronous
        StartCoroutine(ScanSaving.DownloadScansCoroutine(downloadThreadController, dirPath, gameState.currentTeam.artworks, true, callbackEvent));
    }

    public void OnApplicationQuit()
    {
        if (gameState.settings.clearCacheOnQuit)
        {
            string trashPath = Path.Join(Application.streamingAssetsPath, gameState.settings.trashDir);
            ScanSaving.DeleteFolderContents(trashPath);
            string savePath = Path.Join(Application.streamingAssetsPath, gameState.settings.saveDir);
            ScanSaving.DeleteFolderContents(savePath);
        }
    }
}

