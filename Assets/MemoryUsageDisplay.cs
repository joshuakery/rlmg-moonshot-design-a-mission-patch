using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;

public class MemoryUsageDisplay : MonoBehaviour
{
    private TMP_Text display;

    // Start is called before the first frame update
    void Start()
    {
        if (display == null)
            display = GetComponent<TMP_Text>();
    }

    // Update is called once per frame
    void Update()
    {
        
        long memory = GC.GetTotalMemory(true);
        display.text = String.Format("{0:0,0}", memory);
    }
}
