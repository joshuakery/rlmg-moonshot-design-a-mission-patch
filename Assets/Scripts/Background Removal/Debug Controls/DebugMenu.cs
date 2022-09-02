using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ArtScan.CoreModule;
using OpenCVForUnity.UnityUtils.Helper;
using ArtScan.CamConfigLoaderModule;
using rlmg.logging;
using TMPro;

public class DebugMenu : MonoBehaviour
{

    public myWebCamTextureToMatHelper webCamTextureToMatHelper;
    public RemoveBackgroundSettings settings;
    public RemoveBackgroundDisplayOptions displayOptions;

    public CamConfigLoader configLoader;

    /// <summary>
    /// ChangeCamera Button Prefab
    /// </summary>
    public GameObject changeCameraButtonPrefab; 

    /// <summary>
    /// ChangeCamera Buttons Container
    /// </summary>
    public Transform changeCameraButtonsContainer;

    public Toggle flipVerticalToggle;
    public Toggle flipHorizontalToggle;


    public Slider brightnessSlider;
    public Slider contrastSlider;

    public Toggle sobelToggle;
    public Toggle thresholdToggle;
    public Toggle structuredForestsToggle;
    public Toggle cannyToggle;

    public Toggle doSizeToFitToggle;
    public Toggle doSizeToFillToggle;
    public Toggle doCropToBoundingBoxToggle;

    public Slider pBrightnessSlider;
    public Slider pContrastSlider;

    public Toggle doDrawPaperEdgeToggle;
    public Toggle doWarpToggle;
    public Toggle showEdgesToggle;
    public Toggle doRemoveBackgroundToggle;
    public Toggle doDrawMaxAreaContourToggle;
    
    public Button resetButton;
    public GameObject feedback;

    public Color32 SUCCESS_COLOR = new Color32(40,178,52,255);
    public Color32 ERROR_COLOR = new Color32(178,40,40,255);

    private void OnEnable()
    {
        InitializeDebugMenu();
    }

    public void InitializeDebugMenu()
    {
        if (webCamTextureToMatHelper == null)
            webCamTextureToMatHelper = (myWebCamTextureToMatHelper)FindObjectOfType(typeof(myWebCamTextureToMatHelper));

        InstantiateChangeCameraButtons();

        ResetSettingsUI();
        ResetDisplayOptionsUIToCurrentState();

    }

    private void InstantiateChangeCameraButtons()
    {
        foreach (Transform child in changeCameraButtonsContainer) {
            GameObject.Destroy(child.gameObject);
        }

        var devices = WebCamTexture.devices;
        for (int i=0; i<devices.Length; i++)
        {
            var device = devices[i];
            GameObject cameraChoice = Instantiate(changeCameraButtonPrefab, changeCameraButtonsContainer);
            
            cameraChoice.transform.GetChild(0).GetComponent<Text>().text = device.name;

            Button button = cameraChoice.GetComponent<Button>();
            button.onClick.AddListener(delegate {
                OnChangeToSpecificCamera(device.name, button);
            });

            if (webCamTextureToMatHelper != null &&
                (webCamTextureToMatHelper.requestedDeviceName == i.ToString() ||
                webCamTextureToMatHelper.requestedDeviceName == device.name))
            {
                button.interactable = false;
            }
        }
    }

    private void OnChangeToSpecificCamera(string name, Button button)
    {
        webCamTextureToMatHelper.requestedDeviceName = name;
        foreach (Transform child in changeCameraButtonsContainer) {
            child.gameObject.GetComponent<Button>().interactable = true;
        }
        button.interactable = false;
    }

    private void ResetDisplayOptionsUIToCurrentState()
    {
        //Remove Background Toggles
        doDrawPaperEdgeToggle.isOn = displayOptions.doDrawPaperEdge;
        doDrawPaperEdgeToggle.interactable = !displayOptions.doWarp;

        doWarpToggle.isOn = displayOptions.doWarp;

        showEdgesToggle.isOn = displayOptions.showEdges;

        doRemoveBackgroundToggle.isOn = displayOptions.doRemoveBackground;
        doRemoveBackgroundToggle.interactable = displayOptions.doWarp;

        doDrawMaxAreaContourToggle.isOn = displayOptions.doDrawMaxAreaContour;
        doDrawMaxAreaContourToggle.interactable = displayOptions.doWarp;
    }

