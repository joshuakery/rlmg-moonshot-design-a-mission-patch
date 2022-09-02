using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
		public List<MoonshotTeamData> teams;

        public TeamsJSON(List<MoonshotTeamData> n_teams)
        {
            teams = n_teams;
        }
	}

    [System.Serializable]
    public class TeamsJSONDeserializable
    {
        public List<MoonshotTeamData> teams;
    }

    public class TeamsLoader : ContentLoader
    {
        public EventSystem eventSystem;

        public GameState gameState;

        public bool doLoadDefaultTeamOnly = false;

        private void SetupDefaultTeam()
        {
            MoonshotTeamData defaultTeam = new MoonshotTeamData();
            defaultTeam.teamName = "Default Team";
            defaultTeam.chosenWords = new List<string>();

            gameState.AddTeam(defaultTeam);
        }

        protected override IEnumerator PopulateContent(string contentData)
        {
            gameState.saveFile = contentFilename;
            
            TeamsJSONDeserializable teamsJSON = JsonUtility.FromJson<TeamsJSONDeserializable>(contentData);

            if (teamsJSON == null)
            {
                SetupDefaultTeam();
                gameState.currentTeamIndex = 0;
                yield break;
            }

            if (gameState != null && teamsJSON != null)
            {
                if (gameState.teams.Count == 0)
                {
                    if (teamsJSON.teams.Count == 0)
                    {
                        SetupDefaultTeam();
                        gameState.currentTeamIndex = 0;
                    }
                    else
                    {
                        if (doLoadDefaultTeamOnly)
                        {
                            string[] teamNames = teamsJSON.teams.Select(t => t.teamName).ToArray();
                            if (teamNames.Contains("Default Team"))
                            {
                                int index = teamsJSON.teams.FindIndex(t => t.teamName == "Default Team");
                                gameState.AddTeam(teamsJSON.teams[index]);
                                gameState.currentTeamIndex = 0;
                            }
                            else
                            {
                                SetupDefaultTeam();
                                gameState.currentTeamIndex = 0;
                            }

                            yield break;
                        }

                        foreach (MoonshotTeamData team in teamsJSON.teams)
                            gameState.AddTeam(team);

                        gameState.currentTeamIndex = gameState.teams.Count - 1;
                    }
                }

                    
            }
                
            yield break;
        }
    }

}



