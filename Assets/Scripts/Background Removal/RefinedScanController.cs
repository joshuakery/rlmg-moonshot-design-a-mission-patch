using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ObjdetectModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.XimgprocModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.UnityUtils.Helper;
using ArtScan.CoreModule;
using ArtScan;
using ArtScan.ErrorDisplayModule;
using rlmg.logging;
using TMPro;

namespace ArtScan.CoreModule
{
    public class RefinedScanController : MonoBehaviour
    {
        public GameState gameState;

        public AsynchronousRemoveBackground asynchronousRemoveBackground;

        public RefinedScanThread refinedScanThread;

        public bool anotherScanIsUnderway
        {
            get
            {
                return (refinedScanThread != null && !refinedScanThread.IsDone);
            }
        }

        public RemoveBackgroundSettings settings;
        public RemoveBackgroundDisplayOptions displayOptions;
        public ErrorDisplaySettingsSO errorDisplaySettingsSO;

        public GameEvent ScanFailed;
        public GameEvent NewPreview;
        public GameEvent ScanAgain;

        public Mat previewMat;

        public TMP_Text scanFailedDisplay;

        private bool isInRefinedScanProcess = false;
        private bool isWaitingForUpdate = false;
        private bool hasUpdated = false;
        private bool isWaitingToSync = false;

        public bool refinedScanningIsUnderway
        {
            get
            {
                return (isInRefinedScanProcess || isWaitingForUpdate || isWaitingToSync || anotherScanIsUnderway);
            }
        }

        private void Start()
        {
            previewMat = new Mat();
        }

        private void OnEnable()
        {
            asynchronousRemoveBackground.webCamTextureToMatHelper.onInitialized.AddListener(OnInitialized);
        }

        private void OnDisable()
        {
            asynchronousRemoveBackground.webCamTextureToMatHelper.onInitialized.RemoveListener(OnInitialized);
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

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                asynchronousRemoveBackground.shouldCopyToRefinedMat = true;
            }
        }

        private void OnInitialized()
        {
            if (isWaitingForUpdate || isWaitingToSync || anotherScanIsUnderway)
            {
                StopAllCoroutines();
                AbortRefinedScan();

                if (scanFailedDisplay != null) { scanFailedDisplay.text = "Oops!\nThe camera restarted during the scan.\nPlease try again."; }
                RLMGLogger.Instance.Log("Camera was initialized during refined scan.", MESSAGETYPE.ERROR);
                StartCoroutine(RaiseScanFailed());

                isWaitingForUpdate = false;
                isWaitingToSync = false;
                isInRefinedScanProcess = false;
            }
        }

        /// <summary>
        /// Checks if we can safely begin a scan, then calls the coroutine
        /// </summary>
        public void OnBeginScan()
        {
            //Check 1 - is the webcam on?
            if (!asynchronousRemoveBackground.webCamTextureToMatHelper ||
                !asynchronousRemoveBackground.webCamTextureToMatHelper.IsPlaying()
                )
            {
                if (scanFailedDisplay != null) { scanFailedDisplay.text = "Oh no!\nThe webcam is not playing.\nPlease try again."; }
                RLMGLogger.Instance.Log("Cannot do refined scan because webcam is not playing.", MESSAGETYPE.ERROR);
                StartCoroutine(RaiseScanFailed());
                return;
            }

            //Check 2 - have we found the bounds of the paper?
            if (!asynchronousRemoveBackground.paperFound)
            {
                if (scanFailedDisplay != null) { scanFailedDisplay.text = "Oh no!\nNo paper can be found.\nPlease try again."; }
                RLMGLogger.Instance.Log("Cannot do refined scan because the paper cannot be found.", MESSAGETYPE.ERROR);
                StartCoroutine(RaiseScanFailed());
                return;
            }

            //Check 3 - do we have a consistent running average?
            if (!asynchronousRemoveBackground.consistentRunningAverage)
            {
                if (scanFailedDisplay != null) { scanFailedDisplay.text = "Oh no!\nNo artwork was detected.\nPlease try again."; }
                RLMGLogger.Instance.Log("Cannot do refined scan because we do not have a consistent running average.", MESSAGETYPE.ERROR);
                StartCoroutine(RaiseScanFailed());
                return;
            }

            StopAllCoroutines();
            AbortRefinedScan();

            //Check 4
            if (anotherScanIsUnderway) //should always be false after AbortRefinedScan()
            {
                if (scanFailedDisplay != null) { scanFailedDisplay.text = "Oops!\nAnother scan was already underway.\nPlease try again."; }
                RLMGLogger.Instance.Log("Cannot do refined scan because another refined scan is underway.", MESSAGETYPE.ERROR);
                StartCoroutine(RaiseScanFailed());
                return;
                //todo something to stop the audio loop from playing, or keep it from playing before passing this check
            }

            StartCoroutine(DoRefinedScanAfterSyncing());
        }


