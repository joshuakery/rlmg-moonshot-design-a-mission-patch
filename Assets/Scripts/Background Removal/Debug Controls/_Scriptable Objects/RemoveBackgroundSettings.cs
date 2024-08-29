using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ArtScan.CoreModule;

[CreateAssetMenu(menuName = "Remove Background/Settings"), System.Serializable]
public class RemoveBackgroundSettings : ScriptableObject
{
    [System.Serializable]
    public class PostProcessingSettings
    {
        public int brightness = 255;
        public int contrast = 127;
    }

    [System.Serializable]
    public class EnableBeginScanButtonSettings
    {
        public int minimumPaperFoundFrames = 2;
        public int minimumPaperNotFoundFramesToFail = 2;

        public float consistentPaperAreaThreshold = 0.5f;
        public int minimumPaperConsistentFrames = 2;

        public int minimumPaperInconsistentFramesToFail = 2;

        public int minimumScanSize = 25;
        public float maximumArtworkPercentageOfPaper = 0.9f;

        public float consistentScanAreaThreshold = 0.5f;
        public int minimumConsistentFrames = 2;

        public int minimumArtworkInconsistentFramesToFail = 2;
    }

    public string saveDir = "SavedScans";
    public string trashDir = "TrashedScans";

    public bool clearSaveDirOnQuit = false;
    public bool clearTrashDirOnQuit = false;

    /// <summary>
    /// Whether or not to crop to bounding box before saving image
    /// Does not affect doCropToBoundingBox
    /// </summary>
    public bool doSaveCroppedToBoundingBox = false;

    public int targetWidth = 750;
    public int targetHeight = 750;

    public int preProcessingSizeFactor = 2;
    public int postProcessingSizeFactor = 2;

    public int brightness = 255;
    public int contrast = 127;

    public EdgeFindingMethod edgeFindingMethod = EdgeFindingMethod.Sobel;

    public int cannyThreshold1 = 100;
    public int cannyThreshold2 = 200;
    public int cannyApertureSize = 3;

    /// <summary>
    /// Determines whether to crop to bounding box of drawing and scale up.
    /// Does not affect doSaveCroppedToBoundingBox
    /// </summary>
    public bool doCropToBoundingBox = true;

    /// <summary>
    /// Determines how the image is ScaleUpAndDisplay'ed.
    /// </summary>
    public bool doSizeToFit = true;

    /// Post Processing
    public PostProcessingSettings postProcessingSettings;

    public EnableBeginScanButtonSettings enableBeginScanButtonSettings;

}


