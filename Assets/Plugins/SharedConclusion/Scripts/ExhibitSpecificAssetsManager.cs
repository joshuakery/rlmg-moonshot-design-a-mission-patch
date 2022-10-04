using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    void Update()
    {
        
    }

    void IdentifiedCurrentStation()
    {
        //Debug.Log("IdentifiedCurrentStation()   Client.instance._moonshotStation = " + Client.instance._moonshotStation);
        
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
