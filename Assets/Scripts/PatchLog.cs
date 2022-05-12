using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ArtScan;
using TMPro;

public class PatchLog : MonoBehaviour
{
    public Transform patchesContainer;

    public GameState gameState;

    private void OnEnable()
    {
        UpdatePatches();
    }

    public void UpdatePatches()
    {
        for (int i=0; i<patchesContainer.childCount; i++)
        {
            Transform patchLogItem = patchesContainer.GetChild(i);
            Image img = patchLogItem.GetChild(0).GetComponent<Image>();
            RawImage ri = patchLogItem.GetChild(1).GetComponent<RawImage>();

            if (gameState.scans[i] != null)
            {
                Texture2D scan = gameState.scans[i];
                ri.texture = scan;
                
                ri.gameObject.SetActive(true);
                img.gameObject.SetActive(false);
            }
            else
            {
                ri.gameObject.SetActive(false);
                img.gameObject.SetActive(true);
            }
        }

    }
}
