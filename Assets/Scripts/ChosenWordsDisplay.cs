using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using ArtScan;

public class ChosenWordsDisplay : MonoBehaviour
{
    public GameState gameState;

    public Transform chosenWordsContainer;

    private void OnEnable()
    {
        UpdateChosenWords();
    }

    public void UpdateChosenWords()
    {
        for (int i=0; i<chosenWordsContainer.childCount; i++)
        {
            Transform chosenWord = chosenWordsContainer.GetChild(i);
            TMP_Text tmp_text = chosenWord.gameObject.GetComponent<TMP_Text>();

            if (i < gameState.currentTeam.chosenWords.Count)
            {
                tmp_text.text = gameState.currentTeam.chosenWords[i];
                chosenWord.gameObject.SetActive(true);
            }
            else
            {
                chosenWord.gameObject.SetActive(false);
            }
        }

    }
}
