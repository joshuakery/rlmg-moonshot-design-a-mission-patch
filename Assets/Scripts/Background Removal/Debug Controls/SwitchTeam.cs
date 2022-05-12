using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using ArtScan;

public class SwitchTeam : MonoBehaviour
{
    public GameState gameState;
    public TMP_Dropdown dropdown;
    
    // Start is called before the first frame update
    void Start()
    {
        dropdown.ClearOptions();
        List<string> teamnames = gameState.teams.Select(team => team.teamname).ToList();
        dropdown.AddOptions(teamnames);
        dropdown.value = gameState.currentTeam;
    }


}
