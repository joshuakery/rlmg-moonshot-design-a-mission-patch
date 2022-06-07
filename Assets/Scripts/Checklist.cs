using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ArtScan;
public class Checklist : MonoBehaviour
{
    public GameState gameState;

    public GameObject container;

    private void OnEnable()
    {
        UpdateChecklist();
        Debug.Log("Updated");
    }

    public void UpdateChecklist()
    {
        Toggle[] toggles = container.GetComponentsInChildren<Toggle>();
        for (int i = 0; i < toggles.Length; i++ )
        {
            Toggle toggle = toggles[i];
            if (i <= gameState.currentRound)
            {
                toggle.isOn = true;
            }
            else
            {
                toggle.isOn = false;
            }
        }
    }
}
