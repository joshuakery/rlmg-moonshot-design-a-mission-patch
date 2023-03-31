using UnityEngine;
using System;
using System.Collections;
using System.IO;
using ArtScan.CoreModule;
using ArtScan.ScanSavingModule;
using ArtScan.WordSavingUtilsModule;
using rlmg.logging;

namespace ArtScan.ScanSavingModule
{
    public class SaveScansHelper : MonoBehaviour
    {
        [SerializeField]
        private GameState gameState;

        private RefinedScanController refinedScanController;

        [SerializeField]
        private DownloadThreadController downloadThreadController;
        [SerializeField]
        private UploadThreadController uploadThreadController;

        [SerializeField]
        private GameEvent UploadCompleteEvent;
        [SerializeField]
        private GameEvent NewScanEvent;
        [SerializeField]
        private GameEvent UploadFailedEvent;

        [SerializeField]
        private GameEvent DownloadFailedEvent;
        [SerializeField]
        private GameEvent DownloadSucceededEvent;

        [SerializeField]
        private GameEvent DeleteFailedEvent;
        [SerializeField]
        private GameEvent DeleteSucceededEvent;

        [SerializeField]
        private bool doUploadToServer;

        private bool doRaiseUploadFailed;
        private bool doRaiseUploadSucceeded;

        private bool doRaiseDownloadFailed;
        private bool doRaiseDownloadSucceeded;

        private bool doRaiseDeleteFailed;
        private bool doRaiseDeleteSucceeded;

        private bool waitingToUpdateLocalDataWithPreview = false;

        private void Awake()
        {
            if (refinedScanController == null)
                refinedScanController = FindObjectOfType<RefinedScanController>();
        }

        private void OnEnable()
        {
            ClientSend.onUploadFailed += RaiseUploadFailed;
            ClientSend.onUploadSucceeded += RaiseUploadSucceeded;

            ClientSend.onDownloadFailed += RaiseDownloadFailed;
            ClientSend.onDownloadSucceeded += RaiseDownloadSucceeded;

            ClientSend.onDeleteFailed += RaiseDeleteFailed;
            ClientSend.onDeleteSucceeded += RaiseDeleteSucceeded;
        }

        private void OnDisable()
        {
            ClientSend.onUploadFailed -= RaiseUploadFailed;
            ClientSend.onUploadSucceeded -= RaiseUploadSucceeded;

            ClientSend.onDownloadFailed -= RaiseDownloadFailed;
            ClientSend.onDownloadSucceeded -= RaiseDownloadSucceeded;

            ClientSend.onDeleteFailed -= RaiseDeleteFailed;
            ClientSend.onDeleteSucceeded -= RaiseDeleteSucceeded;
        }

        private void RaiseUploadFailed(string msg)
        {
            doRaiseUploadFailed = true;
        }

        private void RaiseUploadSucceeded()
        {
            doRaiseUploadSucceeded = true;
        }

        private void RaiseDownloadFailed(string msg)
        {
            doRaiseDownloadFailed = true;
        }

        private void RaiseDownloadSucceeded()
        {
            doRaiseDownloadSucceeded = true;
        }

        private void RaiseDeleteFailed(string msg)
        {
            doRaiseDeleteFailed = true;
        }

        private void RaiseDeleteSucceeded()
        {
            doRaiseDeleteSucceeded = true;
        }

        private void Update()
        {
            if (doRaiseUploadFailed)
            {
                if (UploadFailedEvent != null) { UploadFailedEvent.Raise(); }
                doRaiseUploadFailed = false;
            }
            if (doRaiseUploadSucceeded)
            {
                if (waitingToUpdateLocalDataWithPreview)
                {
                    UpdateLocalDataWithPreview(); 
                    waitingToUpdateLocalDataWithPreview = false;
                }

                UploadCompleteEvent.Raise();
                doRaiseUploadSucceeded = false;
            }
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
            if (doRaiseDeleteFailed)
            {
                if (DeleteFailedEvent != null) { DeleteFailedEvent.Raise(); }
                doRaiseDeleteFailed = false;
            }
            if (doRaiseDeleteSucceeded)
            {
                if (DeleteSucceededEvent != null) { DeleteSucceededEvent.Raise(); }
                doRaiseDeleteSucceeded = false;
            }
        }

