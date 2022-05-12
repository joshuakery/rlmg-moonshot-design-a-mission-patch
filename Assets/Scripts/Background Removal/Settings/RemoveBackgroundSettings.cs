using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ArtScan.CoreModule;

[CreateAssetMenu(menuName = "Remove Background/Settings"), System.Serializable]
public class RemoveBackgroundSettings : ScriptableObject
{
    public string saveDir = "SavedScans";
    public string trashDir = "TrashedScans";

    
    public int targetWidth = 750;
    public int targetHeight = 750;

    public int brightness = 255;
    public int contrast = 127;

    public EdgeFindingMethod edgeFindingMethod = EdgeFindingMethod.Sobel;

    public int cannyThreshold1 = 100;
    public int cannyThreshold2 = 200;
    public int cannyApertureSize = 3;

    /// <summary>
    /// Determines whether to crop to bounding box of drawing and scale up.
    /// </summary>
    public bool doCropToBoundingBox = true;

    /// <summary>
    /// Determines how the image is ScaleUpAndDisplay'ed.
    /// </summary>
    public bool doSizeToFit = true;

}


