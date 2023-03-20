using System.Collections;
using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using rlmg.logging;
using OpenCVForUnity.UnityUtils.Helper;
using TMPro;
using ArtScan.CoreModule;
using ArtScan.ScanSavingModule;

namespace ArtScan.ErrorDisplayModule
{
    public class CameraDisconnectHandler : MonoBehaviour
    {
        private AsynchronousRemoveBackground asynchronousRemoveBackground;
        private myWebCamTextureToMatHelper liveFeedbackWebCamTextureToMatHelper;


        private myWebCamTextureToMatHelper mainWebCamTextureToMatHelper;

        [SerializeField]
        private myWebCamTextureToMatHelper refinedScanWebCamTextureToMapHelper;

        [SerializeField]
        private ErrorDisplaySettingsSO errorDisplaySettingsSO;

        [SerializeField]
        private Canvas errorDisplay;
        [SerializeField]
        private TMP_Text errorText;
        [SerializeField]
        private Canvas warningDisplay;
        [SerializeField]
        private TMP_Text warningText;

        public int lastUpdateCount = -1;
        public Texture2D lastTexture = null;

        private DateTime timeout;
        private bool configLoaded = false;

        private bool shouldBePlaying = false;

        private int webCamTextureID = 0;

        private void Awake()
        {
            if (asynchronousRemoveBackground == null)
                asynchronousRemoveBackground = FindObjectOfType<AsynchronousRemoveBackground>();

            if (liveFeedbackWebCamTextureToMatHelper == null && asynchronousRemoveBackground != null)
                liveFeedbackWebCamTextureToMatHelper = asynchronousRemoveBackground.gameObject.GetComponent<myWebCamTextureToMatHelper>();

            if (errorDisplay != null)
                errorDisplay.enabled = false;

            if (warningDisplay != null)
                warningDisplay.enabled = false;

            mainWebCamTextureToMatHelper = liveFeedbackWebCamTextureToMatHelper != null ? liveFeedbackWebCamTextureToMatHelper : refinedScanWebCamTextureToMapHelper;
        }

        // Start is called before the first frame update
        void Start()
        {
            configLoaded = false;

            timeout = DateTime.Now + TimeSpan.FromSeconds((double)errorDisplaySettingsSO.errorDisplaySettings.checkForDisconnectInterval);
            RLMGLogger.Instance.Log("Resetting timeout to " + timeout.ToLongTimeString(), MESSAGETYPE.INFO);
        }

        private void OnEnable()
        {
            if (mainWebCamTextureToMatHelper != null)
            {
                mainWebCamTextureToMatHelper.onInitialized.AddListener(ResetUpdateCount);
                mainWebCamTextureToMatHelper.onInitialized.AddListener(HideCameraError);

                mainWebCamTextureToMatHelper.onErrorOccurred.AddListener(ShowCameraError);
                mainWebCamTextureToMatHelper.onErrorOccurred.AddListener(AttemptAutoRecovery);

                mainWebCamTextureToMatHelper.onWarnOccurred.AddListener(ShowWarnDisplay);

                mainWebCamTextureToMatHelper.onSuccessOccurred.AddListener(HideWarnDisplay);

                mainWebCamTextureToMatHelper.onPlay.AddListener(ResetUpdateCount);
                mainWebCamTextureToMatHelper.onPlay.AddListener(SetShouldBePlayingTrue);

                mainWebCamTextureToMatHelper.onPause.AddListener(ResetUpdateCount);
                mainWebCamTextureToMatHelper.onPause.AddListener(SetShouldBePlayingFalse);

                mainWebCamTextureToMatHelper.onStop.AddListener(ResetUpdateCount);
                mainWebCamTextureToMatHelper.onStop.AddListener(SetShouldBePlayingFalse);
            }
        }

        private void OnDisable()
        {
            if (mainWebCamTextureToMatHelper != null)
            {
                mainWebCamTextureToMatHelper.onInitialized.RemoveListener(ResetUpdateCount);
                mainWebCamTextureToMatHelper.onInitialized.RemoveListener(HideCameraError);

                mainWebCamTextureToMatHelper.onErrorOccurred.RemoveListener(ShowCameraError);
                mainWebCamTextureToMatHelper.onErrorOccurred.RemoveListener(AttemptAutoRecovery);

                mainWebCamTextureToMatHelper.onWarnOccurred.RemoveListener(ShowWarnDisplay);

                mainWebCamTextureToMatHelper.onSuccessOccurred.RemoveListener(HideWarnDisplay);

                mainWebCamTextureToMatHelper.onPlay.RemoveListener(ResetUpdateCount);
                mainWebCamTextureToMatHelper.onPlay.RemoveListener(SetShouldBePlayingTrue);

                mainWebCamTextureToMatHelper.onPause.RemoveListener(ResetUpdateCount);
                mainWebCamTextureToMatHelper.onPause.RemoveListener(SetShouldBePlayingFalse);

                mainWebCamTextureToMatHelper.onStop.RemoveListener(ResetUpdateCount);
                mainWebCamTextureToMatHelper.onStop.RemoveListener(SetShouldBePlayingFalse);
            }
        }