        public void DownloadScans(GameEvent callbackEvent)
        {
            string dirPath = Path.Join(Application.streamingAssetsPath, gameState.settings.saveDir);

            if (gameState.currentTeam == null)
            {
                RLMGLogger.Instance.Log("There is no current team. Cannot download scans.", MESSAGETYPE.ERROR);
                return;
            }

            StartCoroutine(ScanSaving.DownloadScansCoroutine(downloadThreadController, dirPath, gameState.currentTeam.artworks, true, callbackEvent));
        }

        public void OnApplicationQuit()
        {
            if (gameState.settings.clearSaveDirOnQuit)
            {
                string savePath = Path.Join(Application.streamingAssetsPath, gameState.settings.saveDir);
                ScanSaving.DeleteFolderContents(savePath);
            }
            if (gameState.settings.clearTrashDirOnQuit)
            {
                string trashPath = Path.Join(Application.streamingAssetsPath, gameState.settings.trashDir);
                ScanSaving.DeleteFolderContents(trashPath);
            }
        }

        public void SavePreview()
        {
            StartCoroutine(_UploadPreview());
        }

        private IEnumerator _UploadPreview()
        {
            if (refinedScanController.previewMat != null)
            {
                int index = gameState.savedScanManager.GetNextScanIndex();

                string filename = ScanSaving.FormatScanFilename(gameState.currentTeam.teamName, index);
                string dirPath = Path.Join(Application.streamingAssetsPath, gameState.settings.saveDir);
                string fullPath = Path.Join(dirPath, filename);

                ScanSaving.SaveScan(refinedScanController.previewMat, dirPath, filename);

                if (doUploadToServer)
                {
                    if (File.Exists(fullPath))
                    {
                        waitingToUpdateLocalDataWithPreview = true;
                        yield return StartCoroutine(uploadThreadController.UploadCoroutine(fullPath));
                    }
                    else
                        RLMGLogger.Instance.Log(System.String.Format("Trying to upload {0} but file does not exist.", fullPath));
                }
                else
                {
                    UpdateLocalDataWithPreview(index, fullPath);
                    UploadCompleteEvent.Raise();
                }
            }
        }

        private void UpdateLocalDataWithPreview()
        {
            int index = gameState.savedScanManager.GetNextScanIndex();

            string filename = ScanSaving.FormatScanFilename(gameState.currentTeam.teamName, index);
            string dirPath = Path.Join(Application.streamingAssetsPath, gameState.settings.saveDir);
            string fullPath = Path.Join(dirPath, filename);

            UpdateLocalDataWithPreview(index, fullPath);
        }

        private void UpdateLocalDataWithPreview(int index, string fullPath)
        {
            UpdateTeamArtworks(index, fullPath);
            gameState.AddPreview();
            NewScanEvent.Raise();
        }

        public void UpdateTeamArtworks(int index, string filepath)
        {
            Debug.Log("updating team artworks");
            string filename = Path.GetFileName(filepath);

            //Update GameState teams
            if (gameState.currentTeam != null)
            {
                if (gameState.currentTeam.artworks == null || gameState.currentTeam.artworks.Length != gameState.scanMax)
                    Array.Resize<string>(ref gameState.currentTeam.artworks, gameState.scanMax);

                gameState.currentTeam.artworks[index] = filename;

                WordSaving.SaveTeamsToFile(gameState.saveFile, gameState.teams);
            }

            //Update Server current team
            if (Client.instance != null && Client.instance.team != null)
            {
                if (Client.instance.team.MoonshotTeamData.artworks == null ||
                    Client.instance.team.MoonshotTeamData.artworks.Length != gameState.scanMax)
                {
                    Array.Resize<string>(ref Client.instance.team.MoonshotTeamData.artworks, gameState.scanMax);
                }


                Client.instance.team.MoonshotTeamData.artworks[index] = filename;

                ClientSend.SendStationDataToServer();
            }
        }

        public void AbortUpload()
        {
            StopAllCoroutines();
            uploadThreadController.AbortThread();
        }
    }

}


