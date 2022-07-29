using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoonshotActivity : MonoBehaviour
{
    public int teamNum = 0;
    //public bool isFinalRound = true;
    public int roundNum = 0;
    public int totalRoundsNum = 5;

    public float clockStartTime = 600;

    public bool useServer = true;

    public GameObject[] welcomeScreens;
    
    public int startStepNum = 0;
    private int currStepNum;

    public ResultsStep[] steps;

    [System.Serializable]
    public class ResultsStep
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
    public Text didRoverActivityDebugText;

    void Awake()
    {
#if !UNITY_EDITOR
	    startStepNum = 0;  //just to prevent this from being set to somethiing else in builds, as it's usually an undesired accident
#endif
    }

    void OnEnable()
    {
        if (useServer == true)
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
    }

    void OnDisable()
    {
        if (Client.instance != null)
        {
            Client.instance.onStartRound -= StartRound;
            Client.instance.onPauseMission -= PauseMission;
            Client.instance.onUnPauseMission -= UnPauseMission;
        }
    }

    public void StartRound(string _teamName, float _roundDuration, float _roundBufferDuration, int _round, string _JsonTeamData)
    {
        Debug.Log("Moonshot Activity received StartRound call from Server");

        clockStartTime = _roundDuration;

        Clock clock = GetComponent<Clock>();
        if (clock != null)
        {
            clock.time = clockStartTime;

            clock.countUp = false;

            clock.onCountdownComplete += TimeIsUp;
            
            clock.isCounting = clock.isVisible = true;
        }

        //set the timing of the per-activity conclusion: indefinite (0) after normal rounds, and the "buffer duration" for the last round
        if (roundNum >= totalRoundsNum - 1)  //final round
        {
            steps[steps.Length - 1].autoAdvanceDur = _roundBufferDuration;
        }
        else
        {
            steps[steps.Length - 1].autoAdvanceDur = 0;  //indefinite
        }

        roundNum = _round;
        
        LoadTeamResults();
    }

    [ContextMenu("Load Team Results")]
    public void LoadTeamResults()
    {
        if (useServer == true)
        {
            LoadTeamResults(Client.instance.team.MoonshotTeamData);
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

        LoadTeamResults(teamData);
    }

    public void LoadTeamResults(MoonshotTeamData teamData)
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

        if (didRoverActivityDebugText != null)
        {
            didRoverActivityDebugText.text = "completed rover activity: " + teamData.didRoverActivity.ToString();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            GoToPrevStep();
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            GoToNextStep();
        }
        
        if (steps[currStepNum].autoAdvanceDur > 0f && Time.time - currStepLoadTime >= steps[currStepNum].autoAdvanceDur)
        {
            GoToNextStep();
        }

        // if (Input.GetKeyDown(KeyCode.R))
        // {
        //     UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        // }
    }

    public void GoToPrevStep()
    {
        if (currStepNum > 0)
        {
            GoToStep(currStepNum - 1);
        }
    }
    
    public void GoToNextStep()
    {
        if (currStepNum < steps.Length - 1)
        {
            GoToStep(currStepNum + 1);

            if (currStepNum == steps.Length - 1)  //is it the per-activity conclusion (the last step of the activity)?
            {
                //set activity completed in local data
                Client.instance.team.MoonshotTeamData.didRoverActivity = true;

                //send updated round data
                ClientSend.SendStationDataToServer();
            }
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

    public void GoToStep(int stepNum)
    {
        for (int i = 0; i < steps.Length; i++)
        {
            steps[i].display1.SetActive(false);
            steps[i].display2.SetActive(false);
        }

        currStepNum = stepNum;

        steps[currStepNum].display1.SetActive(true);
        steps[currStepNum].display2.SetActive(true);

        currStepLoadTime = Time.time;
    }

    private void PauseMission()
    {
        SetClockPaused(true);
    }

    private void UnPauseMission()
    {
        SetClockPaused(false);
    }

    private void SetClockPaused(bool isPaused)
    {
        Clock clock = GetComponent<Clock>();
        if (clock != null)
        {
            clock.isCounting = !isPaused;
        }
    }
    
    private void TimeIsUp()
    {
        GoToStep(steps.Length - 1);
    }

    public void ResetEverything()
    {
        GoToStep(0);
    }
}
