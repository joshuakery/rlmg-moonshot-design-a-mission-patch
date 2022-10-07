using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using rlmg.logging;

public class ExhibitSpecificAssetsManager : MonoBehaviour
{
    public AssetsPairing[] exhibitSpecificAssets;

    [System.Serializable]
    public class AssetsPairing
    {
        //public string stationName;
        public MoonshotStation moonshotStation = MoonshotStation.Map;
        public GameObject[] activeAssets;
    }
    
    private static ClientConfigLoader clientConfigLoader;
    
    void Awake()
    {
        if (clientConfigLoader == null)
        {
            clientConfigLoader = (ClientConfigLoader)FindObjectOfType(typeof(ClientConfigLoader));
        }

        if (clientConfigLoader != null)
        {
            clientConfigLoader.onFinishedLoadingContent.AddListener(IdentifiedCurrentStation);
        }

        // foreach (AssetsPairing assetsPairing in exhibitSpecificAssets)
        // {
        //     //if ()
        // }
    }

    private void OnEnable()
    {
        if (Client.instance != null)
            IdentifiedCurrentStation();
    }

    void Update()
    {
        
    }

    void IdentifiedCurrentStation()
    {
        //Debug.Log("IdentifiedCurrentStation()   Client.instance._moonshotStation = " + Client.instance._moonshotStation);
        RLMGLogger.Instance.Log("IdentifiedCurrentStation()   Client.instance._moonshotStation = " + Client.instance._moonshotStation, MESSAGETYPE.INFO);


        foreach (AssetsPairing assetsPairing in exhibitSpecificAssets)
        {
            if (assetsPairing.moonshotStation != Client.instance._moonshotStation)
            {
                foreach (GameObject asset in assetsPairing.activeAssets)
                {
                    asset.SetActive(false);
                }
            }
        }

        foreach (AssetsPairing assetsPairing in exhibitSpecificAssets)
        {
            if (assetsPairing.moonshotStation == Client.instance._moonshotStation)
            {
                foreach (GameObject asset in assetsPairing.activeAssets)
                {
                    asset.SetActive(true);
                }
            }
        }
    }
}
