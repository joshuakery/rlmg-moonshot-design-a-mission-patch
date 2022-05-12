using System.Collections;
using System.Collections.Generic;
using rlmg.logging;
using UnityEngine;

public class AppSetup : MonoBehaviour 
{
    public bool setResolution = true;
    public int targetScreenWidth = 1920;
    public int targetScreenHeight = 1080;

    void Start()
    {
        RLMGLogger.Instance.Log("Application Awake - version #" + Application.version, MESSAGETYPE.INFO);

        if (!Application.isEditor)
            Cursor.visible = false;

        if (setResolution)
            Screen.SetResolution(targetScreenWidth, targetScreenHeight, true);
    }

    void Update()
    {
        if (Input.GetKeyDown("escape"))
        {
            RLMGLogger.Instance.Log("Quit application via 'Escape' key.", MESSAGETYPE.INFO);

            Application.Quit();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Cursor.visible = !Cursor.visible;
        }
    }

    void OnApplicationQuit()
    {
        RLMGLogger.Instance.Log("Application Quit", MESSAGETYPE.INFO);
    }
}