        private void ResetUpdateCount()
        {
            RLMGLogger.Instance.Log("Resetting update count...", MESSAGETYPE.INFO);
            lastUpdateCount = -1;

            lastTexture = null;
            webCamTextureID = mainWebCamTextureToMatHelper.GetWebCamTexture() != null ?
                mainWebCamTextureToMatHelper.GetWebCamTexture().GetInstanceID() : 0;
        }

        private void SetShouldBePlayingTrue()
        {
            RLMGLogger.Instance.Log("Setting should be playing to true...", MESSAGETYPE.INFO);
            shouldBePlaying = true;
        }

        private void SetShouldBePlayingFalse()
        {
            RLMGLogger.Instance.Log("Setting should be playing to false...", MESSAGETYPE.INFO);
            shouldBePlaying = false;
        }

        private void ShowCameraError(myWebCamTextureToMatHelper.ErrorCode errorCode)
        {
            if (errorDisplay != null)
            {
                errorDisplay.enabled = true;

                switch (errorCode)
                {
                    case myWebCamTextureToMatHelper.ErrorCode.CAMERA_DEVICE_NOT_EXIST:
                        if (errorText != null)
                            errorText.text = "No camera devices detected.";
                        RLMGLogger.Instance.Log("CAMERA ERROR: No camera devices detected", MESSAGETYPE.ERROR);
                        break;

                    case myWebCamTextureToMatHelper.ErrorCode.TIMEOUT:
                        if (errorText != null)
                            errorText.text = "Camera has timed out.\n\nThis might be because the camera is not sending any frame data to the app.";
                        RLMGLogger.Instance.Log("CAMERA ERROR: Camera has timed out.", MESSAGETYPE.ERROR);
                        break;

                    case myWebCamTextureToMatHelper.ErrorCode.CAMERA_PERMISSION_DENIED:
                        if (errorText != null)
                            errorText.text = "Camera permission denied.";
                        RLMGLogger.Instance.Log("CAMERA ERROR: Camera permission denied.", MESSAGETYPE.ERROR);
                        break;
                }
            }

        }

        private void HideCameraError()
        {
            if (errorDisplay != null)
            {
                if (errorDisplay.enabled)
                {
                    RLMGLogger.Instance.Log(
                       System.String.Format("Camera {0} appears to be connected. Dismissing error display...", mainWebCamTextureToMatHelper.requestedDeviceName),
                       MESSAGETYPE.INFO
                   );
                }

                errorDisplay.enabled = false;
            }
        }

        private void AttemptAutoRecovery(myWebCamTextureToMatHelper.ErrorCode errorCode)
        {
            StartCoroutine(AttemptAutoRecoveryCo(errorCode));
        }

        private IEnumerator AttemptAutoRecoveryCo(myWebCamTextureToMatHelper.ErrorCode errorCode)
        {
            //Always wait longer than the CheckDevices interval
            //That way if it's called, this coroutine can be stopped
            float wait = errorDisplaySettingsSO.errorDisplaySettings.checkForDisconnectInterval + 1f;
            yield return new WaitForSeconds(wait);

            RLMGLogger.Instance.Log(
                string.Format("CAMERA RE-INIT: Attempting auto-recovery after a camera error: {0}.", errorCode),
                MESSAGETYPE.INFO
            );

            ReInitialize();
        }

