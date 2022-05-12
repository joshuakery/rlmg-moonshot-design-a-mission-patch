using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using ArtScan;

public class Teamname : MonoBehaviour
{
    public TMP_Text tmp_text;
    public GameState gameState;
    public void SetTeamname()
    {
        tmp_text.text = BoldTags(gameState.teams[gameState.currentTeam].teamname);
    }
    private string BoldTags(string input)
    {
        return "<b>" + input + "</b>";
    }
}
