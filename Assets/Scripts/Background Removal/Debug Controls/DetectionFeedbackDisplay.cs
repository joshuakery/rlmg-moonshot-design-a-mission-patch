using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using OpenCVForUnity.UnityUtils.Helper;

namespace ArtScan.CoreModule
{
    public class DetectionFeedbackDisplay : MonoBehaviour
    {
        public myWebCamTextureToMatHelper myWebCamTextureToMatHelper;
        public AsynchronousRemoveBackground asynchronousRemoveBackground;

        private Canvas canvas;
        private TMP_Text message;

        // Start is called before the first frame update
        void Start()
        {
            if (canvas == null)
                canvas = GetComponent<Canvas>();

            if (message == null)
                message = GetComponentInChildren<TMP_Text>();
        }

        // Update is called once per frame
        void Update()
        {
            canvas.enabled = false;

            if (canvas != null && message != null &&
                myWebCamTextureToMatHelper != null &&
                myWebCamTextureToMatHelper.IsPlaying()
                )
            {
                if (asynchronousRemoveBackground.paperFound)
                {
                    if (asynchronousRemoveBackground.consistentPaperFound &&
                        asynchronousRemoveBackground.consistentPaperArea)
                    {
                        if (!asynchronousRemoveBackground.consistentRunningAverage)
                        {
                            canvas.enabled = true;
                            message.text = "Looking for artwork...";
                        }
                    }
                    else
                    {
                        canvas.enabled = true;
                        message.text = "Detecting paper...";
                    }
                }
                else
                {
                    canvas.enabled = true;
                    message.text = "Looking for paper...";
                }
            }
        }
    }
}


