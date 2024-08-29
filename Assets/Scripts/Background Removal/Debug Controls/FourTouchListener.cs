using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FourTouchListener : MonoBehaviour
{
    private FourTouchOpenManager manager;

    [SerializeField]
    private int[] solution;

    [SerializeField]
    private GameObject toActivate;

    private void Awake()
    {
        manager = FindObjectOfType<FourTouchOpenManager>();
    }
    private void OnEnable()
    {
        if (manager != null)
            manager.onClickAdded.AddListener(CheckSolution);
    }

    private void OnDisable()
    {
        if (manager != null)
            manager.onClickAdded.RemoveListener(CheckSolution);
    }

    private void CheckSolution()
    {
        if (manager != null)
        {
            bool result = manager.CheckSolution(solution);
            if (result && toActivate != null)
                toActivate.SetActive(!toActivate.activeInHierarchy);
        }
        
        
    }
}
