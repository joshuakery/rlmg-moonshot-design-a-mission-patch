using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using OpenCVForUnity.UnityUtils.Helper;
using ArtScan.NamesakesModule;
using ArtScan.WordPoints;
using ArtScan.CoreModule;
using ArtScan.WordScoringUtilsModule;
using ArtScan.WordSavingUtilsModule;
using ArtScan.TeamsModule;
using ArtScan.ScanSavingModule;
using rlmg.logging;

namespace ArtScan
{
    [CreateAssetMenu(fileName = "GameState", menuName = "GameState", order = 0)]
    public class GameState : ScriptableObject {

        public int currentRound;
        public int currentTeamIndex;

        public MoonshotTeamData currentTeam
        {
            get
            {
                if (currentTeamIndex < teams.Count)
                    return teams[currentTeamIndex];
                else
                    return null;
            }
        }

        public List<MoonshotTeamData> teams;
        public WordPointsContent wordPointsContent;
        public Dictionary<string, Namesake> namesakesData = new Dictionary<string, Namesake>();

        public string saveFile;

        public Texture2D preview;
        public int scanMax;

        public Texture2D[] scans;
        public List<int> nextToReplace;

        public List<string> exclude;

        public RemoveBackgroundSettings settings;

        public UploadThreadController uploadThreadController;

        public void SwitchTeam(int i)
        {
            if (i < teams.Count)
            {
                currentTeamIndex = i;
                Reset();
            }
        }

        public void ClearCurrentTeamScores()
        {
            currentTeam.chosenWords = new List<string>();
            currentTeam.namesake = null;
        }

        public void SetNewNamesake()
        {
            currentTeam.namesake = WordScoring.GetWinner(currentTeam.chosenWords, teams, wordPointsContent.wordPoints);

            WordSaving.SaveTeamsToFile(saveFile, teams);

            if (Client.instance.team != null)
                ClientSend.SendStationDataToServer();
        }

        public void UpdateTeamDidActivity()
        {
            if (Client.instance != null && Client.instance.team != null && Client.instance.team.MoonshotTeamData != null)
            {
                Client.instance.team.MoonshotTeamData.didArtActivity = true;
                ClientSend.SendStationDataToServer();
            }

        }

        public void ClearScans()
        {
            scans = new Texture2D[scanMax];
            nextToReplace.Clear();
        }

        public void DownloadScans()
        {
            string dirPath = Path.Join(Application.streamingAssetsPath, settings.saveDir);
            //synchronous
            ScanSavingModule.ScanSaving.DownloadScans(dirPath, currentTeam);
        }

        public void ReadScans(myWebCamTextureToMatHelper webCamTextureToMatHelper)
        {
            ClearScans();

            string dirPath = Path.Join(Application.streamingAssetsPath, settings.saveDir);

            DirectoryInfo mainDI = new DirectoryInfo(dirPath);

            if (currentTeam.artworks != null && mainDI.Exists)
            {
                for (int i = 0; i < currentTeam.artworks.Length; i++)
                {
                    string filename = currentTeam.artworks[i];
                    if (!String.IsNullOrEmpty(filename))
                    {
                        string filepath = Path.Join(dirPath, filename);

                        Texture2D scanTexture = ScanSavingModule.ScanSaving.GetTexture2DFromImageFile(filepath, settings, webCamTextureToMatHelper);

                        AddScan(scanTexture, i);
                    }
                }
            }

            //ScanSaving.ReadScans(dirPath, AddScan, settings, webCamTextureToMatHelper);
        }

        public void TrashScanFromCurrentTeam(string filename, int index)
        {
            //remove from list of Texture2D
            Array.Clear(scans, index, 1);

            //remove from replacement order
            if (nextToReplace.Contains(index))
                nextToReplace.Remove(index);

            //do not remove from local artworks list - but since the image is deleted it won't matter
            //Array.Clear(currentTeam.artworks, index, 1);
            //WordSaving.SaveTeamsToFile(saveFile,teams);

            //do not remove from server artworks list - but since the image is deleted it won't matter
            //Array.Clear(Client.instance.team.MoonshotTeamData.artworks, index, 1);
            //ClientSend.SendStationDataToServer();

            TrashScan(filename);     
        }

        public void TrashScan(string filename)
        {
            string saveDirPath = Path.Join(Application.streamingAssetsPath, settings.saveDir);
            string trashDirPath = Path.Combine(Application.streamingAssetsPath, settings.trashDir);
            ScanSavingModule.ScanSaving.TrashScan(saveDirPath, trashDirPath, filename);

            //ClientSend.DeleteFileFromServer(filename);
            //DeleteThreadController.Delete(filename);
        }

        public void UnTrashScanFromCurrentTeam(string filename, int index, myWebCamTextureToMatHelper webCamTextureToMatHelper)
        {
            UnTrashScan(filename);

            string saveDirPath = Path.Join(Application.streamingAssetsPath, settings.saveDir);
            string fullPath = Path.Join(saveDirPath, filename);

            Texture2D untrashedScan = ScanSavingModule.ScanSaving.GetTexture2DFromImageFile(fullPath, settings, webCamTextureToMatHelper);
            AddScan(untrashedScan, index);
        }

        public void UnTrashScan(string filename)
        {
            string saveDirPath = Path.Join(Application.streamingAssetsPath, settings.saveDir);
            string trashDirPath = Path.Combine(Application.streamingAssetsPath, settings.trashDir);
            ScanSavingModule.ScanSaving.UnTrashScan(saveDirPath, trashDirPath, filename);

            //string fullPath = Path.Join(saveDirPath, filename);
            //ClientSend.SendFileToServer(fullPath);
            //UploadThreadController.Upload(fullPath);
        }

        public int AddScan(Texture2D newScan)
        {
            //If there's room, just add it to the list
            for (int i = 0; i < scans.Length; i++)
            {
                if (scans[i] == null)
                {
                    scans[i] = newScan;

                    if (nextToReplace == null) nextToReplace = new List<int>();
                    nextToReplace.Add(i);

                    return i;
                }
            }

            //Else replace the oldest scan
            if (nextToReplace.Count > 0)
            {
                int toRemove = nextToReplace[0];
                scans[toRemove] = newScan;

                nextToReplace.RemoveAt(0);
                nextToReplace.Add(toRemove);
                return toRemove;
            }
            //Should never happen
            else
            {
                int arbitraryIndex = 0;

                scans[arbitraryIndex] = newScan;
                nextToReplace.Add(arbitraryIndex);
                return arbitraryIndex;
            }
        }

        public int AddScan(Texture2D newScan, int index)
        {
            scans[index] = newScan;
            nextToReplace.Add(index);
            return index;
        }

        public int GetNextScanIndex()
        {
            //If there's room, just add it to the list
            for (int i = 0; i < scans.Length; i++)
            {
                if (scans[i] == null)
                {
                    return i;
                }
            }

            //Else replace the oldest scan
            if (nextToReplace.Count > 0)
            {
                int toRemove = nextToReplace[0];
                return toRemove;
            }
            //Should never happen
            else
            {
                int arbitraryIndex = 0;
                return arbitraryIndex;
            }
        }

        public void Reset()
        {
            ClearScans();
        }

        public void AddTeam(MoonshotTeamData team)
        {
            Debug.Log("Adding " + team.teamName);

            if (teams == null || teams.Count == 0)
            {
                teams = new List<MoonshotTeamData>();
            }

            teams.Add(team);
        }



    }
}


