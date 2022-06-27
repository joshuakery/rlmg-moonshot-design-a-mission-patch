using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils.Helper;
using OpenCVForUnity.UnityUtils;
using ArtScan.CoreModule;
using ArtScan;
using ArtScan.PresentationUtilsModule;
using rlmg.logging;

namespace ArtScan.ScanSavingModule
{
    public static class ScanSaving
    {
        private static Texture2D GetTexture2DFromImageFile(string png, RemoveBackgroundSettings settings, myWebCamTextureToMatHelper webCamTextureToMatHelper)
        {
            using (
                Mat src = OpenCVForUnity.ImgcodecsModule.Imgcodecs.imread(
                        png,
                        OpenCVForUnity.ImgcodecsModule.Imgcodecs.IMREAD_UNCHANGED
                    ),

                    displayMat = new Mat(
                        settings.targetHeight,
                        settings.targetWidth,
                        webCamTextureToMatHelper.GetMat().type(),
                        new Scalar(0,0,0,0)
                    )
            )
            {
                Imgproc.cvtColor(src, src, Imgproc.COLOR_BGRA2RGBA);;

                PresentationUtils.MakeReadyToPresent(
                    src, displayMat,
                    settings.doCropToBoundingBox, settings.doSizeToFit
                );

                Texture2D scanTexture = new Texture2D(displayMat.cols(), displayMat.rows(), TextureFormat.RGBA32, false);
                Utils.fastMatToTexture2D(displayMat,scanTexture,true,0,true);
                return scanTexture;
            }
        }
        private static void DeleteExcessFiles(string[] pngList)
        {
            // //delete excess file(s)
            // if (pngList.Length >= gameState.scanMax)
            // {
            //     //failsafe: delete all files after scanMax
            //     for (int i=gameState.scanMax; i<pngList.Length; i++)
            //     {
            //         File.Delete(pngList[i]);
            //     }
            // }
        }

        private static void DeleteFilenameMatchingIndex(string[] pngList, int index)
        {
            foreach (string png in pngList)
            {
                string number = Path.GetFileNameWithoutExtension(png);
                if (Int32.Parse(number) == index)
                {
                    File.Delete(png);
                }
            }
        }

        private static void SaveMatToFile(Mat src, int index, string fullPath)
        {
            string filename = String.Format("{0}.png",index);
            string filepath = Path.Join(fullPath,filename);
            Imgproc.cvtColor(src, src, Imgproc.COLOR_RGBA2BGRA);
            OpenCVForUnity.ImgcodecsModule.Imgcodecs.imwrite(filepath,src);
        }

        public static void ReadScans(
            Func<Texture2D,int,int> AddScan,
            string readPath,
            RemoveBackgroundSettings settings,
            myWebCamTextureToMatHelper webCamTextureToMatHelper
        )
        {
            
            DateTime before = DateTime.Now;

            DirectoryInfo di = new DirectoryInfo(readPath);

            // Debug.Log("READING SCANS from " + readPath);
            RLMGLogger.Instance.Log("Rading scans from " + readPath, MESSAGETYPE.INFO);
            
            try
            {
                if (di.Exists)
                {
                    string[] pngList = Directory.GetFiles(readPath, "*.png");

                    RLMGLogger.Instance.Log(String.Format("{0} png files found in directory.",pngList.Length.ToString()), MESSAGETYPE.INFO);

                    foreach (string png in pngList)
                    {
                        Texture2D scanTexture = GetTexture2DFromImageFile(png, settings, webCamTextureToMatHelper);
                        string number = Path.GetFileNameWithoutExtension(png);
                        AddScan(scanTexture, Int32.Parse(number));
                    }
                }
            }
            catch (Exception e)
            {
                RLMGLogger.Instance.Log(String.Format("The process failed: {0}.",e.ToString()), MESSAGETYPE.ERROR);
            }

            DateTime after = DateTime.Now; 
            TimeSpan duration = after.Subtract(before);
            // Debug.Log("READ SCANS Duration in milliseconds: " + duration.Milliseconds);
            RLMGLogger.Instance.Log("Read scans in milliseconds: " + duration.Milliseconds, MESSAGETYPE.INFO);

        }

