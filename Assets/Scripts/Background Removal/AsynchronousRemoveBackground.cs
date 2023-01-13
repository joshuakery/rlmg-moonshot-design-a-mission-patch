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
    /// <summary>
    /// Asynchronous Remove Background
    /// Based on Asynchronous Face Detection WebCamTexture Example
    /// Referring to https://github.com/Itseez/opencv/blob/master/modules/objdetect/src/detection_based_tracker.cpp.
    /// </summary>
    [RequireComponent(typeof(myWebCamTextureToMatHelper))]
    public class AsynchronousRemoveBackground : MonoBehaviour
    {

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

        public Mat rawImageDisplayMat;
        public Texture2D rawImageTexture;

        public Scalar DEBUG_PAPER_EDGE_COLOR;
        public Scalar FEEDBACK_PAPER_EDGE_COLOR;

        public RawImage rawImage;
        public RectTransform rawImageRT;
        public Vector2 rawImageSize;

        /// <summary>
        /// The webcam texture to mat helper.
        /// </summary>
        public myWebCamTextureToMatHelper webCamTextureToMatHelper;

        /// <summary>
        /// The FPS monitor.
        /// </summary>
        //FpsMonitor fpsMonitor;

        /// <summary>
        /// The Game State Scriptable Object.
        /// </summary>
        public GameState gameState;

        /// <summary>
        /// GameEvent that's fired after myWebcameTextureHelper initialization
        /// </summary>
        public GameEvent WebCamTextureToMatHelperInitialized;

        public GameEvent NewPreview;
        public GameEvent ScanFailed;

        public CamConfigLoader configLoader;

        public bool paperFound;

        private int paperFoundFrames = 0;
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

        private double currentPaperArea = 0.0d;
        private double runningAveragePaperArea = 0.0d;
        private int paperAtRunningAverageArea = 0;

        public bool consistentPaperArea
        {
            get
            {
                if (settings != null && settings.enableBeginScanButtonSettings != null)
                {
                    return (
                        paperAtRunningAverageArea >= settings.enableBeginScanButtonSettings.minimumPaperConsistentFrames
                    );
                }
                else
                {
                    return (
                        paperAtRunningAverageArea >= 2
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

        private double currentContourArea = 0.0d;
        private double runningAverageContourSize = 0.0d;
        private int contoursAtRunningAverageSize = 0;

        public bool consistentRunningAverage
        {
            get
            {
                if (settings != null && settings.enableBeginScanButtonSettings != null)
                {
                    return (
                        
                        contoursAtRunningAverageSize >= settings.enableBeginScanButtonSettings.minimumConsistentFrames &&
                        currentContourArea >= settings.enableBeginScanButtonSettings.minimumScanSize
                    );
                }
                else
                {
                    return (
                        contoursAtRunningAverageSize >= 2 &&
                        currentContourArea >= 0
                    );
                }

            }
        }

        // for Thread
        System.Object sync = new System.Object();

        //for new Thread
        Mat rgbaMat4Thread;
        public Mat rgbaMat4RefinedThread;
        Mat resultMat;

        public float DOWNSCALE_RATIO = 2f;
        public bool doDownsizeToDisplay = false;

        bool _isThreadRunning = false;

        bool isThreadRunning
        {
            get
            {
                lock (sync)
                    return _isThreadRunning;
            }
            set
            {
                lock (sync)
                    _isThreadRunning = value;
            }
        }

        bool _shouldStopThread = false;

        bool shouldStopThread
        {
            get
            {
                lock (sync)
                    return _shouldStopThread;
            }
            set
            {
                lock (sync)
                    _shouldStopThread = value;
            }
        }

        bool _shouldDetectInMultiThread = false;

        bool shouldDetectInMultiThread
        {
            get
            {
                lock (sync)
                    return _shouldDetectInMultiThread;
            }
            set
            {
                lock (sync)
                    _shouldDetectInMultiThread = value;
            }
        }

        bool _didUpdateTheDetectionResult = false;

        bool didUpdateTheDetectionResult
        {
            get
            {
                lock (sync)
                    return _didUpdateTheDetectionResult;
            }
            set
            {
                lock (sync)
                    _didUpdateTheDetectionResult = value;
            }
        }

        bool _shouldCopyToRefinedMat = false;

        public bool shouldCopyToRefinedMat
        {
            get
            {
                lock (sync)
                    return _shouldCopyToRefinedMat;
            }
            set
            {
                lock (sync)
                    _shouldCopyToRefinedMat = value;
            }
        }

        bool _refinedMatReady = false;

        public bool refinedMatReady
        {
            get
            {
                lock (sync)
                    return _refinedMatReady;
            }
            set
            {
                lock (sync)
                    _refinedMatReady = value;
            }
        }

        private void Awake()
        {
            if (rawImage != null)
            {
                rawImageRT = rawImage.gameObject.GetComponent<RectTransform>();
                rawImageSize = new Vector2(rawImageRT.rect.width, rawImageRT.rect.height);
            }
        }

        // Use this for initialization
        public void SetupRemoveBackground()
        {
            //RLMGLogger.Instance.Log("Setting up remove background...", MESSAGETYPE.INFO);


            //fpsMonitor = GetComponent<FpsMonitor>();

            webCamTextureToMatHelper = gameObject.GetComponent<myWebCamTextureToMatHelper>();

#if UNITY_WEBGL
            // getFilePath_Coroutine = GetFilePath ();
            // StartCoroutine (getFilePath_Coroutine);
#else
            // sForests_model_filepath = Utils.getFilePath(SFORESTS_MODEL_FILENAME);
            // SetupStructuredForests();
#endif

            if (configLoader.configData.defaultCamera != null)
                webCamTextureToMatHelper.requestedDeviceName = configLoader.configData.defaultCamera;

            webCamTextureToMatHelper.flipVertical = configLoader.configData.flipVertical;

            webCamTextureToMatHelper.flipHorizontal = configLoader.configData.flipHorizontal;

            webCamTextureToMatHelper.requestedFPS = configLoader.configData.requestedFPS;


#if UNITY_ANDROID && !UNITY_EDITOR
            // Avoids the front camera low light issue that occurs in only some Android devices (e.g. Google Pixel, Pixel2).
            webCamTextureToMatHelper.avoidAndroidFrontCameraLowLightIssue = true;
#endif

            webCamTextureToMatHelper.Initialize ();
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

        void SetupStructuredForests()
        {
            if (string.IsNullOrEmpty(sForests_model_filepath))
            {
                RLMGLogger.Instance.Log("Structured forests model file is not loaded. \n Please copy to “Assets/StreamingAssets/” folder.", MESSAGETYPE.INFO);
            }
            else
            {
                //RLMGLogger.Instance.Log("Creating edge detection... " + sForests_model_filepath, MESSAGETYPE.INFO);

                edgeDetection = Ximgproc.createStructuredEdgeDetection(sForests_model_filepath);
                
                if (edgeDetection.empty())
                    RLMGLogger.Instance.Log("Structured forests algorithm is empty.", MESSAGETYPE.ERROR);
            }

        }

        /// <summary>
        /// Raises the webcam texture to mat helper initialized event.
        /// </summary>
        public void OnWebCamTextureToMatHelperInitialized()
        {
            RLMGLogger.Instance.Log("Calling OnWebCamTextureToMatHelperInitialized...", MESSAGETYPE.INFO);

            Mat webCamTextureMat = webCamTextureToMatHelper.GetMat();

            int rWidth = webCamTextureMat.cols();
            int rHeight = (int)(rWidth * (settings.targetHeight/settings.targetWidth));
            rawImageDisplayMat = new Mat(rHeight, rWidth, webCamTextureMat.type(), new Scalar(0,0,0,0));
            
            rawImageTexture = new Texture2D(rawImageDisplayMat.cols(), rawImageDisplayMat.rows(), TextureFormat.RGBA32, false);
            rawImage.texture = rawImageTexture;

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

            DEBUG_PAPER_EDGE_COLOR = new Scalar(0,255,0,255);
            FEEDBACK_PAPER_EDGE_COLOR = new Scalar(49,238,255,255);

            InitThread();

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
            

#if !UNITY_WEBGL
            StopThread();
#else
            StopCoroutine ("ThreadWorker");
#endif


            if (rawImageDisplayMat != null)
            {
                rawImageDisplayMat.Dispose();
                rawImageDisplayMat = null;
            }

            if (rgbaMat4Thread != null)
                rgbaMat4Thread.Dispose();

            if (rgbaMat4RefinedThread != null)
                rgbaMat4RefinedThread.Dispose();

            if (resultMat != null)
                resultMat.Dispose();

        }

        /// <summary>
        /// Listener for the web cam texture to mat helper error occurred event.
        /// </summary>
        /// <param name="errorCode">Error code.</param>
        public void OnWebCamTextureToMatHelperErrorOccurred (myWebCamTextureToMatHelper.ErrorCode errorCode)
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

        private void CopyToThreadMat()
        {
            //no need for a 'using' statement here
            //this is just a new reference to either the frameMat
            //or rotatedFrameMat on myWebCamTextureToMatHelper
            //which are disposed of properly
            Mat rgbaMat = webCamTextureToMatHelper.GetMat();

            if (rgbaMat4Thread != null)
                rgbaMat.copyTo(rgbaMat4Thread);
        }

        private void CopyToRefinedThreadMat()
        {
            //slighly inefficient, because onBeginScan we call this twice in one frame
            //but negligible
            Mat rgbaMat = webCamTextureToMatHelper.GetMat();

            if (rgbaMat4RefinedThread != null)
                rgbaMat.copyTo(rgbaMat4RefinedThread);
        }

        private void ToTexture2D()
        {

            Utils.fastMatToTexture2D(resultMat, rawImageTexture, true, 0, true);
        }

        // Update is called once per frame
        void Update()
        {
            bool webCamTextureReady = (webCamTextureToMatHelper &&
                webCamTextureToMatHelper.IsPlaying ());

            //if (webCamTextureToMatHelper != null)
            //{
            //    Debug.Log(webCamTextureToMatHelper.IsPlaying());
            //    Debug.Log(webCamTextureToMatHelper.DidUpdateThisFrame());
            //}

            if (webCamTextureReady && !isThreadRunning)
                InitThread();
            
            if (!webCamTextureReady)
                StopThread();

            if (webCamTextureReady && webCamTextureToMatHelper.DidUpdateThisFrame ())
            {
                if (!shouldDetectInMultiThread)
                {
                    if (shouldCopyToRefinedMat)
                    {
                        DateTime before = DateTime.Now;

                        //Copy for both threads
                        CopyToThreadMat();
                        CopyToRefinedThreadMat();

                        refinedMatReady = true;
                        shouldCopyToRefinedMat = false;

                        DateTime after = DateTime.Now;

                        TimeSpan duration = after.Subtract(before);

                        RLMGLogger.Instance.Log(
                            String.Format("Webcam texture successfuly copied for refined scan in {0} milliseconds.", duration.TotalMilliseconds),
                            MESSAGETYPE.INFO
                        );
                    }
                    else
                    {
                        //Copy for just the continuous feedback
                        CopyToThreadMat();
                    }

                    shouldDetectInMultiThread = true;
                }

                if (didUpdateTheDetectionResult)
                {
                    didUpdateTheDetectionResult = false;

#if UNITY_WEBGL
                    Imgproc.putText (resultMat, "WebGL platform does not support multi-threading.", new Point (5, resultMat.rows () - 10), Imgproc.FONT_HERSHEY_SIMPLEX, 0.5, new Scalar (255, 255, 255, 255), 1, Imgproc.LINE_AA, false);
#endif

                    //perhaps reduce the size of this mat first
                    
                    ToTexture2D();

                    // if (beginScanButton)
                    //     beginScanButton.interactable = beginScanButtonInteractable;
                }
                
            }
        }

        private void InitThread()
        {
            RLMGLogger.Instance.Log("Initializing thread.", MESSAGETYPE.INFO);
            StopThread();

            rgbaMat4Thread = new Mat();
            rgbaMat4RefinedThread = new Mat();
            resultMat = rawImageDisplayMat.clone();

            shouldDetectInMultiThread = false;

#if !UNITY_WEBGL
            StartThread(ThreadWorker);
#else
            StartCoroutine ("ThreadWorker");
#endif
        }

        private void StartThread(Action action)
        {
            shouldStopThread = false;

#if UNITY_METRO && NETFX_CORE
            System.Threading.Tasks.Task.Run(() => action());
#elif UNITY_METRO
            action.BeginInvoke(ar => action.EndInvoke(ar), null);
#else
            ThreadPool.QueueUserWorkItem(_ => action());
#endif

            RLMGLogger.Instance.Log("Thread Start.", MESSAGETYPE.INFO);
        }

        private void StopThread()
        {
            if (!isThreadRunning)
                return;

            shouldStopThread = true;

            while (isThreadRunning)
            {
                //if (RLMGLogger.Instance != null)
                //    RLMGLogger.Instance.Log("Awaiting thread stop...", MESSAGETYPE.INFO);
                //Wait threading stop
            }

            if (RLMGLogger.Instance != null)
                RLMGLogger.Instance.Log("...thread stopped.", MESSAGETYPE.INFO);
        }

#if !UNITY_WEBGL
        private void ThreadWorker()
        {
            isThreadRunning = true;

            while (!shouldStopThread)
            {
                if (!shouldDetectInMultiThread)
                    continue;

                try
                {
                    ProcessImage();
                }
                catch (Exception e)
                {
                    // Debug.LogError(e);
                    RLMGLogger.Instance.Log(e.ToString(), MESSAGETYPE.INFO);
                    shouldStopThread = true;
                }
                
                shouldDetectInMultiThread = false;
                didUpdateTheDetectionResult = true;
            }

            isThreadRunning = false;
        }


#else
        private IEnumerator ThreadWorker ()
        {
            while (true) {
                while (!shouldDetectInMultiThread) {
                    yield return null;
                }

                try
                {
                    ProcessImage();
                }
                catch (Exception e)
                {
                    // Debug.Log(e);
                    RLMGLogger.Instance.Log(e.ToString(), MESSAGETYPE.INFO);
                }

                shouldDetectInMultiThread = false;
                didUpdateTheDetectionResult = true;
            }
        }
#endif

        private void ProcessImage()
        {
            if ( rgbaMat4Thread.empty() ) return;



            //reduce size
            if (DOWNSCALE_RATIO > 0)
                Imgproc.resize (rgbaMat4Thread, rgbaMat4Thread, new Size (), 1.0 / DOWNSCALE_RATIO, 1.0 / DOWNSCALE_RATIO, Imgproc.INTER_LINEAR);
            
            PerspectiveUtils.BrightnessContrast(rgbaMat4Thread,settings.brightness,settings.contrast);

            using (Mat yMat = new Mat())
            {
                if (!doProcessImage)
                {
                    resultMat.setTo(new Scalar(0, 0, 0, 0));
                    PresentationUtils.ScaleUpAndDisplayMat(
                        rgbaMat4Thread, resultMat,
                        settings.doSizeToFit
                    );
                    return;
                }

                EdgeFinding.GetCannyEdgeMat(rgbaMat4Thread,yMat);

                // find potential paper contours.
                List<MatOfPoint> contours = new List<MatOfPoint>();
                PerspectiveUtils.Find4PointContours(yMat, contours);

                // pick the contour of the largest area and rearrange the points in a consistent order.
                MatOfPoint paperMaxAreaContour = PerspectiveUtils.GetMaxAreaContour(contours);
                paperMaxAreaContour = PerspectiveUtils.OrderCornerPoints(paperMaxAreaContour);

                paperFound = (paperMaxAreaContour.size().area() > 0);

                if (paperFound)
                {
                    paperFoundFrames++;

                    currentPaperArea = Imgproc.contourArea(paperMaxAreaContour);
                    DoPaperAreaRunningAverage();
                }
                else
                {
                    paperFoundFrames = 0;
                    ClearPaperAreaRunningAverage();
                }

                if (paperFound && consistentPaperFound && displayOptions.doWarp)
                {
                    // transform the perspective of original image.
                    using (Mat transformedMat = PerspectiveUtils.PerspectiveTransform(rgbaMat4Thread, paperMaxAreaContour))
                    {
                        if (transformedMat.width() > 1 && transformedMat.height() > 1)
                        {
                            //crop in the edges to hide them from the find largest contour later
                            using (Mat croppedMat = PerspectiveUtils.CropByPercent(transformedMat, 0.95f))
                            {
                                if (doDownsizeToDisplay)
                                {
                                    float ratio = rawImageSize.x / transformedMat.width();
                                    if (ratio < 1)
                                    {
                                        Imgproc.resize(transformedMat, transformedMat, new Size(), ratio, ratio, Imgproc.INTER_LINEAR);
                                    }
                                }

                                using (Mat edgeMat = new Mat())
                                {
                                    EdgeFinding.SetEdgeMat(transformedMat, edgeMat, settings, edgeDetection);

                                    if (displayOptions.showEdges)
                                        PresentationUtils.ShowEdges(edgeMat, transformedMat);

                                    using (Mat removedMat = RemoveBackgroundUtils.PolyfillMaskBackground(transformedMat, edgeMat, out MatOfPoint maxAreaContour, out Mat mask))
                                    {
                                        currentContourArea = maxAreaContour.size().area();
                                        DoContourSizeRunningAverage(currentContourArea);

                                        //undo warp perspective and apply mask to input
                                        //if (false)
                                        //{
                                        //    using (Mat reverseTransformedMask = new Mat(rgbaMat4Thread.height(), rgbaMat4Thread.width(), rgbaMat4Thread.type(), new Scalar(0,0,0,0)))
                                        //    {
                                        //        PerspectiveUtils.ReversePerspectiveTransform(mask,paperMaxAreaContour,reverseTransformedMask);

                                        //        using (Mat outputMat = new Mat(rgbaMat4Thread.height(), rgbaMat4Thread.width(), rgbaMat4Thread.type(), new Scalar(0,0,0,255)))
                                        //        {
                                        //            Core.copyTo(rgbaMat4Thread, outputMat, reverseTransformedMask);

                                        //            Core.addWeighted(rgbaMat4Thread,0.2f,outputMat,0.8f,0,outputMat);

                                        //            PresentationUtils.ScaleUpAndDisplayMat(
                                        //                outputMat, resultMat,
                                        //                settings.doSizeToFit
                                        //            );
                                        //        }

                                        //    }

                                        //    return;
                                        //}

                                        if (consistentRunningAverage)
                                        {
                                            if (displayOptions.doRemoveBackground)
                                            {
                                                PerspectiveUtils.BrightnessContrast(removedMat, settings.postProcessingSettings.brightness, settings.postProcessingSettings.contrast, true);

                                                PresentationUtils.MakeReadyToPresent(
                                                    removedMat, resultMat,
                                                    maxAreaContour,
                                                    displayOptions, settings
                                                );
                                            }
                                            else
                                            {
                                                PerspectiveUtils.BrightnessContrast(transformedMat, settings.postProcessingSettings.brightness, settings.postProcessingSettings.contrast, true);

                                                PresentationUtils.MakeReadyToPresent(
                                                    transformedMat, resultMat,
                                                    maxAreaContour,
                                                    displayOptions, settings
                                                );
                                            }
                                        }
                                        else
                                        {
                                            resultMat.setTo(new Scalar(0, 0, 0, 0));

                                            //if (consistentPaperArea)
                                            //    Imgproc.drawContours(rgbaMat4Thread, new List<MatOfPoint> { paperMaxAreaContour }, -1, FEEDBACK_PAPER_EDGE_COLOR, 10);

                                            PresentationUtils.ScaleUpAndDisplayMat(
                                                rgbaMat4Thread, resultMat,
                                                settings.doSizeToFit
                                            );
                                        }
                                    }
                                }
                            }
                        }
                    }
                    
                    
                    
                }
                else //not paperFound and/or paperFoundFrames > x and/or doing warp
                {
                    resultMat.setTo( new Scalar(0,0,0,0) );

                    if (displayOptions.showEdges) //yMat is now Canny edges, see above
                        Imgproc.cvtColor(yMat, rgbaMat4Thread, Imgproc.COLOR_GRAY2RGBA);

                    if (paperFound && displayOptions.doDrawPaperEdge)
                        Imgproc.drawContours(rgbaMat4Thread,new List<MatOfPoint> {paperMaxAreaContour}, -1, DEBUG_PAPER_EDGE_COLOR, 4);

                    PresentationUtils.ScaleUpAndDisplayMat(
                        rgbaMat4Thread, resultMat,
                        settings.doSizeToFit
                    );
                }


                    
            }
            

            
        }

        private void ClearPaperAreaRunningAverage()
        {
            paperAtRunningAverageArea = 0;
            runningAveragePaperArea = 0.0d;
        }

        private void DoPaperAreaRunningAverage()
        {
            float threshold = settings != null && settings.enableBeginScanButtonSettings != null ? settings.enableBeginScanButtonSettings.consistentPaperAreaThreshold : 0.5f;

            if (Math.Abs(currentPaperArea - runningAveragePaperArea) > (double)threshold * runningAveragePaperArea)
            {
                paperAtRunningAverageArea = 1;
                runningAveragePaperArea = currentPaperArea;
            }
            else
            {
                paperAtRunningAverageArea += 1;
                runningAveragePaperArea = (runningAveragePaperArea + currentPaperArea) / 2.0d;
            }
        }

        private void DoPaperCenterRunningAverage()
        {
            if (Vector2.Distance(currentPaperCenter,runningAveragePaperCenter) > 100)
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

        private void DoContourSizeRunningAverage(double currentContourArea)
        {
            float threshold = settings != null && settings.enableBeginScanButtonSettings != null ? settings.enableBeginScanButtonSettings.consistentScanAreaThreshold : 0.5f;

            if (Math.Abs(currentContourArea - runningAverageContourSize) > (double)threshold * runningAverageContourSize)
            {
                contoursAtRunningAverageSize = 1;
                runningAverageContourSize = currentContourArea;
            }
            else
            {
                contoursAtRunningAverageSize += 1;
                runningAverageContourSize = (runningAverageContourSize + currentContourArea) / 2.0d;
            }
        }

        /// <summary>
        /// Raises the destroy event.
        /// </summary>
        void OnDestroy()
        {
            if (webCamTextureToMatHelper != null)
                webCamTextureToMatHelper.Dispose ();

#if UNITY_WEBGL
            if (getFilePath_Coroutine != null) {
                StopCoroutine (getFilePath_Coroutine);
                ((IDisposable)getFilePath_Coroutine).Dispose ();
            }
#endif
        }

      
    }
}

#endif