using UnityEngine;
using System.Collections;

namespace ArtScan.ScanSavingModule
{
    public class AbortThreads : MonoBehaviour
    {
        public UploadThreadController uploadThreadController;
        public DownloadThreadController downloadThreadController;
        public DeleteThreadController deleteThreadController;

        private void OnDestroy()
        {
            uploadThreadController.AbortThread();
            downloadThreadController.AbortThread();
            deleteThreadController.AbortThread();
        }
    }
}



