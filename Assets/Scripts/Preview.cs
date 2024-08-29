using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ArtScan;
using rlmg.logging;

[RequireComponent(typeof(RawImage))]
public class Preview : MonoBehaviour
{
    public GameState gameState;

    public RawImage rawImage;

    private void Start()
    {
        rawImage = GetComponent<RawImage>();
    }

    public void SetPreview()
    {
        if (gameState.preview != null)
            rawImage.texture = gameState.preview;
    }
}
