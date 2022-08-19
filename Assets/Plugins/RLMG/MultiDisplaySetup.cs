using System.Collections;
using System.Collections.Generic;
using rlmg.logging;
using UnityEngine;

public class MultiDisplaySetup : MonoBehaviour
{
    void Start()
    {
        RLMGLogger.Instance.Log("Found " + Display.displays.Length + " displays.", MESSAGETYPE.INFO);

        Screen.SetResolution(1920, 1080, true);

        Display.displays[0].Activate();
        Display.displays[0].SetParams(1920, 1080, 0, 0);

        if (Display.displays.Length > 1)
        {
            Display.displays[1].Activate();
            Display.displays[1].SetParams(1080, 1920, 0, 0);
        }
    }
}