        /// <summary>
        /// Does one additional check to see if the webcam is on
        /// Then waits for a copy of the webcam Mat to be created before refined process
        /// </summary>
        private IEnumerator DoRefinedScanAfterSyncing()
        {
            isInRefinedScanProcess = true;

            //yield return StartCoroutine(WaitForUpdate());

            asynchronousRemoveBackground.shouldCopyToRefinedMat = true;

            DateTime before = DateTime.Now;

            yield return StartCoroutine(WaitForRefinedMatReady());

            DateTime after = DateTime.Now;

            TimeSpan duration = after.Subtract(before);

            RLMGLogger.Instance.Log(
                String.Format("Refined mat ready in {0} milliseconds.", duration.TotalMilliseconds),
                MESSAGETYPE.INFO
            );

            asynchronousRemoveBackground.refinedMatReady = false;

            yield return new WaitForSeconds(10);
            
            yield return StartCoroutine(DoRefinedScan());

            isInRefinedScanProcess = false;
        }

        /// <summary>
        /// Timeout coroutine that waits x seconds before checking if we have had a frame update
        /// </summary>
        /// <returns></returns>
        //private IEnumerator WaitForUpdateTimeout()
        //{
        //    DateTime before = DateTime.Now;

        //    yield return new WaitForSeconds(errorDisplaySettingsSO.errorDisplaySettings.refinedScanTimeout);

        //    if (!hasUpdated)
        //    {
        //        AbortRefinedScan();
        //        StopAllCoroutines();

        //        DateTime after = DateTime.Now;

        //        TimeSpan duration = after.Subtract(before);

        //        RLMGLogger.Instance.Log(
        //            String.Format("Wait for update timeout aborted scan after {0} milliseconds.", duration.TotalMilliseconds),
        //            MESSAGETYPE.INFO
        //        );

        //        if (scanFailedDisplay != null) { scanFailedDisplay.text = "Webcam update timed out."; }
        //        RLMGLogger.Instance.Log("Waiting for an updated frame from webcam timed out.", MESSAGETYPE.ERROR);
        //        StartCoroutine(RaiseScanFailed());
        //    }
        //}


        /// <summary>
        /// Wait for didUpdateThisFrame from webCamTexture, as a check that the webcam is working
        /// </summary>
        /// <returns></returns>
        //private IEnumerator WaitForUpdate()
        //{
        //    isWaitingForUpdate = true;
        //    hasUpdated = false;

        //    StartCoroutine(WaitForUpdateTimeout());

        //    while (!hasUpdated)
        //    {
        //        if (asynchronousRemoveBackground.webCamTextureToMatHelper.GetWebCamTexture() == null ||
        //            !asynchronousRemoveBackground.webCamTextureToMatHelper.IsPlaying())
        //        {
        //            if (scanFailedDisplay != null) { scanFailedDisplay.text = "Webcam update timed out."; }
        //            RLMGLogger.Instance.Log("Waiting for an updated frame from webcam timed out because webcam texture is unavailable.", MESSAGETYPE.ERROR);
        //            StartCoroutine(RaiseScanFailed());
        //            break;
        //        }

