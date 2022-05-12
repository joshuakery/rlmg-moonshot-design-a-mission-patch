using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ArtScan.CoreModule;

[CreateAssetMenu(menuName = "Remove Background/Display Options"), System.Serializable]
public class RemoveBackgroundDisplayOptions : ScriptableObject
{
    /// <summary>
    /// Determines whether to draw paper edge.
    /// </summary>
    public bool doDrawPaperEdge;

    /// <summary>
    /// Determines what mode to present output as. Without warping the paper so it meets the edges of the image, the largest contour would just be the paper so the rest of the background removal process can't be completed.
    /// </summary>
    public bool doWarp;

    /// <summary>
    /// Determines what mode to present output as.
    /// </summary>
    public bool showEdges;

    /// <summary>
    /// Determines what mode to present output as.
    /// </summary>
    public bool doRemoveBackground;

    /// <summary>
    /// Determines whether to draw max area contour.
    /// </summary>
    public bool doDrawMaxAreaContour;

}