        private void ShowWarnDisplay(myWebCamTextureToMatHelper.WarnCode warnCode)
        {
            if (warningDisplay != null)
            {
                RLMGLogger.Instance.Log("Showing warn display...", MESSAGETYPE.INFO);

                warningDisplay.enabled = true;

                switch (warnCode)
                {
                    case myWebCamTextureToMatHelper.WarnCode.WRONG_CAMERA_FRONTFACING_SELECTED:
                        if (warningText != null)
                            warningText.text = "The requested camera was not found, and the first frontfacing camera was used instead.";
                        RLMGLogger.Instance.Log("CAMERA WARNING: First FRONTFACING camera used instead of requested camera.", MESSAGETYPE.INFO);
                        break;

                    case myWebCamTextureToMatHelper.WarnCode.WRONG_CAMERA_FIRST_SELECTED:
                        if (warningText != null)
                            warningText.text = "The requested camera was not found, and the first camera was used instead.";
                        RLMGLogger.Instance.Log("CAMERA WARNING: First camera OF ANY KIND used instead of requested camera.", MESSAGETYPE.INFO);
                        break;
                }
            }

        }

        private void HideWarnDisplay()
        {
            if (warningDisplay != null)
                warningDisplay.enabled = false;
        }



        // Update is called once per frame
        void Update()
        {
            if (DateTime.Now > timeout)
            {
                timeout = DateTime.Now + TimeSpan.FromSeconds((double)errorDisplaySettingsSO.errorDisplaySettings.checkForDisconnectInterval);
                RLMGLogger.Instance.Log("Resetting timeout to " + timeout.ToLongTimeString(), MESSAGETYPE.INFO);

                StartCoroutine(CheckDevicesCo());
            }
        }

        public void SetCanCheckDevices(bool value)
        {
            configLoaded = value;
        }

        private IEnumerator CheckDevicesCo()
        {
            yield return new WaitForEndOfFrame();

            CheckDevices();
        }

        private void CheckDevices()
        {
            if (configLoaded)
            {
                RLMGLogger.Instance.Log(
                    System.String.Format(
                        "Checking devices... webCamTexture is null: {0}; isPlaying: {1}; updateCount: {2}; shouldBePlaying: {3}; saved webCamTextureID: {4}; current ID: {5}; lastUpdateCount: {6}",
                        (mainWebCamTextureToMatHelper.GetWebCamTexture() == null) ? "True" : "False",
                        (mainWebCamTextureToMatHelper.IsPlaying()) ? "True" : "False",
                        (mainWebCamTextureToMatHelper.GetWebCamTexture() != null) ? mainWebCamTextureToMatHelper.GetWebCamTexture().updateCount : "Is Null",
                        (shouldBePlaying) ? "True" : "False",
                        webCamTextureID,
                        mainWebCamTextureToMatHelper.GetWebCamTexture() != null ?
                            mainWebCamTextureToMatHelper.GetWebCamTexture().GetInstanceID() : "Null webCamTexture",
                        lastUpdateCount
                    ),
                    MESSAGETYPE.INFO
                );

                if (mainWebCamTextureToMatHelper == null)
                {
                    RLMGLogger.Instance.Log("...cannot check devices because a webCamTextureToMatHelper is null.", MESSAGETYPE.INFO);
                    return;
                }

                if (!mainWebCamTextureToMatHelper.IsInitialized())
                {
                    RLMGLogger.Instance.Log("...cannot check devices because a webCamTextureToMatHelper is not initialized.", MESSAGETYPE.INFO);
                    return;
                }    

                if (mainWebCamTextureToMatHelper.GetWebCamTexture() != null)
                {
                    if (mainWebCamTextureToMatHelper.IsPlaying())
                    {
                        if (lastUpdateCount == mainWebCamTextureToMatHelper.GetWebCamTexture().updateCount)
                        {
                            RLMGLogger.Instance.Log(
                                string.Format("CAMERA RE-INIT: Currently used webcam, {0}, has not updated since last check. Update count: {1}", mainWebCamTextureToMatHelper.GetWebCamDevice().name, mainWebCamTextureToMatHelper.GetWebCamTexture().updateCount),
                                MESSAGETYPE.ERROR
                            );

                            SaveCurrentAndLastWebCamTexturesToFile();

                            lastUpdateCount = (int)mainWebCamTextureToMatHelper.GetWebCamTexture().updateCount;
                            
                            if (mainWebCamTextureToMatHelper.GetWebCamTexture() != null)
                            {
                                WebCamTexture webCam = mainWebCamTextureToMatHelper.GetWebCamTexture();
                                lastTexture = new Texture2D(webCam.width, webCam.height);
                                lastTexture.SetPixels32(webCam.GetPixels32());
                                lastTexture.Apply();
                            }

                            if (errorDisplaySettingsSO.errorDisplaySettings.doAttemptCameraRestartIfFrozenUpdateCount)
                            {
                                ReInitialize();
                            }

                            return;
                        }
                        else //updateCounts are not equal
                        {
                            lastUpdateCount = (int)mainWebCamTextureToMatHelper.GetWebCamTexture().updateCount;
                            
                            if (mainWebCamTextureToMatHelper.GetWebCamTexture() != null)
                            {
                                WebCamTexture webCam = mainWebCamTextureToMatHelper.GetWebCamTexture();
                                lastTexture = new Texture2D(webCam.width, webCam.height);
                                lastTexture.SetPixels32(webCam.GetPixels32());
                                lastTexture.Apply();
                            }
                        }
                    }
                    else //not isPlaying
                    {
                        lastUpdateCount = -1;
                        lastTexture = null;
                    }                    
                }
                else
                {
                    RLMGLogger.Instance.Log(
                            "Resetting lastUpdateCount because webCamTexture was null.",
                            MESSAGETYPE.ERROR
                        );

                    lastUpdateCount = -1;
                    lastTexture = null;
                }

                if (mainWebCamTextureToMatHelper.GetWebCamTexture() != null)
                {
                    if (shouldBePlaying && !mainWebCamTextureToMatHelper.IsPlaying())
                    {
                        RLMGLogger.Instance.Log("Webcam should be playing but it's not. Playing...", MESSAGETYPE.INFO);
                        mainWebCamTextureToMatHelper.Play();
                    }
                }

                WebCamDevice[] devices = WebCamTexture.devices;
                if (Array.IndexOf(devices, mainWebCamTextureToMatHelper.GetWebCamDevice()) < 0)
                {
                    RLMGLogger.Instance.Log(
                        string.Format("CAMERA RE-INIT: Currently used webcam, {0}, not found in devices list: {1}", mainWebCamTextureToMatHelper.GetWebCamDevice().name, PrintDeviceNames(devices)),
                        MESSAGETYPE.ERROR
                    );

                    if (errorDisplaySettingsSO.errorDisplaySettings.doAttemptCameraRestartIfMissingDevice)
                    {
                        ReInitialize();
                    }

                    return;
                }

                if (mainWebCamTextureToMatHelper.requestedDeviceName != mainWebCamTextureToMatHelper.GetDeviceName())
                {
                    RLMGLogger.Instance.Log(
                        string.Format("CAMERA RE-INIT: Currently used webcam, {0} is not the requested device: {1}", mainWebCamTextureToMatHelper.GetDeviceName(), mainWebCamTextureToMatHelper.requestedDeviceName),
                        MESSAGETYPE.ERROR
                    );

                    if (errorDisplaySettingsSO.errorDisplaySettings.doAttemptCameraRestartIfWrongCamera)
                    {
                        ReInitialize();
                    }

                    return;
                }
            }
            else
            {
                RLMGLogger.Instance.Log(
                    "Cannot check devices because config is not yet loaded.",
                    MESSAGETYPE.INFO
                );
            }
        }

