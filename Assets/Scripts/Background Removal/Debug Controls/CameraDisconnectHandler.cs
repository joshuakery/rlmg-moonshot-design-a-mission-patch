using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using rlmg.logging;
using OpenCVForUnity.UnityUtils.Helper;
using TMPro;
using ArtScan.CoreModule;

namespace ArtScan.ErrorDisplayModule
{
    public class CameraDisconnectHandler : MonoBehaviour
    {
        public AsynchronousRemoveBackground asynchronousRemoveBackground;
        public myWebCamTextureToMatHelper webCamTextureToMatHelper;
        public RefinedScanController refinedScanController;
        public DebugMenu debugMenu;

        public ErrorDisplaySettingsSO errorDisplaySettingsSO;

        public Canvas errorDisplay;
        public TMP_Text errorText;
        public Canvas warningDisplay;
        public TMP_Text warningText;

        private bool canCountTowardsTimeout = false;
        private float lastUpdateTime;

        public int lastUpdateCount = -1;

        private DateTime timeout;
        private bool configLoaded = false;

        private bool shouldBePlaying = false;

        private void Awake()
        {
            if (asynchronousRemoveBackground == null)
                asynchronousRemoveBackground = FindObjectOfType<AsynchronousRemoveBackground>();

            if (webCamTextureToMatHelper == null)
                webCamTextureToMatHelper = (myWebCamTextureToMatHelper)FindObjectOfType(typeof(myWebCamTextureToMatHelper));

            if (refinedScanController == null)
                refinedScanController = FindObjectOfType<RefinedScanController>();

            if (debugMenu == null)
                debugMenu = (DebugMenu)FindObjectOfType(typeof(DebugMenu));

            errorDisplay.enabled = false;
            warningDisplay.enabled = false;
        }

        // Start is called before the first frame update
        void Start()
        {
            lastUpdateTime = 0;
            configLoaded = false;

            timeout = DateTime.Now + TimeSpan.FromSeconds((double)errorDisplaySettingsSO.errorDisplaySettings.checkForDisconnectInterval);
            RLMGLogger.Instance.Log("Resetting timeout to " + timeout.ToLongTimeString(), MESSAGETYPE.INFO);
        }

        private void OnEnable()
        {
            if (webCamTextureToMatHelper != null)
            {
                webCamTextureToMatHelper.onInitialized.AddListener(ResetTimeoutCounter);
                webCamTextureToMatHelper.onInitialized.AddListener(ResetUpdateCount);
                webCamTextureToMatHelper.onInitialized.AddListener(HideCameraError);

                webCamTextureToMatHelper.onErrorOccurred.AddListener(ShowCameraError);
                webCamTextureToMatHelper.onErrorOccurred.AddListener(AttemptAutoRecovery);

                webCamTextureToMatHelper.onWarnOccurred.AddListener(ShowWarnDisplay);

                webCamTextureToMatHelper.onSuccessOccurred.AddListener(HideWarnDisplay);

                webCamTextureToMatHelper.onPlay.AddListener(ResetUpdateCount);
                webCamTextureToMatHelper.onPlay.AddListener(SetShouldBePlayingTrue);

                webCamTextureToMatHelper.onPause.AddListener(ResetUpdateCount);
                webCamTextureToMatHelper.onPlay.AddListener(SetShouldBePlayingFalse);

                webCamTextureToMatHelper.onStop.AddListener(ResetUpdateCount);
                webCamTextureToMatHelper.onStop.AddListener(SetShouldBePlayingFalse);
            }
        }

