using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TeamNameDisplay : MonoBehaviour
{
    public TMP_Text uiText;
    public Text uiTextClassic;
    
    void OnEnable()
    {
        if (Client.instance == null || Client.instance.team == null || Client.instance.team.MoonshotTeamData == null)
        {
            return;
        }
        
        if (uiText != null)
        {
            uiText.text = Client.instance.team.MoonshotTeamData.teamName;
        }

        if (uiTextClassic != null)
        {
            uiTextClassic.text = Client.instance.team.MoonshotTeamData.teamName;
        }
    }
}
