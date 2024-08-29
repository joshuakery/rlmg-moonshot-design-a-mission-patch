using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
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
        if (Input.GetKeyDown(KeyCode.Alpha9))
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
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            if (cameraDisconnectHandler != null)
                cameraDisconnectHandler.SaveCurrentAndLastWebCamTexturesToFile();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            SceneManager.LoadScene("Debug_RefinedScan");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            SceneManager.LoadScene("Debug_LiveFeedback");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            SceneManager.LoadScene("Debug_LiveFeedback_RefinedScan");
        }

    }
}