        private void OnDisable()
        {
            if (webCamTextureToMatHelper != null)
            {
                webCamTextureToMatHelper.onInitialized.RemoveListener(ResetTimeoutCounter);
                webCamTextureToMatHelper.onInitialized.RemoveListener(ResetUpdateCount);
                webCamTextureToMatHelper.onInitialized.RemoveListener(HideCameraError);

                webCamTextureToMatHelper.onErrorOccurred.RemoveListener(ShowCameraError);
                webCamTextureToMatHelper.onErrorOccurred.RemoveListener(AttemptAutoRecovery);

                webCamTextureToMatHelper.onWarnOccurred.RemoveListener(ShowWarnDisplay);

                webCamTextureToMatHelper.onSuccessOccurred.RemoveListener(HideWarnDisplay);

                webCamTextureToMatHelper.onPlay.RemoveListener(ResetUpdateCount);
                webCamTextureToMatHelper.onPlay.RemoveListener(SetShouldBePlayingTrue);

                webCamTextureToMatHelper.onPause.RemoveListener(ResetUpdateCount);
                webCamTextureToMatHelper.onPlay.RemoveListener(SetShouldBePlayingFalse);

                webCamTextureToMatHelper.onStop.RemoveListener(ResetUpdateCount);
                webCamTextureToMatHelper.onStop.RemoveListener(SetShouldBePlayingFalse);
            }
        }

        private void ResetTimeoutCounter()
        {
            RLMGLogger.Instance.Log("Resetting timeout counter...", MESSAGETYPE.INFO);

            canCountTowardsTimeout = true;
            lastUpdateTime = 0;
        }

        private void ResetUpdateCount()
        {
            RLMGLogger.Instance.Log("Resetting update count...", MESSAGETYPE.INFO);
            lastUpdateCount = -1;
        }

        private void SetShouldBePlayingTrue()
        {
            shouldBePlaying = true;
        }

        private void SetShouldBePlayingFalse()
        {
            shouldBePlaying = false;
        }

        private void ShowCameraError(myWebCamTextureToMatHelper.ErrorCode errorCode)
        {
            errorDisplay.enabled = true;

            switch (errorCode)
            {
                case myWebCamTextureToMatHelper.ErrorCode.CAMERA_DEVICE_NOT_EXIST:
                    errorText.text = "No camera devices detected.";
                    RLMGLogger.Instance.Log("CAMERA ERROR: No camera devices detected", MESSAGETYPE.ERROR);
                    break;

                case myWebCamTextureToMatHelper.ErrorCode.TIMEOUT:
                    errorText.text = "Camera has timed out.\n\nThis might be because the camera is not sending any frame data to the app.";
                    RLMGLogger.Instance.Log("CAMERA ERROR: Camera has timed out.", MESSAGETYPE.ERROR);
                    break;

                case myWebCamTextureToMatHelper.ErrorCode.CAMERA_PERMISSION_DENIED:
                    errorText.text = "Camera permission denied.";
                    RLMGLogger.Instance.Log("CAMERA ERROR: Camera permission denied.", MESSAGETYPE.ERROR);
                    break;
            }
        }

        private void HideCameraError()
        {
            if (errorDisplay.enabled)
            {
                RLMGLogger.Instance.Log(
                   System.String.Format("Camera {0} appears to be connected. Dismissing error display...", webCamTextureToMatHelper.requestedDeviceName),
                   MESSAGETYPE.INFO
               );
            }

            errorDisplay.enabled = false;
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
            RLMGLogger.Instance.Log("Showing warn display...", MESSAGETYPE.INFO);

            warningDisplay.enabled = true;

            switch (warnCode)
            {
                case myWebCamTextureToMatHelper.WarnCode.WRONG_CAMERA_FRONTFACING_SELECTED:
                    warningText.text = "The requested camera was not found, and the first frontfacing camera was used instead.";
                    RLMGLogger.Instance.Log("CAMERA WARNING: First FRONTFACING camera used instead of requested camera.", MESSAGETYPE.INFO);
                    break;

                case myWebCamTextureToMatHelper.WarnCode.WRONG_CAMERA_FIRST_SELECTED:
                    warningText.text = "The requested camera was not found, and the first camera was used instead.";
                    RLMGLogger.Instance.Log("CAMERA WARNING: First camera OF ANY KIND used instead of requested camera.", MESSAGETYPE.INFO);
                    break;
            }
        }

