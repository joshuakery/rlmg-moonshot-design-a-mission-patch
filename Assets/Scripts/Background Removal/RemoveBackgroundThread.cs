// using UnityEngine;
// using UnityEngine.SceneManagement;
// using UnityEngine.UI;
// using System;
// using System.Collections;
// using System.Collections.Generic;
// using System.Threading;
// using OpenCVForUnity.CoreModule;
// using OpenCVForUnity.ImgprocModule;
// using OpenCVForUnity.UnityUtils.Helper;
// using OpenCVForUnity.UnityUtils;
// using ArtScan.EdgeFindingModule;
// using ArtScan.PerspectiveUtilsModule;
// using ArtScan.RemoveBackgroundUtilsModule;
// using ArtScan;
// using ArtScan.CoreModule;

// public class RemoveBackgroundThread: MultiThreading.ThreadedJob {

//     public bool IsActive { get; set; }

//     public removeBackground removeBackground;

//     protected override void ThreadFunction()
//     {
//         // IsActive = true;

//         myWebCamTextureToMatHelper webCamTextureToMatHelper = removeBackground.webCamTextureToMatHelper;

//         if (removeBackground.webCamTextureReady &&
//             removeBackground.rgbaMat != null)
//         {
//             using (Mat displayMat = new Mat( removeBackground.rawImageDisplayMat.height(), removeBackground.rawImageDisplayMat.width(), removeBackground.rawImageDisplayMat.type(), new Scalar(0,0,0,0)) )
//             {
//                 PerspectiveUtils.BrightnessContrast(removeBackground.rgbaMat,removeBackground.brightness,removeBackground.contrast);

//                 removeBackground.yMat = EdgeFinding.GetCannyEdgeMat(removeBackground.rgbaMat);

//                 // find potential paper contours.
//                 List<MatOfPoint> contours = new List<MatOfPoint>();
//                 PerspectiveUtils.Find4PointContours(removeBackground.yMat, contours);

//                 // pick the contour of the largest area and rearrange the points in a consistent order.
//                 MatOfPoint paperMaxAreaContour = PerspectiveUtils.GetMaxAreaContour(contours);
//                 paperMaxAreaContour = PerspectiveUtils.OrderCornerPoints(paperMaxAreaContour);

//                 bool paperFound = (paperMaxAreaContour.size().area() > 0);
//                 if (paperFound && removeBackground.doWarp)
//                 {
//                     // transform the perspective of original image.
//                     removeBackground.transformedMat = PerspectiveUtils.PerspectiveTransform(removeBackground.rgbaMat, paperMaxAreaContour);
//                     if (removeBackground.transformedMat.width() > 1 && removeBackground.transformedMat.height() > 1)
//                     {
//                         //crop in the edges to hide them from the find largest contour later
//                         // PerspectiveUtils.CropByPercent(transformedMat, transformedMat, 1.0f);

//                         removeBackground.SetEdgeMat();
                        
//                         if (removeBackground.showEdges)
//                             removeBackground.ShowEdges();

//                         using (Mat removedMat = RemoveBackgroundUtils.PolyfillMaskBackground(removeBackground.transformedMat,removeBackground.edgeMat, out MatOfPoint maxAreaContour))
//                         {
//                             if (removeBackground.doRemoveBackground)
//                             {
//                                 removeBackground.MakeReadyToPresent(removedMat,displayMat,maxAreaContour);
//                             }
//                             else
//                             {
//                                 removeBackground.MakeReadyToPresent(removeBackground.transformedMat,displayMat,maxAreaContour);
//                             }
//                         }
//                     }
                    
//                 }
//                 else //not paperFound and/or doing warp
//                 {
//                     displayMat.setTo( new Scalar(0,0,0,0) );

//                     if (removeBackground.showEdges) //yMat is now Canny edges, see above
//                         Imgproc.cvtColor(removeBackground.yMat, removeBackground.rgbaMat, Imgproc.COLOR_GRAY2RGBA);

//                     if (removeBackground.doDrawPaperEdge)
//                         Imgproc.drawContours(removeBackground.rgbaMat,new List<MatOfPoint> {paperMaxAreaContour}, -1, removeBackground.PAPER_EDGE_COLOR, 4);

//                     removeBackground.ScaleUpAndDisplayMat(removeBackground.rgbaMat,displayMat);
//                 }

//                 displayMat.copyTo(removeBackground.rawImageDisplayMat);
                
//             }

//         }

//         // while (IsActive)
//         // {
//         //     try
//         //     {
                
//         //     }
//         //     catch (ThreadAbortException)
//         //     {
//         //         IsActive = false;
//         //     }
//         //     catch (Exception e)
//         //     {
//         //         Debug.LogException(e);
//         //     }

//         // }

//     }

// }