        public static void SaveScan(Mat src, string saveDirPath, int index)
        {
            DirectoryInfo di = new DirectoryInfo(saveDirPath);
            
            try
            {
                if (di.Exists)
                {
                    string[] pngList = Directory.GetFiles(saveDirPath, "*.png");
                    DeleteExcessFiles(pngList);
                    DeleteFilenameMatchingIndex(pngList, index);
                }
                else
                {
                    di.Create();
                    RLMGLogger.Instance.Log(String.Format("{0} directory created successfully.",saveDirPath), MESSAGETYPE.INFO);
                }

                SaveMatToFile(src,index,saveDirPath);

            }
            catch (Exception e)
            {
                RLMGLogger.Instance.Log(String.Format("The process failed: {0}.",e.ToString()), MESSAGETYPE.ERROR);
            }

        }

        public static void TrashScan(string savePath, string trashPath, int toTrash)
        {
            // string teamDir = gameState.teams[gameState.currentTeam].directory;
            // string teamDirPath = Path.Combine(dirPath,teamDir);
            DirectoryInfo saveDI = new DirectoryInfo(savePath);
            DirectoryInfo trashDI = new DirectoryInfo(trashPath);

            try
            {
                if (!saveDI.Exists)
                {
                    saveDI.Create();
                    RLMGLogger.Instance.Log(String.Format("{0} directory created successfully.",savePath), MESSAGETYPE.INFO);
                }
                if (!trashDI.Exists)
                {
                    trashDI.Create();
                    RLMGLogger.Instance.Log(String.Format("{0} directory created successfully.",trashPath), MESSAGETYPE.INFO);
                }
                if (saveDI.Exists && trashDI.Exists)
                {
                    string[] pngList = Directory.GetFiles(savePath, "*.png");
                    for (int p = 0; p < pngList.Length; p++)
                    {
                        string png = pngList[p];
                        string number = Path.GetFileNameWithoutExtension(png);

                        if (Int32.Parse(number) == toTrash)
                        {
                            RLMGLogger.Instance.Log(String.Format("Trashing {0}",png), MESSAGETYPE.INFO);

                            string filename = Path.GetFileName(png);
                            string trashFilepath = Path.Join(trashPath,filename);

                            File.Copy(png,trashFilepath,true);

                            File.Delete(png);
                        } 
                    }
                }
            }
            catch (Exception e)
            {
                RLMGLogger.Instance.Log(String.Format("The process failed: {0}.",e.ToString()), MESSAGETYPE.ERROR);
            }
        }

        public static void ClearTrash(string trashPath)
        {
            DirectoryInfo trashDI = new DirectoryInfo(trashPath);
            try
            {
                if (trashDI.Exists)
                {
                    string[] pngList = Directory.GetFiles(trashPath, "*.png");
                    for (int p = 0; p < pngList.Length; p++)
                    {
                        File.Delete(pngList[p]);
                    }
                }
            }
            catch (Exception e)
            {
                RLMGLogger.Instance.Log(String.Format("The process failed: {0}.",e.ToString()), MESSAGETYPE.ERROR);
            }
        }

        public static void DeleteAllScans(string savePath)
        {
            DirectoryInfo di = new DirectoryInfo(savePath);
            try
            {
                if (di.Exists)
                {
                    foreach (FileInfo file in di.GetFiles())
                    {
                        file.Delete(); 
                    }
                    foreach (DirectoryInfo dir in di.GetDirectories())
                    {
                        dir.Delete(true); 
                    }
                }
            }
            catch (Exception e)
            {
                RLMGLogger.Instance.Log(String.Format("The process failed: {0}.",e.ToString()), MESSAGETYPE.ERROR);
            }

        }
    }
}

