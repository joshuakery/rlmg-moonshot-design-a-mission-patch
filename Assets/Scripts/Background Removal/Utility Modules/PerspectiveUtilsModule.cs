using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils.Helper;
using OpenCVForUnity.UnityUtils;

namespace ArtScan.PerspectiveUtilsModule
{
    public static class PerspectiveUtils
    {
        public static void BrightnessContrast(Mat src, int brightness, int contrast)
        {
            _BrightnessContrast(src, brightness, contrast);
        }

        public static void BrightnessContrast(Mat src, int brightness, int contrast, bool ignoreAlpha)
        {
            if (ignoreAlpha)
            {
                using (Mat alpha = new Mat())
                {
                    Core.extractChannel(src, alpha, 3);
                    _BrightnessContrast(src, brightness, contrast);
                    Core.insertChannel(alpha, src, 3);
                }
            }
            else
            {
                _BrightnessContrast(src, brightness, contrast);
            }
        }

        private static void _BrightnessContrast(Mat src, int brightness, int contrast)
        {
            brightness = (int)((brightness - 0) * (255 - (-255)) / (510 - 0) + (-255));
            contrast = (int)((contrast - 0) * (127 - (-127)) / (254 - 0) + (-127));

            int shadow;
            int max;

            using (Mat cal = new Mat())
            {
                if (brightness != 0)
                {
                    if (brightness > 0)
                    {
                        shadow = brightness;
                        max = 255;
                    }
                    else
                    {
                        shadow = 0;
                        max = 255 + brightness;
                    }

                    float al_pha = (float)(max - shadow) / 255f;
                    int ga_mma = shadow;

                    Core.addWeighted(
                        src, al_pha,
                        src, 0,
                        ga_mma,
                        cal
                    );

                }
                else
                {
                    src.copyTo(cal);
                }

                if (contrast != 0)
                {
                    float Alpha = (131f * ((float)contrast + 127f) / (127f * (131f - (float)contrast)));
                    float Gamma = 127f * (1f - Alpha);

                    Core.addWeighted(
                        cal, Alpha,
                        cal, 0,
                        Gamma,
                        cal
                    );
                }

                cal.copyTo(src);
            }
        }

        public static void Find4PointContours(Mat image, List<MatOfPoint> contours)
        {
            contours.Clear();
            List<MatOfPoint> tmp_contours = new List<MatOfPoint>();
            Mat hierarchy = new Mat();
            Imgproc.findContours(image, tmp_contours, hierarchy, Imgproc.RETR_EXTERNAL, Imgproc.CHAIN_APPROX_SIMPLE);
            
            foreach (var cnt in tmp_contours)
            {
                MatOfInt hull = new MatOfInt();
                Imgproc.convexHull(cnt, hull, false);

                Point[] cnt_arr = cnt.toArray();
                int[] hull_arr = hull.toArray();
                Point[] pts = new Point[hull_arr.Length];
                for (int i = 0; i < hull_arr.Length; i++)
                {
                    pts[i] = cnt_arr[hull_arr[i]];
                }

                MatOfPoint2f ptsFC2 = new MatOfPoint2f(pts);
                MatOfPoint2f approxFC2 = new MatOfPoint2f();
                MatOfPoint approxSC2 = new MatOfPoint();

                double arclen = Imgproc.arcLength(ptsFC2, true);
                Imgproc.approxPolyDP(ptsFC2, approxFC2, 0.01 * arclen, true);
                approxFC2.convertTo(approxSC2, CvType.CV_32S);

                if (approxSC2.size().area() != 4)
                    continue;

                contours.Add(approxSC2);
            }
        }

        public static MatOfPoint GetMaxAreaContour(List<MatOfPoint> contours)
        {
            if (contours.Count == 0)
                return new MatOfPoint();

            int index = -1;
            double area = 0;
            for (int i = 0; i < contours.Count; i++)
            {
                double tmp = Imgproc.contourArea(contours[i]);
                if (area < tmp)
                {
                    area = tmp;
                    index = i;
                }
            }

            if (index == -1)
                return new MatOfPoint();
            //     Debug.Log(index);

            return contours[index];
        }

