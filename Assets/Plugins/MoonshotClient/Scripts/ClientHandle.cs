//using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using rlmg.logging;

public class ClientHandle : MonoBehaviour
{
    public static void Join(Packet _packet)
    {
        string _msg = _packet.ReadString();
        int _clientId = _packet.ReadInt();

        Debug.Log($"Message from server: {_msg}");
        // bugbug
        //UIManager.instance.statusText.text = $"Message from server: {_msg}";

        Client.instance.clientId = _clientId;
        ClientSend.JoinReceived();

        //Client.instance.udp.Connect(((IPEndPoint)Client.instance.tcp.socket.Client.LocalEndPoint).Port);
    }

    public static void SendStationDataToClient(Packet _packet)
    {
        string _msg = _packet.ReadString();
        string _JsonStationData = _packet.ReadString();

        JsonUtility.FromJsonOverwrite(_JsonStationData, Client.instance.team.MoonshotTeamData);

        Debug.Log($"Message from server: {_msg}");
        // bugbug
        //UIManager.instance.statusText.text = $"Message from server: {_msg}";

        //ClientSend.StationDataReceived();
    }

    internal static void SendAllStationDataToClient(Packet _packet)
    {
        Debug.Log($"Send All Station Data To Client");

        string team1StationData = _packet.ReadString();
        string team2StationData = _packet.ReadString();
        Debug.Log(team2StationData);
        string team3StationData = _packet.ReadString();
        string team4StationData = _packet.ReadString();
        string team5StationData = _packet.ReadString();

        Client.instance.allStationData = new MoonshotTeamData[5]
        {
            JsonUtility.FromJson<MoonshotTeamData>(team1StationData),
            JsonUtility.FromJson<MoonshotTeamData>(team2StationData),
            JsonUtility.FromJson<MoonshotTeamData>(team3StationData),
            JsonUtility.FromJson<MoonshotTeamData>(team4StationData),
            JsonUtility.FromJson<MoonshotTeamData>(team5StationData)
        };

        Client.instance.ReceivedAllStationData();

    }

    public static void SendStartRoundToClient(Packet _packet)
    {
        Debug.Log($"Start Round");
        // bugbug
        //UIManager.instance.statusText.text = $"Start Round";

        string _teamName = _packet.ReadString();
        float _roundDuration = _packet.ReadFloat();
        float _roundBufferDuration = _packet.ReadFloat();
        int _round = _packet.ReadInt();
        string _JsonTeamData = _packet.ReadString();

        if (Client.instance.team == null)
        {
            Client.instance.CreateNewTeam();
        }

        if (!string.IsNullOrEmpty(_JsonTeamData))  //I think this is only normal for the first round?
        {
            JsonUtility.FromJsonOverwrite(_JsonTeamData, Client.instance.team.MoonshotTeamData);
        }
        else
        {
            if (_round != 0)
            {
                RLMGLogger.Instance.Log("Team JSON data received on 'start round' from server is null or empty, despite being round#" + _round + ". team name = " + _teamName, MESSAGETYPE.ERROR);
            }
            
            Client.instance.team.MoonshotTeamData = new MoonshotTeamData();
        }

        Client.instance.team.MoonshotTeamData.teamName = _teamName;

        Debug.Log($"Start round with: {_teamName}");

        Client.instance.StartRound(_teamName, _roundDuration, _roundBufferDuration, _round, _JsonTeamData);



        // // bugbug - this will happen towards the end of the round, here for testing.
        // Client.instance.team.MoonshotTeamData.didRoverActivity = true;

        // // bugbug - get art file from server
        // ClientSend.GetFileFromServer(Client.instance.team.MoonshotTeamData.artworks[0]);

        // // bugbug
        // //UIManager.instance.statusText.text = $"Team {Client.instance.team.Name} started round.";

        // ClientSend.SendStationDataToServer();
    }

    public static void SendPauseToClient(Packet _packet)
    {
        Client.instance.PauseMission();
    }

    public static void SendUnPauseToClient(Packet _packet)
    {
        Client.instance.UnPauseMission();
    }

    internal static void SendStartMissionToClient(Packet _packet)
    {
        // bugbug - not used yet... future proofing...
        int _missionTypeInt = _packet.ReadInt();
        MissionType _missionType = (MissionType)_missionTypeInt;

        Client.instance.StartMission();
    }

    internal static void SendStopMissionToClient(Packet _packet)
    {
        Client.instance.StopMission();
    }

    internal static void SendEndMissionToClient(Packet _packet)
    {
        Client.instance.EndMission();
    }
}
