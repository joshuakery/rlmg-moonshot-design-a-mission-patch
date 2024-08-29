#if !(PLATFORM_LUMIN && !UNITY_EDITOR)

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ObjdetectModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.XimgprocModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.UnityUtils.Helper;
using Rect = OpenCVForUnity.CoreModule.Rect;
using PositionsVector = System.Collections.Generic.List<OpenCVForUnity.CoreModule.Rect>;

using ArtScan;
using ArtScan.CamConfigLoaderModule;
using rlmg.logging;
using ArtScan.EdgeFindingModule;
using ArtScan.PerspectiveUtilsModule;
using ArtScan.RemoveBackgroundUtilsModule;
using ArtScan.PresentationUtilsModule;

namespace ArtScan.CoreModule
{
    public class SynchronousRemoveBackground : MonoBehaviour
    {
        public int copiedFramesCount = 0;

        public RemoveBackgroundSettings settings;
        public bool doProcessImage = true;
        public RemoveBackgroundDisplayOptions displayOptions;

        public StructuredEdgeDetection edgeDetection;

        /// <summary>
        /// STRUCTURED_FORSTS_FILENAME
        /// </summary>
        protected static readonly string SFORESTS_MODEL_FILENAME = "structured_forests.yml.gz";

        /// <summary>
        /// The structured forests model filepath.
        /// </summary>
        string sForests_model_filepath;

#if UNITY_WEBGL
    IEnumerator getFilePath_Coroutine;
#endif

        private Mat rawImageDisplayMat;
        private Texture2D rawImageTexture;

        private Scalar DEBUG_PAPER_EDGE_COLOR;
        private Scalar PAPER_FOUND_CONSISTENTLY_EDGE_COLOR;
        private Scalar PAPER_AREA_CONSISTENT_EDGE_COLOR;
        private Scalar FEEDBACK_PAPER_EDGE_COLOR;

        [SerializeField]
        private RawImage rawImage;
        private RectTransform rawImageRT;
        private Vector2 rawImageSize;

        /// <summary>
        /// The webcam texture to mat helper.
        /// </summary>
        public myWebCamTextureToMatHelper webCamTextureToMatHelper;

        /// <summary>
        /// GameEvent that's fired after myWebcameTextureHelper initialization
        /// </summary>
        public GameEvent WebCamTextureToMatHelperInitialized;

        [SerializeField]
        private CamConfigLoader configLoader;

        public bool paperFound;

        private int paperFoundFrames = 0;
        private int paperNotFoundFrames = 0;

        public bool consistentPaperFound
        {
            get
            {
                if (settings != null && settings.enableBeginScanButtonSettings != null)
                {
                    return (
                        paperFoundFrames >= settings.enableBeginScanButtonSettings.minimumPaperFoundFrames
                    );
                }
                else
                {
                    return (
                        paperFoundFrames >= 2
                    );
                }
            }
        }

        [SerializeField]
        private double currentPaperArea = 0.0d;
        private double runningAveragePaperArea = 0.0d;
        private int paperFramesAtRunningAverageArea = 0;
        private int paperFramesInconsistentWithRunningAverageArea = 0;
        public bool consistentPaperArea
        {
            get
            {
                if (settings != null && settings.enableBeginScanButtonSettings != null)
                {
                    return (
                        paperFramesAtRunningAverageArea >= settings.enableBeginScanButtonSettings.minimumPaperConsistentFrames
                    );
                }
                else
                {
                    return (
                        paperFramesAtRunningAverageArea >= 2
                    );
                }
            }
        }

        private Vector2 currentPaperCenter;
        private Vector2 runningAveragePaperCenter;
        private int paperAtRunningAverageCenter;
        public bool consistentPaperCenter
        {
            get
            {
                return false;
            }
        }

