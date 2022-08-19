using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Client : MonoBehaviour
{
    private static Client _instance;
    public static Client instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = (Client)FindObjectOfType(typeof(Client));

                if (_instance != null && _instance.gameObject != null)
                {
                    DontDestroyOnLoad(_instance.gameObject);
                }

                //Debug.Log("Client get instance == null, so finding one in scene: " + _instance);
            }
            else
            {
                //Debug.Log("Client get instance != null");
            }

            return _instance;
        }
    }

    public bool connectOnStart = true;

    public static int dataBufferSize = 4096;

    //    public string ip = "10.0.0.33";
    public string ip = "localhost";
    public int port = 26950;
    public int ftpsPort = 2222;
    public string ftpsUsername;
    public string ftpsPassword;
    public int clientId = 0;
    public MoonshotStation _moonshotStation = MoonshotStation.Map;
    public TCP tcp;
    public UDP udp;

    //public StationData stationData;
    public Team team;
    public MoonshotTeamData[] allStationData;

    private bool isConnected = false;
    private delegate void PacketHandler(Packet _packet);
    private static Dictionary<int, PacketHandler> packetHandlers;

    public Image downloadImage;
    public string imagePath;

    public bool loadSceneByName = false;
    public string startMissionScene = "ActivitySceneExample";

    public delegate void OnStartRound(string _teamName, float _roundDuration, float _roundBufferDuration, int _round, string _JsonTeamData);
    public OnStartRound onStartRound;

    public delegate void OnPauseMission();
    public OnPauseMission onPauseMission;

    public delegate void OnUnPauseRound();
    public OnUnPauseRound onUnPauseMission;

    public delegate void OnStartMission();
    public OnStartMission onStartMission;

    public delegate void OnStopMission();
    public OnStopMission onStopMission;

    public delegate void OnReceivedAllStationData();
    public OnReceivedAllStationData onReceivedAllStationData;

    public delegate void OnEndMission();
    public OnEndMission onEndMission;

    private void Awake()
    {
        // if (Instance == null)
        // {
        //     Instance = this;

        //     DontDestroyOnLoad(Instance);
        // }
        // else 
        if (instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this.gameObject);
        }
    }

    private void Start()
    {
        Rebex.Licensing.Key = "==FkkUSiRcotfNUNeU8Kj6ljpRThAac6UXzZNxAvu4zzy+Ig2smsV/3eZJFZL8XKLXjVRSb==";

        imagePath = @"C:\Projects\rlmg\git\rlmg_moonshotsharedconclusion\Assets\StreamingAssets\Images";

        tcp = new TCP();
        //udp = new UDP();

        //bugbug this will be moved to where the workstation is initialized.
        //CreateNewTeam();
        if (connectOnStart)
        {
            ConnectToServer();
        }

        //------ test file upload -----
        //string imageFileName = "artwork_flag.png";

        //string imageFilePath = @"C:\temp\Images\";
        //ClientSend.SendFileToServer(imageFilePath + imageFileName);

        //string uploadDir = "Images";
        //string uploadPath = Path.Join(Application.streamingAssetsPath, uploadDir, imageFileName);
        //ClientSend.SendFileToServer(uploadPath);

        //team.MoonshotTeamData.artworks = new string[1];
        //team.MoonshotTeamData.artworks[0] = imageFileName;
        //ClientSend.SendStationDataToServer();
    }

    public void ConnectToServer()
    {
        Client.instance.ConnectToServer(_moonshotStation);
    }

    public void CreateNewTeam()
    {
        team = new Team();
        team.MoonshotTeamData = new MoonshotTeamData();
    }

    private void OnApplicationQuit()
    {
        Disconnect();
    }

    public void ConnectToServer(MoonshotStation moonshotStation)
    {
        _moonshotStation = moonshotStation;

        InitializeClientData();

        isConnected = true;
        tcp.Connect();
    }

    public class TCP
    {
        public TcpClient socket;

        private NetworkStream stream;
        private Packet receivedData;
        private byte[] receiveBuffer;

        public void Connect()
        {
            socket = new TcpClient
            {
                ReceiveBufferSize = dataBufferSize,
                SendBufferSize = dataBufferSize
            };

            receiveBuffer = new byte[dataBufferSize];
            try
            {
                string ipa = instance.ip;
                int p = instance.port;

                socket.BeginConnect(instance.ip, instance.port, ConnectCallback, socket);
            }
            catch (Exception ex)
            {
                // bugbug
                //UIManager.instance.statusText.text = ex.Message;
            }
        }

        private void ConnectCallback(IAsyncResult _result)
        {
            try
            {
                socket.EndConnect(_result);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                return;
            }

            if (!socket.Connected)
            {
                return;
            }

            stream = socket.GetStream();

            receivedData = new Packet();

            stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
        }

        public void SendData(Packet _packet)
        {
            try
            {
                if (socket != null)
                {
                    stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null, null);
                }
            }
            catch (Exception _ex)
            {
                Debug.Log($"Error sending data to server via TCP: {_ex}");
            }
        }

        private void ReceiveCallback(IAsyncResult _result)
        {
            try
            {
                int _byteLength = stream.EndRead(_result);
                if (_byteLength <= 0)
                {
                    instance.Disconnect();
                    return;
                }

                byte[] _data = new byte[_byteLength];
                Array.Copy(receiveBuffer, _data, _byteLength);

                receivedData.Reset(HandleData(_data));
                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
            }
            catch
            {
                Disconnect();
            }
        }

        private bool HandleData(byte[] _data)
        {
            int _packetLength = 0;

            receivedData.SetBytes(_data);

            if (receivedData.UnreadLength() >= 4)
            {
                _packetLength = receivedData.ReadInt();
                if (_packetLength <= 0)
                {
                    return true;
                }
            }

            while (_packetLength > 0 && _packetLength <= receivedData.UnreadLength())
            {
                byte[] _packetBytes = receivedData.ReadBytes(_packetLength);
                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (Packet _packet = new Packet(_packetBytes))
                    {
                        int _packetId = _packet.ReadInt();
                        packetHandlers[_packetId](_packet);
                    }
                });

                _packetLength = 0;
                if (receivedData.UnreadLength() >= 4)
                {
                    _packetLength = receivedData.ReadInt();
                    if (_packetLength <= 0)
                    {
                        return true;
                    }
                }
            }

            if (_packetLength <= 1)
            {
                return true;
            }

            return false;
        }

        private void Disconnect()
        {
            instance.Disconnect();

            stream = null;
            receivedData = null;
            receiveBuffer = null;
            socket = null;
        }
    }

    public class UDP
    {
        public UdpClient socket;
        public IPEndPoint endPoint;

        public UDP()
        {
            endPoint = new IPEndPoint(IPAddress.Parse(instance.ip), instance.port);
        }

        public void Connect(int _localPort)
        {
            socket = new UdpClient(_localPort);

            socket.Connect(endPoint);
            socket.BeginReceive(ReceiveCallback, null);

            using (Packet _packet = new Packet())
            {
                SendData(_packet);
            }
        }

        public void SendData(Packet _packet)
        {
            try
            {
                _packet.InsertInt(instance.clientId);
                if (socket != null)
                {
                    socket.BeginSend(_packet.ToArray(), _packet.Length(), null, null);
                }
            }
            catch (Exception _ex)
            {
                Debug.Log($"Error sending data to server via UDP: {_ex}");
            }
        }

        private void ReceiveCallback(IAsyncResult _result)
        {
            try
            {
                byte[] _data = socket.EndReceive(_result, ref endPoint);
                socket.BeginReceive(ReceiveCallback, null);

                if (_data.Length < 4)
                {
                    instance.Disconnect();
                    return;
                }

                HandleData(_data);
            }
            catch
            {
                Disconnect();
            }
        }

        private void HandleData(byte[] _data)
        {
            using (Packet _packet = new Packet(_data))
            {
                int _packetLength = _packet.ReadInt();
                _data = _packet.ReadBytes(_packetLength);
            }

            ThreadManager.ExecuteOnMainThread(() =>
            {
                using (Packet _packet = new Packet(_data))
                {
                    int _packetId = _packet.ReadInt();
                    packetHandlers[_packetId](_packet);
                }
            });
        }

        private void Disconnect()
        {
            instance.Disconnect();

            endPoint = null;
            socket = null;
        }
    }

    private void InitializeClientData()
    {
        packetHandlers = new Dictionary<int, PacketHandler>()
        {
            { (int)ServerPackets.join, ClientHandle.Join },
            { (int)ServerPackets.sendStationDataToClient, ClientHandle.SendStationDataToClient },
            { (int)ServerPackets.sendStartRoundToClient, ClientHandle.SendStartRoundToClient},
            { (int)ServerPackets.sendStartMissionToClient, ClientHandle.SendStartMissionToClient},
            { (int)ServerPackets.sendStopMissionToClient, ClientHandle.SendStopMissionToClient},
            { (int)ServerPackets.sendPauseMissionToClient, ClientHandle.SendPauseToClient},
            { (int)ServerPackets.sendUnPauseMissionToClient, ClientHandle.SendUnPauseToClient},
            { (int)ServerPackets.sendAllStationDataToClient, ClientHandle.SendAllStationDataToClient},
            { (int)ServerPackets.sendEndMissionToClient, ClientHandle.SendEndMissionToClient},
        };
        Debug.Log("Initialized packets.");
    }

    private void Disconnect()
    {
        if (isConnected)
        {
            isConnected = false;
            if (tcp != null && tcp.socket != null)
            {
                tcp.socket.Close();
            }
            if (udp != null && udp.socket != null)
            {
                udp.socket.Close();
            }

            Debug.Log("Disconnected from server.");
        }
    }

    public void GetFileFromServer()
    {
        StartCoroutine(LoadImage());
    }

    IEnumerator LoadImage()
    {
        using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(imagePath + @"\download\eagle.png"))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("Error : " + webRequest.error);
            }

            else
            {
                Texture imgTexture = ((DownloadHandlerTexture)webRequest.downloadHandler).texture;

                downloadImage.sprite = Sprite.Create((Texture2D)imgTexture, new Rect(0.0f, 0.0f, imgTexture.width, imgTexture.height), new Vector2(0.5f, 0.5f));
            }

            webRequest.Dispose();
        }
    }

    public void StartRound(string _teamName, float _roundDuration, float _roundBufferDuration, int _round, string _JsonTeamData)
    {
        Debug.Log("Client.StartRound()");

        LoadMissionScene(false);

        //because of the prior scene load, wait a moment for any callbacks to be initiated in OnEnable() or Start()
        StopCoroutine(SendStartRoundCallbackOnSlightDelay(_teamName, _roundDuration, _roundBufferDuration, _round, _JsonTeamData));
        StartCoroutine(SendStartRoundCallbackOnSlightDelay(_teamName, _roundDuration, _roundBufferDuration, _round, _JsonTeamData));
    }

    private void LoadMissionScene(bool forceReload = true)
    {
        bool startSceneIsActive = loadSceneByName && !String.IsNullOrEmpty(startMissionScene) ?
            (SceneManager.GetActiveScene() == SceneManager.GetSceneByName(startMissionScene)) :
            (SceneManager.GetActiveScene() == SceneManager.GetSceneByBuildIndex(0));

        if (forceReload || !startSceneIsActive)
        {

            if (loadSceneByName && !System.String.IsNullOrEmpty(startMissionScene))
            {
                Debug.Log(String.Format("Loading scene {0}", startMissionScene));
                SceneManager.LoadScene(startMissionScene);
            }
            else
            {
                Debug.Log("Loading scene 0");
                SceneManager.LoadScene(0);
            }

        }
        else
        {
            Debug.Log("didn't load new scene because scene 0 is already the active scene and 'force reload' was false.   active scene = " + SceneManager.GetActiveScene().name + "   scene 0 = " + SceneManager.GetSceneByBuildIndex(0).name);
        }
    }

    IEnumerator SendStartRoundCallbackOnSlightDelay(string _teamName, float _roundDuration, float _roundBufferDuration, int _round, string _JsonTeamData)
    {
        yield return null;  //wait one frame

        if (onStartRound != null)
        {
            onStartRound(_teamName, _roundDuration, _roundBufferDuration, _round, _JsonTeamData);
        }

        // bugbug testing
        ClientSend.SendStationDataToServer();
    }

    internal void StartMission()
    {
        // todo - start mission....

        LoadMissionScene(false);

        //because of the prior scene load, wait a moment for any callbacks to be initiated in OnEnable() or Start()
        StopCoroutine(SendStartMissionCallbackOnSlightDelay());
        StartCoroutine(SendStartMissionCallbackOnSlightDelay());
    }

    IEnumerator SendStartMissionCallbackOnSlightDelay()
    {
        yield return null;  //wait one frame

        if (onStartMission != null)
        {
            onStartMission();
        }
    }

    internal void StopMission()
    {
        // todo - stop mission....

        LoadMissionScene();

        //because of the prior scene load, wait a moment for any callbacks to be initiated in OnEnable() or Start()
        StopCoroutine(SendStopMissionCallbackOnSlightDelay());
        StartCoroutine(SendStopMissionCallbackOnSlightDelay());
    }

    internal void EndMission()
    {
        if (onEndMission != null)
        {
            onEndMission();
        }
    }

    IEnumerator SendStopMissionCallbackOnSlightDelay()
    {
        yield return null;  //wait one frame

        if (onStopMission != null)
        {
            onStopMission();
        }
    }

    internal void PauseMission()
    {
        // todo - Pause stuff....

        if (onPauseMission != null)
        {
            onPauseMission();
        }
    }

    internal void UnPauseMission()
    {
        // todo - UnPause stuff...

        if (onUnPauseMission != null)
        {
            onUnPauseMission();
        }
    }

    public void ReceivedAllStationData()
    {
        if (onReceivedAllStationData != null)
            onReceivedAllStationData();
    }
}
