using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using rlmg.logging;

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

    public float connectionTimeoutDur = 5f;
    private float lastConnectAttemptTime;
    private float lastDisconnectTime;
    public float reconnectDelay = 1f;

    //public StationData stationData;
    public Team team;
    public MoonshotTeamData[] allStationData;
    public MissionState missionState;

    private bool isConnected = false;
    private delegate void PacketHandler(Packet _packet);
    private static Dictionary<int, PacketHandler> packetHandlers;

    public Image downloadImage;
    public string imagePath;

    public bool loadSceneByName = false;
    public string startMissionScene = "ActivitySceneExample";
    public string sharedConclusionScene = "ResultsScene";

    public delegate void OnConnect();
    public OnConnect onConnect;
    
    public delegate void OnDisconnect();
    public OnDisconnect onDisconnect;

    public delegate void OnStartRound(string _teamName, float _roundDuration, float _roundBufferDuration, int _round, string _JsonTeamData);
    public OnStartRound onStartRound;

    public delegate void OnResumeRound(string _teamName, float _roundDurationRemaining, float _roundBufferDurationRemaining, MissionState _missionState, int _round, string _JsonTeamData);
    public OnResumeRound onResumeRound;

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

    private IEnumerator delayedCallbackCoroutine;

    private bool didInitialSetup = false;
    private bool didApplicationQuit = false;

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
        DoInitialSetupIfNecessary();

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

    private void DoInitialSetupIfNecessary()
    {
        if (didInitialSetup)
        {
            return;
        }
        
        Rebex.Licensing.Key = "==FkkUSiRcotfNUNeU8Kj6ljpRThAac6UXzZNxAvu4zzy+Ig2smsV/3eZJFZL8XKLXjVRSb==";

        imagePath = @"C:\Projects\rlmg\git\rlmg_moonshotsharedconclusion\Assets\StreamingAssets\Images";

        tcp = new TCP();
        //udp = new UDP();

        didInitialSetup = true;
    }

    private void Update()
    {
        if (isConnected && tcp != null && !tcp.IsConnected && Time.time > lastConnectAttemptTime + connectionTimeoutDur)
        {
            RLMGLogger.Instance.Log("Connection to (tcp) server timed out after " + connectionTimeoutDur + " seconds.", MESSAGETYPE.ERROR);

            Disconnect();
        }

        if (!isConnected && !didApplicationQuit && Time.time > lastDisconnectTime + reconnectDelay)
        {
            RLMGLogger.Instance.Log("Attempting reconnect (after being disconnected for " + reconnectDelay + " seconds)...", MESSAGETYPE.INFO);

            ConnectToServer();
        }
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
        didApplicationQuit = true;
        
        Disconnect();
    }

    public void ConnectToServer(MoonshotStation moonshotStation)
    {
        DoInitialSetupIfNecessary();
        
        _moonshotStation = moonshotStation;

        InitializeClientData();

        isConnected = true;
        lastConnectAttemptTime = Time.time;
        tcp.Connect();
    }

    public void ConnectCallback()
    {
        //Debug.Log("Client.instance.ConnectCallback()");
        
        if (onConnect != null)
        {
            onConnect();
        }
    }

    public class TCP
    {
        public TcpClient socket;

        private NetworkStream stream;
        private Packet receivedData;
        private byte[] receiveBuffer;

        private bool isConnected = false;
        public bool IsConnected { get{ return isConnected; } }

        public void Connect()
        {
            isConnected = false;
            
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

                RLMGLogger.Instance.Log(ex.Message, MESSAGETYPE.ERROR);
            }
        }

        private void ConnectCallback(IAsyncResult _result)
        {
            //Debug.Log("TCP.ConnectCallback()");
            
            try
            {
                socket.EndConnect(_result);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);  //While not too concerning, I was getting some null reference errors here that I was oddly struggling to check for. -JY
                return;
            }

            if (!socket.Connected)
            {
                return;
            }

            stream = socket.GetStream();

            receivedData = new Packet();

            //Debug.Log("BeginRead of receiveBuffer with ReceiveCallback in ConnectCallback");
            stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);

            isConnected = true;

            //instance.ConnectCallback();  //I was having weird problems with the delegate function called from this, so I instead tied it to ClientHandle.Join()
        }

        public void SendData(Packet _packet)
        {
            try
            {
                if (socket != null)
                {
                    stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), BeginWriteComplete, null);
                }
            }
            catch (Exception _ex)
            {
                //Debug.Log($"Error sending data to server via TCP: {_ex}");
                RLMGLogger.Instance.Log($"Error sending data to server via TCP: {_ex}", MESSAGETYPE.ERROR);
            }
        }

        private void BeginWriteComplete(IAsyncResult _result)
        {
            //Debug.Log("Begin Write Complete");
            try
            {
                stream.EndWrite(_result);
            }
            catch (Exception _ex)
            {
                RLMGLogger.Instance.Log($"Error sending data to server via UDP: {_ex}", MESSAGETYPE.ERROR);
            }
        }

        private void ReceiveCallback(IAsyncResult _result)
        {
            //Debug.Log("Calling TCP Receive Callback");
            try
            {
                int _byteLength = stream.EndRead(_result);
                if (_byteLength <= 0)
                {
                    Debug.Log("Byte length was 0 so Disconnecting instance.");
                    instance.Disconnect();
                    return;
                }

                byte[] _data = new byte[_byteLength];
                Array.Copy(receiveBuffer, _data, _byteLength);

                receivedData.Reset(HandleData(_data));
                //Debug.Log("BeginRead of receiveBuffer with ReceiveCallback in ReceiveCallback");
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
            //Debug.Log("Calling TCP Disconnect");
            instance.Disconnect();

            stream = null;
            receivedData = null;
            receiveBuffer = null;
            socket = null;

            isConnected = false;
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
                //Debug.Log($"Error sending data to server via UDP: {_ex}");
                RLMGLogger.Instance.Log($"Error sending data to server via UDP: {_ex}", MESSAGETYPE.ERROR);
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
            { (int)ServerPackets.sendErrorToClient, ClientHandle.SendErrorToClient},
            { (int)ServerPackets.sendResumeRoundToClient, ClientHandle.SendResumeRoundToClient}
        };
        //Debug.Log("Initialized packets.");
    }

    public void Disconnect()
    {
        //Debug.Log("Calling Client Disconnect");
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

            lastDisconnectTime = Time.time;

            //Debug.Log("Disconnected from server.");
            RLMGLogger.Instance.Log("Disconnected from server.", MESSAGETYPE.INFO);

            if (onDisconnect != null)
            {
                onDisconnect();
            }
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
                //Debug.Log("Error : " + webRequest.error);
                RLMGLogger.Instance.Log("Load image error : " + webRequest.error, MESSAGETYPE.ERROR);
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
        //Debug.Log("Client.StartRound()");
        RLMGLogger.Instance.Log("Client received 'start round' from server.", MESSAGETYPE.INFO);

        LoadMissionScene(false);

        //because of the prior scene load, wait a moment for any callbacks to be initiated in OnEnable() or Start()
        //StopCoroutine(SendStartRoundCallbackOnSlightDelay(_teamName, _roundDuration, _roundBufferDuration, _round, _JsonTeamData));
        if (delayedCallbackCoroutine != null)
        {
            StopCoroutine(delayedCallbackCoroutine);
        }
        delayedCallbackCoroutine = SendStartRoundCallbackOnSlightDelay(_teamName, _roundDuration, _roundBufferDuration, _round, _JsonTeamData);
        StartCoroutine(delayedCallbackCoroutine);
    }

    public void ResumeRound(string _teamName, float _roundDurationRemaining, float _roundBufferDurationRemaining, MissionState _missionState, int _round, string _JsonTeamData)
    {
        RLMGLogger.Instance.Log("Client received 'resume round' from server.   missionState = " + _missionState.ToString(), MESSAGETYPE.INFO);

        if (_roundDurationRemaining > 0 || _roundBufferDurationRemaining > 0)  //is still in activity or activity conclusion with "buffer" timer
        {
            LoadMissionScene(false);
        }
        else
        {
            LoadSharedConclusionScene(false);
        }

        //because of the prior scene load, wait a moment for any callbacks to be initiated in OnEnable() or Start()
        if (delayedCallbackCoroutine != null)
        {
            StopCoroutine(delayedCallbackCoroutine);
        }
        delayedCallbackCoroutine = SendResumeRoundCallbackOnSlightDelay(_teamName, _roundDurationRemaining, _roundBufferDurationRemaining, _missionState, _round, _JsonTeamData);
        StartCoroutine(delayedCallbackCoroutine);
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
                Debug.Log(String.Format("Loading scene {0}",startMissionScene));
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
            Debug.Log("didn't load new 'activity' scene because it is already the active scene and 'force reload' was false.   active scene = " + SceneManager.GetActiveScene().name);
        }
    }

    private void LoadSharedConclusionScene(bool forceReload = true)
    {
        bool sceneIsAlreadyActive = loadSceneByName && !String.IsNullOrEmpty(sharedConclusionScene) ?
            (SceneManager.GetActiveScene() == SceneManager.GetSceneByName(sharedConclusionScene)) :
            (SceneManager.GetActiveScene() == SceneManager.GetSceneByBuildIndex(0));

        if (forceReload || !sceneIsAlreadyActive)
        {
            
            if (loadSceneByName && !System.String.IsNullOrEmpty(sharedConclusionScene))
            {
                Debug.Log(String.Format("Loading scene {0}",sharedConclusionScene));
                SceneManager.LoadScene(sharedConclusionScene);
            }
            else
            {
                Debug.Log("Loading scene 1");
                SceneManager.LoadScene(1);
            }
        }
        else
        {
            Debug.Log("didn't load new 'shared conclusion' scene because it is already the active scene and 'force reload' was false.   active scene = " + SceneManager.GetActiveScene().name);
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

    IEnumerator SendResumeRoundCallbackOnSlightDelay(string _teamName, float _roundDurationRemaining, float _roundBufferDurationRemaining, MissionState _missionState, int _round, string _JsonTeamData)
    {
        yield return null;  //wait one frame

        if (onResumeRound != null)
        {
            onResumeRound(_teamName, _roundDurationRemaining, _roundBufferDurationRemaining, _missionState, _round, _JsonTeamData);
        }

        // bugbug testing
        //ClientSend.SendStationDataToServer();  //do we want this?
    }

    internal void StartMission()
    {
        // todo - start mission....

        RLMGLogger.Instance.Log("Client received 'start mission' from server.", MESSAGETYPE.INFO);

        LoadMissionScene(false);

        //because of the prior scene load, wait a moment for any callbacks to be initiated in OnEnable() or Start()
        //StopCoroutine(SendStartMissionCallbackOnSlightDelay());
        if (delayedCallbackCoroutine != null)
        {
            StopCoroutine(delayedCallbackCoroutine);
        }
        delayedCallbackCoroutine = SendStartMissionCallbackOnSlightDelay();
        StartCoroutine(delayedCallbackCoroutine);
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

        RLMGLogger.Instance.Log("Client received 'stop mission' from server.", MESSAGETYPE.INFO);

        LoadMissionScene();

        //because of the prior scene load, wait a moment for any callbacks to be initiated in OnEnable() or Start()
        //StopCoroutine(SendStopMissionCallbackOnSlightDelay());
        if (delayedCallbackCoroutine != null)
        {
            StopCoroutine(delayedCallbackCoroutine);
        }
        delayedCallbackCoroutine = SendStopMissionCallbackOnSlightDelay();
        StartCoroutine(delayedCallbackCoroutine);
    }

    internal void EndMission()
    {
        RLMGLogger.Instance.Log("Client received 'end mission' from server.", MESSAGETYPE.INFO);
        
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
        RLMGLogger.Instance.Log("Client received 'pause mission' from server.", MESSAGETYPE.INFO);

        if (onPauseMission != null)
        {
            onPauseMission();
        }
    }

    internal void UnPauseMission()
    {
        RLMGLogger.Instance.Log("Client received 'unpause mission' from server.", MESSAGETYPE.INFO);

        if (onUnPauseMission != null)
        {
            onUnPauseMission();
        }
    }

    public void ReceivedAllStationData()
    {
        if (onReceivedAllStationData != null)
        {
            onReceivedAllStationData();
        }
    }
}
