using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ArtScan.CoreModule
{
    
    [SerializeField]
    public enum EdgeFindingMethod
    {
        Sobel = 0,
        Threshold = 1,
        StructuredForests = 2,
        Canny = 3
    };

}
