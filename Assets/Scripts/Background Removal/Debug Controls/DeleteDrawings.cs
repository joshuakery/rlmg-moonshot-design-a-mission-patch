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

        private bool useServer = false;
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

        [SerializeField]
        private SavedScanManager savedScanManager;

        private Texture2D[] scansOnView
        {
            get
            {
                if (viewedTeamIndex == gameState.currentTeamIndex)
                {
                    return gameState.savedScanManager.scans;
                }
                else
                {
                    return savedScanManager.scans;
                }
            }
        }

        [SerializeField]
        private RemoveBackgroundSettings settings;

        private myWebCamTextureToMatHelper webCamTextureToMatHelper;

        private void Awake()
        {
            webCamTextureToMatHelper = FindObjectOfType<myWebCamTextureToMatHelper>();
            savedScanManager.ClearScans();
        }

        private void Start()
        {
            deleteButton.interactable = false;
            undoButton.interactable = false;

            scanHistories = new List<ScanHistory>();

            viewedTeamIndex = gameState.currentTeamIndex;
            ViewTeam();

            useServer = FindObjectOfType<Client>() != null;
        }

        private void OnEnable()
        {
            UpdatePatches();
        }

        public void ViewTeam()
        {
            StartCoroutine(_ViewTeam());
        }

        /// <summary>
        /// Downloads the requested team's images except if its the current team
        /// </summary>
        /// <returns></returns>
        private IEnumerator _ViewTeam()
        {
            if (viewedTeamIndex != gameState.currentTeamIndex && useServer)
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

                savedScanManager.ReadScans(viewedTeam.artworks, webCamTextureToMatHelper);
            }

            //update special patch log
            UpdatePatches();

            //disable delete and undo
            deleteButton.interactable = false;
            undoButton.interactable = false;

            //clear scanhistories
            if (scanHistories != null) { scanHistories.Clear(); }
        }

        public void UpdatePatches()
        {
            for (int i = 0; i < patchesContainer.childCount; i++)
            {
                Transform patchLogItem = patchesContainer.GetChild(i);
                Image img = patchLogItem.GetChild(0).GetComponent<Image>();
                RawImage ri = patchLogItem.GetChild(1).GetComponent<RawImage>();

                if (i < scansOnView.Length && scansOnView[i] != null)
                {
                    Texture2D scan = scansOnView[i];
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

                if (scansOnView[i] != null && toggle.isOn)
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

                if (scansOnView[i] != null && toggle.isOn)
                {
                    string filename = viewedTeam.artworks[i];

                    if (viewedTeam == gameState.currentTeam)
                    {
                        gameState.savedScanManager.TrashScanAndRemoveFromScans(filename, i);
                    }
                    else
                    {
                        gameState.savedScanManager.TrashScan(filename);
                        if (useServer)
                        {
                            yield return StartCoroutine(deleteThreadController.DeleteCoroutine(filename));
                        }
                        Array.Clear(scansOnView, i, 1);
                    }

                    //Update UI
                    toggle.isOn = false;

                    undoButton.interactable = true;

                    scanHistories.Add(new ScanHistory(filename, i));

                    UpdatePatches();
                }
            }
        }

        public void OnUndo()
        {
            StartCoroutine(_OnUndo());
        }

        private IEnumerator _OnUndo()
        {
            if (scanHistories.Count > 0)
            {
                undoButton.interactable = false;

                ScanHistory mostRecent = scanHistories[scanHistories.Count - 1];

                if (viewedTeam == gameState.currentTeam)
                {
                    gameState.savedScanManager.UnTrashScanAndReadToScans(mostRecent.filename, mostRecent.index, webCamTextureToMatHelper);
                }
                else
                {
                    savedScanManager.UnTrashScanAndReadToScans(mostRecent.filename, mostRecent.index, webCamTextureToMatHelper);
                }


                if (useServer)
                {
                    string saveDirPath = Path.Join(Application.streamingAssetsPath, settings.saveDir);
                    string fullPath = Path.Join(saveDirPath, mostRecent.filename);
                    yield return StartCoroutine(uploadThreadController.UploadCoroutine(fullPath));
                }


                scanHistories.Remove(mostRecent);

                //Update UI
                undoButton.interactable = (scanHistories.Count > 0);

                UpdatePatches();
            }
        }
    }
}

