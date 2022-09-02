using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.XimgprocModule;
using OpenCVForUnity.UnityUtils.Helper;
using OpenCVForUnity.UnityUtils;
using ArtScan.CoreModule;
using ArtScan;
using rlmg.logging;
using ArtScan.EdgeFindingModule;
using ArtScan.PerspectiveUtilsModule;
using ArtScan.RemoveBackgroundUtilsModule;
using ArtScan.PresentationUtilsModule;

public class RefinedScanThread: MultiThreading.ThreadedJob {

    //parameters
    public RemoveBackgroundSettings settings;
    public RemoveBackgroundDisplayOptions displayOptions;
    public StructuredEdgeDetection edgeDetection;

    //input
    public Mat rgbaMat;

    //outputs
    public Mat unscaledMat;
    public Mat displayMat;

    Scalar PAPER_EDGE_COLOR = new Scalar(0,255,0,255);

    protected override void ThreadFunction()
    {
        if (rgbaMat != null)
        {
            PerspectiveUtils.BrightnessContrast(rgbaMat,settings.brightness,settings.contrast);

            using (Mat yMat = new Mat())
            {
                EdgeFinding.GetCannyEdgeMat(rgbaMat,yMat);

                // find potential paper contours.
                List<MatOfPoint> contours = new List<MatOfPoint>();
                PerspectiveUtils.Find4PointContours(yMat, contours);

                // pick the contour of the largest area and rearrange the points in a consistent order.
                MatOfPoint paperMaxAreaContour = PerspectiveUtils.GetMaxAreaContour(contours);
                paperMaxAreaContour = PerspectiveUtils.OrderCornerPoints(paperMaxAreaContour);

                bool paperFound = (paperMaxAreaContour.size().area() > 0);
                if (paperFound && displayOptions.doWarp)
                {
                    // transform the perspective of original image.
                    using (Mat transformedMat = PerspectiveUtils.PerspectiveTransform(rgbaMat, paperMaxAreaContour),
                               edgeMat = new Mat()
                    )
                    {
                        if (transformedMat.width() > 1 && transformedMat.height() > 1)
                        {
                            EdgeFinding.SetEdgeMat(transformedMat,edgeMat,settings,edgeDetection);
                            
                            if (displayOptions.showEdges)
                                PresentationUtils.ShowEdges(edgeMat,transformedMat);

                            using (Mat removedMat = RemoveBackgroundUtils.GrabcutMaskBackground(transformedMat,edgeMat, out MatOfPoint maxAreaContour))
                            {
                                //unscaledMat will later be copied to the RefinedScanController's previewMat
                                if (settings.doSaveCroppedToBoundingBox)
                                {
                                    OpenCVForUnity.CoreModule.Rect roi = Imgproc.boundingRect(maxAreaContour);
                                    if (roi.area() > 0)
                                    {
                                        using (Mat croppedMat = new Mat(removedMat, roi))
                                        {
                                            PerspectiveUtils.BrightnessContrast(croppedMat, settings.postProcessingSettings.brightness, settings.postProcessingSettings.contrast, true);
                                            croppedMat.copyTo(unscaledMat);
                                        }
                                    }
                                    else
                                    {
                                        PerspectiveUtils.BrightnessContrast(removedMat, settings.postProcessingSettings.brightness, settings.postProcessingSettings.contrast, true);
                                        removedMat.copyTo(unscaledMat);
                                    }
                                }
                                else
                                {
                                    PerspectiveUtils.BrightnessContrast(removedMat, settings.postProcessingSettings.brightness, settings.postProcessingSettings.contrast, true);
                                    removedMat.copyTo(unscaledMat);
                                }

                                //displayMat will be simply converted to Texture2D
                                if (displayOptions.doRemoveBackground)
                                {
                                    PresentationUtils.MakeReadyToPresent(
                                        removedMat, displayMat,
                                        maxAreaContour,
                                        displayOptions, settings
                                    );
                                }
                                else
                                {
                                    PresentationUtils.MakeReadyToPresent(
                                        transformedMat, displayMat,
                                        maxAreaContour,
                                        displayOptions, settings
                                    );
                                }
                            }
                        }
                    }
                }
                else //not paperFound and/or doing warp
                {
                    displayMat.setTo( new Scalar(0,0,0,0) );

                    if (displayOptions.showEdges) //yMat is now Canny edges, see above
                        Imgproc.cvtColor(yMat, rgbaMat, Imgproc.COLOR_GRAY2RGBA);

                    if (displayOptions.doDrawPaperEdge)
                        Imgproc.drawContours(rgbaMat,new List<MatOfPoint> {paperMaxAreaContour}, -1, PAPER_EDGE_COLOR, 4);

                    rgbaMat.copyTo(unscaledMat);

                    PresentationUtils.ScaleUpAndDisplayMat(
                        rgbaMat,displayMat,
                        settings.doSizeToFit
                    );
                }
            }
        }
        else
        {
            Debug.Log("No input!");
        }
    }

}