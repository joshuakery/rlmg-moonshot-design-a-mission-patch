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
        private static void SaveMatToFile(Mat src, string fullPath)
        {
            Imgproc.cvtColor(src, src, Imgproc.COLOR_RGBA2BGRA);
            OpenCVForUnity.ImgcodecsModule.Imgcodecs.imwrite(fullPath, src);
        }

        public static string FormatScanFilename(string teamName, int index)
        {
            return String.Format("{0}-{1}.png", teamName, index);
        }

        public static IEnumerator DownloadScansCoroutine(
            DownloadThreadController downloadThreadController,
            string dirPath,
            string[] filenames,
            bool doOverwrite,
            GameEvent callbackEvent
        )
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
                                RLMGLogger.Instance.Log(String.Format("Downloading number {0} of {1} scans: {2}", count, filenames.Length, filename), MESSAGETYPE.INFO);

                                yield return downloadThreadController.DownloadCoroutine(filename, dirPath);

                                count++;
                            }
                                
                        }

                    }
                }
                else
                {
                    RLMGLogger.Instance.Log("Current team has no artworks list. Cannot download scans.", MESSAGETYPE.INFO);
                }
            }

            DateTime after = DateTime.Now;
            TimeSpan duration = after.Subtract(before);

            RLMGLogger.Instance.Log(String.Format("Downloaded scans in {0} milliseconds.", duration.TotalMilliseconds), MESSAGETYPE.INFO);

            if (callbackEvent != null)
                callbackEvent.Raise();
        }

        public static void DownloadScans(string dirPath, MoonshotTeamData currentTeam, bool doOverwrite = true)
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

        public static void SaveTexture2D(Texture2D tex, string dirPath, string filename)
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
                byte[] bytes;

                if (tex != null)
                    bytes = tex.EncodeToPNG();
                else
                {
                    bytes = new Texture2D(2, 2).EncodeToPNG();
                }
                    

                File.WriteAllBytes(fullPath, bytes);
            }
            catch (Exception e)
            {
                RLMGLogger.Instance.Log(String.Format("The process failed: {0}.", e.ToString()), MESSAGETYPE.ERROR);
            }
        }

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

        /// <summary>
        /// Moves given file from srcDir to destDir
        /// </summary>
        /// <param name="srcDirPath">Path of dir to move from</param>
        /// <param name="dstDirPath">Path of dir to move to</param>
        /// <param name="filename">Name of file to move</param>
        public static void MoveFile(string srcDirPath, string dstDirPath, string filename)
        {
            DirectoryInfo srcDI = new DirectoryInfo(srcDirPath);
            DirectoryInfo dstDI = new DirectoryInfo(dstDirPath);

            try
            {
                if (!srcDI.Exists)
                {
                    srcDI.Create();
                    RLMGLogger.Instance.Log(String.Format("Src directory created successfully at {0}.", srcDirPath), MESSAGETYPE.INFO);
                }

                if (!dstDI.Exists)
                {
                    dstDI.Create();
                    RLMGLogger.Instance.Log(String.Format("Dst directory created successfully at {0}.", srcDirPath), MESSAGETYPE.INFO);
                }

                if (srcDI.Exists && dstDI.Exists)
                {
                    string srcFilePath = Path.Join(srcDirPath, filename);
                    string dstFilePath = Path.Join(dstDirPath, filename);
                    File.Copy(srcFilePath, dstFilePath, true);
                    File.Delete(srcFilePath);
                }
            }
            catch (Exception e)
            {
                RLMGLogger.Instance.Log(String.Format("The process failed: {0}.", e.ToString()), MESSAGETYPE.ERROR);
            }
        }

        //public static void ClearTrash(string trashPath)
        //{
        //    DirectoryInfo trashDI = new DirectoryInfo(trashPath);
        //    try
        //    {
        //        if (trashDI.Exists)
        //        {
        //            string[] pngList = Directory.GetFiles(trashPath, "*.png");
        //            for (int p = 0; p < pngList.Length; p++)
        //            {
        //                File.Delete(pngList[p]);
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        RLMGLogger.Instance.Log(String.Format("The process failed: {0}.",e.ToString()), MESSAGETYPE.ERROR);
        //    }
        //}

        public static void DeleteFolderContents(string fullPath)
        {
            DirectoryInfo di = new DirectoryInfo(fullPath);
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