/// <summary>
/// Main demo of drawing background removal
/// </summary>
public class saveScans : MonoBehaviour
{
    public myWebCamTextureToMatHelper webCamTextureToMatHelper;

    public RefinedScanController refinedScanController;

    /// <summary>
    /// Settings used to size and crop scans
    /// </summary>
    public RemoveBackgroundSettings settings;

    /// <summary>
    /// The Game State Scriptable Object.
    /// </summary>
    public GameState gameState;

    public string dirName = "SavedScans";
    public string trashName = "TrashedScans";

    /// <summary>
    /// Path where scans will be saved as a backup.
    /// </summary>
    private string dirPath;

    /// <summary>
    /// Path where scans will be saved as trashed.
    /// </summary>
    private string trashPath;

    private void Start()
    {
        dirPath = Path.Join(Application.streamingAssetsPath,dirName);
        trashPath = Path.Join(Application.streamingAssetsPath,trashName);
    }

    public void ClearTrash()
    {
        DirectoryInfo trashDI = new DirectoryInfo(trashPath);
        try
        {
            if (trashDI.Exists)
            {
                string[] pngList = Directory.GetFiles(trashPath, "*.png");
                for (int p = 0; p < pngList.Length; p++)
                {
                    File.Delete(pngList[p]);
                }
            }
        }
        catch (Exception e)
        {
            RLMGLogger.Instance.Log(String.Format("The process failed: {0}.",e.ToString()), MESSAGETYPE.ERROR);
        }
    }

    public void UnTrashAll()
    {
        DirectoryInfo di = new DirectoryInfo(dirPath);
        DirectoryInfo trashDI = new DirectoryInfo(trashPath);
        try
        {
            if (di.Exists && trashDI.Exists)
            {
                string[] pngList = Directory.GetFiles(trashPath, "*.png");
                foreach (string png in pngList)
                {
                    RLMGLogger.Instance.Log(String.Format("Untrashing {0}",png), MESSAGETYPE.INFO);

                    string number = Path.GetFileNameWithoutExtension(png);
                    string filename = String.Format("{0}.png",Int32.Parse(number));
                    string destFilepath = Path.Join(dirPath,filename);

                    File.Copy(png,destFilepath,true);

                    File.Delete(png);
                }
            }
        }
        catch (Exception e)
        {
            RLMGLogger.Instance.Log(String.Format("The process failed: {0}.",e.ToString()), MESSAGETYPE.ERROR);
        }
    }

    public void TrashScan(int toTrash)
    {
        string teamDir = gameState.teams[gameState.currentTeam].directory;
        string fullPath = Path.Combine(dirPath,teamDir);
        DirectoryInfo teamDI = new DirectoryInfo(fullPath);

        DirectoryInfo trashDI = new DirectoryInfo(trashPath);

        try
        {
            if (teamDI.Exists && trashDI.Exists)
            {
                string[] pngList = Directory.GetFiles(fullPath, "*.png");
                for (int p = 0; p < pngList.Length; p++)
                {
                    string png = pngList[p];
                    string number = Path.GetFileNameWithoutExtension(png);

                    if (Int32.Parse(number) == toTrash)
                    {
                        RLMGLogger.Instance.Log(String.Format("Trashing {0}",png), MESSAGETYPE.INFO);

                        string filename = Path.GetFileName(png);
                        string trashFilepath = Path.Join(trashPath,filename);

                        File.Copy(png,trashFilepath,true);

                        File.Delete(png);
                    } 
                }
            }
        }
        catch (Exception e)
        {
            RLMGLogger.Instance.Log(String.Format("The process failed: {0}.",e.ToString()), MESSAGETYPE.ERROR);
        }
    }

