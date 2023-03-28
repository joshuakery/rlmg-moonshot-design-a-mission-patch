using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OpenCVForUnity.UnityUtils.Helper;

public class CameraFeedWindow : MonoBehaviour
{
    [SerializeField]
    private RawImage ri;

    [SerializeField]
    private myWebCamTextureToMatHelper webCamTextureToMatHelper;

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
