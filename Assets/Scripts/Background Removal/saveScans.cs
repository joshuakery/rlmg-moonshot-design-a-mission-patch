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
        public static Texture2D GetTexture2DFromImageFile(
            string png,
            RemoveBackgroundSettings settings,
            myWebCamTextureToMatHelper webCamTextureToMatHelper
        )
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

                if (src.size().width > 0 && src.size().height > 0)
                    PresentationUtils.MakeReadyToPresent(
                        src, displayMat,
                        settings.doCropToBoundingBox, settings.doSizeToFit
                    );

                Texture2D scanTexture = new Texture2D(displayMat.cols(), displayMat.rows(), TextureFormat.RGBA32, false);
                Utils.fastMatToTexture2D(displayMat,scanTexture,true,0,true);
                return scanTexture;
            }
        }

        private static void SaveMatToFile(Mat src, string fullPath)
        {
            Imgproc.cvtColor(src, src, Imgproc.COLOR_RGBA2BGRA);
            OpenCVForUnity.ImgcodecsModule.Imgcodecs.imwrite(fullPath, src);
        }

        public static string FormatScanFilename(string teamName, int index)
        {
            return String.Format("{0}-{1}.png", teamName, index);
        }

        public static IEnumerator DownloadScansCoroutine(string dirPath, string[] filenames, bool doOverwrite, GameEvent callbackEvent)
        {
            DateTime before = DateTime.Now;

            if (!String.IsNullOrEmpty(dirPath))
            {
                DirectoryInfo mainDI = new DirectoryInfo(dirPath);

                if (!mainDI.Exists)
                {
                    mainDI.Create();
                    RLMGLogger.Instance.Log(String.Format("The directory was created successfully at {0}.", dirPath), MESSAGETYPE.INFO);
                }

                RLMGLogger.Instance.Log(String.Format("Downloading scans to {0}.", dirPath), MESSAGETYPE.INFO);

                if (filenames != null)
                {
                    int count = 0;
                    foreach (string filename in filenames)
                    {
                        if (!String.IsNullOrEmpty(filename))
                        {
                            if (doOverwrite || !File.Exists(Path.Join(dirPath, filename)))
                            {
                                DownloadThreadController.DownloadThread downloadThread = new DownloadThreadController.DownloadThread();
                                downloadThread.filename = filename;
                                downloadThread.dirPath = dirPath;

                                downloadThread.Start();

                                yield return downloadThread.WaitFor();

                                count++;
                                RLMGLogger.Instance.Log(String.Format("Downloaded {0} of {1} scans.", count, filenames.Length), MESSAGETYPE.INFO);
                            }
                                
                        }

                    }
                }
                else
                {
                    RLMGLogger.Instance.Log("Current team has no artworks list. Cannot download scans.", MESSAGETYPE.ERROR);
                }
            }

            DateTime after = DateTime.Now;
            TimeSpan duration = after.Subtract(before);

            RLMGLogger.Instance.Log(String.Format("Downloaded scans in {0} milliseconds.", duration.Milliseconds), MESSAGETYPE.INFO);

            if (callbackEvent != null)
                callbackEvent.Raise();
        }

        public static void DownloadScans(string dirPath, MoonshotTeamData currentTeam)
        {
            DownloadScans(dirPath, currentTeam, true);
        }

        public static void DownloadScans(string dirPath, MoonshotTeamData currentTeam, bool doOverwrite)
        {
            DateTime before = DateTime.Now;

            if (!String.IsNullOrEmpty(dirPath))
            {
                DirectoryInfo mainDI = new DirectoryInfo(dirPath);

                if (!mainDI.Exists)
                {
                    mainDI.Create();
                    RLMGLogger.Instance.Log(String.Format("The directory was created successfully at {0}.", dirPath), MESSAGETYPE.INFO);
                }

                if (currentTeam == null)
                {
                    RLMGLogger.Instance.Log("There is no current team. Cannot download scans.", MESSAGETYPE.ERROR);
                    return;
                }

                RLMGLogger.Instance.Log(String.Format("Downloading scans to {0}.", dirPath), MESSAGETYPE.INFO);

                if (currentTeam.artworks != null)
                {
                    foreach (string filename in currentTeam.artworks)
                    {
                        if (!String.IsNullOrEmpty(filename))
                        {
                            if (doOverwrite || !File.Exists( Path.Join(dirPath,filename) ) )
                                ClientSend.GetFileFromServer(filename, dirPath);
                        }

                    }
                }
                else
                {
                    RLMGLogger.Instance.Log("Current team has no artworks list. Cannot download scans.", MESSAGETYPE.ERROR);
                    return;
                }
            }

            DateTime after = DateTime.Now;
            TimeSpan duration = after.Subtract(before);

            RLMGLogger.Instance.Log(String.Format("Downloaded scans in {0} milliseconds.", duration.Milliseconds), MESSAGETYPE.INFO);
        }

        //public static void ReadScans(
        //    string dirPath,
        //    string[] filenames,
        //    Func<Texture2D, int, int> AddScan,
        //    RemoveBackgroundSettings settings,
        //    myWebCamTextureToMatHelper webCamTextureToMatHelper
        //)
        //{
        //    DateTime before = DateTime.Now;

        //    if (!String.IsNullOrEmpty(dirPath))
        //    {
        //        DirectoryInfo mainDI = new DirectoryInfo(dirPath);

        //        if (!mainDI.Exists)
        //        {
        //            mainDI.Create();
        //            RLMGLogger.Instance.Log(String.Format("The directory was created successfully at {0}.", dirPath), MESSAGETYPE.INFO);
        //        }

        //        if (filenames == null)
        //        {
        //            RLMGLogger.Instance.Log("Artworks list is null. Cannot read scans.", MESSAGETYPE.ERROR);
        //            return;
        //        }

        //        RLMGLogger.Instance.Log(String.Format("Reading scans from {0}.", dirPath), MESSAGETYPE.INFO);

        //        if (filenames != null)
        //        {
        //            for (int i=0; i<filenames.Length; i++)
        //            {
        //                string filename = filenames[i];
        //                if (!String.IsNullOrEmpty(filename))
        //                {
        //                    string filepath = Path.Join(dirPath, filename);

        //                    Texture2D scanTexture = GetTexture2DFromImageFile(filepath, settings, webCamTextureToMatHelper);

        //                    AddScan(scanTexture,i);
        //                }
        //            }
        //        }
        //        else
        //        {
        //            RLMGLogger.Instance.Log("Current team has no artworks list. Cannot read scans.", MESSAGETYPE.ERROR);
        //            return;
        //        }
        //    }

        //    DateTime after = DateTime.Now;
        //    TimeSpan duration = after.Subtract(before);

        //    RLMGLogger.Instance.Log("READ SCANS Duration in milliseconds: " + duration.Milliseconds, MESSAGETYPE.INFO);

        //}

        public static void SaveScan(Mat src, string dirPath, string filename)
        {
            DirectoryInfo mainDI = new DirectoryInfo(dirPath);

            try
            {
                if (!mainDI.Exists)
                {
                    mainDI.Create();
                    RLMGLogger.Instance.Log(String.Format("The directory was created successfully at {0}.", dirPath), MESSAGETYPE.INFO);
                }

                string fullPath = Path.Join(dirPath, filename);
                SaveMatToFile(src, fullPath);
            }
            catch (Exception e)
            {
                RLMGLogger.Instance.Log(String.Format("The process failed: {0}.", e.ToString()), MESSAGETYPE.ERROR);
            }
        }

        public static void TrashScan(string saveDirPath, string trashDirPath, string filename)
        {
            DirectoryInfo saveDI = new DirectoryInfo(saveDirPath);
            DirectoryInfo trashDI = new DirectoryInfo(trashDirPath);

            try
            {
                if (!saveDI.Exists)
                {
                    saveDI.Create();
                    RLMGLogger.Instance.Log(String.Format("Save directory created successfully at {0}.",saveDirPath), MESSAGETYPE.INFO);
                }

                if (!trashDI.Exists)
                {
                    trashDI.Create();
                    RLMGLogger.Instance.Log(String.Format("Trash directory created successfully at {0}.",trashDirPath), MESSAGETYPE.INFO);
                }

                if (saveDI.Exists && trashDI.Exists)
                {
                    string fullSavePath = Path.Join(saveDirPath, filename);
                    string fullTrashPath = Path.Join(trashDirPath, filename);

                    File.Copy(fullSavePath, fullTrashPath, true);

                    File.Delete(fullSavePath);
                }
            }
            catch (Exception e)
            {
                RLMGLogger.Instance.Log(String.Format("The process failed: {0}.",e.ToString()), MESSAGETYPE.ERROR);
            }
        }

        public static void UnTrashScan(string saveDirPath, string trashDirPath, string filename)
        {
            DirectoryInfo saveDI = new DirectoryInfo(saveDirPath);
            DirectoryInfo trashDI = new DirectoryInfo(trashDirPath);

            try
            {
                if (!saveDI.Exists)
                {
                    saveDI.Create();
                    RLMGLogger.Instance.Log(String.Format("Save directory created successfully at {0}.", saveDirPath), MESSAGETYPE.INFO);
                }

                if (!trashDI.Exists)
                {
                    trashDI.Create();
                    RLMGLogger.Instance.Log(String.Format("Trash directory created successfully at {0}.", trashDirPath), MESSAGETYPE.INFO);
                }

                if (saveDI.Exists && trashDI.Exists)
                {
                    string fullSavePath = Path.Join(saveDirPath, filename);
                    string fullTrashPath = Path.Join(trashDirPath, filename);

                    File.Copy(fullTrashPath, fullSavePath, true);

                    File.Delete(fullTrashPath);
                }
            }
            catch (Exception e)
            {
                RLMGLogger.Instance.Log(String.Format("The process failed: {0}.", e.ToString()), MESSAGETYPE.ERROR);
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