        public static MatOfPoint OrderCornerPoints(MatOfPoint corners)
        {
            if (corners.size().area() <= 0 || corners.rows() < 4)
                return corners;

            // rearrange the points in the order of upper left, upper right, lower right, lower left.
            using (Mat x = new Mat(corners.size(), CvType.CV_32SC1))
            using (Mat y = new Mat(corners.size(), CvType.CV_32SC1))
            using (Mat d = new Mat(corners.size(), CvType.CV_32SC1))
            using (Mat dst = new Mat(corners.size(), CvType.CV_32SC2))
            {
                Core.extractChannel(corners, x, 0);
                Core.extractChannel(corners, y, 1);

                // the sum of the upper left points is the smallest and the sum of the lower right points is the largest.
                Core.add(x, y, d);
                Core.MinMaxLocResult result = Core.minMaxLoc(d);
                dst.put(0, 0, corners.get((int)result.minLoc.y, 0));
                dst.put(2, 0, corners.get((int)result.maxLoc.y, 0));

                // the difference in the upper right point is the smallest, and the difference in the lower left is the largest.
                Core.subtract(y, x, d);
                result = Core.minMaxLoc(d);
                dst.put(1, 0, corners.get((int)result.minLoc.y, 0));
                dst.put(3, 0, corners.get((int)result.maxLoc.y, 0));

                dst.copyTo(corners);
            }
            return corners;
        }

        public static void ReversePerspectiveTransform(Mat image, MatOfPoint corners, Mat outputMat)
        {
            if (corners.size().area() <= 0 || corners.rows() < 4)
                return;

            Point[] pts = corners.toArray();
            Point tl = pts[0];
            Point tr = pts[1];
            Point br = pts[2];
            Point bl = pts[3];

            Mat src = new Mat(4, 1, CvType.CV_32FC2);
            src.put(0, 0, 0, 0, image.width() + 1, 0, image.width() + 1, image.height() + 1, 0, image.height() + 1);
            Mat dst = new Mat();
            corners.convertTo(dst, CvType.CV_32FC2);

            // compute and apply the perspective transformation matrix.
            Mat perspectiveTransform = Imgproc.getPerspectiveTransform(src, dst);
            Imgproc.warpPerspective(image, outputMat, perspectiveTransform, new Size(outputMat.cols(), outputMat.rows()));
        }

        public static Mat PerspectiveTransform(Mat image, MatOfPoint corners)
        {
            if (corners.size().area() <= 0 || corners.rows() < 4)
                return image;

            Point[] pts = corners.toArray();
            Point tl = pts[0];
            Point tr = pts[1];
            Point br = pts[2];
            Point bl = pts[3];

            double widthA = Math.Sqrt((br.x - bl.x) * (br.x - bl.x) + (br.y - bl.y) * (br.y - bl.y));
            double widthB = Math.Sqrt((tr.x - tl.x) * (tr.x - tl.x) + (tr.y - tl.y) * (tr.y - tl.y));
            int maxWidth = Math.Max((int)widthA, (int)widthB);

            double heightA = Math.Sqrt((tr.x - br.x) * (tr.x - br.x) + (tr.y - br.y) * (tr.y - br.y));
            double heightB = Math.Sqrt((tl.x - bl.x) * (tl.x - bl.x) + (tl.y - bl.y) * (tl.y - bl.y));
            int maxHeight = Math.Max((int)heightA, (int)heightB);

            maxWidth = (maxWidth < 1) ? 1 : maxWidth;
            maxHeight = (maxHeight < 1) ? 1 : maxHeight;

            Mat src = new Mat();
            corners.convertTo(src, CvType.CV_32FC2);
            Mat dst = new Mat(4, 1, CvType.CV_32FC2);
            dst.put(0, 0, 0, 0, maxWidth - 1, 0, maxWidth - 1, maxHeight - 1, 0, maxHeight - 1);

            // compute and apply the perspective transformation matrix.
            Mat outputMat = new Mat(maxHeight, maxWidth, image.type(), new Scalar(0, 0, 0, 255));
            Mat perspectiveTransform = Imgproc.getPerspectiveTransform(src, dst);
            Imgproc.warpPerspective(image, outputMat, perspectiveTransform, new Size(outputMat.cols(), outputMat.rows()));

            // return the transformed image.
            return outputMat;
        }

        public static void CropByPercent(Mat src, Mat dest, float scale)
        {
            // Debug.Log("Width: " + image.width().ToString());
            // Debug.Log("Height: " + image.height().ToString());

            int w = (int)(src.width() * scale);
            if (src.width() == 1) w = 1; //Don't crop Mats with a width of 1
            int h = (int)(src.height() * scale);
            if (src.height() == 1) h = 1; //Don't crop Mats with a width of 1

            int x = (int)(src.width() / 2 - src.width() * scale / 2);
            int y = (int)(src.height() / 2 - src.height() * scale / 2);
            // Debug.Log("W: " + w.ToString() + "H: " + h.ToString() + "X: " + x.ToString() + "Y: " + y.ToString());

            Mat output = new Mat(src, new OpenCVForUnity.CoreModule.Rect(x,y,w,h));
            dest = output;
        }
    }
}
