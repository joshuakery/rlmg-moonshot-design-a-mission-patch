using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ArtScan.CoreModule;
using rlmg.logging;

public class ProcessCounter : MonoBehaviour
{
    private int notUpdatedFramesCount = 0;
    private int updatedFramesCount = 0;
    private int helperUpdateCount = 0;
    private int nullCount = 0;

    [SerializeField]
    private AsynchronousRemoveBackground asynchronousRemoveBackground;

    private DateTime timeout;

    private void Awake()
    {
        if (asynchronousRemoveBackground == null)
            asynchronousRemoveBackground = FindObjectOfType<AsynchronousRemoveBackground>();
    }

    // Start is called before the first frame update
    void Start()
    {
        notUpdatedFramesCount = 0;
        updatedFramesCount = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (asynchronousRemoveBackground != null)
        {
            if (asynchronousRemoveBackground.webCamTextureToMatHelper.GetWebCamTexture() != null)
            {
                if (asynchronousRemoveBackground.webCamTextureToMatHelper.DidUpdateThisFrame())
                {
                    helperUpdateCount++;
                }

                if (asynchronousRemoveBackground.webCamTextureToMatHelper.GetWebCamTexture().didUpdateThisFrame)
                {
                    updatedFramesCount++;
                }
                else
                {
                    notUpdatedFramesCount++;
                }
            }
            else
            {
                nullCount++;
            }

            if (DateTime.Now > timeout)
            {
                timeout = DateTime.Now + TimeSpan.FromSeconds(10);
                RLMGLogger.Instance.Log(
                    System.String.Format(
                        "Not Updated Frames: {0}; Updated Frames: {1}; Copied Frames: {2}; WebCamTexture updateCount: {3}; Helper Update Count: {4}; WebCamTexture is Null Count: {5}",
                        notUpdatedFramesCount,
                        updatedFramesCount,
                        asynchronousRemoveBackground.copiedFramesCount,
                        asynchronousRemoveBackground.webCamTextureToMatHelper.GetWebCamTexture() != null ?
                            asynchronousRemoveBackground.webCamTextureToMatHelper.GetWebCamTexture().updateCount :
                            "WebCamTexture is null.",
                        helperUpdateCount,
                        nullCount
                    ),
                    MESSAGETYPE.INFO
                );
            }
        }

    }
}
