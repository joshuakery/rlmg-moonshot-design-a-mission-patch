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

    [Serializable]
    public class TrashHistory
    {
        public int index;
        public string teamDir;

        public TrashHistory(int m_index, string m_teamDir)
        {
            index = m_index;
            teamDir = m_teamDir;
        }
    }

    [CreateAssetMenu(fileName = "GameState", menuName = "GameState", order = 0)]
    public class GameState : ScriptableObject {

        public int currentRound;
        public int currentTeam;

        public Team[] teams;
        public WordPointsContent wordPointsContent;
        public Dictionary<string,Namesake> namesakesData = new Dictionary<string,Namesake>();

        public string saveFile;

        public Texture2D preview;
        public int scanMax;

        public Texture2D[] scans;
        public List<TrashHistory> trashHistory;
        public int lastClearedIndex;
        
        public List<string> exclude;

        public RemoveBackgroundSettings settings;

        public void OnSwitchTeam(int i)
        {
            if (i < teams.Length)
            {
                currentTeam = i;
                Reset();
            }  
        }

        public void ClearCurrentTeamScores()
        {
            teams[currentTeam].chosenWords = new List<string>();
            teams[currentTeam].namesake = null;
        }

        public void SetNewNamesake()
        {
            teams[currentTeam].namesake = WordScoring.GetWinner(teams[currentTeam].chosenWords, teams, wordPointsContent.wordPoints);

            WordSaving.SaveTeamsToFile(saveFile, teams);
        }

        public void ClearScans()
        {
            scans = new Texture2D[scanMax];

            lastClearedIndex = 0;
        }

        public void SavePreview(RefinedScanController refinedScanController)
        {
            if (refinedScanController.previewMat != null)
            {
                int index = AddScan(preview);
                if (index >= 0)
                {
                    string[] paths = new string[] {
                        Application.streamingAssetsPath,
                        settings.saveDir,
                        teams[currentTeam].directory
                    };
                    string saveDirPath = Path.Combine(paths);
                    ScanSaving.SaveScan(refinedScanController.previewMat, saveDirPath, index);
                }
            }
            
        }

        public void ReadScans(myWebCamTextureToMatHelper webCamTextureToMatHelper)
        {
            string[] paths = new string[] {
                Application.streamingAssetsPath,
                settings.saveDir,
                teams[currentTeam].directory
            };
            string readPath = Path.Combine(paths);
            ScanSaving.ReadScans(AddScan,readPath,settings,webCamTextureToMatHelper);
        }

        public void TrashScanFromCurrentTeam(int index)
        {
            string[] paths = new string[] {
                Application.streamingAssetsPath,
                settings.saveDir,
                teams[currentTeam].directory
            };
            string savePath = Path.Combine(paths);
            string trashPath = Path.Combine(Application.streamingAssetsPath,settings.trashDir);
            ScanSaving.TrashScan(savePath,trashPath,index);
        }     

        public int AddScan(Texture2D newScan)
        {
            for (int i=0; i<scans.Length; i++ )
            {
                if (scans[i] == null)
                {
                    scans[i] = newScan;
                    return i;
                } 
            }

            lastClearedIndex = (lastClearedIndex + 1) % scans.Length;
            scans[lastClearedIndex] = newScan;
            return lastClearedIndex;
            
        }

        public int AddScan(Texture2D newScan, int index)
        {
            scans[index] = newScan;
            return index;
        }

        public void Reset()
        {
            ClearScans();
        }

    }
}


