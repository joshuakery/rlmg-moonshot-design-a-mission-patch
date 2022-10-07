using UnityEngine;

public enum ErrorCode
{
    duplicateStation = 1
}

public enum MoonshotStation
{
    Controller = 0,
    Rover,
    Map,
    Art,
    Question,
    Hunt
}

public enum MissionType
{
    Opening = 0
}

public enum MissionState
{
    Stopped = 0,
    Running,
    Paused
}

public enum MissionRound
{
    Round1 = 0,
    Round2,
    Round3,
    Round4,
    Round5
}

public class MoonshotSettings
{
    public System.TimeSpan RoundDuration { get; set; }
    public int ServerPort { get; set; }
    public int GameMode { get; set; } // game or free

}

public class Mission
{
    /// <summary>Mission State (0 - Stopped, 1 - Running, 2 - Paused)</summary>
    public MissionState MissionState { get; set; }
    public MissionRound CurrentRound { get; set; }
    public MissionType MissionType { get; set; }
    public Team Team1;
    public Team Team2;
    public Team Team3;
    public Team Team4;
    public Team Team5;
}

public class Round
{
    public int RoundID { get; set; }
    public System.DateTime CreateDate { get; set; }
}

public class Team
{
    public int TeamId { get; set; }
    public string Name { get; set; }
    public int FirstStation { get; set; }
    public int CurrentStation { get; set; }
    public MoonshotTeamData MoonshotTeamData { get; set; }
}

//public class TeamData
//{
//    public string teamName;

//    public string namesake;

//    public bool didRoverActivity;
//    [Range(0, 2)]
//    public int roverStepsCompleted = 0;

//    public bool didMapActivity;
//    public int mapRoundsCompleted;
//    [Range(0, 1)]
//    public float settlementShelterQuality;
//    [Range(0, 1)]
//    public float settlementCommsQuality;
//    [Range(0, 1)]
//    public float settlementSunQuality;
//    [Range(0, 1)]
//    public float settlementWaterQuality;

//    public bool didArtActivity;
//    public string[] artworks;  //probably a list of URLs?
//                               //public Sprite[] artworkSprites;

//    public bool didCharterActivity;
//    public int charterQsCompleted;
//    [Range(-1, 1)]
//    public float charterMeterDecisions;
//    [Range(-1, 1)]
//    public float charterMeterPriorities;
//    [Range(-1, 1)]
//    public float charterMeterStrictness;

//    public bool didHuntActivity;
//    [Range(0, 9)]
//    public int huntNumFound = 0;
//}
//public class StationData
//{
//    // Station 1 - Rover

//    /// <summary>Rover Result(rounds completed) (0-2)</summary>
//    public int RoverRoundsCompleted { get; set; }

//    // Station 2 - Map

//    /// <summary>Map Rounds Completed (0-2)</summary>
//    public int MapRoundsCompleted { get; set; }

//    /// <summary>Map Result (0-100)</summary>
//    public int MapResult { get; set; }

//    /// <summary>Map Coordinates (?)</summary>
//    public int MapCoordinates { get; set; }

//    // Station 3 - Art

//    /// <summary>Art Team Name</summary>
//    public string ArtTeamName { get; set; }

//    /// <summary>Art Number Submitted (0-6)</summary>
//    public int ArtNumber { get; set; }

//    /// <summary>Art File Locations (?)</summary>
//    public string ImageURI { get; set; }

//    // Station 4 - Charter Questions

//    /// <summary>Charter Qusestions Completed</summary>
//    public int CharterQuestionsCompleted { get; set; }

//    /// <summary>Charter Meter 1 (0-10)</summary>
//    public int CharterMeter1 { get; set; }

//    /// <summary>Charter Meter 2 (0-10)</summary>
//    public int CharterMeter2 { get; set; }

//    /// <summary>Charter Meter 3 (0-10)</summary>
//    public int CharterMeter3 { get; set; }

//    //Station 5 - Hunt

//    /// <summary>Hunt Result - Number Found (0-10)</summary>
//    public int HuntResult { get; set; }
//}