using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MoonshotTeamData
{
    public string teamName;

    public string namesake;
    public List<string> chosenWords;

    public bool didRoverActivity;
    [Range(0, 2)]
    public int roverStepsCompleted = 0;

    public bool didMapActivity;
    public int mapRoundsCompleted;
    [Range(0, 1)]
    public float settlementShelterQuality;
    [Range(0, 1)]
    public float settlementCommsQuality;
    [Range(0, 1)]
    public float settlementSunQuality;
    [Range(0, 1)]
    public float settlementWaterQuality;

    public bool didArtActivity;
    public string[] artworks;  //a list of URLs

    public bool didCharterActivity;
    public int charterQsCompleted;
    [Range(-1, 1)]
    public float charterMeterDecisions;
    [Range(-1, 1)]
    public float charterMeterPriorities;
    [Range(-1, 1)]
    public float charterMeterStrictness;

    public bool didHuntActivity;
    [Range(0, 9)]
    public int huntNumFound = 0;
}