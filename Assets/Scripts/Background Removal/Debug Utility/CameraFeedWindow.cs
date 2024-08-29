using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OpenCVForUnity.UnityUtils.Helper;

public class CameraFeedWindow : MonoBehaviour
{
    [SerializeField]
    private RawImage ri;

    private myWebCamTextureToMatHelper webCamTextureToMatHelper;

    private void Awake()
    {
        webCamTextureToMatHelper = FindObjectOfType<myWebCamTextureToMatHelper>();
    }

    // Update is called once per frame
    void Update()
    {
        if (ri == null) return;

        if (webCamTextureToMatHelper != null)
            ri.texture = webCamTextureToMatHelper.GetWebCamTexture();
        else
            ri.texture = null;
    }
}