        private void HideWarnDisplay()
        {
            warningDisplay.enabled = false;
        }



        // Update is called once per frame
        void Update()
        {
            if (DateTime.Now > timeout)
            {
                timeout = DateTime.Now + TimeSpan.FromSeconds((double)errorDisplaySettingsSO.errorDisplaySettings.checkForDisconnectInterval);
                RLMGLogger.Instance.Log("Resetting timeout to " + timeout.ToLongTimeString(), MESSAGETYPE.INFO);

                CheckDevices();
            }
            //CheckForDidUpdateFrame();

            //if (Input.GetKeyDown(KeyCode.G))
            //{
            //    lastUpdateTime = errorDisplaySettingsSO.errorDisplaySettings.cameraDisconnectTimeout + 10f;
            //    ReInitialize();
            //}
            //if (Input.GetKeyDown(KeyCode.H))
            //{
            //    StartCoroutine(SpecialCheckDevices());
            //}
            //if (Input.GetKeyDown(KeyCode.J))
            //{
            //    StartCoroutine(SpecialCheckDevices2());
            //}
            //if (Input.GetKeyDown(KeyCode.F))
            //{
            //    RLMGLogger.Instance.Log(
            //        string.Format("F key pressed: Current requestedFPS: {0}",
            //            webCamTextureToMatHelper.GetWebCamTexture() != null ?
            //            webCamTextureToMatHelper.GetWebCamTexture().requestedFPS :
            //            "WebCamTexture is null."
            //        ),
            //        MESSAGETYPE.INFO
            //    );
            //}
        }

        //private IEnumerator SpecialCheckDevices2()
        //{
        //    webCamTextureToMatHelper.Play();
        //    yield return null;
        //    webCamTextureToMatHelper.Pause();
        //}

        //private IEnumerator SpecialCheckDevices()
        //{
        //    if (webCamTextureToMatHelper.GetWebCamTexture() != null)
        //    {
        //        bool aux = webCamTextureToMatHelper.IsPlaying();
        //        Debug.Log(webCamTextureToMatHelper.GetWebCamTexture().updateCount);
        //        Debug.Log(webCamTextureToMatHelper.IsPlaying());
        //        lastUpdateCount = (int)webCamTextureToMatHelper.GetWebCamTexture().updateCount;
        //        webCamTextureToMatHelper.Pause();
        //        yield return null;
        //        if (aux) webCamTextureToMatHelper.Play();
        //        Debug.Log(webCamTextureToMatHelper.GetWebCamTexture().updateCount);
        //        Debug.Log(webCamTextureToMatHelper.IsPlaying());
        //        Debug.Log(lastUpdateCount);
        //        Debug.Log("HAS UPDATED: " + (!(lastUpdateCount == webCamTextureToMatHelper.GetWebCamTexture().updateCount)).ToString());
        //    }
        //}

        //private IEnumerator CheckDevicesCo()
        //{
        //    while (true)
        //    {
        //        RLMGLogger.Instance.Log("Checking devices...", MESSAGETYPE.INFO);

        //        if ((asynchronousRemoveBackground.shouldCopyToRefinedMat && !asynchronousRemoveBackground.refinedMatReady) ||
        //            refinedScanController.anotherScanIsUnderway)
        //        {
        //            continue;
        //        }

        //        if (webCamTextureToMatHelper.GetWebCamTexture() != null)
        //        {
        //            if (webCamTextureToMatHelper.IsPlaying() &&
        //                lastUpdateCount == webCamTextureToMatHelper.GetWebCamTexture().updateCount)
        //            {
        //                RLMGLogger.Instance.Log(
        //                    string.Format("CAMERA RE-INIT: Currently used webcam, {0}, has not updated since last check. Update count: {1}", webCamTextureToMatHelper.GetWebCamDevice().name, webCamTextureToMatHelper.GetWebCamTexture().updateCount),
        //                    MESSAGETYPE.ERROR
        //                );

