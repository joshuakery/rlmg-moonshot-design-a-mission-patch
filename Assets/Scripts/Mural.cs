using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ArtScan;
using System.IO;
using TMPro;
using rlmg.logging;

namespace ArtScan.MuralPositionsModule
{
    public class Mural : MonoBehaviour
    {
        public Transform patchesParent;
        private Patch[] patches;

        public GameState gameState;

        public MuralPositionsLoader muralPositionsLoader;

        public RectTransform targetPatch;

        private void Awake()
        {
            patches = patchesParent.GetComponentsInChildren<Patch>();

            for (int i=0; i<patches.Length; i++)
            {
                Patch patch = patches[i];
                TMP_Text text = patch.counter.GetComponent<TMP_Text>();
                text.text = (i+1).ToString();

                patch.counter.SetActive(false);
                patch.savingFeedback.SetActive(false);
                patch.highlight.SetActive(false);
            }

            if (muralPositionsLoader == null)
            {
                muralPositionsLoader = GameObject.FindObjectOfType<MuralPositionsLoader>();
            }
        }

        public void UpdateMural()
        {
            for (int i = 0; i < patches.Length; i++)
            {
                Patch patch = patches[i];



                if (gameState.scans[i] != null)
                {
                    Texture2D scan = gameState.scans[i];
                    patch.ri.texture = scan;

                    patch.drawingGenericWindow.Open();
                    patch.defaultGenericWindow.Close();
                }
                else
                {
                    patch.drawingGenericWindow.Close();
                    patch.defaultGenericWindow.Open();
                }

            }
        }

        public void UpdatePositions()
        {
            if (muralPositionsLoader.data != null &&
                muralPositionsLoader.data.muralPatches != null &&
                patches != null)
            {
                for (int i = 0; i < muralPositionsLoader.data.muralPatches.Length; i++)
                {
                    if (i < patches.Length)
                    {
                        Patch patch = patches[i];
                        MuralPatch data = muralPositionsLoader.data.muralPatches[i];
                        patch.rt.anchoredPosition = data.position;
                        patch.rt.sizeDelta = data.size;
                    }
                }

            }
        }

        private IEnumerator DismissSavingFeedback(Patch patch)
        {
            yield return new WaitForSeconds(1);
            patch.savingFeedback.SetActive(false);
        }

        private void SelectTargetPatch(Patch patch)
        {
            foreach (Patch p in patches) { p.highlight.SetActive(false); }

            RectTransform rt = patch.GetComponent<RectTransform>();
            if (targetPatch == rt)
            {
                targetPatch = null;
                patch.highlight.SetActive(false);
            }
            else
            {
                targetPatch = rt;
                patch.highlight.SetActive(true);
            }
        }

        private void Update()
        {

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                SelectTargetPatch(patches[0]);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                SelectTargetPatch(patches[1]);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                SelectTargetPatch(patches[2]);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                SelectTargetPatch(patches[3]);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                SelectTargetPatch(patches[4]);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                SelectTargetPatch(patches[5]);
            }

            int interval = 1;
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) { interval = 10; }

            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                if (targetPatch != null)
                {
                    targetPatch.anchoredPosition = new Vector2(targetPatch.anchoredPosition.x, targetPatch.anchoredPosition.y + interval);
                }
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                if (targetPatch != null)
                {
                    targetPatch.anchoredPosition = new Vector2(targetPatch.anchoredPosition.x, targetPatch.anchoredPosition.y - interval);
                }
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                if (targetPatch != null)
                {
                    targetPatch.anchoredPosition = new Vector2(targetPatch.anchoredPosition.x - interval, targetPatch.anchoredPosition.y);
                }
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                if (targetPatch != null)
                {
                    targetPatch.anchoredPosition = new Vector2(targetPatch.anchoredPosition.x + interval, targetPatch.anchoredPosition.y);
                }
            }

            if (Input.GetKeyDown(KeyCode.Equals))
            {
                
                MuralPositionsJSON data = new MuralPositionsJSON();
                data.muralPatches = new MuralPatch[patches.Length];
                for (int i=0; i<patches.Length; i++)
                {
                    
                    data.muralPatches[i] = new MuralPatch();
                    RectTransform rt = patches[i].GetComponent<RectTransform>();
                    data.muralPatches[i].position = rt.anchoredPosition;
                    data.muralPatches[i].size = rt.sizeDelta;
                }

                string json = JsonUtility.ToJson(data, true);
                RLMGLogger.Instance.Log(json, MESSAGETYPE.INFO); //failsafe

                string filepath = Path.Combine(Application.streamingAssetsPath, muralPositionsLoader.contentFilename);
                RLMGLogger.Instance.Log("Saving positions to " + filepath, MESSAGETYPE.INFO);
                File.WriteAllText(filepath, json);

                for (int i = 0; i < patches.Length; i++)
                {
                    patches[i].savingFeedback.SetActive(true);
                    StartCoroutine(DismissSavingFeedback(patches[i]));
                }
            }
            else if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                foreach (Patch patch in patches)
                {
                    patch.counter.SetActive(!patch.counter.activeSelf);
                    patch.savingFeedback.SetActive(false);
                }
            }
        }
    }
}