        [SerializeField]
        private double currentContourArea = 0.0d;
        private double runningAverageContourSize = 0.0d;
        private int contoursAtRunningAverageSize = 0;
        private int contoursInconsistentWithRunningAverageSize = 0;
        public bool consistentRunningAverage
        {
            get
            {
                if (settings != null && settings.enableBeginScanButtonSettings != null)
                {
                    return (
                        contoursAtRunningAverageSize >= settings.enableBeginScanButtonSettings.minimumConsistentFrames
                    );
                }
                else
                {
                    return (
                        contoursAtRunningAverageSize >= 2
                    );
                }

            }
        }
        public bool artworkSizeWithinLimits
        {
            get
            {
                if (settings != null && settings.enableBeginScanButtonSettings != null)
                {
                    bool isSmallerThanPaper = currentContourArea < (double)settings.enableBeginScanButtonSettings.maximumArtworkPercentageOfPaper * currentPaperArea;

                    return (
                        isSmallerThanPaper &&
                        currentContourArea >= settings.enableBeginScanButtonSettings.minimumScanSize
                    );
                }
                else
                {
                    return (
                        currentContourArea < currentPaperArea &&
                        currentContourArea >= 0
                    );
                }
            }
        }

        private Mat resultMat;

        public bool doDownsizeToDisplay = false;

        private void Awake()
        {
            if (rawImage != null)
            {
                rawImageRT = rawImage.gameObject.GetComponent<RectTransform>();
                rawImageSize = new Vector2(rawImageRT.rect.width, rawImageRT.rect.height);
            }

            if (webCamTextureToMatHelper == null)
                webCamTextureToMatHelper = FindObjectOfType<myWebCamTextureToMatHelper>();

            if (configLoader == null)
                configLoader = FindObjectOfType<CamConfigLoader>();
        }

        // Use this for initialization
        public void SetupRemoveBackground()
        {
            if (webCamTextureToMatHelper == null) return;

#if UNITY_WEBGL
            // getFilePath_Coroutine = GetFilePath ();
            // StartCoroutine (getFilePath_Coroutine);
#else
            // sForests_model_filepath = Utils.getFilePath(SFORESTS_MODEL_FILENAME);
            // SetupStructuredForests();
#endif
            if (configLoader != null && configLoader.configData != null)
            {
                if (configLoader.configData.defaultCamera != null)
                    webCamTextureToMatHelper.requestedDeviceName = configLoader.configData.defaultCamera;

                webCamTextureToMatHelper.flipVertical = configLoader.configData.flipVertical;

                webCamTextureToMatHelper.flipHorizontal = configLoader.configData.flipHorizontal;

                webCamTextureToMatHelper.requestedFPS = configLoader.configData.requestedFPS;

                webCamTextureToMatHelper.requestedWidth = configLoader.configData.requestedWidth;
                webCamTextureToMatHelper.requestedHeight = configLoader.configData.requestedHeight;
            }



#if UNITY_ANDROID && !UNITY_EDITOR
            // Avoids the front camera low light issue that occurs in only some Android devices (e.g. Google Pixel, Pixel2).
            webCamTextureToMatHelper.avoidAndroidFrontCameraLowLightIssue = true;
#endif

            webCamTextureToMatHelper.Initialize();
        }

#if UNITY_WEBGL
        private IEnumerator GetFilePath()
        {
            var getFilePathAsync_0_Coroutine = Utils.getFilePathAsync(SFORESTS_MODEL_FILENAME, (result) =>
            {
                sForests_model_filepath = result;
            });
            yield return getFilePathAsync_0_Coroutine;

            getFilePath_Coroutine = null;

            // SetupStructuredForests();
        }
#endif

