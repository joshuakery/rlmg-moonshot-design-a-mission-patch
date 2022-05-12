using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils.Helper;
using OpenCVForUnity.UnityUtils;
using System;
using ArtScan;

public class GrabcutRemovalThread: MultiThreading.ThreadedJob {

    public Mat converted;
    public Mat trimap;
    public OpenCVForUnity.CoreModule.Rect rectangle;
    public Mat bgdModel;
    public Mat fgdModel;
    public int iterCount;

    protected override void ThreadFunction()
    {
        Imgproc.grabCut (converted, trimap, rectangle, bgdModel, fgdModel, iterCount, Imgproc.GC_INIT_WITH_MASK);
    }

}