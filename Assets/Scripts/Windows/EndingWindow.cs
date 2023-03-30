using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EndingWindow : GenericWindow
{
    public TMP_Text heading;
    public TMP_Text body;

    public static string[] headings = new string[2] {
        "OUT OF TIME",
        "CONGRATULATIONS"
    };

    public static string[] bodies = new string[2] {
        "You didn’t have time to scan in any sketches, <nobr>but your ideas will</nobr> still help <nobr>a lot as we work</nobr> toward a patch design that can show the whole Earth who we are. Thanks for your help!",
        "Thanks for your help. You had some really great ideas! I’ll put them together and we can work toward a patch design that can show the whole Earth who we are!"
    };

    public void SetTexts()
    {
        if (gameState.allScansEmpty)
        {   
            heading.text = headings[0];
            body.text = bodies[0];
        }
        else
        {
            heading.text = headings[1];
            body.text = bodies[1];
        }
    }
}