    private void ResetSettingsUI()
    {
        if (webCamTextureToMatHelper != null)
        {
            //Flip Toggles
            flipVerticalToggle.isOn = webCamTextureToMatHelper.flipVertical;
            flipHorizontalToggle.isOn = webCamTextureToMatHelper.flipHorizontal;
        }

        //Brightness Contrast
        brightnessSlider.value = settings.brightness;
        contrastSlider.value = settings.contrast;

        //Edge Finding Method
        if (settings.edgeFindingMethod == EdgeFindingMethod.Sobel)
            sobelToggle.isOn = true;
        else if (settings.edgeFindingMethod == EdgeFindingMethod.Threshold)
            thresholdToggle.isOn = true;
        else if (settings.edgeFindingMethod == EdgeFindingMethod.StructuredForests)
            structuredForestsToggle.isOn = true;
        else if (settings.edgeFindingMethod == EdgeFindingMethod.Canny)
            cannyToggle.isOn = true;

        // if ()
        
        //Size to Fit
        doSizeToFitToggle.isOn = settings.doSizeToFit;
        doSizeToFillToggle.isOn = !settings.doSizeToFit;
        doCropToBoundingBoxToggle.isOn = settings.doCropToBoundingBox;

        //Post Processing Settings
        pBrightnessSlider.value = settings.postProcessingSettings.brightness;
        pContrastSlider.value = settings.postProcessingSettings.contrast;

        resetButton.interactable = false;
    }

    public void OnResetToConfig()
    {
        if (webCamTextureToMatHelper != null)
        {
            webCamTextureToMatHelper.Pause();

            CamConfigJSON originalConfigData = configLoader.configData;

            webCamTextureToMatHelper.requestedDeviceName = originalConfigData.defaultCamera;
            webCamTextureToMatHelper.flipVertical = originalConfigData.flipVertical;
            webCamTextureToMatHelper.flipHorizontal = originalConfigData.flipHorizontal;

            settings.brightness = originalConfigData.brightness;
            settings.contrast = originalConfigData.contrast;

            settings.edgeFindingMethod = (EdgeFindingMethod)originalConfigData.edgeFindingMethod;

            settings.doSizeToFit = originalConfigData.doSizeToFit;
            settings.doCropToBoundingBox = originalConfigData.doCropToBoundingBox;

            settings.postProcessingSettings = originalConfigData.postProcessingSettings;

            ResetSettingsUI();

            webCamTextureToMatHelper.Play();

            if (feedback != null)
                feedback.GetComponent<Image>().color = SUCCESS_COLOR;
                feedback.transform.GetChild(0).GetComponent<TMP_Text>().text = "Reset to defaults at " + DateTime.Now.ToString();
        }
        else
        {
            if (feedback != null)
                feedback.GetComponent<Image>().color = ERROR_COLOR;
                feedback.transform.GetChild(0).GetComponent<TMP_Text>().text = "Failed reset at " + DateTime.Now.ToString();
        }
    }

    public void OnSaveSettingsToJson()
    {
        if (webCamTextureToMatHelper != null)
        {
            CamConfigJSON originalConfigData = configLoader.configData;

            originalConfigData.defaultCamera  = webCamTextureToMatHelper.requestedDeviceName;
            originalConfigData.flipVertical = webCamTextureToMatHelper.flipVertical;
            originalConfigData.flipHorizontal = webCamTextureToMatHelper.flipHorizontal;

            originalConfigData.brightness = settings.brightness;
            originalConfigData.contrast = settings.contrast;

            originalConfigData.edgeFindingMethod = (int)settings.edgeFindingMethod;

            originalConfigData.doSizeToFit = settings.doSizeToFit;
            originalConfigData.doCropToBoundingBox = settings.doCropToBoundingBox;

            originalConfigData.postProcessingSettings = settings.postProcessingSettings;

            string json = JsonUtility.ToJson(originalConfigData, true);

            string filename = configLoader.contentFilename;
            string filepath = Path.Join(Application.streamingAssetsPath,filename);
            File.WriteAllText(filepath, json);

            resetButton.interactable = false;

            RLMGLogger.Instance.Log("Camera settings saved to JSON.", MESSAGETYPE.INFO);

            if (feedback != null)
                feedback.GetComponent<Image>().color = SUCCESS_COLOR;
                feedback.transform.GetChild(0).GetComponent<TMP_Text>().text = "Camera settings saved at " + DateTime.Now.ToString();
        }
        else
        {
            RLMGLogger.Instance.Log("Camera settings not saved!", MESSAGETYPE.ERROR);

            if (feedback != null)
                feedback.GetComponent<Image>().color = ERROR_COLOR;
                feedback.transform.GetChild(0).GetComponent<TMP_Text>().text = "Settings NOT saved at " + DateTime.Now.ToString();
        }
    }

