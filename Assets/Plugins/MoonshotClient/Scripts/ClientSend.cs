//using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Rebex.IO;



public class ClientSend : MonoBehaviour
{
    private static void SendTCPData(Packet _packet)
    {
        _packet.WriteLength();
        Client.instance.tcp.SendData(_packet);
    }

    private static void SendUDPData(Packet _packet)
    {
        _packet.WriteLength();
        Client.instance.udp.SendData(_packet);
    }

    #region Packets
    public static void JoinReceived()
    {
        using (Packet _packet = new Packet((int)ClientPackets.joinReceived))
        {
            _packet.Write(Client.instance.clientId);
            _packet.Write((int)Client.instance._moonshotStation);

            SendTCPData(_packet);
            //SendUDPData(_packet);
        }
    }

    public static void SendStationDataToServer()
    {
        using (Packet _packet = new Packet((int)ClientPackets.sendStationDataToServer))
        {
            _packet.Write(Client.instance.clientId);

            //Client.instance.team.MoonshotTeamData.roverStepsCompleted = 1;

            //Client.instance.team.TeamData.artworks.SetValue("test", 0);

            // bugbug - testing
            // Client.instance.team.MoonshotTeamData.didRoverActivity = true;

            string output = JsonUtility.ToJson(Client.instance.team.MoonshotTeamData, true); // JsonConvert.SerializeObject(Client.instance.team.StationData);
            _packet.Write(output);

            SendTCPData(_packet);

            // bugbug
            //UIManager.instance.statusText.text = $"Sending station data to server";

        }
    }

    public static void SendStartMissionToServer()
    {
        using (Packet _packet = new Packet((int)ClientPackets.sendStartMissionToServer))
        {
            //_packet.Write(Client.instance.myId);

            SendTCPData(_packet);
            //SendUDPData(_packet);

            // bugbug
            //UIManager.instance.statusText.text = $"Sending start mission to server";
        }
    }

    internal static void SendStopMissionToServer()
    {
        using (Packet _packet = new Packet((int)ClientPackets.sendStopMissionToServer))
        {
            SendTCPData(_packet);
            // bugbug
            //UIManager.instance.statusText.text = $"Sending stop mission to server";
        }
    }

    public static void SendFileToServer(string filename)
    {
        using (var client = new Rebex.Net.Sftp())
        {
            try
            {
                // connect and log in
                client.Connect(Client.instance.ip, Client.instance.ftpsPort);
                client.Login(Client.instance.ftpsUsername, Client.instance.ftpsUsername);

                // upload a file
                client.Upload(filename, "/",
                    Rebex.IO.TraversalMode.MatchFilesDeep,
                    Rebex.IO.TransferMethod.Copy,
                    Rebex.IO.ActionOnExistingFiles.OverwriteAll
                 );
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
            }
        }
    }

    public static void GetFileFromServer(string filename, string destDir)
    {
        using (var client = new Rebex.Net.Sftp())
        {
            try
            {
                // connect and log in
                client.Connect(Client.instance.ip, Client.instance.ftpsPort);
                client.Login(Client.instance.ftpsUsername, Client.instance.ftpsUsername);

                // download a file
                //client.Download(filename, Client.instance.imagePath + @"\download\");
                // download a file with local overwrite privileges
                client.Download(filename, destDir, TraversalMode.Recursive, TransferMethod.Copy, ActionOnExistingFiles.OverwriteAll);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
            }

        }
    }

    public static void DeleteFileFromServer(string filename)
    {
        using (var client = new Rebex.Net.Sftp())
        {
            try
            {
                // connect and log in
                client.Connect(Client.instance.ip, Client.instance.ftpsPort);
                client.Login(Client.instance.ftpsUsername, Client.instance.ftpsUsername);

                //delete file
                client.DeleteFile(filename);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
            }
        }
    }

    public static void SendPauseMissionToServer()
    {
        using (Packet _packet = new Packet((int)ClientPackets.sendPauseMissionToServer))
        {
            SendTCPData(_packet);

            // bugbug
            //UIManager.instance.statusText.text = $"Sending pause mission to server";
        }
    }

    public static void SendUnPauseMissionToServer()
    {
        using (Packet _packet = new Packet((int)ClientPackets.sendUnPauseMissionToServer))
        {
            SendTCPData(_packet);

            // bugbug
            //UIManager.instance.statusText.text = $"Sending un-pause mission to server";
        }
    }

    public static void RequestAllStationData()
    {
        using (Packet _packet = new Packet((int)ClientPackets.requestAllStationData))
        {
            SendTCPData(_packet);
        }
    }


    #endregion
}