        //                lastUpdateCount = (int)webCamTextureToMatHelper.GetWebCamTexture().updateCount;

        //                if (errorDisplaySettingsSO.errorDisplaySettings.doAttemptCameraRestart)
        //                    ReInitialize();

        //                continue;
        //            }

        //            lastUpdateCount = (int)webCamTextureToMatHelper.GetWebCamTexture().updateCount;
        //        }
        //        else
        //        {
        //            lastUpdateCount = -1;
        //        }

        //        WebCamDevice[] devices = WebCamTexture.devices;
        //        if (Array.IndexOf(devices, webCamTextureToMatHelper.GetWebCamDevice()) < 0)
        //        {
        //            RLMGLogger.Instance.Log(
        //                string.Format("CAMERA RE-INIT: Currently used webcam, {0}, not found in devices list: {1}", webCamTextureToMatHelper.GetWebCamDevice().name, PrintDeviceNames(devices)),
        //                MESSAGETYPE.ERROR
        //            );

        //            if (errorDisplaySettingsSO.errorDisplaySettings.doAttemptCameraRestart)
        //                ReInitialize();

        //            //do not continue
        //        }

        //        yield return new WaitForSeconds(errorDisplaySettingsSO.errorDisplaySettings.checkForDisconnectInterval);
        //    }

        //}

        public void SetCanCheckDevices(bool value)
        {
            configLoaded = value;
        }

