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

        public GameEvent ScanFailed;
        public GameEvent NewPreview;

        public Mat previewMat;

        public TMP_Text scanFailedDisplay;

        private void Start()
        {
            previewMat = new Mat();
        }

        private void OnDestroy()
        {
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

                // if (IsAllTransparent(displayMat))
                // {
                //     ScanFailed.Raise();
                // }
                // else
                // {
                //     unscaledMat.copyTo(previewMat);
                //     NewPreview.Raise();
                // }

            }
        }

        private IEnumerator SyncingTimeout()
        {
            DateTime before = DateTime.Now;

            yield return new WaitForSeconds(5f);

            if (asynchronousRemoveBackground.shouldCopyToRefinedMat)
            {
                asynchronousRemoveBackground.shouldCopyToRefinedMat = false;

                AbortRefinedScan();
                StopAllCoroutines();

                DateTime after = DateTime.Now;

                TimeSpan duration = after.Subtract(before);

                RLMGLogger.Instance.Log(
                    String.Format("Syncing timeout aborted scan after {0} milliseconds.", duration.TotalMilliseconds),
                    MESSAGETYPE.INFO
                );

                if (scanFailedDisplay != null) { scanFailedDisplay.text = "Syncing timed out."; }
                RLMGLogger.Instance.Log("Getting a frame from live preview timed out.", MESSAGETYPE.ERROR);
                StartCoroutine(RaiseScanFailed());
            }

        }

        private IEnumerator DoRefinedScanAfterSyncing()
        {
            asynchronousRemoveBackground.shouldCopyToRefinedMat = true;

            StartCoroutine(SyncingTimeout());

            DateTime before = DateTime.Now;

            yield return StartCoroutine(WaitForRefinedMatReady());

            DateTime after = DateTime.Now;

            TimeSpan duration = after.Subtract(before);

            RLMGLogger.Instance.Log(
                String.Format("Refined mat ready in {0} milliseconds.", duration.TotalMilliseconds),
                MESSAGETYPE.INFO
            );

            asynchronousRemoveBackground.refinedMatReady = false;
            StartCoroutine(DoRefinedScan());
        }

        private IEnumerator WaitForRefinedMatReady()
        {
            while (!asynchronousRemoveBackground.refinedMatReady)
                yield return null;
        }

        private bool IsAllTransparent(Mat src)
        {
            if (src.channels() == 4)
            {
                List<Mat> planes = new List<Mat>();
                Core.split(src, planes);
                Mat alpha = planes[3];

                int rows = alpha.rows();
                int cols = alpha.cols();
                for (int i0 = 0; i0 < rows; i0++)
                {
                    for (int i1 = 0; i1 < cols; i1++)
                    {
                        byte[] p = new byte[1];
                        alpha.get(i0, i1, p);
                        if (p[0] > 0) return false;
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public void OnBeginScan()
        {
            //Check 1 - is the webcam on?
            if (!asynchronousRemoveBackground.webCamTextureToMatHelper ||
                !asynchronousRemoveBackground.webCamTextureToMatHelper.IsPlaying()
                )
            {
                if (scanFailedDisplay != null) { scanFailedDisplay.text = "Webcam is not playing."; }
                RLMGLogger.Instance.Log("Cannot do refined scan because webcam is not playing.", MESSAGETYPE.ERROR);
                StartCoroutine(RaiseScanFailed());
                return;
            }

            //Check 2 - have we found the bounds of the paper?
            if (!asynchronousRemoveBackground.paperFound)
            {
                if (scanFailedDisplay != null) { scanFailedDisplay.text = "No paper found."; }
                RLMGLogger.Instance.Log("Cannot do refined scan because the paper cannot be found.", MESSAGETYPE.ERROR);
                StartCoroutine(RaiseScanFailed());
                return;
            }

            //Check 3 - do we have a consistent running average?
            if (!asynchronousRemoveBackground.consistentRunningAverage)
            {
                if (scanFailedDisplay != null) { scanFailedDisplay.text = "No artwork detected."; }
                RLMGLogger.Instance.Log("Cannot do refined scan because we do not have a consistent running average.", MESSAGETYPE.ERROR);
                StartCoroutine(RaiseScanFailed());
                return;
            }

            StopAllCoroutines();
            AbortRefinedScan();

            //Check 4
            if (anotherScanIsUnderway) //should always be false after AbortRefinedScan()
            {
                if (scanFailedDisplay != null) { scanFailedDisplay.text = "Another scan is underway."; }
                RLMGLogger.Instance.Log("Cannot do refined scan because another refined scan is underway.", MESSAGETYPE.ERROR);
                StartCoroutine(RaiseScanFailed());
                return;
                //todo something to stop the audio loop from playing, or keep it from playing before passing this check
            }

            StartCoroutine(DoRefinedScanAfterSyncing());
        }

        /// <summary>
        /// Yields one frame so that the audio loop is properly turned off.
        /// </summary>
        public IEnumerator RaiseScanFailed()
        {
            yield return null;
            ScanFailed.Raise();
        }

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
        }
    }
}


