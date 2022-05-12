using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.EventSystems;
using rlmg.logging;
using ArtScan;

namespace ArtScan.TeamsModule
{

    [System.Serializable]
	public class TeamsJSON
	{
		public Team[] teams;

        public TeamsJSON(Team[] n_teams)
        {
            teams = n_teams;
        }
		
	}

    [System.Serializable]
    public class TeamsJSONDeserializable
    {
        public List<Team> teams;
    }

    [Serializable]
    public class Team
    {
        public string directory;
        public string teamname;

        public string namesake;

        public List<string> chosenWords;

    }

    public class TeamsLoader : ContentLoader
    {
        public EventSystem eventSystem;

        public GameState gameState;

        private void SetupDefaultTeam()
        {
            Team defaultTeam = new Team();
            defaultTeam.directory = "defaultTeam";
            defaultTeam.teamname = "Default Team";
            defaultTeam.chosenWords = new List<string>();

            Team[] teams = new Team[] { defaultTeam };
            gameState.teams = teams;
        }

        protected override IEnumerator PopulateContent(string contentData)
        {
            gameState.saveFile = contentFilename;
            
            TeamsJSONDeserializable teamsJSON = JsonUtility.FromJson<TeamsJSONDeserializable>(contentData);

            if (teamsJSON == null)
            {
                SetupDefaultTeam();
                yield break;
            }

            if (gameState != null && teamsJSON != null)
            {
                gameState.teams = teamsJSON.teams.ToArray();
            }
                
            yield break;
        }
    }

}