        /// <summary>
        /// Raises the webcam texture to mat helper initialized event.
        /// </summary>
        public void OnWebCamTextureToMatHelperInitialized()
        {
            if (RLMGLogger.Instance != null)
                RLMGLogger.Instance.Log("Calling OnWebCamTextureToMatHelperInitialized...", MESSAGETYPE.INFO);

            Mat webCamTextureMat = webCamTextureToMatHelper.GetMat();

            int rWidth = webCamTextureMat.cols();
            int rHeight = (int)(rWidth * (settings.targetHeight / settings.targetWidth));

            if (rawImageDisplayMat != null) rawImageDisplayMat.Dispose();
            rawImageDisplayMat = new Mat(rHeight, rWidth, webCamTextureMat.type(), new Scalar(0, 0, 0, 0));

            if (rawImageTexture != null) { Destroy(rawImageTexture); }
            rawImageTexture = new Texture2D(rawImageDisplayMat.cols(), rawImageDisplayMat.rows(), TextureFormat.RGBA32, false);
            rawImageTexture.name = "Raw Image Texture";
            rawImage.texture = rawImageTexture;

            if (RLMGLogger.Instance != null)
                RLMGLogger.Instance.Log("OnWebCamTextureToMatHelperInitialized. Screen.width " + Screen.width + " Screen.height " + Screen.height + " Screen.orientation " + Screen.orientation, MESSAGETYPE.INFO);

            float width = rawImageDisplayMat.width();
            float height = rawImageDisplayMat.height();

            float widthScale = (float)Screen.width / width;
            float heightScale = (float)Screen.height / height;
            if (widthScale < heightScale)
            {
                Camera.main.orthographicSize = (width * (float)Screen.height / (float)Screen.width) / 2;
            }
            else
            {
                Camera.main.orthographicSize = height / 2;
            }

            DEBUG_PAPER_EDGE_COLOR = new Scalar(0, 255, 0, 255);
            PAPER_FOUND_CONSISTENTLY_EDGE_COLOR = new Scalar(255, 0, 0, 255);
            PAPER_AREA_CONSISTENT_EDGE_COLOR = new Scalar(250, 250, 0, 255);
            FEEDBACK_PAPER_EDGE_COLOR = new Scalar(49, 238, 255, 255);

            resultMat = rawImageDisplayMat.clone();

            WebCamTextureToMatHelperInitialized.Raise();
        }

        /// <summary>
        /// Raises the webcam texture to mat helper disposed event.
        /// </summary>
        public void OnWebCamTextureToMatHelperDisposed()
        {
            if (RLMGLogger.Instance != null) //always false?
            {
                RLMGLogger.Instance.Log("OnWebCamTextureToMatHelperDisposed", MESSAGETYPE.INFO);
            }
            else
            {
                Debug.Log("OnWebCamTextureToMatHelperDisposed");
            }


            if (rawImageDisplayMat != null)
            {
                rawImageDisplayMat.Dispose();
                rawImageDisplayMat = null;
            }

            if (rawImageTexture != null)
            {
                Destroy(rawImageTexture);
                rawImageTexture = null;
            }

            if (resultMat != null)
            {
                resultMat.Dispose();
                resultMat = null;
            }
                

        }

        /// <summary>
        /// Listener for the web cam texture to mat helper error occurred event.
        /// </summary>
        /// <param name="errorCode">Error code.</param>
        public void OnWebCamTextureToMatHelperErrorOccurred(myWebCamTextureToMatHelper.ErrorCode errorCode)
        {
            if (RLMGLogger.Instance != null) //always false?
            {
                RLMGLogger.Instance.Log("Error occurred while initializing camera: " + errorCode, MESSAGETYPE.ERROR);
            }
            else
            {
                Debug.LogError("Error occurred while initializing camera: " + errorCode);
            }

        }

        /// <summary>
        /// Listenener for the web cam texture to mat helper warn occurred event.
        /// </summary>
        /// <param name="warnCode">Warn code.</param>
        public void OnWebCamTextureToMatHelperWarnOccurred(myWebCamTextureToMatHelper.WarnCode warnCode)
        {
            if (RLMGLogger.Instance != null) //always false?
            {
                RLMGLogger.Instance.Log("Warning occurred while initializing camera: " + warnCode, MESSAGETYPE.INFO);
            }
            else
            {
                Debug.LogWarning("Warning occurred while initializing camera: " + warnCode);
            }
        }

        /// <summary>
        /// Listener for the web cam texture to mat helper warn occurred event.
        /// </summary>
        public void OnWebCamTextureToMatHelperSuccessOccurred()
        {
            if (RLMGLogger.Instance != null) //always false?
            {
                RLMGLogger.Instance.Log("Successfully found correct camera.", MESSAGETYPE.INFO);
            }
            else
            {
                Debug.LogWarning("Successfully found correct camera.");
            }
        }

