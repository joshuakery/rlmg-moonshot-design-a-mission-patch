using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using ArtScan;

public class SwitchTeam : MonoBehaviour
{
    public GameState gameState;
    public DeleteDrawings deleteDrawings;
    public TMP_Dropdown dropdown;

    private void Start()
    {
        dropdown.value = gameState.currentTeamIndex;
    }

    void OnEnable()
    {
        dropdown.ClearOptions();
        List<string> teamnames = gameState.teams.Select(team => team.teamName).ToList();
        dropdown.AddOptions(teamnames);
        dropdown.value = deleteDrawings.viewedTeamIndex;
    }

    public void OnSwitchTeam(int i)
    {
        deleteDrawings.viewedTeamIndex = i;
        deleteDrawings.ViewTeam();
    }



}