        //        if (asynchronousRemoveBackground.webCamTextureToMatHelper.GetWebCamTexture() != null &&
        //            asynchronousRemoveBackground.webCamTextureToMatHelper.IsPlaying() &&
        //            asynchronousRemoveBackground.webCamTextureToMatHelper.GetWebCamTexture().didUpdateThisFrame)
        //            hasUpdated = true;

        //        yield return null;
        //    }

        //    isWaitingForUpdate = false;
        //}

        private IEnumerator WaitForRefinedMatReady()
        {
            isWaitingToSync = true;

            DateTime timeout = DateTime.Now + TimeSpan.FromSeconds(errorDisplaySettingsSO.errorDisplaySettings.refinedScanTimeout);
            DateTime before = DateTime.Now;

            while (!asynchronousRemoveBackground.refinedMatReady)
            {
                if (DateTime.Now > timeout)
                {
                    asynchronousRemoveBackground.shouldCopyToRefinedMat = false;
                    asynchronousRemoveBackground.refinedMatReady = false;

                    AbortRefinedScan();
                    StopAllCoroutines();

                    DateTime after = DateTime.Now;

                    TimeSpan duration = after.Subtract(before);

                    RLMGLogger.Instance.Log(
                        String.Format("Syncing timeout aborted scan after {0} milliseconds.", duration.TotalMilliseconds),
                        MESSAGETYPE.INFO
                    );

                    if (scanFailedDisplay != null) { scanFailedDisplay.text = "Oops!\nThe scanner was taking too long, so we stopped it.\nPlease try again."; }
                    RLMGLogger.Instance.Log("Getting a frame from live preview timed out.", MESSAGETYPE.ERROR);
                    StartCoroutine(RaiseScanFailed());

                    timeout = DateTime.Now + TimeSpan.FromSeconds(errorDisplaySettingsSO.errorDisplaySettings.refinedScanTimeout);

                    isInRefinedScanProcess = false;

                    break;
                }
                else
                {
                    yield return null;
                }
            }

            isWaitingToSync = false;
        }

        /// <summary>
        /// Timeout coroutine that waits x seconds before checking if we are still waiting to copy to refined Mat
        /// </summary>
        /// <returns></returns>
        //private IEnumerator WaitForRefinedMatReadyTimeout()
        //{
        //    DateTime before = DateTime.Now;

        //    yield return new WaitForSeconds(errorDisplaySettingsSO.errorDisplaySettings.refinedScanTimeout);

        //    if (isWaitingToSync)
        //    {
        //        asynchronousRemoveBackground.shouldCopyToRefinedMat = false;
        //        asynchronousRemoveBackground.refinedMatReady = false;

        //        AbortRefinedScan();
        //        StopAllCoroutines();

        //        DateTime after = DateTime.Now;

        //        TimeSpan duration = after.Subtract(before);

        //        RLMGLogger.Instance.Log(
        //            String.Format("Syncing timeout aborted scan after {0} milliseconds.", duration.TotalMilliseconds),
        //            MESSAGETYPE.INFO
        //        );

        //        if (scanFailedDisplay != null) { scanFailedDisplay.text = "Syncing timed out."; }
        //        RLMGLogger.Instance.Log("Getting a frame from live preview timed out.", MESSAGETYPE.ERROR);
        //        StartCoroutine(RaiseScanFailed());

        //        isWaitingToSync = false;
        //    }

        //}

        private IEnumerator DoRefinedScan()
        {
            // asynchronousRemoveBackground.webCamTextureToMatHelper.Pause();

            using (Mat displayMat = new Mat(settings.targetHeight, settings.targetWidth, asynchronousRemoveBackground.rgbaMat4RefinedThread.type(), new Scalar(0, 0, 0, 0)),
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
                refinedScanThread.edgeDetection = asynchronousRemoveBackground.edgeDetection;
                //input
                refinedScanThread.rgbaMat = asynchronousRemoveBackground.rgbaMat4RefinedThread;
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

                isInRefinedScanProcess = false;

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
            asynchronousRemoveBackground.shouldCopyToRefinedMat = false;
            asynchronousRemoveBackground.refinedMatReady = false;
            isInRefinedScanProcess = false;
        }
    }
}