        private void ToTexture2D()
        {
            if (resultMat != null && rawImageTexture != null)
                Utils.fastMatToTexture2D(resultMat, rawImageTexture, true, 0, true);
        }

        // Update is called once per frame
        void Update()
        {
            bool webCamTextureReady = webCamTextureToMatHelper &&
                webCamTextureToMatHelper.IsInitialized() &&
                webCamTextureToMatHelper.IsPlaying();

            if (webCamTextureReady && webCamTextureToMatHelper.DidUpdateThisFrame())
            {
                copiedFramesCount++;
                ProcessImage();
                ToTexture2D();
                GC.Collect();
            }
        }

        /// <summary>
        /// Scales the x and y values of each point in a given contour by the scaleFactor
        /// </summary>
        /// <param name="paperMaxAreaContour"></param>
        /// <param name="scaleFactor"></param>
        private void ResizeContour(MatOfPoint paperMaxAreaContour, float scaleFactor)
        {
            Point[] points = paperMaxAreaContour.toArray();
            for (int i = 0; i < points.Length; i++)
            {
                Point p = points[i];
                p.x = (double)Mathf.Round((float)p.x * scaleFactor);
                p.y = (double)Mathf.Round((float)p.y * scaleFactor);
            }

            paperMaxAreaContour.fromArray(points);
        }

