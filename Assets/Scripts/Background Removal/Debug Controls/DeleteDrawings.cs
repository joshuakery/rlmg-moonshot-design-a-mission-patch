using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using ArtScan;
using ArtScan.ScanSavingModule;
using OpenCVForUnity.UnityUtils.Helper;

namespace ArtScan.ScanSavingModule
{
    public class DeleteDrawings : MonoBehaviour
    {
        public class ScanHistory
        {
            public string filename;
            public int index;

            public ScanHistory(string _filename, int _index)
            {
                filename = _filename;
                index = _index;
            }
        }

        public DownloadThreadController downloadThreadController;
        public DeleteThreadController deleteThreadController;
        public UploadThreadController uploadThreadController;

        public RectTransform patchesContainer;
        public GameObject loadingFeedback;

        public GameState gameState;

        public Button undoButton;
        public Button deleteButton;

        public List<ScanHistory> scanHistories;

        public int viewedTeamIndex;

        public MoonshotTeamData viewedTeam
        {
            get
            {
                if (viewedTeamIndex < gameState.teams.Count)
                    return gameState.teams[viewedTeamIndex];
                else
                    return null;
            }
        }

        public Texture2D[] scans;

        public RemoveBackgroundSettings settings;
        public myWebCamTextureToMatHelper webCamTextureToMatHelper;

        private void Start()
        {
            deleteButton.interactable = false;
            undoButton.interactable = false;

            scanHistories = new List<ScanHistory>();

            viewedTeamIndex = gameState.currentTeamIndex;
            ViewTeam();
        }

        private void OnEnable()
        {
            SyncScans();
            UpdatePatches();
        }

        public void ViewTeam()
        {
            StartCoroutine(_ViewTeam());
        }

        private IEnumerator _ViewTeam()
        {
            if (viewedTeamIndex != gameState.currentTeamIndex)
            {
                patchesContainer.gameObject.SetActive(false);
                if (loadingFeedback != null)
                    loadingFeedback.SetActive(true);

                //download images to aux folder
                string dirPath = Path.Join(Application.streamingAssetsPath, settings.saveDir);
                //synchronous
                //ScanSaving.DownloadScans(dirPath, viewedTeam, false);
                //asynchronous
                yield return StartCoroutine(ScanSaving.DownloadScansCoroutine(downloadThreadController, dirPath, viewedTeam.artworks, false, null));

                patchesContainer.gameObject.SetActive(true);
                if (loadingFeedback != null)
                    loadingFeedback.SetActive(false);

                //read images to Texture2D and add to scans list
                DirectoryInfo mainDI = new DirectoryInfo(dirPath);
                if (viewedTeam.artworks != null && mainDI.Exists)
                {
                    scans = new Texture2D[viewedTeam.artworks.Length];

                    for (int i = 0; i < viewedTeam.artworks.Length; i++)
                    {
                        string filename = viewedTeam.artworks[i];
                        if (!String.IsNullOrEmpty(filename))
                        {
                            string filepath = Path.Join(dirPath, filename);

                            Texture2D scanTexture = ScanSaving.GetTexture2DFromImageFile(filepath, settings, webCamTextureToMatHelper);

                            scans[i] = scanTexture;
                        }
                    }
                }
            }
            else
            {
                SyncScans();
            }

            //update special patch log
            UpdatePatches();

            //disable delete and undo
            deleteButton.interactable = false;
            undoButton.interactable = false;

            //clear scanhistories
            if (scanHistories != null) { scanHistories.Clear(); }
        }

        public void SyncScans()
        {
            if (viewedTeamIndex == gameState.currentTeamIndex)
            {
                scans = gameState.scans;
            }
        }

        public void UpdatePatches()
        {
            for (int i = 0; i < patchesContainer.childCount; i++)
            {
                Transform patchLogItem = patchesContainer.GetChild(i);
                Image img = patchLogItem.GetChild(0).GetComponent<Image>();
                RawImage ri = patchLogItem.GetChild(1).GetComponent<RawImage>();

                if (i < scans.Length && scans[i] != null)
                {
                    Texture2D scan = scans[i];
                    ri.texture = scan;

                    ri.gameObject.SetActive(true);
                    img.gameObject.SetActive(false);
                }
                else
                {
                    ri.gameObject.SetActive(false);
                    img.gameObject.SetActive(true);
                }
            }

        }

        public void OnToggleClicked()
        {
            for (int i = 0; i < patchesContainer.childCount; i++)
            {
                Transform patchLogItem = patchesContainer.GetChild(i);
                Toggle toggle = patchLogItem.GetChild(1).GetComponent<Toggle>();

                if (scans[i] != null && toggle.isOn)
                {
                    deleteButton.interactable = true;
                    return;
                }
            }
            deleteButton.interactable = false;
        }

        public void OnDeleteSelected()
        {
            StartCoroutine(_OnDeleteSelected());
        }

        private IEnumerator _OnDeleteSelected()
        {
            deleteButton.interactable = false;

            for (int i = 0; i < patchesContainer.childCount; i++)
            {
                Transform patchLogItem = patchesContainer.GetChild(i);
                Toggle toggle = patchLogItem.GetChild(1).GetComponent<Toggle>();

                if (scans[i] != null && toggle.isOn)
                {
                    string filename = viewedTeam.artworks[i];

                    if (viewedTeam == gameState.currentTeam)
                    {
                        gameState.TrashScanFromCurrentTeam(filename, i);
                    }
                    else
                    {
                        gameState.TrashScan(filename);
                        yield return StartCoroutine(deleteThreadController.DeleteCoroutine(filename));
                        Array.Clear(scans, i, 1);
                    }

                    //Update UI
                    toggle.isOn = false;

                    undoButton.interactable = true;

                    scanHistories.Add(new ScanHistory(filename, i));

                    UpdatePatches();
                }
            }
        }

        public void OnUndo(myWebCamTextureToMatHelper webCamTextureToMatHelper)
        {
            StartCoroutine(_OnUndo(webCamTextureToMatHelper));
        }

        private IEnumerator _OnUndo(myWebCamTextureToMatHelper webCamTextureToMatHelper)
        {
            if (scanHistories.Count > 0)
            {
                undoButton.interactable = false;

                ScanHistory mostRecent = scanHistories[scanHistories.Count - 1];

                string saveDirPath = Path.Join(Application.streamingAssetsPath, settings.saveDir);
                string fullPath = Path.Join(saveDirPath, mostRecent.filename);

                if (viewedTeam == gameState.currentTeam)
                {
                    gameState.UnTrashScanFromCurrentTeam(mostRecent.filename, mostRecent.index, webCamTextureToMatHelper);
                    yield return StartCoroutine(uploadThreadController.UploadCoroutine(fullPath));
                }
                else
                {
                    gameState.UnTrashScan(mostRecent.filename);
                    yield return StartCoroutine(uploadThreadController.UploadCoroutine(fullPath));

                    Texture2D untrashedScan = ScanSaving.GetTexture2DFromImageFile(fullPath, settings, webCamTextureToMatHelper);
                    scans[mostRecent.index] = untrashedScan;
                }

                scanHistories.Remove(mostRecent);

                //Update UI
                undoButton.interactable = (scanHistories.Count > 0);

                UpdatePatches();
            }
        }
    }
}

