using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ArtScan.MuralPositionsModule
{
    public class Patch : MonoBehaviour
    {
        public GameObject defaultDisplay;
        public GameObject drawingDisplay;

        public RawImage ri;

        public GenericWindow1 defaultGenericWindow;
        public GenericWindow1 drawingGenericWindow;

        public RectTransform rt;

        public GameObject counter;
        public GameObject savingFeedback;
        public GameObject highlight;

        private void Awake()
        {
            ri = drawingDisplay.GetComponent<RawImage>();

            defaultGenericWindow = defaultDisplay.GetComponent<GenericWindow1>();
            drawingGenericWindow = drawingDisplay.GetComponent<GenericWindow1>();

            rt = GetComponent<RectTransform>();
        }
    }
}


