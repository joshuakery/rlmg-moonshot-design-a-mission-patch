using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TeamChecklist : MonoBehaviour
{
    public Toggle checklistToggleRover;
    public Toggle checklistToggleMap;
    public Toggle checklistToggleArt;
    public Toggle checklistToggleCharter;
    public Toggle checklistToggleHunt;

    // void Update()
    // {
        // UpdateChecklist();
    // }

    void OnEnable()
    {
        UpdateChecklist();
    }

    void UpdateChecklist()
    {
        if (Client.instance == null || Client.instance.team == null || Client.instance.team.MoonshotTeamData == null)
        {
            Debug.Log("failed to update team checklist because team data was unavailable");
            
            return;
        }
        
        if (checklistToggleRover != null)
        {
            checklistToggleRover.isOn = Client.instance.team.MoonshotTeamData.didRoverActivity;
        }

        if (checklistToggleMap != null)
        {
            checklistToggleMap.isOn = Client.instance.team.MoonshotTeamData.didMapActivity;
        }

        if (checklistToggleArt != null)
        {
            checklistToggleArt.isOn = Client.instance.team.MoonshotTeamData.didArtActivity;
        }

        if (checklistToggleCharter != null)
        {
            checklistToggleCharter.isOn = Client.instance.team.MoonshotTeamData.didCharterActivity;
        }

        if (checklistToggleHunt != null)
        {
            checklistToggleHunt.isOn = Client.instance.team.MoonshotTeamData.didHuntActivity;
        }

        Debug.Log("team checklist: didRoverActivity="+Client.instance.team.MoonshotTeamData.didRoverActivity
                    +" didMapActivity="+Client.instance.team.MoonshotTeamData.didMapActivity
                    +" didArtActivity="+Client.instance.team.MoonshotTeamData.didArtActivity
                    +" didCharterActivity="+Client.instance.team.MoonshotTeamData.didCharterActivity
                    +" didHuntActivity="+Client.instance.team.MoonshotTeamData.didHuntActivity);
    }
}
