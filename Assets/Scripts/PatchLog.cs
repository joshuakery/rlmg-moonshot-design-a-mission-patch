using UnityEngine;
using UnityEngine.UI;
using ArtScan;

namespace ArtScan.MuralPositionsModule
{
    public class PatchLog : MonoBehaviour
    {
        public Transform patchesParent;
        private Patch[] patches;

        public GameState gameState;

        private void Awake()
        {
            patches = patchesParent.GetComponentsInChildren<Patch>();
        }

        private void OnEnable()
        {
            UpdatePatches();
        }

        public void UpdatePatches()
        {
            for (int i = 0; i < patches.Length; i++)
            {
                Patch patch = patches[i];

                if (gameState.savedScanManager.scans[i] != null) //if we have a scan, show it
                {
                    Texture2D scan = gameState.savedScanManager.scans[i];
                    patch.ri.texture = scan;

                    patch.drawingGenericWindow.Open();
                    patch.defaultGenericWindow.Close();
                }
                else if (i == 0 || (i >= 1 && gameState.savedScanManager.scans[i - 1] != null))
                //if not, if this is the first one, or if the last one was a scan, show the placeholder
                {
                    patch.drawingGenericWindow.Close();
                    patch.defaultGenericWindow.Open();
                }
                else
                {
                    patch.drawingGenericWindow.Close();
                    patch.defaultGenericWindow.Close();
                }
            }

        }
    }
}


