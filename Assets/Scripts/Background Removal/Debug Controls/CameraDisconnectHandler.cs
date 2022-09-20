using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using rlmg.logging;
using OpenCVForUnity.UnityUtils.Helper;
using ArtScan.CoreModule;

public class CameraDisconnectHandler : MonoBehaviour
{
    public myWebCamTextureToMatHelper webCamTextureToMatHelper;
    public DebugMenu debugMenu;
    public Canvas errorDisplay;

    public bool doAttemptCameraRestart = true;

    public float timeout = 5f;

    public float lastUpdateTime;

    public bool displayError = false;

    private void Awake()
    {
        if (webCamTextureToMatHelper == null)
            webCamTextureToMatHelper = (myWebCamTextureToMatHelper)FindObjectOfType(typeof(myWebCamTextureToMatHelper));

        if (debugMenu == null)
            debugMenu = (DebugMenu)FindObjectOfType(typeof(DebugMenu));
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
        }
    }

    private void OnDisable()
    {
        if (webCamTextureToMatHelper != null)
        {
            webCamTextureToMatHelper.onInitialized.RemoveListener(ResetTimeoutCounter);
        }
    }

    private void ResetTimeoutCounter()
    {
        lastUpdateTime = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (webCamTextureToMatHelper != null)
        {
            if (webCamTextureToMatHelper.IsPlaying())
            {
                if (!webCamTextureToMatHelper.DidUpdateThisFrame())
                {
                    lastUpdateTime += Time.deltaTime;
                }
                else
                {
                    lastUpdateTime = 0;
                    if (displayError == true)
                    {
                        if (errorDisplay != null)
                            errorDisplay.enabled = false;
                        displayError = false;
                    }
                }

                if (lastUpdateTime > timeout)
                {
                    webCamTextureToMatHelper.Pause();
                    RLMGLogger.Instance.Log(
                        System.String.Format("Camera {0} appears to be disconnected.", webCamTextureToMatHelper.requestedDeviceName),
                        MESSAGETYPE.ERROR
                    );

                    displayError = true;
                    if (errorDisplay != null)
                        errorDisplay.enabled = true;

                    if (doAttemptCameraRestart)
                    {
                        webCamTextureToMatHelper.Initialize();
                    }
                    else
                    {
                        if (debugMenu != null)
                            debugMenu.InitializeDebugMenu();
                    }
                }
            }
        }
    }
}
