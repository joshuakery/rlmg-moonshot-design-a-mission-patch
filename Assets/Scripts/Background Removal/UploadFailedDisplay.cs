using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ArtScan.ScanSavingModule
{
    public class UploadFailedDisplay : MonoBehaviour
    {
        private Canvas canvas;

        private bool doShowDisplay;

        private void Awake()
        {
            canvas = GetComponent<Canvas>();
        }

        private void OnEnable()
        {
            ClientSend.onUploadFailed += ShowDisplay;
        }

        private void OnDisable()
        {
            ClientSend.onUploadFailed -= ShowDisplay;
        }

        private void Update()
        {
            if (doShowDisplay)
            {
                if (canvas != null)
                    canvas.enabled = true;

                doShowDisplay = false;
            }

            if (Input.GetKeyDown(KeyCode.V))
            {
                doShowDisplay = true;
            }
        }

        private void ShowDisplay(string msg)
        {
            Debug.Log("show display");
            doShowDisplay = true;
        }
    }
}


