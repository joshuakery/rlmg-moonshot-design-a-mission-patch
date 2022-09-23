using UnityEngine;
using UnityEngine.UI;
using ArtScan;

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

            if (gameState.scans[i] != null) //if we have a scan, show it
            {
                Texture2D scan = gameState.scans[i];
                ri.texture = scan;
                
                ri.gameObject.SetActive(true);
                img.gameObject.SetActive(false);
            }
            else if (i == 0 || (i >= 1 && gameState.scans[i-1] != null))
                //if not, if this is the first one, or if the last one was a scan, show the placeholder
            {
                ri.gameObject.SetActive(false);
                img.gameObject.SetActive(true);
            }
            else
            {
                ri.gameObject.SetActive(false);
                img.gameObject.SetActive(false);
            }
        }

    }
}
