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

namespace ArtScan.CoreModule
{
    public class RefinedScanController : MonoBehaviour
    {
        public GameState gameState;

        public AsynchronousRemoveBackground asynchronousRemoveBackground;

        private RefinedScanThread refinedScanThread;

        public RemoveBackgroundSettings settings;
        public RemoveBackgroundDisplayOptions displayOptions;

        public GameEvent ScanFailed;
        public GameEvent NewPreview;

        public Mat previewMat;

        private void Start()
        {
            previewMat = new Mat();
        }

        private void OnDestroy()
        {
            if (refinedScanThread != null && !refinedScanThread.IsDone)
            {
                RLMGLogger.Instance.Log("Ending parallel thread...", MESSAGETYPE.INFO);
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

                Utils.fastMatToTexture2D(displayMat, scanTexture, true, 0, true);

                gameState.preview = scanTexture;

                unscaledMat.copyTo(previewMat);
                NewPreview.Raise();

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

            // asynchronousRemoveBackground.webCamTextureToMatHelper.Play();  


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
            StopAllCoroutines();
            if (refinedScanThread == null || refinedScanThread.IsDone)
            {
                StartCoroutine(DoRefinedScan());
            }
        }
    }
}


