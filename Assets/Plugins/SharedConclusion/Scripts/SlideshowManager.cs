using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SlideshowManager : MonoBehaviour
{
    //public int teamNum = 0;

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

    void Start()
    {
        GoToStep(startStepNum);
        //ResetEverything();
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
        }
        else
        {
            ResetEverything();
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

    public void ResetEverything()
    {
        SceneManager.LoadScene(0);
    }
}
