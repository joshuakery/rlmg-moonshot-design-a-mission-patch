using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils.Helper;
using OpenCVForUnity.UnityUtils;
using ArtScan.NamesakesModule;
using ArtScan.WordPoints;
using ArtScan.CoreModule;
using ArtScan.PresentationUtilsModule;
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

        public SavedScanManager savedScanManager;

        public bool allScansEmpty
        {
            get
            {
                return savedScanManager.allScansEmpty;
            }
        }

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
            if (currentTeam != null)
            {
                currentTeam.chosenWords = new List<string>();
                currentTeam.namesake = null;
            }
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

        public void AddPreview()
        {
            savedScanManager.AddScanAsCopyOfTexture(preview);
        }

        /// <summary>
        /// Calls the static DownloadScans method to download and save images to the saveDir
        /// </summary>
        public void DownloadScans()
        {
            string dirPath = Path.Join(Application.streamingAssetsPath, settings.saveDir);
            //synchronous
            ScanSavingModule.ScanSaving.DownloadScans(dirPath, currentTeam);
        }

        /// <summary>
        /// Reads scans from file and adds them to the _scans array
        /// </summary>
        /// <param name="webCamTextureToMatHelper">Instance passed along to get Mat format</param>
        public void ReadScans(myWebCamTextureToMatHelper webCamTextureToMatHelper)
        {
            if (currentTeam != null)
                savedScanManager.ReadScans(currentTeam.artworks, webCamTextureToMatHelper);
        }

        public void Reset()
        {
            savedScanManager.ClearScans();
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


