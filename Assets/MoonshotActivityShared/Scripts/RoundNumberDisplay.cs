using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RoundNumberDisplay : MonoBehaviour
{
    public TMP_Text uiText;
    public Text uiTextClassic;
    
    void OnEnable()
    {
        if (uiText != null)
        {
            uiText.text = MoonshotActivity.roundNum.ToString();
        }

        if (uiTextClassic != null)
        {
            uiTextClassic.text = MoonshotActivity.roundNum.ToString();
        }
    }
}
