using System.Linq;
using UnityEngine;
using System.Collections;
using ArtScan;
using rlmg.logging;

public class ClientSubscriber : MonoBehaviour
{
    public UIManager uiManager;
    public MoonshotTimer.Timer mainTimer;
    public MoonshotTimer.Timer closeTimer;

    public GameState gameState;

    public bool useServer;

    private void OnEnable()
    {
        if (useServer)
        {
            Client.instance.onStartRound += StartRound;
            Client.instance.onPauseMission += PauseMission;
            Client.instance.onUnPauseMission += UnPauseMission;
            Client.instance.onReceivedAllStationData += HandleAllStationData;
            Client.instance.onEndMission += EndMission;
        }
    }

    private void OnDisable()
    {
        if (Client.instance != null)
        {
            Client.instance.onStartRound -= StartRound;
            Client.instance.onPauseMission -= PauseMission;
            Client.instance.onUnPauseMission -= UnPauseMission;
            Client.instance.onReceivedAllStationData -= HandleAllStationData;
            Client.instance.onEndMission -= EndMission;
        }
    }

    public void StartRound(string _teamName, float _roundDuration, float _roundBufferDuration, int _round, string _JsonTeamData)
    {
        //Reset Timer
        mainTimer.duration = _roundDuration;
        mainTimer.Reset();
        mainTimer.StartCounting();

        //Close Timer - just for animating the station to a close
        closeTimer.duration = _roundDuration + _roundBufferDuration - 1; //close 1 second before next round
        closeTimer.Reset();
        closeTimer.StartCounting();

        //Update Existing Team data or Add Team to gameState
        string[] teamNames = gameState.teams.Select(t => t.teamName).ToArray();
        if (teamNames.Contains(Client.instance.team.MoonshotTeamData.teamName))
        {
            int index = gameState.teams.FindIndex(t => t.teamName == Client.instance.team.MoonshotTeamData.teamName);
            gameState.teams[index] = Client.instance.team.MoonshotTeamData;
            gameState.SwitchTeam(index);
        }
        else
        {
            gameState.AddTeam(Client.instance.team.MoonshotTeamData);
            gameState.SwitchTeam(gameState.teams.Count - 1);
        }

        ClientSend.RequestAllStationData();

        uiManager.ResetGame(true); //do open Welcome window
    }

    private void PauseMission()
    {
        mainTimer.PauseCounting();
        closeTimer.PauseCounting();
    }

    private void UnPauseMission()
    {
        mainTimer.StartCounting();
        closeTimer.StartCounting();
    }

    private void HandleAllStationData()
    {
        if (Client.instance.allStationData == null)
        {
            RLMGLogger.Instance.Log("The all station data on the client instance is null.", MESSAGETYPE.ERROR);
            return;
        }
        else if (Client.instance.allStationData.Length != 5)
        {
            RLMGLogger.Instance.Log("The all station data's length on the client instance is not 5.", MESSAGETYPE.ERROR);
            return;
        }

        //Update Existing Team Data or Add Team to gameState
        string[] teamNames = gameState.teams.Select(t => t.teamName).ToArray();
        foreach (MoonshotTeamData teamData in Client.instance.allStationData)
        {
            if (teamData != null)
            {
                if (teamNames.Contains(teamData.teamName))
                {
                    int index = gameState.teams.FindIndex(t => t.teamName == teamData.teamName);
                    gameState.teams[index] = teamData; //overwrite that existing team's data
                }
                else
                    gameState.AddTeam(teamData);
            }
        }
    }

    private void EndMission()
    {
        uiManager.GoToConclusion();
    }


}

