using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils.Helper;
using OpenCVForUnity.UnityUtils;
using ArtScan.PerspectiveUtilsModule;

namespace ArtScan.PresentationUtilsModule
{
    public static class PresentationUtils
    {
        static Scalar PAPER_EDGE_COLOR = new Scalar(0,255,0,255);
        static Scalar CONTOUR_COLOR = new Scalar(255, 0, 0, 255);
        static Scalar DEBUG_CONTOUR_COLOR = new Scalar(255, 255, 0, 255);

        public static void ShowEdges(Mat edgeMat, Mat dest)
        {
            using (Mat sobelEdgeVisualized = new Mat())
            {
                edgeMat.convertTo(sobelEdgeVisualized, CvType.CV_8UC4);
                Imgproc.resize(sobelEdgeVisualized, sobelEdgeVisualized, new Size(), dest.width() / edgeMat.width(), dest.width() / edgeMat.width(), Imgproc.INTER_LINEAR);
                Imgproc.cvtColor(sobelEdgeVisualized, dest, Imgproc.COLOR_RGB2RGBA);
            }
        }

        //Sizing is done after reading the image, in order to preserve quality
        public static void MakeReadyToPresent(Mat src, Mat dest, bool doCropToBoundingBox, bool doSizeToFit)
        {
            if (doCropToBoundingBox)
            {
                List<Mat> planes = new List<Mat>();
                Core.split(src,planes);

                Mat alpha = planes[3];

                OpenCVForUnity.CoreModule.Rect roi = Imgproc.boundingRect(alpha);

                using (Mat croppedMat = new Mat(src, roi))
                {
                    ScaleUpAndDisplayMat(croppedMat, dest, doSizeToFit);
                }
            }
            else
            {
                ScaleUpAndDisplayMat(src,dest,doSizeToFit);
            }
        }

        public static void MakeReadyToPresent(Mat src, Mat dest, MatOfPoint maxAreaContour, RemoveBackgroundDisplayOptions displayOptions, RemoveBackgroundSettings settings)
        {
            dest.setTo(new Scalar(0, 0, 0, 0));

            if (displayOptions.doDrawMaxAreaContour)
            {
                OpenCVForUnity.CoreModule.Rect roi = Imgproc.boundingRect(maxAreaContour);
                Imgproc.rectangle(src, new Point(roi.x, roi.y), new Point(roi.x + roi.width, roi.y + roi.height), DEBUG_CONTOUR_COLOR, 5);
                Imgproc.drawContours(src, new List<MatOfPoint> { maxAreaContour }, -1, CONTOUR_COLOR, 2);
            }

            if (settings.doCropToBoundingBox)
            {
                OpenCVForUnity.CoreModule.Rect roi = Imgproc.boundingRect(maxAreaContour);
                if (roi.area() > 0)
                {
                    using (Mat croppedMat = new Mat(src, roi))
                    {
                        ScaleUpAndDisplayMat(croppedMat, dest, settings.doSizeToFit);
                    }
                }
                else
                {
                    ScaleUpAndDisplayMat(src, dest, settings.doSizeToFit);
                }
            }
            else
            {
                ScaleUpAndDisplayMat(src, dest, settings.doSizeToFit);
            }

            // Post Processing Effects
            //if (settings.postProcessingSettings != null)
            //{
            //    PerspectiveUtils.BrightnessContrast(dest, settings.postProcessingSettings.brightness, settings.postProcessingSettings.contrast);
            //}
        }

        public static void ScaleUpAndDisplayMat(Mat src, Mat outputDisplayAreaMat, bool doSizeToFit)
        {
            if (src.height() == 0 || src.width() == 0)
                return;

            int nh, nw;
            if (doSizeToFit)
            {
                if (src.height() > src.width())
                {
                    nh = outputDisplayAreaMat.height();
                    nw = (int)( outputDisplayAreaMat.height() * ( (float)src.width() / (float)src.height() ) );
                }
                else
                {
                    nw = outputDisplayAreaMat.width();
                    nh = (int)( outputDisplayAreaMat.width() * ( (float)src.height() / (float)src.width() ) );
                }
            }
            else
            {
                if (src.height() < src.width())
                {
                    nh = outputDisplayAreaMat.height();
                    nw = (int)( outputDisplayAreaMat.height() * ( (float)src.width() / (float)src.height() ) );
                }
                else
                {
                    nw = outputDisplayAreaMat.width();
                    nh = (int)( outputDisplayAreaMat.width() * ( (float)src.height() / (float)src.width() ) );
                }
            }

            using (Mat resized = new Mat(nw,nh, src.type(), new Scalar(0, 0, 0, 0)))
            {
                Imgproc.resize(src, resized, new Size(nw,nh) );

                int w = Mathf.Min(resized.width(),outputDisplayAreaMat.width());
                int h = Mathf.Min(resized.height(),outputDisplayAreaMat.height());
                int sx = (int)(resized.width() / 2 - w/2 );
                int sy = (int)(resized.height() / 2 - h/2 );
                int tx = (int)(outputDisplayAreaMat.width() / 2 - w/2);
                int ty = (int)(outputDisplayAreaMat.height() / 2 - h/2);

                using (Mat sourceWindow = new Mat(resized, new OpenCVForUnity.CoreModule.Rect(sx,sy,w,h)),
                           targetWindow = new Mat(outputDisplayAreaMat, new OpenCVForUnity.CoreModule.Rect(tx,ty,w,h)) )
                {
                    sourceWindow.copyTo(targetWindow);
                }
                 
            }
        }

        //Convenience function to copy src to the given section of displayMat, outputDisplayAreaMat
        public static void DisplayMat(Mat src, Mat outputDisplayAreaMat)
        {
            outputDisplayAreaMat.setTo(new Scalar(0, 0, 0, 255));

            if (src.width() <= outputDisplayAreaMat.width() && src.height() <= outputDisplayAreaMat.height()
                && src.total() >= outputDisplayAreaMat.total() / 16)
            {
                int x = outputDisplayAreaMat.width() / 2 - src.width() / 2;
                int y = outputDisplayAreaMat.height() / 2 - src.height() / 2;
                using (Mat dstAreaMat = new Mat(outputDisplayAreaMat, new OpenCVForUnity.CoreModule.Rect(x, y, src.width(), src.height())))
                {
                    src.copyTo(dstAreaMat);
                }
            }
        }
    }
}