        /// <summary>
        /// MAIN FUNCTION FOR PROCESSING IMAGE TO REMOVE ITS BACKGROUND
        /// </summary>
        private void ProcessImage()
        {
            Mat rgbaMat = webCamTextureToMatHelper.GetMat();

            if (rgbaMat.empty()) return;
            if (rawImageDisplayMat == null) return;
            if (resultMat == null) return;

            if (!doProcessImage)
            {
                resultMat.setTo(new Scalar(0, 0, 0, 0));
                PresentationUtils.ScaleUpAndDisplayMat(
                    rgbaMat, resultMat,
                    settings.doSizeToFit
                );
                return;
            }

            using (Mat resizedMat = new Mat())
            {
                Imgproc.resize(rgbaMat, resizedMat, new Size(), 1.0f / settings.preProcessingSizeFactor, 1.0f / settings.preProcessingSizeFactor, Imgproc.INTER_LINEAR);

                PerspectiveUtils.BrightnessContrast(resizedMat, settings.brightness, settings.contrast);

                using (Mat yMat = new Mat())
                {
                    EdgeFinding.GetCannyEdgeMat(resizedMat, yMat);

                    // find potential paper contours.
                    List<MatOfPoint> contours = new List<MatOfPoint>();
                    PerspectiveUtils.Find4PointContours(yMat, contours);

                    // pick the contour of the largest area and rearrange the points in a consistent order.
                    MatOfPoint paperMaxAreaContour = PerspectiveUtils.GetMaxAreaContour(contours);
                    paperMaxAreaContour = PerspectiveUtils.OrderCornerPoints(paperMaxAreaContour);

                    paperFound = (paperMaxAreaContour.size().area() > 0);
                    CalculateConsistentPaperFound();

                    if (paperFound)
                    {
                        currentPaperArea = Imgproc.contourArea(paperMaxAreaContour);
                        //currentContourArea = paperMaxAreaContour.size().area()
                        bool paperThisFrameIsConsistentArea = CalculatePaperAreaRunningAverage();

                        if (consistentPaperFound)
                        {
                            if (consistentPaperArea)
                            {
                                if (displayOptions.doWarp)
                                {
                                    using (Mat transformedResizedMat = PerspectiveUtils.PerspectiveTransform(resizedMat, paperMaxAreaContour))
                                    {
                                        if (transformedResizedMat.width() > 1 && transformedResizedMat.height() > 1)
                                        {
                                            //crop in the edges to hide them from the find largest contour later
                                            using (
                                                Mat croppedMat = PerspectiveUtils.CropByPercent(
                                                    transformedResizedMat,
                                                    1f
                                                )
                                            )
                                            {
                                                if (doDownsizeToDisplay)
                                                {
                                                    float ratio = rawImageSize.x / transformedResizedMat.width();
                                                    if (ratio < 1)
                                                    {
                                                        Imgproc.resize(transformedResizedMat, transformedResizedMat, new Size(), ratio, ratio, Imgproc.INTER_LINEAR);
                                                    }
                                                }

                                                using (Mat edgeMat = new Mat())
                                                {
                                                    EdgeFinding.SetEdgeMat(transformedResizedMat, edgeMat, settings, edgeDetection);

                                                    ResizeContour(paperMaxAreaContour, settings.preProcessingSizeFactor);
                                                    using (Mat transformedRGBAMat = PerspectiveUtils.PerspectiveTransform(rgbaMat, paperMaxAreaContour))
                                                    {
                                                        if (displayOptions.showEdges)
                                                            PresentationUtils.ShowEdges(edgeMat, transformedRGBAMat);

                                                        using (Mat removedMat = RemoveBackgroundUtils.PolyfillMaskBackground(transformedRGBAMat, edgeMat, out MatOfPoint maxAreaContour, out Mat mask))
                                                        {
                                                            //currentContourArea = maxAreaContour.size().area();
                                                            currentContourArea = Imgproc.contourArea(maxAreaContour);
                                                            bool thisFrameIsConsistentWithAverageArea = CalculateContourSizeRunningAverage();

                                                            //undo warp perspective and apply mask to input
                                                            /*                                                      using (Mat reverseTransformedMask = new Mat(resizedMat.height(), resizedMat.width(), resizedMat.type(), new Scalar(0, 0, 0, 0)))
                                                                                                                  {
                                                                                                                      PerspectiveUtils.ReversePerspectiveTransform(mask, paperMaxAreaContour, reverseTransformedMask);

                                                                                                                      using (Mat outputMat = new Mat(rgbaMat4Thread.height(), rgbaMat4Thread.width(), rgbaMat4Thread.type(), new Scalar(0, 0, 0, 255)))
                                                                                                                      {
                                                                                                                          Core.copyTo(rgbaMat4Thread, outputMat, reverseTransformedMask);

                                                                                                                          Core.addWeighted(rgbaMat4Thread, 0.2f, outputMat, 0.8f, 0, outputMat);

                                                                                                                          PresentationUtils.ScaleUpAndDisplayMat(
                                                                                                                              outputMat, resultMat,
                                                                                                                              settings.doSizeToFit
                                                                                                                          );
                                                                                                                      }

                                                                                                                  }

                                                                                                                  return;*/

                                                            if (consistentRunningAverage)
                                                            {
                                                                if (!thisFrameIsConsistentWithAverageArea &&
                                                                    displayOptions.doDropInconsistentFrames)
                                                                {
                                                                    //do nothing
                                                                }
                                                                else
                                                                {
                                                                    if (displayOptions.doRemoveBackground)
                                                                    {
                                                                        PerspectiveUtils.BrightnessContrast(removedMat, settings.postProcessingSettings.brightness, settings.postProcessingSettings.contrast, true);

                                                                        ResizeContour(maxAreaContour, settings.preProcessingSizeFactor);

                                                                        PresentationUtils.MakeReadyToPresent(
                                                                            removedMat, resultMat,
                                                                            maxAreaContour,
                                                                            displayOptions, settings
                                                                        );
                                                                    }
                                                                    else
                                                                    {
                                                                        PerspectiveUtils.BrightnessContrast(transformedResizedMat, settings.postProcessingSettings.brightness, settings.postProcessingSettings.contrast, true);

                                                                        PresentationUtils.MakeReadyToPresent(
                                                                            transformedResizedMat, resultMat,
                                                                            maxAreaContour,
                                                                            displayOptions, settings
                                                                        );
                                                                    }
                                                                }


                                                            }
                                                            else
                                                            {
                                                                resultMat.setTo(new Scalar(0, 0, 0, 0));

                                                                if (displayOptions.doDrawPaperEdge)
                                                                {
                                                                    //no need to resize paperMaxAreaContour here; it is resized before this
                                                                    Imgproc.drawContours(rgbaMat, new List<MatOfPoint> { paperMaxAreaContour }, -1, FEEDBACK_PAPER_EDGE_COLOR, 10);
                                                                }

                                                                PresentationUtils.ScaleUpAndDisplayMat(
                                                                    rgbaMat, resultMat,
                                                                    settings.doSizeToFit
                                                                );


                                                            }
                                                        }
                                                    }


                                                }
                                            }
                                        }
                                    }
                                }
                                else //don't do warp - just show the camera feed, with display options
                                {
                                    resultMat.setTo(new Scalar(0, 0, 0, 0));

                                    if (displayOptions.showEdges) //yMat is now Canny edges, see above
                                        Imgproc.cvtColor(yMat, rgbaMat, Imgproc.COLOR_GRAY2RGBA);

                                    if (displayOptions.doDrawPaperEdge)
                                    {
                                        ResizeContour(paperMaxAreaContour, settings.preProcessingSizeFactor);
                                        Imgproc.drawContours(rgbaMat, new List<MatOfPoint> { paperMaxAreaContour }, -1, PAPER_AREA_CONSISTENT_EDGE_COLOR, 4);
                                    }

                                    PresentationUtils.ScaleUpAndDisplayMat(
                                        rgbaMat, resultMat,
                                        settings.doSizeToFit
                                    );
                                }
                            }
                            else //paper found consistently but no consistent paper area - just show the camera feed, with display options
                            {
                                if (!paperThisFrameIsConsistentArea && displayOptions.doDropInconsistentFrames)
                                {
                                    //do nothing
                                }
                                else
                                {
                                    resultMat.setTo(new Scalar(0, 0, 0, 0));

                                    if (displayOptions.showEdges) //yMat is now Canny edges, see above
                                        Imgproc.cvtColor(yMat, rgbaMat, Imgproc.COLOR_GRAY2RGBA);

                                    if (displayOptions.doDrawPaperEdge)
                                    {
                                        ResizeContour(paperMaxAreaContour, settings.preProcessingSizeFactor);
                                        Imgproc.drawContours(rgbaMat, new List<MatOfPoint> { paperMaxAreaContour }, -1, PAPER_FOUND_CONSISTENTLY_EDGE_COLOR, 4);
                                    }

                                    PresentationUtils.ScaleUpAndDisplayMat(
                                        rgbaMat, resultMat,
                                        settings.doSizeToFit
                                    );
                                }

                            }
                        } //paperFound but not consistently - just show the camera feed, with display options
                        else
                        {
                            resultMat.setTo(new Scalar(0, 0, 0, 0));

                            if (displayOptions.showEdges) //yMat is now Canny edges, see above
                                Imgproc.cvtColor(yMat, rgbaMat, Imgproc.COLOR_GRAY2RGBA);

                            if (displayOptions.doDrawPaperEdge)
                            {
                                ResizeContour(paperMaxAreaContour, settings.preProcessingSizeFactor);
                                Imgproc.drawContours(rgbaMat, new List<MatOfPoint> { paperMaxAreaContour }, -1, DEBUG_PAPER_EDGE_COLOR, 4);
                            }

                            PresentationUtils.ScaleUpAndDisplayMat(
                                rgbaMat, resultMat,
                                settings.doSizeToFit
                            );
                        }
                    }
                    else //not paperFound - just show the camera feed, with edges optionally
                    {
                        resultMat.setTo(new Scalar(0, 0, 0, 0));

                        if (displayOptions.showEdges) //yMat is now Canny edges, see above
                            Imgproc.cvtColor(yMat, rgbaMat, Imgproc.COLOR_GRAY2RGBA);

                        PresentationUtils.ScaleUpAndDisplayMat(
                            rgbaMat, resultMat,
                            settings.doSizeToFit
                        );
                    }
                }

            }

        }