    private bool AreSettingsChanged()
    {
        CamConfigJSON originalConfigData = configLoader.configData;
        return (
            webCamTextureToMatHelper.requestedDeviceName != originalConfigData.defaultCamera ||
            webCamTextureToMatHelper.flipVertical != originalConfigData.flipVertical ||
            webCamTextureToMatHelper.flipHorizontal != originalConfigData.flipHorizontal ||
            
            settings.brightness != originalConfigData.brightness ||
            settings.contrast != originalConfigData.contrast ||
            settings.edgeFindingMethod != (EdgeFindingMethod)originalConfigData.edgeFindingMethod ||
            settings.doCropToBoundingBox != originalConfigData.doCropToBoundingBox ||
            settings.doSizeToFit != originalConfigData.doSizeToFit ||

            settings.postProcessingSettings.brightness != originalConfigData.postProcessingSettings.brightness ||
            settings.postProcessingSettings.contrast != originalConfigData.postProcessingSettings.contrast
        );
    }

    /// <summary>
    /// Raises the play button click event.
    /// </summary>
    public void OnPlayButtonClick ()
    {
        webCamTextureToMatHelper.Play ();
    }

    /// <summary>
    /// Raises the pause button click event.
    /// </summary>
    public void OnPauseButtonClick ()
    {
       webCamTextureToMatHelper.Pause ();
    }

    /// <summary>
    /// Raises the stop button click event.
    /// </summary>
    public void OnStopButtonClick ()
    {
        webCamTextureToMatHelper.Stop ();
    }

    /// <summary>
    /// Raises the flip vertical button click event.
    /// </summary>
    public void OnFlipVertical (bool value)
    {
        webCamTextureToMatHelper.flipVertical = value;

        resetButton.interactable = AreSettingsChanged();
    }

    /// <summary>
    /// Raises the flip horizontal button click event.
    /// </summary>
    public void OnFlipHorizontal (bool value)
    {
        webCamTextureToMatHelper.flipHorizontal = value;

        resetButton.interactable = AreSettingsChanged();
    }

    public void OnEdgeFindingMethodToggleValueChanged(int method)
    {
        settings.edgeFindingMethod = (EdgeFindingMethod)method;

        resetButton.interactable = AreSettingsChanged();
    }

    public void OnBrightnessValueChanged()
    {
        settings.brightness = (int)brightnessSlider.value;

        resetButton.interactable = AreSettingsChanged();
    }

    public void OnContrastValueChanged()
    {
        settings.contrast = (int)contrastSlider.value;

        resetButton.interactable = AreSettingsChanged();
    }

    /// <summary>
    /// Raises the "do crop to bounding box" toggle value changed event.
    /// </summary>
    public void OnDoCropToBoundingBoxToggleValueChanged(bool value)
    {
        settings.doCropToBoundingBox = value;

        resetButton.interactable = AreSettingsChanged();
    }

    /// <summary>
    /// Raises the is final mode toggle value changed event.
    /// </summary>
    public void OnDoSizeToFitToggleValueChanged(bool value)
    {
        settings.doSizeToFit = value;

        resetButton.interactable = AreSettingsChanged();
    }

    public void OnPBrightnessValueChanged()
    {
        settings.postProcessingSettings.brightness = (int)pBrightnessSlider.value;

        resetButton.interactable = AreSettingsChanged();
    }

    public void OnPContrastValueChanged()
    {
        settings.postProcessingSettings.contrast = (int)pContrastSlider.value;

        resetButton.interactable = AreSettingsChanged();
    }

    /// <summary>
    /// Raises the is final mode toggle value changed event.
    /// </summary>
    public void OnDoWarpToggleValueChanged(bool value)
    {
        displayOptions.doWarp = value;

        //cannot draw paper edge after warping so disable...
        doDrawPaperEdgeToggle.interactable = !value;
        //conversely, cannot remove background without warping, so...
        doRemoveBackgroundToggle.interactable = value;
        doDrawMaxAreaContourToggle.interactable = value;
    }

    /// <summary>
    /// Raises the is edges mode toggle value changed event.
    /// </summary>
    public void OnShowEdgesToggleValueChanged(bool value)
    {
        displayOptions.showEdges = value;
    }

    /// <summary>
    /// Raises the is final mode toggle value changed event.
    /// </summary>
    public void OnDoRemoveBackgroundToggleValueChanged(bool value)
    {
        displayOptions.doRemoveBackground = value;
    }

    /// <summary>
    /// Raises the "draw paper edge" toggle value changed event.
    /// </summary>
    public void OnDoDrawPaperEdgeToggleValueChanged(bool value)
    {
        displayOptions.doDrawPaperEdge = value;
    }

    /// <summary>
    /// Raises the "do draw max area contour" toggle value changed event.
    /// </summary>
    public void OnDoDrawMaxAreaContourToggleValueChanged(bool value)
    {
        displayOptions.doDrawMaxAreaContour = value;
    }

}
