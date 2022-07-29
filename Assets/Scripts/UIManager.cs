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

public class UIManager : MonoBehaviour
{
    public GameState gameState;
    public GameEvent closeAllWindows;
    public GameEvent startEvent;
    //debug
    public GameEvent ConfigLoaded;
    public Timer mainTimer;
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

    private void Start() 
    {
        started = false;
        gameState.teams = new List<MoonshotTeamData>();
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

    public void StartGameOrReopenCurrentWindow()
    {
        if (!started) StartGame();
        else CloseAndReopenCurrentWindow();
    }

    public void StartGame()
    {
        started = true;
        //closeAllWindows.Raise();
        //startEvent.Raise();
        StartCoroutine(LateStart());
        RLMGLogger.Instance.Log("Starting game...", MESSAGETYPE.INFO);
    }

    private IEnumerator LateStart()
    {
        yield return null; //just need to wait one frame
        closeAllWindows.Raise();
        startEvent.Raise();
    }

    public void CloseAndReopenCurrentWindow()
    {
        closeAllWindows.Raise();
        windowEvents[currentWindow].Raise();
    }


    public void ResetGame()
    {
        gameState.Reset();

        //closeAllWindows.Raise();

        StartGame();

        RLMGLogger.Instance.Log("Resetting game.", MESSAGETYPE.INFO);
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
            closeAllWindows.Raise();
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            startEvent.Raise();
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
                closeAllWindows.Raise();
                windowEvents[currentWindow].Raise();  
            }
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (windowEvents.Count > 0)
            {
                currentWindow -= 1;
                if (currentWindow < 0) currentWindow = windowEvents.Count - 1;
                closeAllWindows.Raise();
                windowEvents[currentWindow].Raise();
            }
        }

    }

}
