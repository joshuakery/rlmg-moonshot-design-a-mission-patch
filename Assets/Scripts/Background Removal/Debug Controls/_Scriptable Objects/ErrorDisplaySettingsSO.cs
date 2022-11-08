using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ArtScan.CoreModule;
using ArtScan.ErrorDisplayModule;

[CreateAssetMenu(menuName = "Remove Background/Error Display Settings"), System.Serializable]
public class ErrorDisplaySettingsSO : ScriptableObject
{
    public ErrorDisplaySettings errorDisplaySettings;
}


