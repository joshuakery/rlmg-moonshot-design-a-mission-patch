using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Newtonsoft.Json;

public class PrintActivityJSON : MonoBehaviour
{
    public TMP_Text timestamp;
    public TMP_Text display;

    private void Awake()
    {
        if (display == null)
            display = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        if (timestamp != null)
            timestamp.text = System.String.Format("As of {0}", System.DateTime.Now);

        if (Client.instance != null &&
            Client.instance.team != null &&
            Client.instance.team.MoonshotTeamData != null)
        {
            string json = JsonConvert.SerializeObject(
                    Client.instance.team.MoonshotTeamData,
                    Newtonsoft.Json.Formatting.Indented
                );

            display.text = json;
        }
        else
        {
            if (Client.instance == null) { display.text = "Client.instance is null."; }
            else if (Client.instance.team == null) { display.text = "Client.instance.team is null."; }
            else if (Client.instance.team.MoonshotTeamData == null) { display.text = "Client.instance.team.MoonshotTeamData is null."; }
        }
    }
}
