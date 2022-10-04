using UnityEngine;
using System;
using System.Collections;
using System.IO;
// using ArtScan;
// using ArtScan.ScanSavingModule;
// using ArtScan.WordSavingUtilsModule;
using rlmg.logging;

public class SaveScansHelper : MonoBehaviour
{
    // public GameState gameState;
    // public RefinedScanController refinedScanController;
    public DownloadThreadController downloadThreadController;
    // public UploadThreadController uploadThreadController;

    // public GameEvent UploadCompleteEvent;
    // public GameEvent NewScanEvent;
    // public GameEvent UploadFailedEvent;

    public GameEvent DownloadFailedEvent;
    public GameEvent DownloadSucceededEvent;

    public GameEvent DeleteFailedEvent;
    public GameEvent DeleteSucceededEvent;

    // public bool doUploadToServer;

    // public bool doRaiseUploadFailed;
    // public bool doRaiseUploadSucceeded;

    public bool doRaiseDownloadFailed;
    public bool doRaiseDownloadSucceeded;

    // public bool doRaiseDeleteFailed;
    // public bool doRaiseDeleteSucceeded;

    public bool clearSaveDirOnQuit;
    // public bool clearTrashDirOnQuit;

    public string saveDir;
    // public string trashDir;

    private void OnEnable()
    {
        // ClientSend.onUploadFailed += RaiseUploadFailed;
        // ClientSend.onUploadSucceeded += RaiseUploadSucceeded;

        ClientSend.onDownloadFailed += RaiseDownloadFailed;
        ClientSend.onDownloadSucceeded += RaiseDownloadSucceeded;

        // ClientSend.onDeleteFailed += RaiseDeleteFailed;
        // ClientSend.onDeleteSucceeded += RaiseDeleteSucceeded;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        
        // ClientSend.onUploadFailed -= RaiseUploadFailed;
        // ClientSend.onUploadSucceeded -= RaiseUploadSucceeded;

        ClientSend.onDownloadFailed -= RaiseDownloadFailed;
        ClientSend.onDownloadSucceeded -= RaiseDownloadSucceeded;

        // ClientSend.onDeleteFailed -= RaiseDeleteFailed;
        // ClientSend.onDeleteSucceeded -= RaiseDeleteSucceeded;
    }

    // private void RaiseUploadFailed()
    // {
    //     doRaiseUploadFailed = true;
    // }

    // private void RaiseUploadSucceeded()
    // {
    //     doRaiseUploadSucceeded = true;
    // }

    private void RaiseDownloadFailed(string msg)
    {
        RLMGLogger.Instance.Log("Artworks download failed. Error msg: "+ msg, MESSAGETYPE.ERROR);
        
        doRaiseDownloadFailed = true;
    }

    private void RaiseDownloadSucceeded()
    {
        doRaiseDownloadSucceeded = true;
    }

    // private void RaiseDeleteFailed()
    // {
    //     doRaiseDeleteFailed = true;
    // }

    // private void RaiseDeleteSucceeded()
    // {
    //     doRaiseDeleteSucceeded = true;
    // }

    private void Update()
    {
        // if (doRaiseUploadFailed)
        // {
        //     if (UploadFailedEvent != null) { UploadFailedEvent.Raise(); }
        //     doRaiseUploadFailed = false;
        // }
        // if (doRaiseUploadSucceeded)
        // {
        //     int index = gameState.GetNextScanIndex();
        //     if (index >= 0)
        //     {
        //         string filename = ScanSaving.FormatScanFilename(gameState.currentTeam.teamName, index);
        //         string dirPath = Path.Join(Application.streamingAssetsPath, gameState.settings.saveDir);
        //         string fullPath = Path.Join(dirPath, filename);

        //         UpdateTeamArtworks(index, fullPath);

        //         gameState.AddScan(gameState.preview);

        //         UploadCompleteEvent.Raise();
        //         NewScanEvent.Raise();
        //     }

        //     doRaiseUploadSucceeded = false;
        // }
        if (doRaiseDownloadFailed)
        {
            if (DownloadFailedEvent != null) { DownloadFailedEvent.Raise(); }
            doRaiseDownloadFailed = false;
        }
        if (doRaiseDownloadSucceeded)
        {
            if (DownloadSucceededEvent != null) { DownloadSucceededEvent.Raise(); }
            doRaiseDownloadSucceeded = false;
        }
        // if (doRaiseDeleteFailed)
        // {
        //     if (DeleteFailedEvent != null) { DeleteFailedEvent.Raise(); }
        //     doRaiseDeleteFailed = false;
        // }
        // if (doRaiseDeleteSucceeded)
        // {
        //     if (DeleteSucceededEvent != null) { DeleteSucceededEvent.Raise(); }
        //     doRaiseDeleteSucceeded = false;
        // }
    }

