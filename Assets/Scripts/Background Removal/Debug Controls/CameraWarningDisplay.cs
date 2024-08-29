using UnityEngine;
using TMPro;
using OpenCVForUnity.UnityUtils.Helper;
using rlmg.logging;

[RequireComponent(typeof(Canvas))]
public class CameraWarningDisplay : MonoBehaviour
{
    private myWebCamTextureToMatHelper webCamTextureToMatHelper;

    private Canvas warningDisplay;

    [SerializeField]
    private TMP_Text warningText;

    private void Awake()
    {
        if (webCamTextureToMatHelper == null)
            webCamTextureToMatHelper = FindObjectOfType<myWebCamTextureToMatHelper>();


        if (warningDisplay == null)
            warningDisplay = GetComponent<Canvas>();

        if (warningDisplay != null)
            warningDisplay.enabled = false;
    }

    private void OnEnable()
    {
        if (webCamTextureToMatHelper != null)
        {
            webCamTextureToMatHelper.onWarnOccurred.AddListener(ShowWarnDisplay);
            webCamTextureToMatHelper.onSuccessOccurred.AddListener(HideWarnDisplay);
        }    
    }

    private void OnDisable()
    {
        if (webCamTextureToMatHelper != null)
        {
            webCamTextureToMatHelper.onWarnOccurred.RemoveListener(ShowWarnDisplay);
            webCamTextureToMatHelper.onSuccessOccurred.RemoveListener(HideWarnDisplay);
        }
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
                    RLMGLogger.Instance.Log("CAMERA WARNING: First FRONTFACING camera used instead of requested camera.", MESSAGETYPE.ERROR);
                    break;

                case myWebCamTextureToMatHelper.WarnCode.WRONG_CAMERA_FIRST_SELECTED:
                    if (warningText != null)
                        warningText.text = "The requested camera was not found, and the first camera was used instead.";
                    RLMGLogger.Instance.Log("CAMERA WARNING: First camera OF ANY KIND used instead of requested camera.", MESSAGETYPE.ERROR);
                    break;
            }
        }

    }

    private void HideWarnDisplay()
    {
        if (warningDisplay != null)
            warningDisplay.enabled = false;
    }


}
