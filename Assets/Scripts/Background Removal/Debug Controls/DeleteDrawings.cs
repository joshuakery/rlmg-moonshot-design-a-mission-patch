using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ArtScan;

public class DeleteDrawings : MonoBehaviour
{
    public Transform patchesContainer;

    public GameState gameState;

    public Button undoButton;
    public Button deleteButton;

    private void Start() 
    {
        // gameState.toTrash = new Texture2D[gameState.scanMax];

        deleteButton.interactable = false;
        undoButton.interactable = false;
    }

    public void OnToggleClicked()
    {
        for (int i=0; i<patchesContainer.childCount; i++)
        {
            Transform patchLogItem = patchesContainer.GetChild(i);
            Toggle toggle = patchLogItem.GetChild(1).GetComponent<Toggle>();

            if (gameState.scans[i] != null && toggle.isOn)
            {
                deleteButton.interactable = true;
                return;
            }
        }
        deleteButton.interactable = false;
    }

    public void OnDeleteSelected()
    {
        // saveScans.ClearTrash();

        // Array.Clear(gameState.toTrash,0,gameState.toTrash.Length);

        for (int i=0; i<patchesContainer.childCount; i++)
        {
            Transform patchLogItem = patchesContainer.GetChild(i);
            Toggle toggle = patchLogItem.GetChild(1).GetComponent<Toggle>();

            if (gameState.scans[i] != null && toggle.isOn)
            {
                // gameState.toTrash[i] = gameState.scans[i];

                Array.Clear(gameState.scans, i, 1);
                gameState.TrashScanFromCurrentTeam(i);

                toggle.isOn = false;

                undoButton.interactable = true;
            }
        }

        // for (int i=0; i<gameState.toTrash.Length; i++)
        // {
        //     if (gameState.toTrash[i] != null)
        //         Array.Clear(gameState.scans, i, 1);
        // }
        // saveScans.TrashScans(gameState.toTrash);

    }

    public void OnUndo()
    {
        // for (int i=0; i<gameState.toTrash.Length; i++)
        // {
        //     if (gameState.toTrash[i] != null)
        //         gameState.scans[i] = gameState.toTrash[i];
        // }

        // saveScans.UnTrashAll();

        // ScanAdded.Raise();

        // undoButton.interactable = false;
    }
}