        private string PrintDeviceNames(WebCamDevice[] devices)
        {
            string names = "";
            foreach (WebCamDevice d in devices)
            {
                names += String.Format("; {0}",d.name);
            }
            return names;
        }

        public void SaveCurrentAndLastWebCamTexturesToFile()
        {
            string dirPath = Path.Join(Application.streamingAssetsPath, "Debug Images");
            string filename = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + "_1.png";
            ArtScan.ScanSavingModule.ScanSaving.SaveTexture2D(lastTexture, dirPath, filename);

            string filename2 = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + "_2.png";
            WebCamTexture webCam2 = mainWebCamTextureToMatHelper.GetWebCamTexture();
            Texture2D currentTexture = new Texture2D(webCam2.width, webCam2.height);
            currentTexture.SetPixels32(webCam2.GetPixels32());
            currentTexture.Apply();
            ArtScan.ScanSavingModule.ScanSaving.SaveTexture2D(currentTexture, dirPath, filename2);
        }

        private void ReInitialize()
        {
            StopAllCoroutines();

            RLMGLogger.Instance.Log(
                System.String.Format("Re-initializing {0}...", mainWebCamTextureToMatHelper.requestedDeviceName),
                MESSAGETYPE.INFO
            );

            mainWebCamTextureToMatHelper.Initialize();

            if (refinedScanWebCamTextureToMapHelper != null && refinedScanWebCamTextureToMapHelper != mainWebCamTextureToMatHelper)
                refinedScanWebCamTextureToMapHelper.Initialize();
        }

        public void DoReInitialize()
        {
            ReInitialize();
        }
    }
}


