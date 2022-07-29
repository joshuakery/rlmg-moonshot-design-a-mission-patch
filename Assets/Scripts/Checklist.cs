using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ArtScan;
public class Checklist : MonoBehaviour
{
    public GameState gameState;

    //public GameObject container;

    public Toggle didHuntToggle;
    public Toggle didMapToggle;
    public Toggle didArtToggle;
    public Toggle didCharterToggle;
    public Toggle didRoverToggle;

    private void OnEnable()
    {
        UpdateChecklist();
    }

    public void UpdateChecklist()
    {
        if (Client.instance == null || Client.instance.team == null || Client.instance.team.MoonshotTeamData == null)
            return;

        //Rover
        if (Client.instance.team.MoonshotTeamData.didRoverActivity)
        {
            didRoverToggle.isOn = true;
        }
        else
        {
            didRoverToggle.isOn = false;
            didRoverToggle.gameObject.transform.SetAsLastSibling();
        }

        //Map
        if (Client.instance.team.MoonshotTeamData.didMapActivity)
        {
            didMapToggle.isOn = true;
        }
        else
        {
            didMapToggle.isOn = false;
            didMapToggle.gameObject.transform.SetAsLastSibling();
        }

        //Art
        if (Client.instance.team.MoonshotTeamData.didArtActivity)
        {
            didArtToggle.isOn = true;
        }
        else
        {
            didArtToggle.isOn = false;
            didArtToggle.gameObject.transform.SetAsLastSibling();
        }

        //Charter
        if (Client.instance.team.MoonshotTeamData.didCharterActivity)
        {
            didCharterToggle.isOn = true;
        }
        else
        {
            didCharterToggle.isOn = false;
            didCharterToggle.gameObject.transform.SetAsLastSibling();
        }

        //Hunt
        if (Client.instance.team.MoonshotTeamData.didHuntActivity)
        {
            didHuntToggle.isOn = true;
        }
        else
        {
            didHuntToggle.isOn = false;
            didHuntToggle.gameObject.transform.SetAsLastSibling();
        }

        //Toggle[] toggles = container.GetComponentsInChildren<Toggle>();
        //for (int i = 0; i < toggles.Length; i++ )
        //{
        //    Toggle toggle = toggles[i];
        //    if (i <= gameState.currentRound)
        //    {
        //        toggle.isOn = true;
        //    }
        //    else
        //    {
        //        toggle.isOn = false;
        //    }
        //}
    }
}
