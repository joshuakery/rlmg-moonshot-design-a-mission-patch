using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils.Helper;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.XimgprocModule;
using ArtScan.CoreModule;

namespace ArtScan.EdgeFindingModule
{
    public static class EdgeFinding
    {
        public static void SetEdgeMat(Mat src, Mat dest, EdgeFindingMethod method)
        {
            using (Mat edges = new Mat())
            {
                //find edges
                if (method == EdgeFindingMethod.Sobel)
                {
                    GetSobelEdgeMat(src,edges);
                }
                else if (method == EdgeFindingMethod.Threshold)
                {
                    GetThreshEdgeMat(src,edges);
                }
                else if (method == EdgeFindingMethod.Canny)
                {
                    GetCannyEdgeMat(src,edges);
                }
                else //default
                {
                    GetSobelEdgeMat(src,edges);
                }

                edges.copyTo(dest);
            }
        } 

        public static void SetEdgeMat(Mat src, Mat dest, RemoveBackgroundSettings settings, StructuredEdgeDetection edgeDetection)
        {
            EdgeFindingMethod method = settings.edgeFindingMethod;
            using (Mat edges = new Mat())
            {
                //find edges
                if (method == EdgeFindingMethod.Sobel)
                {
                    GetSobelEdgeMat(src,edges);
                }
                else if (method == EdgeFindingMethod.Threshold)
                {
                    GetThreshEdgeMat(src,edges);
                }
                else if (
                    method == EdgeFindingMethod.StructuredForests &&
                    edgeDetection != null &&
                    !edgeDetection.empty()
                )
                {
                    GetStructuredForestsEdgeMat(src,edges,edgeDetection);
                }
                else if (method == EdgeFindingMethod.Canny)
                {
                    GetCannyEdgeMat(
                        src,edges,
                        settings.cannyThreshold1,settings.cannyThreshold2,
                        settings.cannyApertureSize
                    );
                }
                else //default
                {
                    GetSobelEdgeMat(src,edges);
                }

                edges.copyTo(dest);
            }
        } 


        //Helper function for GetSobelEdgeMat
        //Gets the magnitude of Sobel in both directions
        //as opposed to Sobel(..., 1,1)
        public static void SobelEdgeDetect(Mat src, Mat dest)
        {
            using (
                Mat sobelX = new Mat(),
                    sobelY = new Mat(),
                    magnitude = new Mat()
            )
            {
                Imgproc.Sobel(src, sobelX, CvType.CV_32FC1, 1, 0);
                Imgproc.Sobel(src, sobelY, CvType.CV_32FC1, 0, 1);

                Core.magnitude(sobelX,sobelY,magnitude);

                magnitude.copyTo(dest); //32FC1
            }
        }

        //Gets an edge image by running Sobel on each channel
        public static void GetSobelEdgeMat(Mat src, Mat dest)
        {
           
            using (
                Mat blurred = new Mat(),
                    rMat = new Mat(),
                    gMat = new Mat(),
                    bMat = new Mat()
            )
            {
                // blur the image to reduce high frequency noises.
                Imgproc.GaussianBlur(src, blurred, new Size(3, 3), 0);

                Core.extractChannel(blurred,rMat,0);
                Core.extractChannel(blurred,gMat,1);
                Core.extractChannel(blurred,bMat,2);

                
                using (
                    Mat rSobel = new Mat(),
                        gSobel = new Mat(),
                        bSobel = new Mat(),
                        rgMax = new Mat(),
                        rgbMax = new Mat(),            
                        outputMat = new Mat(src.height(), src.width(), src.type(), new Scalar(0, 0, 0, 0)),
                        mask = new Mat()
                )
                {
                    //get sobel edge detection for each color channel
                    SobelEdgeDetect(rMat, rSobel);
                    SobelEdgeDetect(gMat, gSobel);
                    SobelEdgeDetect(bMat, bSobel);

                    //get the maximum across the three sobels
                    Core.max(rSobel,gSobel,rgMax);
                    Core.max(rgMax,bSobel,rgbMax);

                    //zero values less than the mean to reduce noise
                    Scalar mean = Core.mean(rgbMax);
                    Core.inRange(rgbMax, mean, new Scalar(255,255,255,255), mask);
                    Core.copyTo(rgbMax, outputMat, mask);

                    outputMat.copyTo(dest);
                }
                
            }
        }

