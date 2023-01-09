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
        public myWebCamTextureToMatHelper webCamTextureToMatHelper;
        public RefinedScanController refinedScanController;
        public DebugMenu debugMenu;

        public ErrorDisplaySettingsSO errorDisplaySettingsSO;

        public Canvas errorDisplay;
        public TMP_Text errorText;
        public Canvas warningDisplay;
        public TMP_Text warningText;

        [SerializeField]
        private bool canCountTowardsTimeout = false;
        private float lastUpdateTime;

        private void Awake()
        {
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
        }

        private void OnEnable()
        {
            if (webCamTextureToMatHelper != null)
            {
                webCamTextureToMatHelper.onInitialized.AddListener(ResetTimeoutCounter);
                webCamTextureToMatHelper.onInitialized.AddListener(HideCameraError);

                webCamTextureToMatHelper.onErrorOccurred.AddListener(ShowCameraError);
                webCamTextureToMatHelper.onErrorOccurred.AddListener(AttemptAutoRecovery);

                webCamTextureToMatHelper.onWarnOccurred.AddListener(ShowWarnDisplay);

                webCamTextureToMatHelper.onSuccessOccurred.AddListener(HideWarnDisplay);
            }
        }

        private void OnDisable()
        {
            if (webCamTextureToMatHelper != null)
            {
                webCamTextureToMatHelper.onInitialized.RemoveListener(ResetTimeoutCounter);
                webCamTextureToMatHelper.onInitialized.RemoveListener(HideCameraError);

                webCamTextureToMatHelper.onErrorOccurred.RemoveListener(ShowCameraError);
                webCamTextureToMatHelper.onErrorOccurred.RemoveListener(AttemptAutoRecovery);

                webCamTextureToMatHelper.onWarnOccurred.RemoveListener(ShowWarnDisplay);

                webCamTextureToMatHelper.onSuccessOccurred.RemoveListener(HideWarnDisplay);
            }
        }

        private void ResetTimeoutCounter()
        {
            RLMGLogger.Instance.Log("Resetting timeout counter...", MESSAGETYPE.INFO);

            canCountTowardsTimeout = true;
            lastUpdateTime = 0;
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
            CheckForDidUpdateFrame();

            if (Input.GetKeyDown(KeyCode.G))
            {
                lastUpdateTime = errorDisplaySettingsSO.errorDisplaySettings.cameraDisconnectTimeout + 10f;
                ReInitialize();
            }
        }

        private void CheckForDidUpdateFrame()
        {
            if (!canCountTowardsTimeout) { return; }
            if (refinedScanController.anotherScanIsUnderway)
            {
                lastUpdateTime = 0;
                return;
            }

            if (webCamTextureToMatHelper != null)
            {
                if (webCamTextureToMatHelper.IsPlaying())
                {
                    if (webCamTextureToMatHelper.IsInitialized() == false)
                        RLMGLogger.Instance.Log("WebCamTexture is not initialized.", MESSAGETYPE.ERROR);

                    if (webCamTextureToMatHelper.GetWebCamTexture() == null)
                        RLMGLogger.Instance.Log("WebCamTexture is null", MESSAGETYPE.ERROR);


                    if (!webCamTextureToMatHelper.DidUpdateThisFrame())
                    {
                        if (Time.deltaTime > errorDisplaySettingsSO.errorDisplaySettings.cameraDisconnectTimeout)
                        {
                            RLMGLogger.Instance.Log("Frame update was longer than timeout.", MESSAGETYPE.ERROR);
                        }
                        else if (Time.deltaTime > 3f)
                        {
                            RLMGLogger.Instance.Log("Frame update was longer than 3 seconds.", MESSAGETYPE.ERROR);
                        }

                        lastUpdateTime += Time.deltaTime;
                    }
                    else
                    {
                        lastUpdateTime = 0;
                    }

                    if (lastUpdateTime > errorDisplaySettingsSO.errorDisplaySettings.cameraDisconnectTimeout)
                    {
                        if (errorDisplaySettingsSO.errorDisplaySettings.doAttemptCameraRestart)
                        {
                            webCamTextureToMatHelper.Pause();

                            RLMGLogger.Instance.Log(
                                string.Format("CAMERA RE-INIT: Checking for updated frames timed out after {0} seconds.", lastUpdateTime),
                                MESSAGETYPE.ERROR
                            );

                            canCountTowardsTimeout = false;
                            lastUpdateTime = 0;

                            try
                            {
                                ReInitialize();
                            }
                            catch (Exception e)
                            {
                                RLMGLogger.Instance.Log(
                                    string.Format("Exception occurred during re-initialize: {0}", e.ToString()),
                                    MESSAGETYPE.ERROR
                                );
                            }

                        }
                    }
                }
                else
                {
                    lastUpdateTime = 0;
                }
            }
        }

        private void ReInitialize()
        {
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


