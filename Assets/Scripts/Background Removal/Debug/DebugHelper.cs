using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ArtScan.CoreModule;
using ArtScan.ErrorDisplayModule;

public class DebugHelper : MonoBehaviour
{
    private AsynchronousRemoveBackground asynchronousRemoveBackground;
    private CameraDisconnectHandler cameraDisconnectHandler;

    // Start is called before the first frame update
    void Start()
    {
        if (asynchronousRemoveBackground == null)
            asynchronousRemoveBackground = FindObjectOfType<AsynchronousRemoveBackground>();

        if (cameraDisconnectHandler == null)
            cameraDisconnectHandler = FindObjectOfType<CameraDisconnectHandler>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            if (asynchronousRemoveBackground != null)
                asynchronousRemoveBackground.SetupRemoveBackground();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (cameraDisconnectHandler != null)
                cameraDisconnectHandler.SetCanCheckDevices(true);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            if (cameraDisconnectHandler != null)
                cameraDisconnectHandler.DoReInitialize();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (asynchronousRemoveBackground != null)
            {
                asynchronousRemoveBackground.doProcessImage = !asynchronousRemoveBackground.doProcessImage;
            }
        }

    }
}
