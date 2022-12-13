using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientConfigLoader : ContentLoader
{
	[System.Serializable]
	public class ConfigJSON
	{
		public string serverAddress;
		public int port;
        public float connectionTimeout;
        public int ftpsPort;
        public string ftpsUsername;
        public string ftpsPassword;
		public string stationOverride;
    }

    protected override IEnumerator PopulateContent(string contentData)
	{
		//ConfigJSON configData = JsonConvert.DeserializeObject<ConfigJSON>(contentData);
		ConfigJSON configData = new ConfigJSON();
        JsonUtility.FromJsonOverwrite(contentData, configData);

        if (configData == null)
        {
            yield break;
        }

        if (Client.instance != null)
        {
            Client.instance.ip = configData.serverAddress;
            Client.instance.port = configData.port;
            Client.instance.connectionTimeoutDur = configData.connectionTimeout;
            Client.instance.ftpsPort = configData.ftpsPort;
            Client.instance.ftpsUsername = configData.ftpsUsername;
            Client.instance.ftpsPassword = configData.ftpsPassword;

            if (!string.IsNullOrEmpty(configData.stationOverride))
            {
                Client.instance._moonshotStation = (MoonshotStation)System.Enum.Parse(typeof(MoonshotStation), configData.stationOverride);
            }
        }

        yield break;
	}

    protected override void FinishedLoadingContent()
    {
        base.FinishedLoadingContent();

        if (Client.instance != null)
        {
            Client.instance.ConnectToServer();
        }
    }
}