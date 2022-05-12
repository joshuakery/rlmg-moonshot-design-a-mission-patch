using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ArtScan;

public class Mural : MonoBehaviour
{
    public GameObject[] patches;

    public GameState gameState;

    public void UpdateMural()
    {
        for (int i=0; i<patches.Length; i++)
        {
            GameObject patch = patches[i];
            Image img = patch.transform.GetChild(0).GetComponent<Image>();
            RawImage ri = patch.transform.GetChild(1).GetComponent<RawImage>();

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
