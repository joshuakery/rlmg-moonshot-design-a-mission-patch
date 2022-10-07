using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectionFailedErrorDisplay : MonoBehaviour
{
    public GameObject errorDisplay;

    private static MoonshotActivity thisActivity;
    
    void Start()
    {
        if (Client.instance != null)
        {
            Client.instance.onDisconnect += OnConnectionError;
        }

        if (thisActivity == null)
        {
            thisActivity = (MoonshotActivity)FindObjectOfType(typeof(MoonshotActivity));
        }

        if (errorDisplay != null)
        {
            errorDisplay.SetActive(false);
        }
    }

    void OnConnectionError()
    {
        //if (errorDisplay != null)
        if (errorDisplay != null && (thisActivity == null || thisActivity.useServer))
        {
            errorDisplay.SetActive(true);
        }
    }
}
