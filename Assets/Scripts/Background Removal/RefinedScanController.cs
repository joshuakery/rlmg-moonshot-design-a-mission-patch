using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ObjdetectModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.XimgprocModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.UnityUtils.Helper;
using ArtScan.CoreModule;
using ArtScan.CamConfigLoaderModule;
using ArtScan;
using ArtScan.ErrorDisplayModule;
using rlmg.logging;
using TMPro;

namespace ArtScan.CoreModule
{
    public class RefinedScanController : MonoBehaviour
    {
        [SerializeField]
        private GameState gameState;

        private AsynchronousRemoveBackground asynchronousRemoveBackground;

        private myWebCamTextureToMatHelper webCamTextureToMatHelper;

        [SerializeField]
        private CamConfigLoader configLoader;

        public RefinedScanThread refinedScanThread;

        public bool anotherScanIsUnderway
        {
            get
            {
                return (refinedScanThread != null && !refinedScanThread.IsDone);
            }
        }

        [SerializeField]
        private RemoveBackgroundSettings settings;
        [SerializeField]
        private RemoveBackgroundDisplayOptions displayOptions;
        [SerializeField]
        private ErrorDisplaySettingsSO errorDisplaySettingsSO;

        [SerializeField]
        private GameEvent ScanFailed;
        [SerializeField]
        private GameEvent NewPreview;
        [SerializeField]
        private GameEvent ScanAgain;

        public Mat previewMat;

        [SerializeField]
        private TMP_Text scanFailedDisplay;

        public bool refinedScanningIsUnderway
        {
            get
            {
                return (anotherScanIsUnderway);
            }
        }

        private void Awake()
        {
            if (asynchronousRemoveBackground == null)
                asynchronousRemoveBackground = FindObjectOfType<AsynchronousRemoveBackground>();
        }

        private void Start()
        {
            previewMat = new Mat();
        }

        private void OnDestroy()
        {
            StopAllCoroutines();

            if (refinedScanThread != null && !refinedScanThread.IsDone)
            {
                RLMGLogger.Instance.Log("Ending parallel refined scan thread...", MESSAGETYPE.INFO);
                refinedScanThread.Abort();
                RLMGLogger.Instance.Log("...ended", MESSAGETYPE.INFO);
            }

            if (previewMat != null)
            {
                previewMat.Dispose();
            }
        }

        public void SetUpRemoveBackground()
        {
            webCamTextureToMatHelper = gameObject.GetComponent<myWebCamTextureToMatHelper>();

            if (configLoader != null && configLoader.configData != null)
            {
                if (configLoader.configData.defaultCamera != null)
                    webCamTextureToMatHelper.requestedDeviceName = configLoader.configData.defaultCamera;

                webCamTextureToMatHelper.flipVertical = configLoader.configData.flipVertical;

                webCamTextureToMatHelper.flipHorizontal = configLoader.configData.flipHorizontal;

                webCamTextureToMatHelper.requestedFPS = configLoader.configData.requestedFPS;
            }

            webCamTextureToMatHelper.Initialize();
        }

        /// <summary>
        /// Checks if we can safely begin a scan, then calls the coroutine
        /// </summary>
        public void OnBeginScan()
        {
            Debug.Log("Beginning scan");

            StopAllCoroutines();
            AbortRefinedScan();

            if (anotherScanIsUnderway) //should always be false after AbortRefinedScan()
            {
                if (scanFailedDisplay != null) { scanFailedDisplay.text = "Oops!\nAnother scan was already underway.\nPlease try again."; }
                RLMGLogger.Instance.Log("Cannot do refined scan because another refined scan is underway.", MESSAGETYPE.ERROR);
                StartCoroutine(RaiseScanFailed());
                return;
                //todo something to stop the audio loop from playing, or keep it from playing before passing this check
            }

            if (webCamTextureToMatHelper == null || !webCamTextureToMatHelper.IsInitialized())
            {
                if (scanFailedDisplay != null) { scanFailedDisplay.text = "Oops!\nThe camera was not initialized.\nPlease try again."; }
                RLMGLogger.Instance.Log("Cannot do refined scan because the camera was not initialized.", MESSAGETYPE.ERROR);
                StartCoroutine(RaiseScanFailed());
                return;
            }

            Mat rgbaMat = webCamTextureToMatHelper.GetMat();

            StartCoroutine(DoRefinedScan(rgbaMat));
        }

        private IEnumerator DoRefinedScan(Mat targetMat)
        {
            using (Mat displayMat = new Mat(settings.targetHeight, settings.targetWidth, targetMat.type(), new Scalar(0, 0, 0, 0)),
                       unscaledMat = new Mat()
            )
            {
                DateTime before = DateTime.Now;

                Texture2D scanTexture = new Texture2D(displayMat.cols(), displayMat.rows(), TextureFormat.RGBA32, false);

                MatOfPoint maxAreaContourDest = new MatOfPoint();

                //Threading
                refinedScanThread = new RefinedScanThread();
                //parameters
                refinedScanThread.settings = settings;
                refinedScanThread.displayOptions = displayOptions;
                refinedScanThread.edgeDetection = asynchronousRemoveBackground != null ?
                    asynchronousRemoveBackground.edgeDetection : null
                    ;
                //input
                refinedScanThread.rgbaMat = targetMat;
                //outputs
                refinedScanThread.unscaledMat = unscaledMat;
                refinedScanThread.displayMat = displayMat;

                refinedScanThread.Start();

                yield return refinedScanThread.WaitFor();

                DateTime after = DateTime.Now;

                TimeSpan duration = after.Subtract(before);

                RLMGLogger.Instance.Log(
                    String.Format("Refined scan thread successfully created and executed in {0} milliseconds.", duration.TotalMilliseconds),
                    MESSAGETYPE.INFO
                );

                DateTime before2 = DateTime.Now;

                Utils.fastMatToTexture2D(displayMat, scanTexture, true, 0, true);

                gameState.preview = scanTexture;

                unscaledMat.copyTo(previewMat);
                NewPreview.Raise();

                DateTime after2 = DateTime.Now;

                TimeSpan duration2 = after2.Subtract(before2);

                RLMGLogger.Instance.Log(
                    String.Format("Refined scan successfully copied to preview mats in {0} milliseconds.", duration2.TotalMilliseconds),
                    MESSAGETYPE.INFO
                );
            }
        }

        /// <summary>
        /// Yields one frame so that the audio loop is properly turned off.
        /// </summary>
        public IEnumerator RaiseScanFailed()
        {
            yield return null;
            ScanFailed.Raise();
        }

        /// <summary>
        /// Aborts refined scan thread and resets asynchronous feed variables
        /// </summary>
        public void AbortRefinedScan()
        {
            StopAllCoroutines();
            if (refinedScanThread != null && !refinedScanThread.IsDone)
            {
                refinedScanThread.Abort(); //thread is disposed after abort or use
                refinedScanThread = null; //clear reference so we know to create new threaded job
            }
        }
    }
}


