using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using ArtScan;

public class ChosenWordsDisplay : MonoBehaviour
{
    public GameState gameState;

    public Transform chosenWordsContainer;

    public void UpdateChosenWords()
    {
        for (int i=0; i<chosenWordsContainer.childCount; i++)
        {
            Transform chosenWord = chosenWordsContainer.GetChild(i);
            TMP_Text tmp_text = chosenWord.gameObject.GetComponent<TMP_Text>();

            if (i < gameState.teams[gameState.currentTeam].chosenWords.Count)
            {
                tmp_text.text = gameState.teams[gameState.currentTeam].chosenWords[i];
                chosenWord.gameObject.SetActive(true);
            }
            else
            {
                chosenWord.gameObject.SetActive(false);
            }
        }

    }
}
