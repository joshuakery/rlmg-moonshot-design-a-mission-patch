using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OpenCVForUnity.UnityUtils.Helper;

namespace ArtScan.CoreModule
{
    public class beginScanEnabler : MonoBehaviour
    {
        public myWebCamTextureToMatHelper myWebCamTextureToMatHelper;
        public AsynchronousRemoveBackground asynchronousRemoveBackground;
        public RefinedScanController refinedScanController;

        public Button button;

        private void Start()
        {
            if (button == null)
                button = GetComponent<Button>();
        }

        private void Update()
        {
            if (button != null &&
                myWebCamTextureToMatHelper != null &&
                asynchronousRemoveBackground != null)
            {
                button.interactable = (
                    myWebCamTextureToMatHelper.IsPlaying() &&
                    asynchronousRemoveBackground.paperFound &&
                    asynchronousRemoveBackground.consistentPaperFound &&
                    asynchronousRemoveBackground.consistentPaperArea &&
                    asynchronousRemoveBackground.consistentRunningAverage &&
                    !refinedScanController.anotherScanIsUnderway
                );
            }
        }
    }
}


