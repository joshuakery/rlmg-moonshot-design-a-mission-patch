#if !(PLATFORM_LUMIN && !UNITY_EDITOR)

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.XimgprocModule;
using OpenCVForUnity.UnityUtils.Helper;
using OpenCVForUnity.UnityUtils;
using System;
using System.IO;
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
    /// Main demo of drawing background removal
    /// </summary>
    [RequireComponent (typeof(myWebCamTextureToMatHelper))]
    public class removeBackground : MonoBehaviour
    {

        public RemoveBackgroundSettings settings;
        public RemoveBackgroundDisplayOptions displayOptions;

        private StructuredEdgeDetection edgeDetection;

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



        public Mat rgbaMat;

        public bool webCamTextureReady = false;

        public Mat yMat;
        public Mat edgeMat;
        public Mat transformedMat;

        public Mat rawImageDisplayMat;
        public Texture2D rawImageTexture;

        public Scalar PAPER_EDGE_COLOR;

        /// <summary>
        /// The texture.
        /// </summary>
        Texture2D texture;

        public RawImage rawImage;

        /// <summary>
        /// The webcam texture to mat helper.
        /// </summary>
        public myWebCamTextureToMatHelper webCamTextureToMatHelper;

        /// <summary>
        /// The FPS monitor.
        /// </summary>
        FpsMonitor fpsMonitor;

        /// <summary>
        /// The Game State Scriptable Object.
        /// </summary>
        public GameState gameState;

        /// <summary>
        /// Parallel thread for Imgproc.grabcut
        /// </summary>
        GrabcutRemovalThread grabcutThread;

        // RemoveBackgroundThread removeBackgroundThread;

        /// <summary>
        /// The unscaled Mat from which the preview Texture2D was derived
        /// </summary>
        public Mat previewMat;

        /// <summary>
        /// GameEvent that's fired after myWebcameTextureHelper initialization
        /// </summary>
        public GameEvent WebCamTextureToMatHelperInitialized;

        public GameEvent NewPreview;
        public GameEvent ScanFailed;

        public CamConfigLoader configLoader;
        // public string defaultCamera;
        //FaceTime HD Camera (Built-in)
        //HD Pro Webcam C920

        DateTime before;
        DateTime after;

        // Use this for initialization
        public void SetupRemoveBackground ()
        {
            // Debug.Log("SETTING UP REMOVE BACKGROUND");
            before = DateTime.Now;
        
            fpsMonitor = GetComponent<FpsMonitor> ();
            
            webCamTextureToMatHelper = gameObject.GetComponent<myWebCamTextureToMatHelper> ();

#if UNITY_WEBGL
            getFilePath_Coroutine = GetFilePath();
            StartCoroutine(getFilePath_Coroutine);
#else
            sForests_model_filepath = Utils.getFilePath(SFORESTS_MODEL_FILENAME);
            SetupStructuredForests();
#endif

            if (configLoader.configData.defaultCamera != null)
                webCamTextureToMatHelper.requestedDeviceName = configLoader.configData.defaultCamera;
                
            webCamTextureToMatHelper.flipVertical = configLoader.configData.flipVertical;

            webCamTextureToMatHelper.flipHorizontal = configLoader.configData.flipHorizontal;

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

            SetupStructuredForests();
        }
#endif

        void SetupStructuredForests()
        {
            if (string.IsNullOrEmpty(sForests_model_filepath))
            {
                RLMGLogger.Instance.Log("Structured forests model file is not loaded. \n Please copy to “Assets/StreamingAssets/” folder.", MESSAGETYPE.ERROR);
            }
            else
            {
                RLMGLogger.Instance.Log("Creating edge detection... " + sForests_model_filepath, MESSAGETYPE.INFO);

                edgeDetection = Ximgproc.createStructuredEdgeDetection(sForests_model_filepath);
                
                if (edgeDetection.empty())
                    RLMGLogger.Instance.Log("Structured forests algorithm is empty.", MESSAGETYPE.ERROR);
            }

        }

        /// <summary>
        /// Raises the web cam texture to mat helper initialized event.
        /// </summary>
        public void OnWebCamTextureToMatHelperInitialized ()
        {
            RLMGLogger.Instance.Log("OnWebCamTextureToMatHelperInitialized", MESSAGETYPE.INFO);
            
            Mat webCamTextureMat = webCamTextureToMatHelper.GetMat ();

            int rWidth = webCamTextureMat.cols();
            int rHeight = (int)(rWidth * (settings.targetHeight/settings.targetWidth));
            rawImageDisplayMat = new Mat(rHeight, rWidth, webCamTextureMat.type(), new Scalar(0,0,0,0));
            
            rawImageTexture = new Texture2D(rawImageDisplayMat.cols(), rawImageDisplayMat.rows(), TextureFormat.RGBA32, false);
            rawImage.texture = rawImageTexture;

            RLMGLogger.Instance.Log("Screen.width " + Screen.width + " Screen.height " + Screen.height + " Screen.orientation " + Screen.orientation, MESSAGETYPE.INFO);



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

            // yuvMat = new Mat();
            yMat = new Mat();

            previewMat = new Mat();

            PAPER_EDGE_COLOR = new Scalar(0,255,0,255);

            // if WebCamera is frontFaceing, flip Mat.
            // webCamTextureToMatHelper.flipHorizontal = webCamTextureToMatHelper.GetWebCamDevice().isFrontFacing;

            WebCamTextureToMatHelperInitialized.Raise();

            after = DateTime.Now; 
            TimeSpan duration = after.Subtract(before);
            Debug.Log("SETUP REMOVE BACKGROUND & WEBCAMHELPER INITIALIZED Duration in milliseconds: " + duration.Milliseconds);
        }

        /// <summary>
        /// Raises the web cam texture to mat helper disposed event.
        /// </summary>
        public void OnWebCamTextureToMatHelperDisposed ()
        {
            Debug.Log("OnWebCamTextureToMatHelperDisposed");

            if (texture != null)
            {
                Texture2D.Destroy (texture);
                texture = null;
            }

            if (rgbaMat != null)
            {
                rgbaMat.Dispose();
                rgbaMat = null;
            }


            if (yMat != null)
            {
                yMat.Dispose();
                yMat = null;
            }


            if (rawImageDisplayMat != null)
            {
                rawImageDisplayMat.Dispose();
                rawImageDisplayMat = null;
            }


            if (transformedMat != null)
            {
                transformedMat.Dispose();
                transformedMat = null;
            }

            if (edgeMat != null)
            {
                edgeMat.Dispose();
                edgeMat = null;
            }

            if (previewMat != null)
            {
                previewMat.Dispose();
                previewMat = null;
            }

        }

        /// <summary>
        /// Raises the web cam texture to mat helper error occurred event.
        /// </summary>
        /// <param name="errorCode">Error code.</param>
        public void OnWebCamTextureToMatHelperErrorOccurred (myWebCamTextureToMatHelper.ErrorCode errorCode)
        {
            RLMGLogger.Instance.Log("OnWebCamTextureToMatHelperErrorOccurred " + errorCode, MESSAGETYPE.ERROR);
        }

        // Update is called once per frame
        void Update ()
        {
            webCamTextureReady = (webCamTextureToMatHelper &&
                webCamTextureToMatHelper.IsPlaying () &&
                webCamTextureToMatHelper.DidUpdateThisFrame ());

            if (webCamTextureReady)
            {
                // if (readyToUpdate) StartCoroutine(StartScanThread());
                rgbaMat = webCamTextureToMatHelper.GetMat ();

                PerspectiveUtils.BrightnessContrast(rgbaMat,settings.brightness,settings.contrast);

                EdgeFinding.GetCannyEdgeMat(rgbaMat,yMat);

                // find potential paper contours.
                List<MatOfPoint> contours = new List<MatOfPoint>();
                PerspectiveUtils.Find4PointContours(yMat, contours);

                // pick the contour of the largest area and rearrange the points in a consistent order.
                MatOfPoint paperMaxAreaContour = PerspectiveUtils.GetMaxAreaContour(contours);
                paperMaxAreaContour = PerspectiveUtils.OrderCornerPoints(paperMaxAreaContour);

                bool paperFound = (paperMaxAreaContour.size().area() > 0);
                if (paperFound && displayOptions.doWarp)
                {
                    // transform the perspective of original image.
                    transformedMat = PerspectiveUtils.PerspectiveTransform(rgbaMat, paperMaxAreaContour);
                    if (transformedMat.width() > 1 && transformedMat.height() > 1)
                    {
                        //crop in the edges to hide them from the find largest contour later
                        // PerspectiveUtils.CropByPercent(transformedMat, transformedMat, 1.0f);

                        //find edges
                        edgeMat = new Mat();
                        EdgeFinding.SetEdgeMat(transformedMat,edgeMat,settings.edgeFindingMethod);

                        if (displayOptions.showEdges)
                            PresentationUtils.ShowEdges(edgeMat,transformedMat);

                        using (Mat removedMat = RemoveBackgroundUtils.PolyfillMaskBackground(transformedMat,edgeMat, out MatOfPoint maxAreaContour))
                        {
                            if (displayOptions.doRemoveBackground)
                            {
                                PresentationUtils.MakeReadyToPresent(
                                    removedMat, rawImageDisplayMat,
                                    maxAreaContour,
                                    displayOptions, settings
                                );
                            }
                            else
                            {
                                PresentationUtils.MakeReadyToPresent(
                                    transformedMat, rawImageDisplayMat,
                                    maxAreaContour,
                                    displayOptions, settings
                                );
                            }
                        }
                    }
                    
                }
                else //not paperFound and/or doing warp
                {
                    rawImageDisplayMat.setTo( new Scalar(0,0,0,0) );

                    if (displayOptions.showEdges) //yMat is now Canny edges, see above
                        Imgproc.cvtColor(yMat, rgbaMat, Imgproc.COLOR_GRAY2RGBA);

                    if (displayOptions.doDrawPaperEdge)
                        Imgproc.drawContours(rgbaMat,new List<MatOfPoint> {paperMaxAreaContour}, -1, PAPER_EDGE_COLOR, 4);

                    PresentationUtils.ScaleUpAndDisplayMat(
                        rgbaMat,rawImageDisplayMat,
                        settings.doSizeToFit
                    );
                }

                Utils.fastMatToTexture2D(rawImageDisplayMat, rawImageTexture, true, 0, true);

                
            }
        }

        /// <summary>
        /// Raises the destroy event.
        /// </summary>
        void OnDestroy ()
        {
            if (webCamTextureToMatHelper != null)
            {
                webCamTextureToMatHelper.Dispose ();
            }
            if (grabcutThread != null && !grabcutThread.IsDone)
            {
                RLMGLogger.Instance.Log("Ending parallel thread...", MESSAGETYPE.INFO);
                grabcutThread.Abort();
                // removeBackgroundThread.Abort();
                RLMGLogger.Instance.Log("...ended", MESSAGETYPE.INFO);
            }
            
        }

        private IEnumerator DoRefinedScan()
        {
            using (
                Mat removedMat = OpenCVForUnity.CoreModule.Mat.zeros(transformedMat.height(), transformedMat.width(), CvType.CV_8UC4),
                    displayMat = new Mat (settings.targetHeight,settings.targetWidth,webCamTextureToMatHelper.GetMat().type(),new Scalar(0,0,0,0))
            )
            {   
                Texture2D scanTexture = new Texture2D(displayMat.cols(), displayMat.rows(), TextureFormat.RGBA32, false);

                MatOfPoint maxAreaContourDest = new MatOfPoint();

                yield return StartCoroutine( RemoveBackgroundUtils.GrabCutCoroutine(transformedMat,edgeMat,removedMat,grabcutThread,maxAreaContourDest ) );

                PresentationUtils.MakeReadyToPresent(
                    removedMat, displayMat,
                    maxAreaContourDest,
                    displayOptions, settings  
                );

                Utils.fastMatToTexture2D(displayMat, scanTexture, true, 0, true);

                gameState.preview = scanTexture;

                if (IsAllTransparent(removedMat))
                {
                    ScanFailed.Raise();
                }
                else
                {
                    previewMat = removedMat.clone();
                    NewPreview.Raise();
                }
                
            }    
        }

        private bool IsAllTransparent(Mat src)
        {
            if (src.channels() == 4)
            {
                List<Mat> planes = new List<Mat>();
                Core.split(src,planes);
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

        public void SetRefinedScan()
        {
            StopAllCoroutines();
            if (grabcutThread == null || grabcutThread.IsDone)
            {
                StartCoroutine(DoRefinedScan());
            }
            
        }

    }


}


#endif
