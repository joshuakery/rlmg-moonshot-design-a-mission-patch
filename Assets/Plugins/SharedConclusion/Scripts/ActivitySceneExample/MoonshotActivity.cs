using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class MoonshotActivity : MonoBehaviour
{
    public bool useServer = true;

    //public MoonshotStation moonshotStation = MoonshotStation.Map;
    
    public int teamNum = 0;
    //public bool isFinalRound = true;
    static public int roundNum = 0;
    public int totalRoundsNum = 5;

    public Clock clock;
    //public float clockStartTime = 600;
    private float autoAdvanceTimer = 0f;
    private bool isPaused = false;
    private float roundConclusionDur = 60;

    public GameObject[] welcomeScreens;
    
    public int startStepNum = 0;
    protected int currStepNum;

    [FormerlySerializedAs("scenes")]
    public SlideshowStep[] steps;

    [System.Serializable]
    public class SlideshowStep
    {
        public GameObject display1;
        public GameObject display2;

        public float autoAdvanceDur;
    }

    private float currStepLoadTime;

    public MoonshotDataHandler _userData;
    private MoonshotDataHandler userData
    {
        get
        {
            if (_userData == null)
            {
                _userData = GetComponentInParent<MoonshotDataHandler>();
            }

            return _userData;
        }
    }
    
    public Text roundNumberDebugText;
    public Text teamNameDebugText;

    protected virtual void Awake()
    {
#if !UNITY_EDITOR
	    startStepNum = 0;  //just to prevent this from being set to somethiing else in builds, as it's usually an undesired accident
#endif

        if (clock == null)
        {
            clock = GetComponent<Clock>();
        }
    }

    protected virtual void OnEnable()
    {
        if (useServer)
        {
            if (Client.instance != null)
            {
                //I used to have these in Start(), but I think this should work now that Client.cs is a little more robust in terms of setting the singleton instance prior to its own Awake()
                Client.instance.onStartRound += StartRound;
                Client.instance.onPauseMission += PauseMission;
                Client.instance.onUnPauseMission += UnPauseMission;
            }
            
            foreach (GameObject welcomeScreen in welcomeScreens)
            {
                welcomeScreen.SetActive(true);
            }

            //GoToStep(-1);

            DisableAllSteps();
        }
        else
        {
            StartRound("not online", 600, 60, 0, null);
        }
    }

    protected virtual void OnDisable()
    {
        if (Client.instance != null)
        {
            Client.instance.onStartRound -= StartRound;
            Client.instance.onPauseMission -= PauseMission;
            Client.instance.onUnPauseMission -= UnPauseMission;
        }
    }

    public virtual void StartRound(string _teamName, float _roundDuration, float _roundBufferDuration, int _round, string _JsonTeamData)
    {
        Debug.Log("Moonshot Activity received StartRound call from Server");

        roundNum = _round;
        //clockStartTime = _roundDuration;
        roundConclusionDur = _roundBufferDuration;

        StartRoundClock(_roundDuration);

        //set the timing of the per-activity conclusion: indefinite (0) after normal rounds, and the "buffer duration" for the last round
        if (roundNum >= totalRoundsNum - 1)  //final round
        {
            steps[steps.Length - 1].autoAdvanceDur = roundConclusionDur;
        }
        else
        {
            steps[steps.Length - 1].autoAdvanceDur = 0;  //indefinite
        }

        LoadCurrentTeamData();
    }

    [ContextMenu("Load Current Team Data")]
    public void LoadCurrentTeamData()
    {
        if (useServer == true)
        {
            StartActivityWithLoadedTeamData(Client.instance.team.MoonshotTeamData);
        }
        else if (userData != null)
        {
            userData.teamNum = teamNum;
            
            userData.onLoadingComplete += JsonLoadingCompleteCallback;

            userData.LoadExternalJson();
        }
    }

    private void JsonLoadingCompleteCallback(MoonshotTeamData teamData)
    {
        userData.onLoadingComplete -= JsonLoadingCompleteCallback;

        StartActivityWithLoadedTeamData(teamData);
    }

    protected virtual void StartActivityWithLoadedTeamData(MoonshotTeamData teamData)
    {
        if (teamData == null)
        {
            return;
        }

        foreach (GameObject welcomeScreen in welcomeScreens)
        {
            welcomeScreen.SetActive(false);
        }
        
        GoToStep(startStepNum);


        //debug
        if (roundNumberDebugText != null)
        {
            roundNumberDebugText.text = "round number: " + roundNum;
        }
        
        if (teamNameDebugText != null)
        {
            teamNameDebugText.text = "team name: " + teamData.teamName;
        }
    }

    protected virtual void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            GoToPrevStep();
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            GoToNextStep();
        }
        
        if (!isPaused)
        {
            autoAdvanceTimer += Time.deltaTime;
        }

        //if (steps[currStepNum].autoAdvanceDur > 0f && Time.time - currStepLoadTime >= steps[currStepNum].autoAdvanceDur)
        if (steps[currStepNum].autoAdvanceDur > 0f && autoAdvanceTimer >= steps[currStepNum].autoAdvanceDur)
        {
            GoToNextStep();
        }

        // if (Input.GetKeyDown(KeyCode.R))
        // {
        //     UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        // }
    }

    public virtual void GoToPrevStep()
    {
        if (currStepNum > 0)
        {
            GoToStep(currStepNum - 1);
        }
    }
    
    public virtual void GoToNextStep()
    {
        if (currStepNum < steps.Length - 1)
        {
            GoToStep(currStepNum + 1);
        }
        else
        {
            //if (isFinalRound)
            if (roundNum >= totalRoundsNum - 1)
            {
                ResultsDisplay.teamNum = teamNum;  //this isn't really necessary for the server-based data loading
                
                //UnityEngine.SceneManagement.SceneManager.LoadScene(finalResultsSceneName);  //alternative to making it the second scene in build settings
                UnityEngine.SceneManagement.SceneManager.LoadScene(1);
            }
            else
            {
                //ResetEverything();
            }
        }
    }

    public virtual void GoToStep(int stepNum)
    {
        for (int i = 0; i < steps.Length; i++)
        {
            steps[i].display1.SetActive(false);
            steps[i].display2.SetActive(false);
        }

        currStepNum = stepNum;

        if (currStepNum == steps.Length - 1)  //is it the per-activity conclusion (the last step of the activity)?
        {
            CompletedActivity();
        }

        if (currStepNum >= 0 && currStepNum < steps.Length)
        {
            steps[currStepNum].display1.SetActive(true);
            steps[currStepNum].display2.SetActive(true);
        }

        currStepLoadTime = Time.time;
        autoAdvanceTimer = 0f;
    }

    public virtual void DisableAllSteps()
    {
        Debug.Log("MoonshotActivity disable all steps");
        for (int i = 0; i < steps.Length; i++)
        {
            steps[i].display1.SetActive(false);
            steps[i].display2.SetActive(false);
        }
    }

    public virtual void PauseMission()
    {
        isPaused = true;
        
        SetClockPaused(true);
    }

    public virtual void UnPauseMission()
    {
        isPaused = false;
        
        SetClockPaused(false);
    }

    protected virtual void StartRoundClock(float clockStartTime)
    {
        if (clock != null)
        {
            clock.time = clockStartTime;

            clock.countUp = false;

            clock.onCountdownComplete += TimeIsUp;
            
            clock.isCounting = clock.isVisible = true;
        }
    }

    protected virtual void StartConclusionClock(float clockStartTime)
    {
        if (clock != null)
        {
            clock.time = clockStartTime;

            clock.countUp = false;

            clock.onCountdownComplete -= TimeIsUp;
            
            clock.isCounting = !isPaused;
            clock.isVisible = true;
        }
    }
    
    protected virtual void SetClockPaused(bool isPaused)
    {
        if (clock != null)
        {
            clock.isCounting = !isPaused;
        }
    }
    
    protected virtual void TimeIsUp()
    {
        if (clock != null)
        {
            clock.onCountdownComplete -= TimeIsUp;
        }
        
        GoToStep(steps.Length - 1);
    }

    protected virtual void CompletedActivity()  //This is called even if they ran out of time or did poorly.
    {
        //set activity completed in local data
        if (Client.instance != null && Client.instance.team != null && Client.instance.team.MoonshotTeamData != null)
        {
            switch (Client.instance._moonshotStation)
            {
            case MoonshotStation.Rover:
                Client.instance.team.MoonshotTeamData.didRoverActivity = true;
                break;
            case MoonshotStation.Map:
                Client.instance.team.MoonshotTeamData.didMapActivity = true;
                break;
            case MoonshotStation.Art:
                Client.instance.team.MoonshotTeamData.didArtActivity = true;
                break;
            case MoonshotStation.Question:
                Client.instance.team.MoonshotTeamData.didCharterActivity = true;
                break;
            case MoonshotStation.Hunt:
                Client.instance.team.MoonshotTeamData.didHuntActivity = true;
                break;
            default:
                break;
            }

            Debug.Log("completed activity: " + Client.instance._moonshotStation);
        }

        //send updated data
        ClientSend.SendStationDataToServer();

        StartConclusionClock(roundConclusionDur);
    }

    public virtual void ResetEverything()
    {
        GoToStep(0);
    }
}