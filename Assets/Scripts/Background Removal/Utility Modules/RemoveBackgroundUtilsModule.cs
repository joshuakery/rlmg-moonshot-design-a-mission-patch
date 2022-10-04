using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using ArtScan.CoreModule;
using ArtScan.PerspectiveUtilsModule;

namespace ArtScan.RemoveBackgroundUtilsModule
{
    public static class RemoveBackgroundUtils
    {
        //Given an edge image
        //Finds the largest contour
        //and simply masks out src where it's not inside the contour boundaries
        public static Mat PolyfillMaskBackground(Mat src, Mat edgeImage, out MatOfPoint maxAreaContour)
        {
            // Find Largest Contour
            edgeImage.convertTo(edgeImage,CvType.CV_8UC1);
            List<MatOfPoint> contours = new List<MatOfPoint>();
            Mat hierarchy = new Mat();
            Imgproc.findContours(edgeImage, contours, hierarchy, Imgproc.RETR_EXTERNAL, Imgproc.CHAIN_APPROX_SIMPLE);
            maxAreaContour = PerspectiveUtils.GetMaxAreaContour(contours);

            //Create Mask
            Mat mask = OpenCVForUnity.CoreModule.Mat.zeros(src.height(),src.width(),CvType.CV_8UC1);
            Imgproc.fillPoly(mask,new List<MatOfPoint> { maxAreaContour }, new Scalar(255,255,255,255));

            //Remove Background
            Mat outputMat = OpenCVForUnity.CoreModule.Mat.zeros(src.height(),src.width(),src.type());
            Core.copyTo(src, outputMat, mask);

            return outputMat;
        }

        public static Mat PolyfillMaskBackground(Mat src, Mat edgeImage, out MatOfPoint maxAreaContour, out Mat mask)
        {
            // Find Largest Contour
            edgeImage.convertTo(edgeImage,CvType.CV_8UC1);
            List<MatOfPoint> contours = new List<MatOfPoint>();
            Mat hierarchy = new Mat();
            Imgproc.findContours(edgeImage, contours, hierarchy, Imgproc.RETR_EXTERNAL, Imgproc.CHAIN_APPROX_SIMPLE);
            maxAreaContour = PerspectiveUtils.GetMaxAreaContour(contours);

            //Create Mask
            mask = OpenCVForUnity.CoreModule.Mat.zeros(src.height(),src.width(),CvType.CV_8UC1);
            Imgproc.fillPoly(mask,new List<MatOfPoint> { maxAreaContour }, new Scalar(255,255,255,255));

            //Remove Background
            Mat outputMat = OpenCVForUnity.CoreModule.Mat.zeros(src.height(),src.width(),src.type());
            Core.copyTo(src, outputMat, mask);

            return outputMat;
        }