        //Gets edge image using adaptive (local) thresholding
        public static void GetThreshEdgeMat(Mat src, Mat dest)
        {
            using (
                Mat blurred = new Mat(),
                    gray = new Mat(),
                    T = new Mat(),
                    mask = new Mat(),
                    outputMat = OpenCVForUnity.CoreModule.Mat.zeros(src.height(),src.width(),CvType.CV_8UC1)
            )
            {
                Imgproc.GaussianBlur(src, blurred, new Size(3, 3), 0);

                Imgproc.cvtColor(blurred, gray, Imgproc.COLOR_RGBA2GRAY);

                Imgproc.adaptiveThreshold(gray, T, 255, Imgproc.ADAPTIVE_THRESH_GAUSSIAN_C, Imgproc.THRESH_BINARY, 11, 0.217);

                Core.compare(gray, T, mask, Core.CMP_GT);
                gray.copyTo(outputMat,mask);

                outputMat.copyTo(dest);
            }
        }

        //Gets edge image using structured forests model
        //Based on Code Pasta:
        //https://www.codepasta.com/computer-vision/2019/04/26/background-segmentation-removal-with-opencv-take-2.html
        public static void GetStructuredForestsEdgeMat(Mat src, Mat dest, StructuredEdgeDetection edgeDetection)
        {
            using (
                Mat blurred = new Mat(),
                    edges = new Mat(src.height(),src.width(),src.type(), new Scalar(0, 0, 0, 0))
            )
            {
                Imgproc.GaussianBlur(src, blurred, new Size(3, 3), 0);

                //StructuredEdgeDetection requires BGR floating point Mat
                Imgproc.cvtColor(blurred, blurred, Imgproc.COLOR_BGRA2BGR); //BGR
                blurred.convertTo(blurred,CvType.CV_32FC4,(1.0/255.0)); //floating point

                edgeDetection.detectEdges(blurred,edges);

                edges.convertTo(edges,CvType.CV_8UC1,255.0);
                Imgproc.medianBlur(edges,edges,11); //filter out salt and pepper noise
                
                edges.copyTo(dest);
            }
        }

        public static void GetCannyEdgeMat(Mat src, Mat dest)
        {
            using (Mat yuvMat = new Mat())
            {
                // change the color space to YUV.
                Imgproc.cvtColor(src, yuvMat, Imgproc.COLOR_RGBA2RGB);
                Imgproc.cvtColor(yuvMat, yuvMat, Imgproc.COLOR_RGB2YUV);

                using (Mat yMat = new Mat())
                {
                    // grap only the Y component.
                    Core.extractChannel(yuvMat, yMat, 0);

                    // blur the image to reduce high frequency noises.
                    Imgproc.GaussianBlur(yMat, yMat, new Size(3, 3), 0);
                    // find edges in the image.
                    Imgproc.Canny(yMat, yMat, 50, 200, 3);

                    yMat.copyTo(dest);
                    
                }
            }         
        }

        public static void GetCannyEdgeMat(Mat src, Mat dest, int threshold1, int threshold2, int apertureSize)
        {
            using (Mat yuvMat = new Mat())
            {
                // change the color space to YUV.
                Imgproc.cvtColor(src, yuvMat, Imgproc.COLOR_RGBA2RGB);
                Imgproc.cvtColor(yuvMat, yuvMat, Imgproc.COLOR_RGB2YUV);

                using (Mat yMat = new Mat())
                {
                    // grap only the Y component.
                    Core.extractChannel(yuvMat, yMat, 0);

                    // blur the image to reduce high frequency noises.
                    Imgproc.GaussianBlur(yMat, yMat, new Size(3, 3), 0);
                    // find edges in the image.
                    Imgproc.Canny(yMat, yMat, threshold1, threshold2, apertureSize);

                    yMat.copyTo(dest);
                    
                }
            }    
        }
    }
}