        private void CalculateConsistentPaperFound()
        {
            if (paperFound)
            {
                paperFoundFrames++;
                paperNotFoundFrames = 0;
            }
            else
            {
                paperNotFoundFrames++;
                if (paperNotFoundFrames >= settings.enableBeginScanButtonSettings.minimumPaperNotFoundFramesToFail)
                {
                    paperFoundFrames = 0;
                }
            }
        }

        private bool CalculatePaperAreaRunningAverage()
        {
            float threshold = settings != null && settings.enableBeginScanButtonSettings != null ?
                settings.enableBeginScanButtonSettings.consistentPaperAreaThreshold :
                0.5f;

            if (Math.Abs(currentPaperArea - runningAveragePaperArea) > (double)threshold * runningAveragePaperArea)
            {
                paperFramesInconsistentWithRunningAverageArea++;

                int minimumPaperInconsistentFramesToFail = settings != null && settings.enableBeginScanButtonSettings != null ?
                    settings.enableBeginScanButtonSettings.minimumPaperInconsistentFramesToFail :
                    2;

                if (paperFramesInconsistentWithRunningAverageArea >= minimumPaperInconsistentFramesToFail)
                {
                    paperFramesAtRunningAverageArea = 1;
                    runningAveragePaperArea = currentPaperArea;
                }

                return false;
            }
            else
            {
                paperFramesInconsistentWithRunningAverageArea = 0;

                paperFramesAtRunningAverageArea += 1;
                runningAveragePaperArea = (runningAveragePaperArea + currentPaperArea) / 2.0d;

                return true;
            }
        }