        //Helper function used on output of grabcut
        private static void convertToGrayScaleValues (Mat mask)
        {
            int width = mask.rows ();
            int height = mask.cols ();
            byte[] buffer = new byte[width * height];
            mask.get (0, 0, buffer);
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    int value = buffer [y * width + x];

                    if (value == Imgproc.GC_BGD) {
                        buffer [y * width + x] = 0; // for sure background
                    } else if (value == Imgproc.GC_PR_BGD) {
                        buffer [y * width + x] = 85; // probably background
                    } else if (value == Imgproc.GC_PR_FGD) {
                        buffer [y * width + x] = (byte)170; // probably foreground
                    } else {
                        buffer [y * width + x] = (byte)255; // for sure foreground
                    }
                }
            }
            mask.put (0, 0, buffer);
        }

        public static Mat GrabcutMaskBackground(Mat src, Mat edges, out MatOfPoint maxAreaContour)
        {
            // Find Largest Contour
            edges.convertTo(edges,CvType.CV_8UC1);
            List<MatOfPoint> contours = new List<MatOfPoint>();
            Mat hierarchy = new Mat();
            Imgproc.findContours(edges, contours, hierarchy, Imgproc.RETR_EXTERNAL, Imgproc.CHAIN_APPROX_SIMPLE);
            maxAreaContour = PerspectiveUtils.GetMaxAreaContour(contours);

            //Create Mask
            Mat mask = OpenCVForUnity.CoreModule.Mat.zeros(src.height(),src.width(),CvType.CV_8UC1);
            Imgproc.fillPoly(mask,new List<MatOfPoint> { maxAreaContour }, new Scalar(255,255,255,255));

            //Erode mask to create a "sure foreground" area
            Mat sureFg = new Mat();
            Imgproc.erode(mask, sureFg, OpenCVForUnity.CoreModule.Mat.ones(5,5,CvType.CV_8UC1), new Point(-1.0,-1.0), 10);

            //Build trimap for grabcut. Start with everything as sure background...
            Mat trimap = new Mat(src.height(), src.width(), CvType.CV_8UC1, new Scalar(Imgproc.GC_BGD));
            //Use fill poly mask as probable foreground...
            trimap.setTo(new Scalar(Imgproc.GC_PR_FGD), mask);
            //Use eroded fill poly mask as sure foreground.
            trimap.setTo(new Scalar(Imgproc.GC_FGD), sureFg);

            Mat converted = new Mat();
            Imgproc.cvtColor(src, converted, Imgproc.COLOR_RGBA2RGB);

            //Grabcut
            OpenCVForUnity.CoreModule.Rect rectangle = new OpenCVForUnity.CoreModule.Rect (0, 0, converted.cols () - 1, converted.rows () - 1);
            Mat bgdModel = new Mat (); // extracted features for background
            Mat fgdModel = new Mat (); // extracted features for foreground       
            int iterCount = 5;

            Imgproc.grabCut (converted, trimap, rectangle, bgdModel, fgdModel, iterCount, Imgproc.GC_INIT_WITH_MASK);

            convertToGrayScaleValues (trimap); // back to grayscale values
            Imgproc.threshold (trimap, trimap, 128, 255, Imgproc.THRESH_TOZERO);

            //Remove Background
            Mat outputMat = OpenCVForUnity.CoreModule.Mat.zeros(src.height(),src.width(),src.type());
            Core.copyTo(src, outputMat, trimap);

            return outputMat;
        }

        public static IEnumerator GrabCutCoroutine(Mat src, Mat edges, Mat target, GrabcutRemovalThread thread, MatOfPoint maxAreaContourDest)
        {
            // Find Largest Contour
            edges.convertTo(edges,CvType.CV_8UC1);
            List<MatOfPoint> contours = new List<MatOfPoint>();
            Mat hierarchy = new Mat();
            Imgproc.findContours(edges, contours, hierarchy, Imgproc.RETR_EXTERNAL, Imgproc.CHAIN_APPROX_SIMPLE);
            MatOfPoint maxAreaContour = PerspectiveUtils.GetMaxAreaContour(contours);

            //Create Mask
            Mat mask = OpenCVForUnity.CoreModule.Mat.zeros(src.height(),src.width(),CvType.CV_8UC1);
            Imgproc.fillPoly(mask,new List<MatOfPoint> { maxAreaContour }, new Scalar(255,255,255,255));

            //Erode mask to create a "sure foreground" area
            Mat sureFg = new Mat();
            Imgproc.erode(mask, sureFg, OpenCVForUnity.CoreModule.Mat.ones(5,5,CvType.CV_8UC1), new Point(-1.0,-1.0), 10);

            //Build trimap for grabcut. Start with everything as sure background...
            Mat trimap = new Mat(src.height(), src.width(), CvType.CV_8UC1, new Scalar(Imgproc.GC_BGD));
            //Use fill poly mask as probable foreground...
            trimap.setTo(new Scalar(Imgproc.GC_PR_FGD), mask);
            //Use eroded fill poly mask as sure foreground.
            trimap.setTo(new Scalar(Imgproc.GC_FGD), sureFg);

            Mat converted = new Mat();
            Imgproc.cvtColor(src, converted, Imgproc.COLOR_RGBA2RGB);

            //Grabcut
            OpenCVForUnity.CoreModule.Rect rectangle = new OpenCVForUnity.CoreModule.Rect (0, 0, converted.cols () - 1, converted.rows () - 1);
            Mat bgdModel = new Mat (); // extracted features for background
            Mat fgdModel = new Mat (); // extracted features for foreground       
            int iterCount = 5;

            thread = new GrabcutRemovalThread();
            thread.converted = converted;
            thread.trimap = trimap;
            thread.rectangle = rectangle;
            thread.bgdModel = bgdModel;
            thread.fgdModel = fgdModel;
            thread.iterCount = iterCount;
            thread.Start();
            
            while (!thread.Update())
            {
                yield return null;
            }

            convertToGrayScaleValues (trimap); // back to grayscale values
            Imgproc.threshold (trimap, trimap, 128, 255, Imgproc.THRESH_TOZERO);

            //Remove Background
            Core.copyTo(src, target, trimap);

            //Output maxAreaContourDest
            maxAreaContour.copyTo(maxAreaContourDest);

        }
    }
}
