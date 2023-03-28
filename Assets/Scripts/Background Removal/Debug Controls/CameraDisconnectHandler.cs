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
        private myWebCamTextureToMatHelper mainWebCamTextureToMatHelper;

        [SerializeField]
        private ErrorDisplaySettingsSO errorDisplaySettingsSO;

        public int lastUpdateCount = -1;

        /// <summary>
        /// Track Texture2D from WebCamTexture between Update calls
        /// </summary>
        private Texture2D lastWebCamTexture = null;

        private DateTime timeout;
        private bool configLoaded = false;

        private bool shouldBePlaying = false;

        private int webCamTextureID = 0;

        private void Awake()
        {
            if (mainWebCamTextureToMatHelper == null)
                mainWebCamTextureToMatHelper = FindObjectOfType<myWebCamTextureToMatHelper>();
        }

        // Start is called before the first frame update
        void Start()
        {
            configLoaded = false;

            timeout = DateTime.Now + TimeSpan.FromSeconds((double)errorDisplaySettingsSO.errorDisplaySettings.checkForDisconnectInterval);

            if (errorDisplaySettingsSO.errorDisplaySettings.doVerboseLoggingOfDisconnectHandler)
            {
                RLMGLogger.Instance.Log(
                    "Resetting timeout to " + timeout.ToLongTimeString(),
                    MESSAGETYPE.INFO
                );
            }

        }

        private void OnEnable()
        {
            if (mainWebCamTextureToMatHelper != null)
            {
                mainWebCamTextureToMatHelper.onInitialized.AddListener(ResetUpdateCount);

                mainWebCamTextureToMatHelper.onErrorOccurred.AddListener(AttemptAutoRecovery);

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

                mainWebCamTextureToMatHelper.onErrorOccurred.RemoveListener(AttemptAutoRecovery);

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
            if (errorDisplaySettingsSO.errorDisplaySettings.doVerboseLoggingOfDisconnectHandler)
            {
                RLMGLogger.Instance.Log(
                    "Resetting update count...",
                    MESSAGETYPE.INFO
                );
            }

            lastUpdateCount = -1;

            if (lastWebCamTexture != null) { Destroy(lastWebCamTexture); }
            lastWebCamTexture = null;
            webCamTextureID = mainWebCamTextureToMatHelper.GetWebCamTexture() != null ?
                mainWebCamTextureToMatHelper.GetWebCamTexture().GetInstanceID() : 0;
        }

        private void SetShouldBePlayingTrue()
        {
            if (errorDisplaySettingsSO.errorDisplaySettings.doVerboseLoggingOfDisconnectHandler)
                RLMGLogger.Instance.Log("Setting should be playing to true...", MESSAGETYPE.INFO);

            shouldBePlaying = true;
        }

        private void SetShouldBePlayingFalse()
        {
            if (errorDisplaySettingsSO.errorDisplaySettings.doVerboseLoggingOfDisconnectHandler)
                RLMGLogger.Instance.Log("Setting should be playing to false...", MESSAGETYPE.INFO);

            shouldBePlaying = false;
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
                MESSAGETYPE.ERROR
            );

            ReInitialize();
        }

        // Update is called once per frame
        void Update()
        {
            if (DateTime.Now > timeout)
            {
                timeout = DateTime.Now + TimeSpan.FromSeconds((double)errorDisplaySettingsSO.errorDisplaySettings.checkForDisconnectInterval);

                if (errorDisplaySettingsSO.errorDisplaySettings.doVerboseLoggingOfDisconnectHandler)
                {
                    RLMGLogger.Instance.Log(
                        "Resetting timeout to " + timeout.ToLongTimeString(),
                        MESSAGETYPE.INFO
                    );
                }

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

        private void LogWebCamTextureToMatHelperStatus()
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
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Bool if the helper exists and IsInitialized()</returns>
        private bool CheckForWebCamTextureToMatHelperReady()
        {
            if (mainWebCamTextureToMatHelper == null)
            {
                RLMGLogger.Instance.Log("...cannot check devices because a webCamTextureToMatHelper is null.", MESSAGETYPE.ERROR);
                return false;
            }

            else if (!mainWebCamTextureToMatHelper.IsInitialized())
            {
                RLMGLogger.Instance.Log("...cannot check devices because a webCamTextureToMatHelper is not initialized.", MESSAGETYPE.ERROR);
                return false;
            }

            else return true;
        }

        /// <summary>
        /// Sets the lastUpdateCount value and also performs a comparison between the last updateCount and the current
        /// </summary>
        /// <returns>
        /// False if the helper says the WebCamTexture exists and isPlaying, but the updateCount has remained the same
        /// between two timeouts; true if the updateCount has incremented, if the WebCamTexture is not playing, or if it does not exist
        /// </returns>
        private bool CheckIfFramesAreUpdatingWhenTheyShouldBe()
        {
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

                        if (errorDisplaySettingsSO.errorDisplaySettings.doSaveCurrentAndLastWebCamTexturesToDisk)
                        {
                            SaveCurrentAndLastWebCamTexturesToFile();
                            SetLastWebCamTexture();
                        }

                        lastUpdateCount = (int)mainWebCamTextureToMatHelper.GetWebCamTexture().updateCount;

                        return false;
                    }
                    else //updateCounts are not equal
                    {
                        lastUpdateCount = (int)mainWebCamTextureToMatHelper.GetWebCamTexture().updateCount;

                        if (errorDisplaySettingsSO.errorDisplaySettings.doSaveCurrentAndLastWebCamTexturesToDisk)
                            SetLastWebCamTexture();

                        return true;
                    }
                }
                else //not isPlaying
                {
                    lastUpdateCount = -1;
                    if (lastWebCamTexture != null) { Destroy(lastWebCamTexture); }
                    lastWebCamTexture = null;
                    return true;
                }
            }
            else
            {
                RLMGLogger.Instance.Log(
                        "Resetting lastUpdateCount because webCamTexture was null.",
                        MESSAGETYPE.ERROR
                    );

                lastUpdateCount = -1;
                if (lastWebCamTexture != null) { Destroy(lastWebCamTexture); }
                lastWebCamTexture = null;
                return true;
            }
        }

        /// <summary>
        /// If the webcam is not playing but this handler says it should be, calls the Play() method on the helper
        /// The 'shouldBeValue' is changed by this handler's listeners to the helper's Play, Pause, and Stop methods
        /// This feature solves a Windows issue in which a momentarily disconnected camera Paused or Stopped the camera
        /// with otherwise no noticed problems
        /// </summary>
        private void TryPlayingWebCamIfNotPlayingButShouldBePlaying()
        {
            if (mainWebCamTextureToMatHelper.GetWebCamTexture() != null)
            {
                if (shouldBePlaying && !mainWebCamTextureToMatHelper.IsPlaying())
                {
                    RLMGLogger.Instance.Log(
                        "Webcam should be playing but it's not. Playing...",
                        MESSAGETYPE.ERROR
                    );
                    mainWebCamTextureToMatHelper.Play();
                }
            }
        }

        /// <summary>
        /// Checks if the current device on the helper is in the polled list of available devices
        /// </summary>
        /// <returns>Bool the device name is in the list of available devices</returns>
        private bool CheckForMissingDevice()
        {
            WebCamDevice[] devices = WebCamTexture.devices;
            if (Array.IndexOf(devices, mainWebCamTextureToMatHelper.GetWebCamDevice()) < 0)
            {
                RLMGLogger.Instance.Log(
                    string.Format("CAMERA RE-INIT: Currently used webcam, {0}, not found in devices list: {1}", mainWebCamTextureToMatHelper.GetWebCamDevice().name, PrintDeviceNames(devices)),
                    MESSAGETYPE.ERROR
                );
                return false;
            }
            else return true;
        }

        /// <summary>
        /// Checks if the requested device on the helper matches the current device
        /// </summary>
        /// <returns>Bool the device names match</returns>
        private bool CheckForRequestedDevice()
        {
            if (mainWebCamTextureToMatHelper.requestedDeviceName != mainWebCamTextureToMatHelper.GetDeviceName())
            {
                RLMGLogger.Instance.Log(
                    string.Format("CAMERA RE-INIT: Currently used webcam, {0} is not the requested device: {1}", mainWebCamTextureToMatHelper.GetDeviceName(), mainWebCamTextureToMatHelper.requestedDeviceName),
                    MESSAGETYPE.ERROR
                );

                return false;
            }
            else return true;
        }

        private void CheckDevices()
        {
            if (configLoaded)
            {
                if (errorDisplaySettingsSO.errorDisplaySettings.doVerboseLoggingOfDisconnectHandler)
                    LogWebCamTextureToMatHelperStatus();

                bool webCamTextureToMatHelperReady = CheckForWebCamTextureToMatHelperReady();
                if (webCamTextureToMatHelperReady)
                {
                    if (!CheckIfFramesAreUpdatingWhenTheyShouldBe() &&
                        errorDisplaySettingsSO.errorDisplaySettings.doAttemptCameraRestartIfFrozenUpdateCount)
                    {
                        ReInitialize();
                        return;
                    }

                    TryPlayingWebCamIfNotPlayingButShouldBePlaying();

                    if (!CheckForMissingDevice() &&
                        errorDisplaySettingsSO.errorDisplaySettings.doAttemptCameraRestartIfMissingDevice)
                    {
                        ReInitialize();
                        return;
                    }
                        
                    if (!CheckForRequestedDevice() &&
                        errorDisplaySettingsSO.errorDisplaySettings.doAttemptCameraRestartIfWrongCamera)
                    {
                        ReInitialize();
                        return;
                    }
                }
            }
            else
            {
                if (errorDisplaySettingsSO.errorDisplaySettings.doVerboseLoggingOfDisconnectHandler)
                {
                    RLMGLogger.Instance.Log(
                         "Cannot check devices because config is not yet loaded.",
                         MESSAGETYPE.INFO
                     );
                }
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

        private void SetLastWebCamTexture()
        {
            if (lastWebCamTexture != null) { Destroy(lastWebCamTexture); }

            if (mainWebCamTextureToMatHelper.GetWebCamTexture() != null)
            {
                WebCamTexture webCam = mainWebCamTextureToMatHelper.GetWebCamTexture();
                lastWebCamTexture = new Texture2D(webCam.width, webCam.height);
                lastWebCamTexture.name = "Disconnect Handler Last Texture";
                lastWebCamTexture.SetPixels32(webCam.GetPixels32());
                lastWebCamTexture.Apply();
            }
        }

        public void SaveCurrentAndLastWebCamTexturesToFile()
        {
            string dirPath = Path.Join(Application.streamingAssetsPath, "Debug Images");
            string filename = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + "_1.png";
            ArtScan.ScanSavingModule.ScanSaving.SaveTexture2D(lastWebCamTexture, dirPath, filename);

            string filename2 = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + "_2.png";
            WebCamTexture webCam2 = mainWebCamTextureToMatHelper.GetWebCamTexture();
            Texture2D currentTexture = new Texture2D(webCam2.width, webCam2.height);
            currentTexture.name = "Disconnect Handler Current Texture";
            currentTexture.SetPixels32(webCam2.GetPixels32());
            currentTexture.Apply();
            ArtScan.ScanSavingModule.ScanSaving.SaveTexture2D(currentTexture, dirPath, filename2);
            Destroy(currentTexture);
        }

        private void ReInitialize()
        {
            StopAllCoroutines();

            RLMGLogger.Instance.Log(
                System.String.Format("Re-initializing {0}...", mainWebCamTextureToMatHelper.requestedDeviceName),
                MESSAGETYPE.ERROR
            );

            mainWebCamTextureToMatHelper.Initialize();
        }

        public void DoReInitialize()
        {
            ReInitialize();
        }
    }
}


