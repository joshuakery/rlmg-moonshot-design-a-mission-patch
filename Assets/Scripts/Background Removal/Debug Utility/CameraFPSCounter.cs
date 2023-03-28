using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCVForUnity.UnityUtils.Helper;
using TMPro;

public class CameraFPSCounter : MonoBehaviour
{
    [SerializeField]
    private TMP_Text display;

    [SerializeField]
    private myWebCamTextureToMatHelper webCamTextureToMatHelper;

    private DateTime timeout;

    private int countThisSecond = 0;

    private void Awake()
    {
        if (display == null)
            display = GetComponent<TMP_Text>();

        if (webCamTextureToMatHelper == null)
            webCamTextureToMatHelper = FindObjectOfType<myWebCamTextureToMatHelper>();
    }

    // Start is called before the first frame update
    void Start()
    {
        timeout = DateTime.Now + TimeSpan.FromSeconds(1f);
    }

    // Update is called once per frame
    void Update()
    {
        if (webCamTextureToMatHelper != null && webCamTextureToMatHelper.IsInitialized())
        {
            WebCamTexture webCamTexture = webCamTextureToMatHelper.GetWebCamTexture();
            if (webCamTexture != null)
                if (webCamTexture.didUpdateThisFrame)
                    countThisSecond++;
        }

        if (DateTime.Now > timeout)
        {
            timeout = DateTime.Now + TimeSpan.FromSeconds(1f);

            display.text = countThisSecond.ToString();
            countThisSecond = 0;
        }
    }
}
