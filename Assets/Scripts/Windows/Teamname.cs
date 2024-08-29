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
        if (tmp_text != null && gameState != null && gameState.currentTeam != null)
            tmp_text.text = BoldTags(gameState.currentTeam.teamName);
    }
    private string BoldTags(string input)
    {
        return "<b>" + input + "</b>";
    }
}