    public void DownloadScans(GameEvent callbackEvent)
    {
        string dirPath = Path.Join(Application.streamingAssetsPath, saveDir);

        if (Client.instance.team == null)
        {
            RLMGLogger.Instance.Log("There is no current team. Cannot download scans.", MESSAGETYPE.ERROR);
            return;
        }

        if (Client.instance.team.MoonshotTeamData == null)
        {
            RLMGLogger.Instance.Log("There is no current team data. Cannot download scans.", MESSAGETYPE.ERROR);
            return;
        }

        if (Client.instance.team.MoonshotTeamData.artworks == null)
        {
            RLMGLogger.Instance.Log("There is no current team artworks data. Cannot download scans.", MESSAGETYPE.ERROR);
            return;
        }

        StartCoroutine(ScanSaving.DownloadScansCoroutine(downloadThreadController, dirPath, Client.instance.team.MoonshotTeamData.artworks, true, callbackEvent));
    }

    public IEnumerator DownloadScansCoroutine(GameEvent callbackEvent)
    {
        string dirPath = Path.Join(Application.streamingAssetsPath, saveDir);

        if (Client.instance.team == null)
        {
            RLMGLogger.Instance.Log("There is no current team. Cannot download scans.", MESSAGETYPE.ERROR);
            yield break;
        }

        if (Client.instance.team.MoonshotTeamData == null)
        {
            RLMGLogger.Instance.Log("There is no current team data. Cannot download scans.", MESSAGETYPE.ERROR);
            yield break;
        }

        if (Client.instance.team.MoonshotTeamData.artworks == null)
        {
            RLMGLogger.Instance.Log("There is no current team artworks data. Cannot download scans.", MESSAGETYPE.ERROR);
            yield break;
        }

        yield return StartCoroutine(ScanSaving.DownloadScansCoroutine(downloadThreadController, dirPath, Client.instance.team.MoonshotTeamData.artworks, true, callbackEvent));
    }

    public void OnApplicationQuit()
    {
        if (clearSaveDirOnQuit)
        {
            string savePath = Path.Join(Application.streamingAssetsPath, saveDir);
            ScanSaving.DeleteFolderContents(savePath);
        }
        // if (clearTrashDirOnQuit)
        // {
        //     string trashPath = Path.Join(Application.streamingAssetsPath, trashDir);
        //     ScanSaving.DeleteFolderContents(trashPath);
        // }
    }

    // public void SavePreview()
    // {
    //     Debug.Log("saving preview");
    //     StartCoroutine(_UploadPreview());
    // }

    // private IEnumerator _UploadPreview()
    // {
    //     if (refinedScanController.previewMat != null)
    //     {
    //         int index = gameState.GetNextScanIndex();
    //         if (index >= 0)
    //         {
    //             string filename = ScanSaving.FormatScanFilename(gameState.currentTeam.teamName, index);
    //             string dirPath = Path.Join(Application.streamingAssetsPath, gameState.settings.saveDir);
    //             string fullPath = Path.Join(dirPath, filename);

    //             ScanSaving.SaveScan(refinedScanController.previewMat, dirPath, filename);

    //             if (doUploadToServer)
    //             {
    //                 if (File.Exists(fullPath))
    //                     yield return StartCoroutine(uploadThreadController.UploadCoroutine(fullPath));
    //                 else
    //                     RLMGLogger.Instance.Log(System.String.Format("Trying to upload {0} but file does not exist.", fullPath));
    //             }  
    //             else
    //             {
    //                 UpdateTeamArtworks(index, fullPath);

    //                 gameState.AddScan(gameState.preview);

    //                 UploadCompleteEvent.Raise();
    //                 NewScanEvent.Raise();
    //             }

    //         }
    //     }
    // }

    // public void UpdateTeamArtworks(int index, string filepath)
    // {
    //     string filename = Path.GetFileName(filepath);

    //     //Update GameState teams
    //     if (gameState.currentTeam != null)
    //     {
    //         if (gameState.currentTeam.artworks == null || gameState.currentTeam.artworks.Length != gameState.scanMax)
    //             Array.Resize<string>(ref gameState.currentTeam.artworks, gameState.scanMax);

    //         gameState.currentTeam.artworks[index] = filename;

    //         WordSaving.SaveTeamsToFile(gameState.saveFile, gameState.teams);
    //     }

    //     //Update Server current team
    //     if (Client.instance.team != null)
    //     {
    //         if (Client.instance.team.MoonshotTeamData.artworks == null ||
    //             Client.instance.team.MoonshotTeamData.artworks.Length != gameState.scanMax)
    //         {
    //             Array.Resize<string>(ref Client.instance.team.MoonshotTeamData.artworks, gameState.scanMax);
    //         }


    //         Client.instance.team.MoonshotTeamData.artworks[index] = filename;

    //         ClientSend.SendStationDataToServer();
    //     }


    // }
}

