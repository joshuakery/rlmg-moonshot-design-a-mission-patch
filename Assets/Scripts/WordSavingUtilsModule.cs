using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ArtScan;
using ArtScan.TeamsModule;
using rlmg.logging;

namespace ArtScan.WordSavingUtilsModule
{

    public static class WordSaving
    {
        public static void SaveTeamsToFile(string saveFile, Team[] teams)
        {
            TeamsJSON teamsJSON = new TeamsJSON(teams);
            string json = JsonUtility.ToJson(teamsJSON, true);
            
            string filepath = Path.Join(Application.streamingAssetsPath, saveFile);
            File.WriteAllText(filepath, json);
        }
        public static void DeleteFile(string saveFile)
        {
            string filepath = Path.Join(Application.streamingAssetsPath, saveFile);
            if (File.Exists(filepath))
                File.Delete(filepath);
        }

        public static void SetTeamsFromFile(string saveFile, Team[] teams)
        {
            string filepath = Path.Join(Application.streamingAssetsPath, saveFile);
            if (File.Exists(filepath))
            {
                using (StreamReader r = new StreamReader(filepath))
                {
                    string json = r.ReadToEnd();
                    if (json.Length > 0)
                    {
                        TeamsJSON teamsJSON = JsonUtility.FromJson<TeamsJSON>(json);
                        teams = teamsJSON.teams;
                    }
                }
            }
        }
    }
}