        private void CheckDevices()
        {
            if (configLoaded)
            {
                RLMGLogger.Instance.Log("Checking devices...", MESSAGETYPE.INFO);

                if (refinedScanController.refinedScanningIsUnderway)
                {
                    RLMGLogger.Instance.Log("...cannot check devices because a refined scan is underway.", MESSAGETYPE.INFO);
                    return;
                }

                refinedScanController.StopAllCoroutines();
                refinedScanController.AbortRefinedScan();

                if (webCamTextureToMatHelper.GetWebCamTexture() != null)
                {
                    if (webCamTextureToMatHelper.IsPlaying() &&
                        lastUpdateCount == webCamTextureToMatHelper.GetWebCamTexture().updateCount)
                    {
                        RLMGLogger.Instance.Log(
                            string.Format("CAMERA RE-INIT: Currently used webcam, {0}, has not updated since last check. Update count: {1}", webCamTextureToMatHelper.GetWebCamDevice().name, webCamTextureToMatHelper.GetWebCamTexture().updateCount),
                            MESSAGETYPE.ERROR
                        );


                        lastUpdateCount = (int)webCamTextureToMatHelper.GetWebCamTexture().updateCount;

                        if (errorDisplaySettingsSO.errorDisplaySettings.doAttemptCameraRestart)
                        {
                            ReInitialize();
                        }

                        return;
                    }

                    lastUpdateCount = (int)webCamTextureToMatHelper.GetWebCamTexture().updateCount;
                }
                else
                {
                    RLMGLogger.Instance.Log(
                            "Resetting lastUpdateCount because webCamTexture was null.",
                            MESSAGETYPE.ERROR
                        );
                    lastUpdateCount = -1;
                }

                WebCamDevice[] devices = WebCamTexture.devices;
                if (Array.IndexOf(devices, webCamTextureToMatHelper.GetWebCamDevice()) < 0)
                {
                    RLMGLogger.Instance.Log(
                        string.Format("CAMERA RE-INIT: Currently used webcam, {0}, not found in devices list: {1}", webCamTextureToMatHelper.GetWebCamDevice().name, PrintDeviceNames(devices)),
                        MESSAGETYPE.ERROR
                    );

                    if (errorDisplaySettingsSO.errorDisplaySettings.doAttemptCameraRestart)
                    {
                        ReInitialize();
                    }

                    return;
                }

                if (errorDisplaySettingsSO.errorDisplaySettings.doAttemptCameraRestartIfWrongCamera)
                {
                    if (webCamTextureToMatHelper.requestedDeviceName != webCamTextureToMatHelper.GetDeviceName())
                    {
                        RLMGLogger.Instance.Log(
                            string.Format("CAMERA RE-INIT: Currently used webcam, {0} is not the requested device: {1}", webCamTextureToMatHelper.GetDeviceName(), webCamTextureToMatHelper.requestedDeviceName),
                            MESSAGETYPE.ERROR
                        );

                        if (errorDisplaySettingsSO.errorDisplaySettings.doAttemptCameraRestart)
                        {
                            ReInitialize();
                        }

                        return;
                    }
                }

                if (shouldBePlaying && !webCamTextureToMatHelper.IsPlaying())
                    webCamTextureToMatHelper.Play();


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

        //private void CheckForDidUpdateFrame()
        //{
        //    if (!canCountTowardsTimeout) { return; }
        //    if (refinedScanController.anotherScanIsUnderway)
        //    {
        //        lastUpdateTime = 0;
        //        return;
        //    }

        //    if (webCamTextureToMatHelper != null)
        //    {
        //        if (webCamTextureToMatHelper.IsPlaying())
        //        {
        //            if (webCamTextureToMatHelper.IsInitialized() == false)
        //                RLMGLogger.Instance.Log("WebCamTexture is not initialized.", MESSAGETYPE.ERROR);

        //            if (webCamTextureToMatHelper.GetWebCamTexture() == null)
        //                RLMGLogger.Instance.Log("WebCamTexture is null", MESSAGETYPE.ERROR);


        //            if (!webCamTextureToMatHelper.DidUpdateThisFrame())
        //            {
        //                if (Time.deltaTime > errorDisplaySettingsSO.errorDisplaySettings.cameraDisconnectTimeout)
        //                {
        //                    RLMGLogger.Instance.Log("Frame update was longer than timeout.", MESSAGETYPE.ERROR);
        //                }
        //                else if (Time.deltaTime > 3f)
        //                {
        //                    RLMGLogger.Instance.Log("Frame update was longer than 3 seconds.", MESSAGETYPE.ERROR);
        //                }

        //                lastUpdateTime += Time.deltaTime;
        //            }
        //            else
        //            {
        //                lastUpdateTime = 0;
        //            }

        //            if (lastUpdateTime > errorDisplaySettingsSO.errorDisplaySettings.cameraDisconnectTimeout)
        //            {
        //                if (errorDisplaySettingsSO.errorDisplaySettings.doAttemptCameraRestart)
        //                {
        //                    webCamTextureToMatHelper.Pause();

        //                    RLMGLogger.Instance.Log(
        //                        string.Format("CAMERA RE-INIT: Checking for updated frames timed out after {0} seconds.", lastUpdateTime),
        //                        MESSAGETYPE.ERROR
        //                    );

        //                    canCountTowardsTimeout = false;
        //                    lastUpdateTime = 0;

        //                    try
        //                    {
        //                        ReInitialize();
        //                    }
        //                    catch (Exception e)
        //                    {
        //                        RLMGLogger.Instance.Log(
        //                            string.Format("Exception occurred during re-initialize: {0}", e.ToString()),
        //                            MESSAGETYPE.ERROR
        //                        );
        //                    }

        //                }
        //            }
        //        }
        //        else
        //        {
        //            lastUpdateTime = 0;
        //        }
        //    }
        //}

        private void ReInitialize()
        {
            StopAllCoroutines();

            RLMGLogger.Instance.Log(
                System.String.Format("Re-initializing {0}...", webCamTextureToMatHelper.requestedDeviceName),
                MESSAGETYPE.INFO
            );

            webCamTextureToMatHelper.Initialize();

            if (debugMenu != null)
                debugMenu.InitializeDebugMenu();
        }
    }
}


