using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class FourTouchOpenManager : MonoBehaviour
{
    [SerializeField]
    private int index;

    [SerializeField]
    private int[] clicks;

    public UnityEvent onClickAdded;

    [SerializeField]
    private Button button1;
    [SerializeField]
    private Button button2;
    [SerializeField]
    private Button button3;
    [SerializeField]
    private Button button4;

    private void OnEnable()
    {
        if (button1 != null)
            button1.onClick.AddListener(OnClick1);
        if (button2 != null)
            button2.onClick.AddListener(OnClick2);
        if (button3 != null)
            button3.onClick.AddListener(OnClick3);
        if (button4 != null)
            button4.onClick.AddListener(OnClick4);
    }

    private void OnDisable()
    {
        if (button1 != null)
            button1.onClick.RemoveListener(OnClick1);
        if (button2 != null)
            button2.onClick.RemoveListener(OnClick2);
        if (button3 != null)
            button3.onClick.RemoveListener(OnClick3);
        if (button4 != null)
            button4.onClick.RemoveListener(OnClick4);
    }

    private void OnClick1()
    {
        OnClick(1);
    }

    private void OnClick2()
    {
        OnClick(2);
    }

    private void OnClick3()
    {
        OnClick(3);
    }

    private void OnClick4()
    {
        OnClick(4);
    }

    public void OnClick(int n)
    {
        if (clicks == null || clicks.Length != 4)
        {
            clicks = new int[4];
            index = 0;
        }

        if (Array.IndexOf(clicks,n) > -1)
        {
            for (int i = 0; i < clicks.Length; i++)
            {
                clicks[i] = 0;
            }

            index = 0;
        }

        clicks[index] = n;

        index = (index + 1) % clicks.Length;

        onClickAdded.Invoke();
    }

    public bool CheckSolution(int[] solution)
    {
        if (solution == null || solution.Length != clicks.Length)
            return false;

        for (int i=0; i<clicks.Length; i++)
        {
            if (clicks[i] != solution[i])
                return false;
        }

        return true;
    }


}
