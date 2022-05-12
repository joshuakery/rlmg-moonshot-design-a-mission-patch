using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using ArtScan;
using ArtScan.WordSavingUtilsModule;

public class UIManager : MonoBehaviour
{
    public GameState gameState;
    public saveScans saveScans;
    public GameEvent closeAllWindows;
    public GameEvent startEvent;
    //debug
    public Timer mainTimer;
    public int currentWindow;
    public List<GameEvent> windowEvents;
    public GameObject RemoveBackgroundDebugMenu;
    public GameObject DrawingsMenu;
    public Canvas primaryCanvas;
    public Canvas secondaryCanvas;

    public bool started = false;

    private void Start() 
    {
        started = false;
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

    public void CloseAndReopenCurrentWindow()
    {
        closeAllWindows.Raise();
        windowEvents[currentWindow].Raise();  
    }

    private IEnumerator LateStart()
    {
        yield return null; //just need to wait one frame
        startEvent.Raise();
    }

    public void StartGame()
    {
        started = true;
        StartCoroutine(LateStart());
    }

    public void StartGameOrReopenCurrentWindow()
    {
        if (!started) StartGame();
        else CloseAndReopenCurrentWindow();
    }

    public void ResetGame()
    {
        gameState.Reset();
        closeAllWindows.Raise();
        StartGame();
    }

    private void Awake()
    {
        SetCurrentWindowListeners();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            DrawingsMenu.SetActive(!DrawingsMenu.activeSelf);
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            RemoveBackgroundDebugMenu.SetActive(!RemoveBackgroundDebugMenu.activeSelf);
        }
        //Main Timer
        else if (Input.GetKeyDown(KeyCode.F))
        {
            mainTimer.time = 5;
        }
        else if (Input.GetKeyDown(KeyCode.P))
        {
            mainTimer.isCounting = !mainTimer.isCounting;
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            mainTimer.time = mainTimer.duration;
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
            saveScans.DeleteAllScans();
            ResetGame();
        }
        //Windows Events
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (windowEvents.Count > 0)
            {
                currentWindow = (currentWindow + 1) % windowEvents.Count;
                closeAllWindows.Raise();
                mainTimer.time = mainTimer.duration;
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
                mainTimer.time = mainTimer.duration;
                windowEvents[currentWindow].Raise();
            }
        }

    }

}