    public void TrashScans(Texture2D[] toTrash)
    {
        string teamDir = gameState.teams[gameState.currentTeam].directory;
        string fullPath = Path.Combine(dirPath,teamDir);
        DirectoryInfo teamDI = new DirectoryInfo(fullPath);

        DirectoryInfo trashDI = new DirectoryInfo(trashPath);

        try
        {
            if (teamDI.Exists && trashDI.Exists)
            {
                string[] pngList = Directory.GetFiles(fullPath, "*.png");
                for (int p = 0; p < pngList.Length; p++)
                {
                    string png = pngList[p];
                    string number = Path.GetFileNameWithoutExtension(png);

                    if (toTrash[Int32.Parse(number)] != null)
                    {
                        RLMGLogger.Instance.Log(String.Format("Trashing {0}",png), MESSAGETYPE.INFO);

                        string filename = Path.GetFileName(png);
                        string trashFilepath = Path.Join(trashPath,filename);

                        File.Copy(png,trashFilepath,true);

                        File.Delete(png);
                    } 
                }
            }
        }
        catch (Exception e)
        {
            RLMGLogger.Instance.Log(String.Format("The process failed: {0}.",e.ToString()), MESSAGETYPE.ERROR);
        }

    }

    private Texture2D GetTexture2DFromImageFile(string png)
    {
        using (
            Mat src = OpenCVForUnity.ImgcodecsModule.Imgcodecs.imread(
                    png,
                    OpenCVForUnity.ImgcodecsModule.Imgcodecs.IMREAD_UNCHANGED
                ),

                displayMat = new Mat(
                    settings.targetHeight,
                    settings.targetWidth,
                    webCamTextureToMatHelper.GetMat().type(),
                    new Scalar(0,0,0,0)
                )
        )
        {
            Imgproc.cvtColor(src, src, Imgproc.COLOR_BGRA2RGBA);;

            PresentationUtils.MakeReadyToPresent(
                src, displayMat,
                settings.doCropToBoundingBox, settings.doSizeToFit
            );

            Texture2D scanTexture = new Texture2D(displayMat.cols(), displayMat.rows(), TextureFormat.RGBA32, false);
            Utils.fastMatToTexture2D(displayMat,scanTexture,true,0,true);
            return scanTexture;
        }
    }

    public void ReadScans(GameEvent callback)
    {
        
        DateTime before = DateTime.Now;

        if (!String.IsNullOrEmpty(dirPath))
        {
            DirectoryInfo mainDI = new DirectoryInfo(dirPath);

            string teamDir = gameState.teams[gameState.currentTeam].directory;
            string fullPath = Path.Combine(dirPath,teamDir);
            DirectoryInfo teamDI = new DirectoryInfo(fullPath);

            RLMGLogger.Instance.Log("Reading scans from " + fullPath, MESSAGETYPE.INFO);
             
            try
            {
                if (mainDI.Exists)
                {
                    if (teamDI.Exists)
                    {
                        string[] pngList = Directory.GetFiles(fullPath, "*.png");

                        RLMGLogger.Instance.Log(String.Format("{0} png files found in directory.",pngList.Length.ToString()), MESSAGETYPE.INFO);

                        foreach (string png in pngList)
                        {
                            Texture2D scanTexture = GetTexture2DFromImageFile(png);
                            string number = Path.GetFileNameWithoutExtension(png);
                            gameState.AddScan(scanTexture, Int32.Parse(number));
                        }

                        if (callback != null)
                            callback.Raise();
                    }
                }
            }
            catch (Exception e)
            {
                RLMGLogger.Instance.Log(String.Format("The process failed: {0}.",e.ToString()), MESSAGETYPE.ERROR);
            }

        }

        DateTime after = DateTime.Now; 
        TimeSpan duration = after.Subtract(before);
        // Debug.Log();
        RLMGLogger.Instance.Log("READ SCANS Duration in milliseconds: " + duration.Milliseconds, MESSAGETYPE.INFO);

    }

