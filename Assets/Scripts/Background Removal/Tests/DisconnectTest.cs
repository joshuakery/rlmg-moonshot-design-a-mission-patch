using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using rlmg.logging;

public class DisconnectTest : MonoBehaviour
{
    public GameEvent OnBeginScanEvent;
    public GameEvent ScanAgainEvent;

    private bool canDoNextScan = false;

    public void CanDoNextScan(bool value)
    {
        canDoNextScan = value;
    }

    public void Abort()
    {
        StopAllCoroutines();
    }

    // Update is called once per frame
    void Update()
    {
        if (
            (Input.GetKey(KeyCode.RightShift) || Input.GetKey(KeyCode.LeftShift)) &&
            Input.GetKeyDown(KeyCode.T)
        )
        {
            RLMGLogger.Instance.Log("Running disconnect test...", MESSAGETYPE.INFO);

            StopAllCoroutines();
            StartCoroutine(Run());
        }
    }

    private IEnumerator Run()
    {
        for (int i = 0; i < 100; i++)
        {
            RLMGLogger.Instance.Log("Test number: " + i.ToString(), MESSAGETYPE.INFO);

            canDoNextScan = false;

            OnBeginScanEvent.Raise();

            while(!canDoNextScan)
            {
                yield return null;
            }

            yield return new WaitForSeconds(1f);

            ScanAgainEvent.Raise();

            yield return new WaitForSeconds(0.5f);
        }
    }
}
