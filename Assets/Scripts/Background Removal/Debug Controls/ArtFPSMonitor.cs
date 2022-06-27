using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace ArtScan.CoreModule
{
    public class ArtFPSMonitor : MonoBehaviour
    {
        int tick = 0;
        float elapsed = 0;
        float fps = 0;

        public TMP_Text display;

        // Update is called once per frame
        void Update()
        {
            tick++;
            elapsed += Time.deltaTime;
            if (elapsed >= 1f)
            {
                fps = tick / elapsed;
                tick = 0;
                elapsed = 0;

                display.text = fps.ToString();
            }
        }
    }

}

