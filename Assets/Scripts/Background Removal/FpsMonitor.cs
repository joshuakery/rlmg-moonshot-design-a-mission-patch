using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ArtScan.CoreModule
{
    // v2.0.0
    public class FpsMonitor : MonoBehaviour
    {
        int tick = 0;
        float elapsed = 0;
        float fps = 0;

        public TMP_Text display;

        void Update()
        {
            tick++;
            elapsed += Time.deltaTime;
            if (elapsed >= 1f)
            {
                fps = tick / elapsed;
                tick = 0;
                elapsed = 0;
            }

            if (display != null)
                display.text = fps.ToString();
        }

      
    }
}