using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using ArtScan;
using ArtScan.WordSavingUtilsModule;
using rlmg.logging;
using DG.Tweening;
using MoonshotTimer;
using OpenCVForUnity.UnityUtils.Helper;

public class UIManager : MonoBehaviour
{
    public GameState gameState;
    public GameEvent CloseAllWindowsEvent;
    public GameEvent StartEvent;
    public myWebCamTextureToMatHelper myWebCamTextureToMatHelper;
    public GameEvent EndingEvent;

    public UISequenceManager primarySequenceManager;
    public UISequenceManager namesakeSequenceManager;
    //debug
    public GameEvent ConfigLoaded;
    public MoonshotTimer.Timer mainTimer;
    public int currentWindow;
    public List<GameEvent> windowEvents;
    public GameObject RemoveBackgroundDebugMenu;
    public GameObject DrawingsMenu;
    public Canvas primaryCanvas;
    public Canvas secondaryCanvas;

    public bool started = false;

    public UITweener UITweener1;
    public UITweener UITweener2;

    private void Awake()
    {
        SetCurrentWindowListeners();
    }



    private void SetCurrentWindowListeners()
    {
        for (int i=0; i<windowEvents.Count; i++)
        {
            GameObject empty = new GameObject("Current Window Listener");
            empty.SetActive(false);

            GameEventListener listener = empty.AddComponent<GameEventListener>();

            listener.Event = windowEvents[i];

            int k = i; //capture the value of i, not the variable
            listener.Response = new UnityEvent();
            listener.Response.AddListener(delegate {
                currentWindow = k;
            });

            empty.transform.parent = gameObject.transform;
            empty.SetActive(true);
        }
    }

    //public void StartGameOrReopenCurrentWindow()
    //{
    //    if (!started) StartGame();
    //    else CloseAndReopenCurrentWindow();
    //}

    //public void StartGame()
    //{


    //    started = true;
    //    //closeAllWindows.Raise();
    //    //startEvent.Raise();
    //    StartCoroutine(LateStart());
    //    RLMGLogger.Instance.Log("Starting game...", MESSAGETYPE.INFO);
    //}






    //public void ResetGame()
    //{
    //    gameState.Reset();

    //    //closeAllWindows.Raise();

    //    StartGame();

    //    RLMGLogger.Instance.Log("Resetting game.", MESSAGETYPE.INFO);
    //}

    private void Start()
    {
        started = false;
        gameState.teams = new List<MoonshotTeamData>();

        ResetGame(false); //do not open Welcome window
    }

    public void ResetGame()
    {
        _ResetGame(true);
    }

    public void ResetGame(bool doRestart)
    {
        _ResetGame(doRestart);
    }

    private void _ResetGame(bool doRestart)
    {
        primarySequenceManager.CompleteCurrentSequence();
        namesakeSequenceManager.CompleteCurrentSequence();

        gameState.Reset();

        if (mainTimer != null && mainTimer.time == 0) { mainTimer.Reset(); } //only ok to reset main timer if not set by server

        if (doRestart) { StartCoroutine(LateStart()); }
        else { StartCoroutine(LateClose()); }

    }

    public void CallLateStartCoroutine()
    {
        StartCoroutine(LateStart());
    }

    private IEnumerator LateStart()
    {
        if (!started)
        {
            yield return null; //just need to wait one frame
            CloseAllWindowsEvent.Raise();

            if (myWebCamTextureToMatHelper != null && myWebCamTextureToMatHelper.IsInitialized())
            {
                StartEvent.Raise();
                started = true;
            }
        }
    }

    private IEnumerator LateClose()
    {
        yield return null; //just need to wait one frame
        CloseAllWindowsEvent.Raise();
        currentWindow = -1;
    }

    public void CloseAndReopenCurrentWindow()
    {
        primarySequenceManager.CompleteCurrentSequence();
        namesakeSequenceManager.CompleteCurrentSequence();

        CloseAllWindowsEvent.Raise();

        if (currentWindow >= 0 && currentWindow < windowEvents.Count)
        { windowEvents[currentWindow].Raise(); }
    }

    public void OnMainTimeout(float delay)
    {
        StartCoroutine(WrapUp(delay));
    }

    private IEnumerator WrapUp(float delay)
    {
        mainTimer.PauseCounting();
        mainTimer.SetToZero();

        yield return new WaitForSeconds(delay);

        CloseAllWindowsEvent.Raise();
        EndingEvent.Raise();
    }



    public void GoToConclusion()
    {
        ResultsDisplayManager.teamNum = gameState.currentTeamIndex;

        string finalResultsSceneName = "ResultsScene";

        SceneManager.LoadScene(finalResultsSceneName);  //alternative string-based approach
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            DrawingsMenu.SetActive(!DrawingsMenu.activeSelf);
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            RemoveBackgroundDebugMenu.SetActive(!RemoveBackgroundDebugMenu.activeSelf);
        }
        //Shared Conclusion
        else if (Input.GetKeyDown(KeyCode.C))
        {
            GoToConclusion();
        }
        //Switch Displays
        else if (Input.GetKeyDown(KeyCode.S))
        {
            int aux = primaryCanvas.targetDisplay;
            primaryCanvas.targetDisplay = secondaryCanvas.targetDisplay;
            secondaryCanvas.targetDisplay = aux;
        }
        //Main Timer
        else if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            mainTimer.time = 2;
            mainTimer.StartCounting();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            mainTimer.time = 12;
            mainTimer.StartCounting();
        }
        //Reset Game
        else if (Input.GetKeyDown(KeyCode.Z))
        {
            ResetGame();
        }
        else if (Input.GetKeyDown(KeyCode.K))
        {
            WordSaving.DeleteFile(gameState.saveFile);
            ResetGame();
        }
        else if (Input.GetKeyDown(KeyCode.U))
        {
            CloseAllWindowsEvent.Raise();
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            StartEvent.Raise();
        }
        else if (Input.GetKeyDown(KeyCode.T))
        {
            UITweener1.AppendTweener(UITweener.TweenType.Exit);
            UITweener2.AppendTweener(UITweener.TweenType.Entry);
        }
        //Windows Events
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (windowEvents.Count > 0)
            {
                currentWindow = (currentWindow + 1) % windowEvents.Count;
                CloseAllWindowsEvent.Raise();
                windowEvents[currentWindow].Raise();  
            }
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (windowEvents.Count > 0)
            {
                currentWindow -= 1;
                if (currentWindow < 0) currentWindow = windowEvents.Count - 1;
                CloseAllWindowsEvent.Raise();
                windowEvents[currentWindow].Raise();
            }
        }

    }

}
