using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectionErrorScreen : MonoBehaviour
{
    public GameObject targetScreen;

    private void Start()
    {
        if (Client.instance != null)
        {
            Client.instance.onDisconnect += OnConnectionError;
        }

        if (targetScreen != null)
        {
            targetScreen.SetActive(false);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            targetScreen.SetActive(!targetScreen.activeSelf);
        }
    }

    void OnConnectionError()
    {
        if (targetScreen != null)
        {
            targetScreen.SetActive(true);
        }
    }

}
