using UnityEngine;
using TMPro;
using OpenCVForUnity.UnityUtils.Helper;
using rlmg.logging;

[RequireComponent(typeof(Canvas))]
public class CameraDisconnectedDisplay : MonoBehaviour
{
    private myWebCamTextureToMatHelper webCamTextureToMatHelper;

    private Canvas errorDisplay;

    [SerializeField]
    private TMP_Text errorText;

    private void Awake()
    {
        if (webCamTextureToMatHelper == null)
            webCamTextureToMatHelper = FindObjectOfType<myWebCamTextureToMatHelper>();

        if (errorDisplay == null)
            errorDisplay = GetComponent<Canvas>();

        if (errorDisplay != null)
            errorDisplay.enabled = false;
    }

    private void OnEnable()
    {
        if (webCamTextureToMatHelper != null)
        {
            webCamTextureToMatHelper.onInitialized.AddListener(HideCameraError);
            webCamTextureToMatHelper.onErrorOccurred.AddListener(ShowCameraError);
        }
    }

    private void OnDisable()
    {
        if (webCamTextureToMatHelper != null)
        {
            webCamTextureToMatHelper.onInitialized.RemoveListener(HideCameraError);
            webCamTextureToMatHelper.onErrorOccurred.RemoveListener(ShowCameraError);
        }
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
                    System.String.Format(
                        "Camera {0} appears to be connected. Dismissing error display...",
                        webCamTextureToMatHelper.requestedDeviceName
                    ),
                    MESSAGETYPE.INFO
                );
            }

            errorDisplay.enabled = false;
        }
    }
}