        private void CalculatePaperCenterRunningAverage()
        {
            if (Vector2.Distance(currentPaperCenter, runningAveragePaperCenter) > 100)
            {
                paperAtRunningAverageCenter = 1;
                runningAveragePaperCenter = currentPaperCenter;
            }
            else
            {
                paperAtRunningAverageCenter += 1;
                runningAveragePaperCenter = Vector2.Lerp(runningAveragePaperCenter, currentPaperCenter, 0.5f);
            }
        }
        private bool CalculateContourSizeRunningAverage()
        {
            float threshold = settings != null && settings.enableBeginScanButtonSettings != null ? settings.enableBeginScanButtonSettings.consistentScanAreaThreshold : 0.5f;

            if (!artworkSizeWithinLimits ||
                Math.Abs(currentContourArea - runningAverageContourSize) > (double)threshold * runningAverageContourSize)
            {
                contoursInconsistentWithRunningAverageSize++;

                int minimumArtworkInconsistentFramesToFail = settings != null && settings.enableBeginScanButtonSettings != null ?
                    settings.enableBeginScanButtonSettings.minimumArtworkInconsistentFramesToFail :
                    2;

                if (contoursInconsistentWithRunningAverageSize >= minimumArtworkInconsistentFramesToFail)
                {
                    contoursAtRunningAverageSize = 1;
                    runningAverageContourSize = currentContourArea;
                }

                return false;
            }
            else
            {
                contoursInconsistentWithRunningAverageSize = 0;

                contoursAtRunningAverageSize += 1;
                runningAverageContourSize = (runningAverageContourSize + currentContourArea) / 2.0d;

                return true;
            }
        }

        public void ChangeDoProcessImageValue(bool value)
        {
            doProcessImage = value;
        }

        /// <summary>
        /// Raises the destroy event.
        /// </summary>
        void OnDestroy()
        {
            if (webCamTextureToMatHelper != null)
                webCamTextureToMatHelper.Dispose();

#if UNITY_WEBGL
            if (getFilePath_Coroutine != null) {
                StopCoroutine (getFilePath_Coroutine);
                ((IDisposable)getFilePath_Coroutine).Dispose ();
            }
#endif

            if (rawImageDisplayMat != null)
            {
                rawImageDisplayMat.Dispose();
                rawImageDisplayMat = null;
            }

            if (rawImageTexture != null)
            {
                Destroy(rawImageTexture);
                rawImageTexture = null;
            }

            if (resultMat != null)
            {
                resultMat.Dispose();
                resultMat = null;
            }

        }
    }

}


#endif