    public void SavePreview()
    {
        if (refinedScanController.previewMat != null)
        {
            int index = gameState.AddScan(gameState.preview);
            if (index >= 0)
                SaveScan(refinedScanController.previewMat, index);
        }
        
    }

    private void DeleteExcessFiles(string[] pngList)
    {
        // //delete excess file(s)
        // if (pngList.Length >= gameState.scanMax)
        // {
        //     //failsafe: delete all files after scanMax
        //     for (int i=gameState.scanMax; i<pngList.Length; i++)
        //     {
        //         File.Delete(pngList[i]);
        //     }
        // }
    }

    private void DeleteFilenameMatchingIndex(string[] pngList, int index)
    {
        foreach (string png in pngList)
        {
            string number = Path.GetFileNameWithoutExtension(png);
            if (Int32.Parse(number) == index)
            {
                File.Delete(png);
            }
        }
    }

    private void SaveMatToFile(Mat src, int index, string fullPath)
    {
        string filename = String.Format("{0}.png",index);
        string filepath = Path.Join(fullPath,filename);
        Imgproc.cvtColor(src, src, Imgproc.COLOR_RGBA2BGRA);
        OpenCVForUnity.ImgcodecsModule.Imgcodecs.imwrite(filepath,src);
    }

    private void SaveScan(Mat src, int index)
    {
        DirectoryInfo mainDI = new DirectoryInfo(dirPath);

        string teamDir = gameState.teams[gameState.currentTeam].directory;
        string fullPath = Path.Combine(dirPath,teamDir);
        DirectoryInfo teamDI = new DirectoryInfo(fullPath);
        
        try
        {
            if (!mainDI.Exists)
            {
                mainDI.Create();
                RLMGLogger.Instance.Log("The main directory was created successfully.", MESSAGETYPE.INFO);
            }

            ClearTrash();

            if (teamDI.Exists)
            {
                string[] pngList = Directory.GetFiles(fullPath, "*.png");

                DeleteExcessFiles(pngList);
                DeleteFilenameMatchingIndex(pngList, index);

                SaveMatToFile(src,index,fullPath);
            }
            else
            {
                teamDI.Create();
                RLMGLogger.Instance.Log(String.Format("{0} directory created successfully.",teamDir), MESSAGETYPE.INFO);

                SaveMatToFile(src,index,fullPath);
            }

        }
        catch (Exception e)
        {
            RLMGLogger.Instance.Log(String.Format("The process failed: {0}.",e.ToString()), MESSAGETYPE.ERROR);
        }

    }

    public void DeleteAllScans()
    {
        DirectoryInfo di = new DirectoryInfo(dirPath);
        try
        {
            if (di.Exists)
            {
                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete(); 
                }
                foreach (DirectoryInfo dir in di.GetDirectories())
                {
                    dir.Delete(true); 
                }
            }
        }
        catch (Exception e)
        {
            RLMGLogger.Instance.Log(String.Format("The process failed: {0}.",e.ToString()), MESSAGETYPE.ERROR);
        }

    }

    // private void RenameSavedScansBasedOnOrder()
    // {
    //     DirectoryInfo di = new DirectoryInfo(dirPath);
    //     try
    //     {
    //         string[] pngList = Directory.GetFiles(dirPath, "*.png");
    //         for (int i=0; i<pngList.Length; i++)
    //         {
    //             string newFilename = String.Format("{0}.png", i);
    //             string newFilepath = Path.Join(dirPath,newFilename);

    //             if (pngList[i] != newFilepath)
    //             {
    //                 File.Copy(pngList[i],newFilepath,true);
    //                 File.Delete(pngList[i]);
    //             }
                    
    //         }
    //     }
    //     catch (Exception e)
    //     {
    //         RLMGLogger.Instance.Log(String.Format("The process failed: {0}.",e.ToString()), MESSAGETYPE.ERROR);
    //     }
    // }
}
