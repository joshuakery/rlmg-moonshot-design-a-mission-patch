using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ArtScan;
using UnityEngine.UI;
using TMPro;
using ArtScan.WordPoints;

public class WordChoices : MonoBehaviour
{
    public GameObject wordChoicePrefab;
    public Transform choicesContainer;

    public GameState gameState;

    public Button continueButton;

    public List<Toggle> offToggles;

    public void SetWords()
    {
        offToggles = new List<Toggle>();

        foreach (Transform child in choicesContainer) {
            GameObject.Destroy(child.gameObject);
        }

        if (gameState.wordPointsContent != null && gameState.wordPointsContent.wordPoints != null)
        {
            foreach (KeyValuePair<string,Dictionary<string,int>> word in gameState.wordPointsContent.wordPoints)
            {
                GameObject wordChoice = Instantiate(wordChoicePrefab, choicesContainer);

                string name = word.Key;

                Toggle toggle = wordChoice.gameObject.GetComponent<Toggle>();
                toggle.onValueChanged.AddListener(delegate {
                    ToggleChoice(name,toggle);
                });

                wordChoice.transform.GetChild(1).GetComponent<TMP_Text>().text = name;

                offToggles.Add(toggle);
            }
        }

    }


    public void ToggleChoice(string choice, Toggle toggle)
    {
        if (gameState.teams[gameState.currentTeam].chosenWords.Contains(choice))
        {
            gameState.teams[gameState.currentTeam].chosenWords.Remove(choice);
            offToggles.Add(toggle);
        }
        else if (gameState.teams[gameState.currentTeam].chosenWords.Count <= 2)
        {
            gameState.teams[gameState.currentTeam].chosenWords.Add(choice);
            offToggles.Remove(toggle);
        }

        if (gameState.teams[gameState.currentTeam].chosenWords.Count == 3)
        {
            continueButton.interactable = true;
            foreach(Toggle t in offToggles)
            {
                t.interactable = false;
            }
        }
        else
        {
            continueButton.interactable = false;
            foreach(Toggle t in offToggles)
            {
                t.interactable = true;
            }
        }
    }
    
}
